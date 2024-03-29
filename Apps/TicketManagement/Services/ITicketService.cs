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
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Models.Statistics;

    /// <summary>
    /// Definition for the Token Service.
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Gets the ticket for the supplied room if found and the nonce matches.
        /// </summary>
        /// <param name="ticketRequest">The ticket request.</param>
        /// <param name="utcUnixTime">If supplied performs validation on the CheckInAfter.</param>
        /// <returns>A ticket if found.</returns>
        Task<Ticket> GetTicketAsync(TicketRequest ticketRequest, long? utcUnixTime = null);

        /// <summary>
        /// Requests the creation of a ticket.
        /// </summary>
        /// <param name="room">The room to use.</param>
        /// <returns>A ticket containing a token if processed.</returns>
        Task<Ticket> RequestTicketAsync(string room);

        /// <summary>
        /// Updates the ticket to reflect a CheckInAsync.
        /// </summary>
        /// <param name="ticketRequest">The ticket request.</param>
        /// <returns>The updated Ticket.</returns>
        Task<Ticket> CheckInAsync(TicketRequest ticketRequest);

        /// <summary>
        /// Queries the statistics for a room.
        /// </summary>
        /// <param name="room">The room to query statistics for.</param>
        /// <returns>A statistics instance with counters.</returns>
        Task<RoomStatistics> QueryRoomStatistics(string room);
    }
}
