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
namespace WaitingRoom
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WebCommon.Delegates;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;

    /// <summary>
    /// Azure function to process create ticket requests.
    /// </summary>
    public class TicketFunction
    {
        private readonly ILogger<TicketFunction> logger;
        private readonly IWebTicketDelegate ticketDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketFunction"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="ticketDelegate">The injected web ticket delegate.</param>
        public TicketFunction(ILogger<TicketFunction> logger, IWebTicketDelegate ticketDelegate)
        {
            this.logger = logger;
            this.ticketDelegate = ticketDelegate;
        }

        /// <summary>
        /// Request a ticket which either creates a ticket or puts the user in a waiting room.
        /// </summary>
        /// <returns>A ticket response when successful.</returns>
        /// <param name="request">The HTTP Request object.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="400">The request was invalid.</response>
        /// <response code="404">The requested room was not found.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        /// <response code="503">The service is too busy, retry after the amount of time specified in retry-after.</response>
        [FunctionName("CreateTicket")]
        [OpenApiOperation(operationId: "RequestTicket", tags: new[] { "Ticket" })]
        [OpenApiParameter(name: "room", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The room for which the client is requesting a ticket.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(Ticket), Description = "The ticket returned")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The requested room was not found.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.ServiceUnavailable, Description = "The waiting queue has exceeded maximum capacity")]
        public async Task<IActionResult> RequestTicket(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Post), Route = "Ticket")] HttpRequest request)
        {
            this.logger.LogDebug("Starting Create Ticket function");
            string room = request.Query["room"].ToString();

            return new OkObjectResult(await this.ticketDelegate.CreateTicket(room).ConfigureAwait(true));
        }

        /// <summary>
        /// Performs a check-in on the Ticket which will update the state and/or ticket associated.
        /// </summary>
        /// <returns>The updated Ticket.</returns>
        /// <param name="request">The http request message.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="400">The requested was invalid.</response>
        /// <response code="404">The requested ticket was not found.</response>
        /// <response code="412">The service is unable to complete the request, review the error.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        [FunctionName("CheckIn")]
        [OpenApiOperation(operationId: "CheckIn", tags: new[] { "Ticket" })]
        [OpenApiRequestBody(bodyType: typeof(CheckInRequest), contentType: MediaTypeNames.Application.Json, Required = true, Description = "The ticket request to check-in.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(Ticket), Description = "The ticket returned")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The requested ticket was not found")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.PreconditionFailed, Description = "The user has made too many requests in the given timeframe.")]
        public async Task<IActionResult> CheckIn(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Put), Route = "Ticket/check-in")] HttpRequestMessage request)
        {
            this.logger.LogInformation("Starting check-in function");
            CheckInRequest checkInRequest = await request.Content.ReadAsAsync<CheckInRequest>().ConfigureAwait(true);
            return new OkObjectResult(await this.ticketDelegate.CheckIn(checkInRequest).ConfigureAwait(true));
        }

        /// <summary>
        /// Releases the ticket and associated resources from the server.
        /// A good client will call this as they are disconnecting the session.
        /// </summary>
        /// <returns>The ticket that was removed.</returns>
        /// <param name="request">The http request message.</param>
        /// <response code="200">The ticket was removed.</response>
        /// <response code="404">The requested was not found.</response>
        [FunctionName("RemoveTicket")]
        [OpenApiOperation(operationId: "RemoveTicket", tags: new[] { "Ticket" })]
        [OpenApiRequestBody(bodyType: typeof(CheckInRequest), contentType: MediaTypeNames.Application.Json, Required = true, Description = "The ticket request to check-in.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The ticket was removed.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The ticket was not found.")]
        public async Task<IActionResult> RemoveTicket(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Delete), Route = "Ticket")] HttpRequestMessage request)
        {
            this.logger.LogInformation("Starting remove ticket function");
            CheckInRequest checkInRequest = await request.Content.ReadAsAsync<CheckInRequest>().ConfigureAwait(true);
            return await this.ticketDelegate.RemoveTicket(checkInRequest).ConfigureAwait(true);
        }
    }
}
