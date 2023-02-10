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
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Common.Delegates;
    using BCGov.WaitingQueue.TicketManagement.Api;
    using BCGov.WaitingQueue.TicketManagement.Constants;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Models.Keycloak;
    using BCGov.WaitingQueue.TicketManagement.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using StackExchange.Redis;
    using TicketRequest = BCGov.WaitingQueue.TicketManagement.Models.TicketRequest;

    /// <summary>
    /// A Redis implementation of the Ticket Service interface.
    /// </summary>
    public class RedisTicketService : ITicketService
    {
        private const string ParticipantsKey = "Participants";
        private const string WaitingKey = "Waiting";
        private const string CheckInKey = "CheckIn";

        private readonly ILogger<RedisTicketService> logger;
        private readonly IConfiguration configuration;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IDateTimeDelegate dateTimeDelegate;
        private readonly IKeycloakApi keycloakApi;

        private readonly OpenIdConnectProtocolValidator nonceGenerator = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisTicketService"/> class.
        /// </summary>
        /// <param name="logger">The logging provider.</param>
        /// <param name="configuration">The configuration provider.</param>
        /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
        /// <param name="dateTimeDelegate">The datetime delegate.</param>
        /// <param name="keycloakApi">The keycloak api.</param>
        public RedisTicketService(
            ILogger<RedisTicketService> logger,
            IConfiguration configuration,
            IConnectionMultiplexer connectionMultiplexer,
            IDateTimeDelegate dateTimeDelegate,
            IKeycloakApi keycloakApi)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.connectionMultiplexer = connectionMultiplexer;
            this.dateTimeDelegate = dateTimeDelegate;
            this.keycloakApi = keycloakApi;
        }

        /// <inheritdoc />
        public async Task<Ticket> GetTicketAsync(TicketRequest ticketRequest, long? utcUnixTime = null)
        {
            RoomConfiguration? roomConfig = this.GetRoomConfiguration(ticketRequest.Room);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IDatabase database = this.connectionMultiplexer.GetDatabase();
            RedisValue redisTicket = await database.StringGetAsync(GetTicketKey(roomConfig, ticketRequest.Id)).ConfigureAwait(true);
            TicketCheckin.ValidateRedisTicket(redisTicket);

            Ticket? ticket = JsonSerializer.Deserialize<Ticket>(redisTicket.ToString());
            TicketCheckin.ValidateTicket(ticket, ticketRequest.Nonce, utcUnixTime);

            stopwatch.Stop();
            this.logger.LogDebug("GetTicketAsync Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);

            return ticket;
        }

        /// <inheritdoc />
        public async Task<Ticket> RequestTicketAsync(string room)
        {
            Ticket ticket = new()
            {
                Id = Guid.NewGuid(),
                Room = room,
                Status = TicketStatus.Processed,
                CreatedTime = this.dateTimeDelegate.UtcUnixTime,
            };
            RoomConfiguration? roomConfig = this.GetRoomConfiguration(room);
            Validation.TicketRequest.ValidateRoomConfig(roomConfig);

            Stopwatch stopwatch = new();
            stopwatch.Start();
            IDatabase database = this.connectionMultiplexer.GetDatabase();
            (long participantCount, long waitingCount, _) = await this.RoomCountsAsync(roomConfig).ConfigureAwait(true);
            Validation.TicketRequest.ValidateWaitingCount(waitingCount, roomConfig.QueueMaxSize);

            string member = ticket.Id.ToString();
            ITransaction trans;
            if (participantCount < roomConfig.QueueThreshold)
            {
                ticket.Status = TicketStatus.Processed;
                trans = database.CreateTransaction();
                _ = trans.HashSetAsync(GetRoomKey(roomConfig, ParticipantsKey), member, string.Empty);
                await this.CheckInAsync(trans, roomConfig, ticket).ConfigureAwait(true);
                await trans.ExecuteAsync().ConfigureAwait(true);
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
                        -1)
                    .ConfigureAwait(true)).FirstOrDefault();
                trans = database.CreateTransaction();
                _ = trans.SortedSetAddAsync(
                        GetRoomKey(roomConfig, WaitingKey),
                        member,
                        waitingScoreEntry.Score + 1)
                    .ConfigureAwait(true);
                Task<long?> positionTask = trans.SortedSetRankAsync(GetRoomKey(roomConfig, WaitingKey), member);
                await this.CheckInAsync(trans, roomConfig, ticket, nextCheckIn).ConfigureAwait(true);
                await trans.ExecuteAsync().ConfigureAwait(true);

                long? position = await positionTask.ConfigureAwait(true);
                ticket.QueuePosition = position + 1 ?? 0;
            }

            stopwatch.Stop();
            this.logger.LogDebug("RequestTicketAsync Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return ticket;
        }

        /// <inheritdoc />
        public async Task<Ticket> CheckInAsync(TicketRequest ticketRequest)
        {
            RoomConfiguration? roomConfig = this.GetRoomConfiguration(ticketRequest.Room);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            Ticket ticket = await this.GetTicketAsync(ticketRequest, this.dateTimeDelegate.UtcUnixTime).ConfigureAwait(true);
            bool admit = false;
            string member = ticket.Id.ToString();

            if (ticket.Status == TicketStatus.Queued)
            {
                (long participantCount, _, long position) = await this.RoomCountsAsync(roomConfig, member).ConfigureAwait(true);
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

            await this.CheckInAsync(trans, roomConfig, ticket).ConfigureAwait(true);
            await trans.ExecuteAsync().ConfigureAwait(true);
            stopwatch.Stop();
            this.logger.LogDebug("CheckInAsync Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return ticket;
        }

        private static string GetRoomKey(RoomConfiguration config, string roomType)
        {
            return $"{{{config.Name}}}:Room:{roomType}";
        }

        private static string GetTicketKey(RoomConfiguration config, Guid ticketId)
        {
            return $"{{{config.Name}}}:Ticket:{ticketId}";
        }

        private async Task<(string Token, long Expires)> CreateJwtAsync(RoomConfiguration roomConfig)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            TokenResponse tokenResponse = await this.keycloakApi.AuthenticateAsync(roomConfig.TokenRequest).ConfigureAwait(true);
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken token = handler.ReadJwtToken(tokenResponse.AccessToken);
            DateTimeOffset ticketExpiry = token.ValidTo;
            stopwatch.Stop();
            this.logger.LogDebug("CreateJwtAsync Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return (tokenResponse.AccessToken, ticketExpiry.ToUnixTimeSeconds());
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
                .ConfigureAwait(true);
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

            await transaction.ExecuteAsync().ConfigureAwait(true);
            long participantCount = await participantCountTask.ConfigureAwait(true);
            long waitingCount = await waitingCountTask.ConfigureAwait(true);
            long position = 0;
            if (positionTask != null)
            {
                position = await positionTask.ConfigureAwait(true) ?? 0;
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
                (ticket.Token, ticket.TokenExpires) = await this.CreateJwtAsync(roomConfig).ConfigureAwait(true);
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

        private RoomConfiguration? GetRoomConfiguration(string room)
        {
            RoomConfiguration? roomConfig = this.configuration.GetSection($"Room{room}").Get<RoomConfiguration>();
            return roomConfig;
        }
    }
}
