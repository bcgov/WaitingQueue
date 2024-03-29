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
namespace BCGov.WaitingQueue.Admin.Server.Authorization
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Assets the user has access to the room.
    /// </summary>
    public class RoomAccessHandler : IAuthorizationHandler
    {
        private const string RouteResourceIdentifier = "room";
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomAccessHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The injected HttpContext accessor.</param>
        public RoomAccessHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// .
        /// </summary>
        /// <param name="context">The authorization information.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (RoomAccessRequirement requirement in context.PendingRequirements.OfType<RoomAccessRequirement>())
            {
                string? room = this.httpContextAccessor.HttpContext?.Request.RouteValues[RouteResourceIdentifier] as string;
                if (room != null && requirement.SupportsRoomAccess(this.httpContextAccessor.HttpContext, room))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
