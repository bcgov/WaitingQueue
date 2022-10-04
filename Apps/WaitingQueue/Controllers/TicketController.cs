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
    using BCGov.WaitingQueue.Models;
    using BCGov.WaitingQueue.TicketManagement.Constants;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to request Tickets to interact with the associated system.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TicketController : Controller
    {
        private readonly ITicketService ticketService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketController"/> class.
        /// </summary>
        /// <param name="ticketService">The injected ticket service.</param>
        public TicketController(ITicketService ticketService)
        {
            this.ticketService = ticketService;
        }

        /// <summary>
        /// Request a ticket which either creates a ticket or puts the user in a waiting room.
        /// </summary>
        /// <returns>A ticket response when successful.</returns>
        /// <param name="room">The room for which the client is requesting a ticket.</param>
        /// <response code="200">Token Response returned.</response>
        /// <response code="400">The requested was invalid.</response>
        /// <response code="404">The requested room was not found.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        /// <response code="503">The service is too busy, retry after the amount of time specified in retry-after.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Ticket), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<Ticket>> CreateTicket([FromQuery]string room)
        {
            Ticket ticket = await this.ticketService.RequestTicket(room).ConfigureAwait(true);
            switch (ticket.Status)
            {
                case TicketStatus.NotFound:
                    return new JsonResult(new ErrorResult()
                    {
                        Code = string.Empty,
                        Message = $"The requested room: {room} was not found.",
                    })
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                    };
                case TicketStatus.TooBusy:
                    return new JsonResult(new ErrorResult()
                    {
                        Code = string.Empty,
                        Message = "The waiting queue has exceeded maximum capacity, try again later",
                    })
                    {
                        StatusCode = StatusCodes.Status503ServiceUnavailable,
                    };
                case TicketStatus.Processed:
                case TicketStatus.Queued:
                    return ticket;
                default:
                    return new BadRequestResult();
            }
        }

        /// <summary>
        /// Performs a check-in on the Ticket which will update the state and/or ticket associated.
        /// </summary>
        /// <returns>The updated Ticket.</returns>
        /// <param name="checkInRequest">The ticket request to check-in.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="400">The requested was invalid.</response>
        /// <response code="404">The requested ticket was not found.</response>
        /// <response code="412">The service is unable to complete the request, review the error.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        [HttpPut]
        [Route("check-in")]
        [ProducesResponseType(typeof(Ticket), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(void), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<Ticket>> CheckIn(CheckInRequest checkInRequest)
        {
            Ticket ticket = await this.ticketService.CheckIn(checkInRequest).ConfigureAwait(true);
            switch (ticket.Status)
            {
                case TicketStatus.TooEarly:
                    return new JsonResult(new ErrorResult()
                    {
                        Code = string.Empty,
                        Message = $"The check-in request is too early",
                    })
                    {
                        StatusCode = StatusCodes.Status412PreconditionFailed,
                    };
                case TicketStatus.NotFound:
                    return new JsonResult(new ErrorResult()
                    {
                        Code = string.Empty,
                        Message = $"The supplied ticket id or nonce was invalid.",
                    })
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                    };
                case TicketStatus.Processed:
                case TicketStatus.Queued:
                    return ticket;
                default:
                    return new BadRequestResult();
            }
        }

        /// <summary>
        /// Releases the ticket and associated resources from the server.
        /// A good client will call this as they are disconnecting the session.
        /// </summary>
        /// <returns>TBD>The ticket that was removed.</returns>
        [HttpDelete]
        public ActionResult<Ticket> Release()
        {
            // TODO: Implement remove.
            return new BadRequestResult();
        }
    }
}
