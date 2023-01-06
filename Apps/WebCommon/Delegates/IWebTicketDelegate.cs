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
namespace BCGov.WebCommon.Delegates
{
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Wraps Ticket Management responses into reusable web responses.
    /// </summary>
    public interface IWebTicketDelegate
    {
        /// <summary>
        /// Request a ticket which either creates a ticket or puts the user in a waiting room.
        /// </summary>
        /// <returns>A ticket response when successful.</returns>
        /// <param name="room">The room for which the client is requesting a ticket.</param>
        /// <response code="200">Ticket returned.</response>
        /// <response code="400">The request was invalid.</response>
        /// <response code="404">The requested room was not found.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        /// <response code="503">The service is too busy, retry after the amount of time specified in retry-after.</response>
        Task<Ticket> CreateTicket(string room);

        /// <summary>
        /// Performs a check-in on the Ticket which will update the state and/or ticket associated.
        /// </summary>
        /// <returns>The updated Ticket.</returns>
        /// <param name="checkInRequest">The ticket request to check-in.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="400">The request was invalid.</response>
        /// <response code="404">The requested ticket was not found.</response>
        /// <response code="412">The service is unable to complete the request, review the error.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        Task<Ticket> CheckInAsync(CheckInRequest checkInRequest);

        /// <summary>
        /// Removes a ticket from the system.
        /// </summary>
        /// <returns>Ok or NotFound Result.</returns>
        /// <param name="checkInRequest">The ticket request to check-in.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="404">The requested ticket was not found.</response>
        Task<IActionResult> RemoveTicketAsync(CheckInRequest checkInRequest);
    }
}
