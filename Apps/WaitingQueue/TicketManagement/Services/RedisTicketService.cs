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
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Common.Delegates;
    using BCGov.WaitingQueue.TicketManagement.Constants;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
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
        private readonly IConfiguration configuration;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IDateTimeDelegate dateTimeDelegate;
        private readonly OpenIdConnectProtocolValidator nonceGenerator = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisTicketService"/> class.
        /// </summary>
        /// <param name="logger">The logging provider.</param>
        /// <param name="configuration">The configuration provider.</param>
        /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
        /// <param name="dateTimeDelegate">The datetime delegate.</param>
        public RedisTicketService(
            ILogger<RedisTicketService> logger,
            IConfiguration configuration,
            IConnectionMultiplexer connectionMultiplexer,
            IDateTimeDelegate dateTimeDelegate)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.connectionMultiplexer = connectionMultiplexer;
            this.dateTimeDelegate = dateTimeDelegate;
        }

        /// <inheritdoc />
        public async Task<TicketResponse> RequestTicket(string room)
        {
            TicketResponse ticketResponse = new()
            {
                Id = Guid.NewGuid(),
                Room = room,
                Status = TicketStatus.NotFound,
                CreatedTime = this.dateTimeDelegate.UtcUnixTime,
            };
            RoomConfiguration? roomConfig = this.GetRoomConfiguration(room);
            if (roomConfig is not null)
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();
                IDatabase database = this.connectionMultiplexer.GetDatabase();
                (long participantCount, long waitingCount, _) = await this.RoomCounts(roomConfig).ConfigureAwait(true);
                if (waitingCount >= roomConfig.QueueMaxSize)
                {
                    ticketResponse.Status = TicketStatus.TooBusy;
                }
                else
                {
                    string member = ticketResponse.Id.ToString();
                    ITransaction trans;
                    if (participantCount < roomConfig.QueueThreshold)
                    {
                        ticketResponse.Status = TicketStatus.Processed;
                        ticketResponse.Ticket = this.CreateJwt(roomConfig, ticketResponse.Id);
                        trans = database.CreateTransaction();
                        _ = trans.HashSetAsync(GetRoomName(roomConfig, ParticipantsKey), member, string.Empty);
                        this.CheckIn(trans, roomConfig, ticketResponse);
                        await trans.ExecuteAsync().ConfigureAwait(true);
                    }
                    else
                    {
                        ticketResponse.Status = TicketStatus.Queued;
                        long? nextCheckIn = null;
                        if (participantCount + waitingCount < roomConfig.ParticipantLimit)
                        {
                            nextCheckIn = this.dateTimeDelegate.UtcUnixTime;
                        }

                        SortedSetEntry waitingScoreEntry = (await database.SortedSetRangeByRankWithScoresAsync(
                            GetRoomName(roomConfig, WaitingKey),
                            -1).ConfigureAwait(true)).FirstOrDefault();
                        trans = database.CreateTransaction();
                        _ = trans.SortedSetAddAsync(
                            GetRoomName(roomConfig, WaitingKey),
                            member,
                            waitingScoreEntry.Score + 1).ConfigureAwait(true);
                        Task<long?> positionTask = trans.SortedSetRankAsync(GetRoomName(roomConfig, WaitingKey), member);
                        this.CheckIn(trans, roomConfig, ticketResponse, nextCheckIn);
                        await trans.ExecuteAsync().ConfigureAwait(true);

                        long? position = await positionTask.ConfigureAwait(true);
                        ticketResponse.QueuePosition = position + 1 ?? 0;
                    }
                }

                stopwatch.Stop();
                this.logger.LogDebug("RequestTicket Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            }

            return ticketResponse;
        }

        /// <inheritdoc />
        public async Task<TicketResponse> CheckIn(CheckInRequest checkInRequest)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            TicketResponse? ticketResponse;
            IDatabase database = this.connectionMultiplexer.GetDatabase();
            RedisValue redisTicket = await database.StringGetAsync($"{checkInRequest.Room}:{checkInRequest.Id}").ConfigureAwait(true);
            if (redisTicket.HasValue)
            {
                ticketResponse = JsonSerializer.Deserialize<TicketResponse>(redisTicket.ToString());
                if (ticketResponse != null && ticketResponse.Nonce == checkInRequest.Nonce)
                {
                    if (ticketResponse.CheckInAfter > this.dateTimeDelegate.UtcUnixTime)
                    {
                        ticketResponse.Status = TicketStatus.TooEarly;
                    }
                    else
                    {
                        RoomConfiguration? roomConfig = this.GetRoomConfiguration(ticketResponse.Room);
                        bool admit = false;
                        string member = ticketResponse.Id.ToString();
                        if (ticketResponse.Status == TicketStatus.Queued)
                        {
                            (long participantCount, _, long position) = await this.RoomCounts(roomConfig, member).ConfigureAwait(true);
                            admit = participantCount + position < roomConfig.ParticipantLimit;
                        }

                        ITransaction trans = database.CreateTransaction();
                        if (admit)
                        {
                            ticketResponse.Status = TicketStatus.Processed;
                            ticketResponse.Ticket = this.CreateJwt(roomConfig, ticketResponse.Id);
                            _ = trans.SortedSetRemoveAsync(GetRoomName(roomConfig, WaitingKey), member);
                            _ = trans.HashSetAsync(GetRoomName(roomConfig, ParticipantsKey), member, string.Empty);
                        }

                        this.CheckIn(trans, roomConfig, ticketResponse);
                        await trans.ExecuteAsync().ConfigureAwait(true);
                    }
                }
                else
                {
                    ticketResponse = new()
                    {
                        Status = TicketStatus.InvalidRequest,
                    };
                }
            }
            else
            {
                ticketResponse = new()
                {
                    Status = TicketStatus.NotFound,
                };
            }

            stopwatch.Stop();
            this.logger.LogDebug("CheckIn Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return ticketResponse;
        }

        private static string GetRoomName(RoomConfiguration config, string roomType)
        {
            return $"{{{config.Name}}}:Room:{roomType}";
        }

        private string CreateJwt(RoomConfiguration roomConfig, Guid id)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            this.logger.LogTrace("Room PrivateKey\n{PrivateKey}", roomConfig.PrivateKey);
            byte[] privateKey = Convert.FromBase64String(roomConfig.PrivateKey);
            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(privateKey, out _);
            SigningCredentials signingCredentials = new(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory
                {
                    CacheSignatureProviders = false,
                },
            };

            JwtSecurityToken jwt = new(
                issuer: roomConfig.Issuer,
                audience: roomConfig.Name,
                claims: new[]
                {
                    new Claim(
                        JwtRegisteredClaimNames.Iat,
                        this.dateTimeDelegate.UtcUnixTime.ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Jti, id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Azp, roomConfig.Name),
                },
                notBefore: this.dateTimeDelegate.UtcNowDateTime,
                expires: this.dateTimeDelegate.UtcNowDateTime.AddMinutes(roomConfig.TicketTtl),
                signingCredentials: signingCredentials);

            stopwatch.Stop();
            this.logger.LogDebug("CreateJwt Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private async Task<(long ParticipantCount, long WaitingCount, long Position)> RoomCounts(
            RoomConfiguration roomConfig,
            string? member = null)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            IDatabase database = this.connectionMultiplexer.GetDatabase();
            RedisValue[] expired = await database.SortedSetRangeByScoreAsync(
                GetRoomName(roomConfig, CheckInKey),
                0,
                this.dateTimeDelegate.UtcUnixTime,
                take: roomConfig.RemoveExpiredMax).ConfigureAwait(true);
            ITransaction transaction = database.CreateTransaction();
            if (expired.Length > 0)
            {
                _ = transaction.SortedSetRemoveAsync(GetRoomName(roomConfig, CheckInKey), expired, CommandFlags.FireAndForget);
                _ = transaction.SortedSetRemoveAsync(GetRoomName(roomConfig, WaitingKey), expired, CommandFlags.FireAndForget);
                _ = transaction.HashDeleteAsync(GetRoomName(roomConfig, ParticipantsKey), expired, CommandFlags.FireAndForget);
            }

            Task<long> participantCountTask = transaction.HashLengthAsync(GetRoomName(roomConfig, ParticipantsKey));
            Task<long> waitingCountTask = transaction.SortedSetLengthAsync(GetRoomName(roomConfig, WaitingKey));
            Task<long?>? positionTask = null;
            if (member != null)
            {
                positionTask = transaction.SortedSetRankAsync(GetRoomName(roomConfig, WaitingKey), member);
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
            this.logger.LogDebug("RoomCounts Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return (participantCount, waitingCount, position);
        }

        private void CheckIn(ITransaction transaction, RoomConfiguration roomConfig, TicketResponse ticketResponse, long? nextCheckIn = null)
        {
            ticketResponse.CheckInAfter = nextCheckIn ?? this.dateTimeDelegate.UtcUnixTime + roomConfig.CheckInFrequency;
            long checkInScore = ticketResponse.CheckInAfter + roomConfig.CheckInGrace;
            TimeSpan expiry = TimeSpan.FromSeconds(roomConfig.CheckInFrequency + roomConfig.CheckInGrace);
            TimeSpan roomIdleTtl = TimeSpan.FromSeconds(roomConfig.RoomIdleTtl);
            ticketResponse.Nonce = this.nonceGenerator.GenerateNonce();
            string ticketJson = JsonSerializer.Serialize(ticketResponse);
            _ = transaction.StringSetAsync(
                $"{ticketResponse.Room}:{ticketResponse.Id}",
                ticketJson,
                expiry,
                flags: CommandFlags.FireAndForget);
            _ = transaction.SortedSetAddAsync(
                GetRoomName(roomConfig, CheckInKey),
                ticketResponse.Id.ToString(),
                checkInScore,
                CommandFlags.FireAndForget);
            _ = transaction.KeyExpireAsync(GetRoomName(roomConfig, CheckInKey), roomIdleTtl);
            _ = transaction.KeyExpireAsync(GetRoomName(roomConfig, ParticipantsKey), roomIdleTtl);
            _ = transaction.KeyExpireAsync(GetRoomName(roomConfig, WaitingKey), roomIdleTtl);
        }

        private RoomConfiguration? GetRoomConfiguration(string room)
        {
            RoomConfiguration? roomConfig = this.configuration.GetSection($"Room{room}").Get<RoomConfiguration>();
            return roomConfig;
        }
    }
}
