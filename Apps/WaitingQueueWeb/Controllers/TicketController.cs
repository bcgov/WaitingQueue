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
    using System;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Services;
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
        /// Gets The Oidc Configuration for the given room.
        /// </summary>
        /// <param name="room">The room to get signing tokens for.</param>
        /// <returns>The OIDC Configuration.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("/{room}/.well-known/openid-configuration")]
        public IActionResult GetOidcConfiguration([FromRoute] string room)
        {
            OidcConfiguration? config = this.ticketDelegate.GetOidcConfiguration(room);
            if (config is null)
            {
                return this.BadRequest("OIDC Configuration not available.");
            }

            return this.Ok(config);
        }

        /// <summary>
        /// Gets a list of signing keys for token validation.
        /// </summary>
        /// <param name="room">The room to get signing tokens for.</param>
        /// <returns>The list of valid signing tokens.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{room}/protocol/openid-connect/jwks")]
        public IActionResult GetJwks([FromRoute] string room)
        {
            return this.Ok(this.ticketDelegate.GetJsonWebKeys(room));
        }

        /// <summary>
        /// Gets an existing ticket for the user.
        /// </summary>
        /// <returns>A ticket response when successful.</returns>
        /// <param name="room">The room to query.</param>
        /// <param name="ticketId">The ticket id to query.</param>
        /// <param name="nonce">The nonce for the given ticket.</param>
        /// <response code="200">Ticket returned.</response>
        /// <response code="404">The requested room was not found.</response>
        /// <response code="503">The service is too busy, retry after the amount of time specified in retry-after.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<Ticket>> GetTicket(string room, Guid ticketId, string nonce)
        {
            return await this.ticketDelegate.GetTicket(room, ticketId, nonce).ConfigureAwait(true);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<Ticket>> CreateTicket([FromQuery]string room)
        {
            return await this.ticketDelegate.CreateTicket(room).ConfigureAwait(true);
        }

        /// <summary>
        /// Performs a check-in on the Ticket which will update the state and/or ticket associated.
        /// </summary>
        /// <returns>The updated Ticket.</returns>
        /// <param name="ticketRequest">The ticket request to check-in.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="404">The requested ticket was not found.</response>
        /// <response code="412">The service is unable to complete the request, review the error.</response>
        /// <response code="500">The service is unable to complete the request due to an unexpected exception.</response>
        [HttpPut]
        [Route("check-in")]
        [ProducesResponseType(typeof(Ticket), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckIn(TicketRequest ticketRequest)
        {
            return this.Ok(await this.ticketDelegate.CheckInAsync(ticketRequest).ConfigureAwait(true));
        }

        /// <summary>
        /// Releases the ticket and associated resources from the server.
        /// A good client will call this as they are disconnecting the session.
        /// </summary>
        /// <returns>The ticket that was removed.</returns>
        /// <param name="ticketRequest">The ticket request to remove.</param>
        /// <response code="200">The ticket was removed.</response>
        /// <response code="404">The requested was not found.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete]
        public async Task RemoveTicket(TicketRequest ticketRequest)
        {
            await this.ticketDelegate.RemoveTicketAsync(ticketRequest).ConfigureAwait(true);
        }
    }
}
