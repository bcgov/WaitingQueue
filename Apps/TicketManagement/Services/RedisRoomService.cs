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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Common.Delegates;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <inheritdoc />
    public class RedisRoomService : IRoomService
    {
        private const string ConfigKey = "config";
        private const string VersionKey = "version";
        private const string IndexKey = "Configuration:Index";

        private readonly ILogger<RedisRoomService> logger;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IDateTimeDelegate dateTimeDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisRoomService"/> class.
        /// </summary>
        /// <param name="logger">The logging provider.</param>
        /// <param name="configuration">The configuration provider.</param>
        /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
        /// <param name="dateTimeDelegate">The datetime delegate.</param>
        public RedisRoomService(
            ILogger<RedisRoomService> logger,
            IConfiguration configuration,
            IConnectionMultiplexer connectionMultiplexer,
            IDateTimeDelegate dateTimeDelegate)
        {
            this.logger = logger;
            this.connectionMultiplexer = connectionMultiplexer;
            this.dateTimeDelegate = dateTimeDelegate;
        }

        /// <inheritdoc />
        public async Task<RoomConfiguration?> ReadConfigurationAsync(string room)
        {
            this.logger.LogDebug("Fetching room configuration for {Room}", room);
            IDatabase db = this.connectionMultiplexer.GetDatabase();
            RedisValue value = await db.HashGetAsync(GetRoomConfigKey(room), ConfigKey);
            if (value.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<RoomConfiguration>(value);
        }

        /// <inheritdoc />
        public async Task<(bool Committed, RoomConfiguration RoomConfig)> WriteConfigurationAsync(RoomConfiguration roomConfig)
        {
            this.logger.LogDebug("Writing room configuration for {Room}", roomConfig.Name);
            long oldUpdated = roomConfig.LastUpdated;
            roomConfig.LastUpdated = this.dateTimeDelegate.UtcUnixTime;
            IDatabase db = this.connectionMultiplexer.GetDatabase();
            ITransaction transaction = db.CreateTransaction();
            string key = GetRoomConfigKey(roomConfig.Name);
            string configJson = JsonSerializer.Serialize(roomConfig);
            bool create = oldUpdated == 0;
            transaction.AddCondition(create ? Condition.KeyNotExists(key) : Condition.HashEqual(key, VersionKey, oldUpdated));
            _ = transaction.HashSetAsync(
                key,
                new[]
                {
                    new HashEntry(ConfigKey, configJson),
                    new HashEntry(VersionKey, roomConfig.LastUpdated),
                });
            bool committed = await transaction.ExecuteAsync();
            if (committed)
            {
                // Add to index to query for all rooms
                await db.HashSetAsync(IndexKey, roomConfig.Name.ToUpperInvariant(), string.Empty);
            }

            return (committed, roomConfig);
        }

        /// <inheritdoc />
        public async Task<bool> RoomExists(string room)
        {
            this.logger.LogDebug("Querying if room exists {Room}", room);
            IDatabase db = this.connectionMultiplexer.GetDatabase();
            return await db.HashExistsAsync(IndexKey, room.ToUpperInvariant());
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, RoomConfiguration>> GetRoomsAsync(IEnumerable<string> rooms)
        {
            this.logger.LogDebug("Fetching configured rooms");
            Dictionary<string, RoomConfiguration> roomsConfig = new();
            IDatabase db = this.connectionMultiplexer.GetDatabase();
            RedisValue[] keys = await db.HashKeysAsync(IndexKey);
            IEnumerable<RedisValue> queryKeys = keys.Where(key => rooms.Contains(key.ToString(), StringComparer.OrdinalIgnoreCase));
            foreach (RedisValue k in queryKeys)
            {
                RoomConfiguration? roomConfig = await this.ReadConfigurationAsync(k);
                if (roomConfig != null)
                {
                    roomsConfig.Add(k, roomConfig);
                }
            }

            return roomsConfig;
        }

        private static string GetRoomConfigKey(string room)
        {
            return $"Configuration:{{{room.ToUpperInvariant()}}}";
        }
    }
}
