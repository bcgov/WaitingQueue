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

    /// <summary>
    /// Definition for the Ticket Service.
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Requests the creation of a Ticket.
        /// </summary>
        /// <param name="room">The room to use.</param>
        /// <returns>A ticket response containing a ticket if completed.</returns>
        Task<TicketResponse> RequestTicket(string room);

        /// <summary>
        /// Updates the Ticket Response to reflect a CheckIn.
        /// </summary>
        /// <param name="checkInRequest">The ticket request.</param>
        /// <returns>The updated Ticket Response.</returns>
        Task<TicketResponse> CheckIn(CheckInRequest checkInRequest);
    }
}
