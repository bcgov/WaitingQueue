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
    using BCGov.WaitingQueue.TicketManagement.Constants;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Services;
    using BCGov.WebCommon.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <inheritdoc />
    public class WebTicketDelegate : IWebTicketDelegate
    {
        private readonly ITicketService ticketService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTicketDelegate"/> class.
        /// </summary>
        /// <param name="ticketService">The injected ticket service.</param>
        public WebTicketDelegate(ITicketService ticketService)
        {
            this.ticketService = ticketService;
        }

        /// <inheritdoc />
        public async Task<IActionResult> CreateTicket(string room)
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
                    return new JsonResult(ticket);
                default:
                    return new BadRequestResult();
            }
        }

        /// <inheritdoc />
        public async Task<IActionResult> CheckIn(CheckInRequest checkInRequest)
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
                    return new JsonResult(ticket);
                default:
                    return new BadRequestResult();
            }
        }

        /// <inheritdoc />
        public async Task<IActionResult> RemoveTicket(CheckInRequest checkInRequest)
        {
            return new OkResult();
        }
    }
}
