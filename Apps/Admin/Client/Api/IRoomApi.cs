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
namespace BCGov.WaitingQueue.Admin.Client.Api
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Admin.Common.Models;
    using Refit;

    /// <summary>
    /// API to interact with the Room configuration.
    /// </summary>
    public interface IRoomApi
    {
        /// <summary>
        /// Returns key/value pairing of room name and room configuration.
        /// </summary>
        /// <returns>The list of room configs.</returns>
        [Get("/")]
        Task<IDictionary<string, RoomConfiguration>> GetRoomsAsync();

        /// <summary>
        /// Creates or updates the room configuration.
        /// </summary>
        /// <param name="roomConfig">The room to create or update.</param>
        /// <returns>The newly created or updated room configuration.</returns>
        [Put("/")]
        Task<RoomConfiguration> UpsertConfiguration(RoomConfiguration roomConfig);

        /// <summary>
        /// Gets a room's statistics.
        /// </summary>
        /// <returns>The room statistics.</returns>
        [Get("/stats")]
        Task<IEnumerable<RoomStatistics>> GetRoomStatistics();
    }
}
