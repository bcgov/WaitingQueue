// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace BCGov.WaitingQueue.TicketManagement.Services
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Common.Delegates;
    using BCGov.WaitingQueue.TicketManagement.Constants;
    using BCGov.WaitingQueue.TicketManagement.Issuers;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Models.Statistics;
    using BCGov.WaitingQueue.TicketManagement.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using StackExchange.Redis;

    /// <summary>
    /// A Redis implementation of the Ticket Service interface.
    /// </summary>
    public class RedisTicketService : ITicketService
    {
        private const string ParticipantsKey = "Participants";
        private const string WaitingKey = "Waiting";
        private const string CheckInKey = "CheckIn";

        private readonly ILogger<RedisTicketService> logger;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IDateTimeDelegate dateTimeDelegate;
        private readonly ITokenIssuer tokenIssuer;
        private readonly IRoomService roomService;

        private readonly OpenIdConnectProtocolValidator nonceGenerator = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisTicketService"/> class.
        /// </summary>
        /// <param name="logger">The logging provider.</param>
        /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
        /// <param name="dateTimeDelegate">The datetime delegate.</param>
        /// <param name="tokenIssuer">The token issuer.</param>
        /// <param name="roomService">The room service for configuration lookup.</param>
        public RedisTicketService(
            ILogger<RedisTicketService> logger,
            IConnectionMultiplexer connectionMultiplexer,
            IDateTimeDelegate dateTimeDelegate,
            ITokenIssuer tokenIssuer,
            IRoomService roomService)
        {
            this.logger = logger;
            this.connectionMultiplexer = connectionMultiplexer;
            this.dateTimeDelegate = dateTimeDelegate;
            this.tokenIssuer = tokenIssuer;
            this.roomService = roomService;
        }

        /// <inheritdoc />
        public async Task<Ticket> GetTicketAsync(TicketRequest ticketRequest, long? utcUnixTime = null)
        {
            RoomConfiguration? roomConfig = await this.GetRoomConfiguration(ticketRequest.Room);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IDatabase database = this.connectionMultiplexer.GetDatabase();
            RedisValue redisTicket = await database.StringGetAsync(GetTicketKey(roomConfig, ticketRequest.Id));
            CheckIn.ValidateRedisTicket(redisTicket);

            Ticket? ticket = JsonSerializer.Deserialize<Ticket>(redisTicket.ToString());
            CheckIn.ValidateTicket(ticket, ticketRequest.Nonce, utcUnixTime);

            stopwatch.Stop();
            this.logger.LogDebug("GetTicketAsync Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);

            return ticket;
        }

        /// <inheritdoc />
        public async Task<Ticket> RequestTicketAsync(string room)
        {
            RoomConfiguration? roomConfig = await this.GetRoomConfiguration(room);
            Request.ValidateRoomConfig(roomConfig);
            Ticket ticket = new()
            {
                Id = Guid.NewGuid(),
                Room = roomConfig!.Name,
                Status = TicketStatus.Processed,
                CreatedTime = this.dateTimeDelegate.UtcUnixTime,
            };

            Stopwatch stopwatch = new();
            stopwatch.Start();
            IDatabase database = this.connectionMultiplexer.GetDatabase();
            (long participantCount, long waitingCount, _) = await this.RoomCountsAsync(roomConfig);
            Request.ValidateWaitingCount(waitingCount, roomConfig.QueueMaxSize);

            string member = ticket.Id.ToString();
            ITransaction trans;
            if (participantCount < roomConfig.QueueThreshold)
            {
                ticket.Status = TicketStatus.Processed;
                trans = database.CreateTransaction();
                _ = trans.HashSetAsync(GetRoomKey(roomConfig, ParticipantsKey), member, string.Empty);
                await this.CheckInAsync(trans, roomConfig, ticket);
                await trans.ExecuteAsync();
            }
            else
            {
                ticket.Status = TicketStatus.Queued;
                long? nextCheckIn = null;
                if (participantCount + waitingCount < roomConfig.ParticipantLimit)
                {
                    nextCheckIn = this.dateTimeDelegate.UtcUnixTime;
                }

                SortedSetEntry waitingScoreEntry = (await database.SortedSetRangeByRankWithScoresAsync(
                    GetRoomKey(roomConfig, WaitingKey),
                    -1)).FirstOrDefault();
                trans = database.CreateTransaction();
                _ = trans.SortedSetAddAsync(
                        GetRoomKey(roomConfig, WaitingKey),
                        member,
                        waitingScoreEntry.Score + 1)
                    ;
                Task<long?> positionTask = trans.SortedSetRankAsync(GetRoomKey(roomConfig, WaitingKey), member);
                await this.CheckInAsync(trans, roomConfig, ticket, nextCheckIn);
                await trans.ExecuteAsync();

                long? position = await positionTask;
                ticket.QueuePosition = position + 1 ?? 0;
            }

            stopwatch.Stop();
            this.logger.LogDebug("RequestTicketAsync Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return ticket;
        }

        /// <inheritdoc />
        public async Task<Ticket> CheckInAsync(TicketRequest ticketRequest)
        {
            RoomConfiguration? roomConfig = await this.GetRoomConfiguration(ticketRequest.Room);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            Ticket ticket = await this.GetTicketAsync(ticketRequest, this.dateTimeDelegate.UtcUnixTime);
            bool admit = false;
            string member = ticket.Id.ToString();

            if (ticket.Status == TicketStatus.Queued)
            {
                (long participantCount, _, long position) = await this.RoomCountsAsync(roomConfig, member);
                ticket.QueuePosition = position + 1;
                admit = participantCount + position < roomConfig.ParticipantLimit;
            }

            IDatabase database = this.connectionMultiplexer.GetDatabase();
            ITransaction trans = database.CreateTransaction();
            if (admit)
            {
                ticket.Status = TicketStatus.Processed;
                ticket.QueuePosition = 0;
                _ = trans.SortedSetRemoveAsync(GetRoomKey(roomConfig, WaitingKey), member);
                _ = trans.HashSetAsync(GetRoomKey(roomConfig, ParticipantsKey), member, string.Empty);
            }

            await this.CheckInAsync(trans, roomConfig, ticket);
            await trans.ExecuteAsync();
            stopwatch.Stop();
            this.logger.LogDebug("CheckInAsync Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return ticket;
        }

        /// <inheritdoc />
        public async Task<RoomStatistics> QueryRoomStatistics(string room)
        {
            RoomConfiguration? roomConfig = await this.GetRoomConfiguration(room);
            (long participantCount, long waitingCount, _) = await this.RoomCountsAsync(roomConfig);
            return new RoomStatistics(
                room,
                new[]
                {
                    new Counter("ParticipantCount", "Participant Count", participantCount),
                    new Counter("WaitingCount", "Waiting Count", waitingCount),
                });
        }

        private static string GetRoomKey(RoomConfiguration config, string roomType)
        {
            return $"{{{config.Name}}}:Room:{roomType}";
        }

        private static string GetTicketKey(RoomConfiguration config, Guid ticketId)
        {
            return $"{{{config.Name}}}:Ticket:{ticketId}";
        }

        private async Task<(long ParticipantCount, long WaitingCount, long Position)> RoomCountsAsync(
            RoomConfiguration roomConfig,
            string? member = null)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IDatabase database = this.connectionMultiplexer.GetDatabase();
            RedisValue[] expired = await database.SortedSetRangeByScoreAsync(
                    GetRoomKey(roomConfig, CheckInKey),
                    0,
                    this.dateTimeDelegate.UtcUnixTime,
                    take: roomConfig.RemoveExpiredMax)
                ;
            ITransaction transaction = database.CreateTransaction();
            if (expired.Length > 0)
            {
                _ = transaction.SortedSetRemoveAsync(GetRoomKey(roomConfig, CheckInKey), expired, CommandFlags.FireAndForget);
                _ = transaction.SortedSetRemoveAsync(GetRoomKey(roomConfig, WaitingKey), expired, CommandFlags.FireAndForget);
                _ = transaction.HashDeleteAsync(GetRoomKey(roomConfig, ParticipantsKey), expired, CommandFlags.FireAndForget);
            }

            Task<long> participantCountTask = transaction.HashLengthAsync(GetRoomKey(roomConfig, ParticipantsKey));
            Task<long> waitingCountTask = transaction.SortedSetLengthAsync(GetRoomKey(roomConfig, WaitingKey));
            Task<long?>? positionTask = null;
            if (member != null)
            {
                positionTask = transaction.SortedSetRankAsync(GetRoomKey(roomConfig, WaitingKey), member);
            }

            await transaction.ExecuteAsync();
            long participantCount = await participantCountTask;
            long waitingCount = await waitingCountTask;
            long position = 0;
            if (positionTask != null)
            {
                position = await positionTask ?? 0;
            }

            stopwatch.Stop();
            this.logger.LogDebug("RoomCountsAsync Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return (participantCount, waitingCount, position);
        }

        private async Task CheckInAsync(ITransaction transaction, RoomConfiguration roomConfig, Ticket ticket, long? nextCheckIn = null)
        {
            ticket.CheckInAfter = nextCheckIn ?? this.dateTimeDelegate.UtcUnixTime + roomConfig.CheckInFrequency;
            long checkInScore = ticket.CheckInAfter + roomConfig.CheckInGrace;
            TimeSpan expiry = TimeSpan.FromSeconds(roomConfig.CheckInFrequency + roomConfig.CheckInGrace);
            TimeSpan roomIdleTtl = TimeSpan.FromSeconds(roomConfig.RoomIdleTtl);
            ticket.Nonce = this.nonceGenerator.GenerateNonce();
            if (ticket.Status == TicketStatus.Processed && ticket.CheckInAfter >= ticket.TokenExpires)
            {
                (ticket.Token, ticket.TokenExpires) = await this.tokenIssuer.CreateTokenAsync(roomConfig.Name, ticket.Id.ToString("D"));
            }

            string ticketJson = JsonSerializer.Serialize(ticket);
            _ = transaction.StringSetAsync(
                GetTicketKey(roomConfig, ticket.Id),
                ticketJson,
                expiry,
                flags: CommandFlags.FireAndForget);
            _ = transaction.SortedSetAddAsync(
                GetRoomKey(roomConfig, CheckInKey),
                ticket.Id.ToString(),
                checkInScore,
                CommandFlags.FireAndForget);
            _ = transaction.KeyExpireAsync(GetRoomKey(roomConfig, CheckInKey), roomIdleTtl);
            _ = transaction.KeyExpireAsync(GetRoomKey(roomConfig, ParticipantsKey), roomIdleTtl);
            _ = transaction.KeyExpireAsync(GetRoomKey(roomConfig, WaitingKey), roomIdleTtl);
        }

        private async Task<RoomConfiguration?> GetRoomConfiguration(string room)
        {
            RoomConfiguration? roomConfig = await this.roomService.ReadConfigurationAsync(room);
            return roomConfig;
        }
    }
}
