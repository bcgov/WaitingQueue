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
namespace BCGov.WaitingQueue.Controllers
{
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WebCommon.Delegates;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to request Tickets to interact with the associated system.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TicketController : Controller
    {
        private readonly IWebTicketDelegate ticketDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketController"/> class.
        /// </summary>
        /// <param name="ticketDelegate">The injected web ticket delegate.</param>
        public TicketController(IWebTicketDelegate ticketDelegate)
        {
            this.ticketDelegate = ticketDelegate;
        }

        /// <summary>
        /// Request a ticket which either creates a ticket or puts the user in a waiting room.
        /// </summary>
        /// <returns>A ticket response when successful.</returns>
        /// <param name="room">The room for which the client is requesting a ticket.</param>
        /// <response code="200">Ticket returned.</response>
        /// <response code="404">The requested room was not found.</response>
        /// <response code="503">The service is too busy, retry after the amount of time specified in retry-after.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Ticket), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]

        public async Task<IActionResult> CreateTicket([FromQuery]string room)
        {
            return this.Ok(await this.ticketDelegate.CreateTicket(room).ConfigureAwait(true));
        }

        /// <summary>
        /// Performs a check-in on the Ticket which will update the state and/or ticket associated.
        /// </summary>
        /// <returns>The updated Ticket.</returns>
        /// <param name="checkInRequest">The ticket request to check-in.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="404">The requested ticket was not found.</response>
        /// <response code="412">The service is unable to complete the request, review the error.</response>
        [HttpPut]
        [Route("check-in")]
        [ProducesResponseType(typeof(Ticket), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        public async Task<IActionResult> CheckIn(CheckInRequest checkInRequest)
        {
            return this.Ok(await this.ticketDelegate.CheckIn(checkInRequest).ConfigureAwait(true));
        }

        /// <summary>
        /// Releases the ticket and associated resources from the server.
        /// A good client will call this as they are disconnecting the session.
        /// </summary>
        /// <returns>The ticket that was removed.</returns>
        /// <param name="checkInRequest">The ticket request to remove.</param>
        /// <response code="200">The ticket was removed.</response>
        /// <response code="404">The requested was not found.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete]
        public async Task<IActionResult> RemoveTicket(CheckInRequest checkInRequest)
        {
            return await this.ticketDelegate.RemoveTicket(checkInRequest).ConfigureAwait(true);
        }
    }
}
