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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;

    /// <summary>
    /// Defines commons operations interacting with the room.
    /// </summary>
    public interface IRoomService
    {
        /// <summary>
        /// Reads the room configuration from the datastore.
        /// </summary>
        /// <param name="room">The room to retrieve the configuration.</param>
        /// <returns>The read RoomConfiguration.</returns>
        Task<RoomConfiguration?> ReadConfigurationAsync(string room);

        /// <summary>
        /// Creates or Updates the room configuration in the datastore.
        /// </summary>
        /// <param name="roomConfig">The room configuration to create or update.</param>
        /// <param name="create">If true attempts to create the configuration instead of updating.</param>
        /// <returns>A boolean indicating if the operation was successful and the configuration.</returns>
        Task<(bool Committed, RoomConfiguration RoomConfig)> WriteConfigurationAsync(RoomConfiguration roomConfig, bool create = false);

        /// <summary>
        /// Returns an optimistic indicator for the existence of the room.
        /// </summary>
        /// <param name="room">The room to check for the existence of.</param>
        /// <returns>An indicator</returns>
        Task<bool> RoomExists(string room);

        /// <summary>
        /// Lists all the rooms in the datastore.
        /// </summary>
        /// <returns>A list of strings identifying each room.</returns>
        Task<IEnumerable<string>> GetRoomsAsync();
    }
}
