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
    using BCGov.WaitingQueue.TicketManagement.Services;
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
        public async Task<Ticket> CreateTicket(string room)
        {
            return await this.ticketService.RequestTicket(room).ConfigureAwait(true);
        }

        /// <inheritdoc />
        public async Task<Ticket> CheckIn(CheckInRequest checkInRequest)
        {
            return await this.ticketService.CheckIn(checkInRequest).ConfigureAwait(true);
        }

        /// <inheritdoc />
        public async Task<IActionResult> RemoveTicket(CheckInRequest checkInRequest)
        {
            return new OkResult();
        }
    }
}
