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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Asserts authorization to access one or more rooms.
    /// </summary>
    public class RoomAccessRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the rooms the user is authorized to access.
        /// </summary>
        /// <param name="context">The supplied HttpContext.</param>
        /// <returns>An Enumerable of rooms the user has access to.</returns>
        public static IEnumerable<string> GetUserRooms(HttpContext? context)
        {
            string prefix = "room::";
            IEnumerable<string>? rooms = context?.User.FindAll(ClaimTypes.Role).Select(c => c.Value)
                .Where(role => role.StartsWith(prefix, true, CultureInfo.InvariantCulture))
                .Select(role => role[prefix.Length..]);
            return rooms ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets the rooms the user is authorized to access.
        /// </summary>
        /// <param name="contextAccessor">The supplied HttpContextAccessor.</param>
        /// <returns>An Enumerable of rooms the user has access to.</returns>
        public static IEnumerable<string> GetUserRooms(IHttpContextAccessor contextAccessor)
        {
            return GetUserRooms(contextAccessor.HttpContext);
        }

        /// <summary>
        /// Returns true if the user can interact with the room.
        /// </summary>
        /// <param name="context">The Http context.</param>
        /// <param name="room">The room to validate.</param>
        /// <returns>A bool indicating if the user can interact with this room.</returns>
        public bool SupportsRoomAccess(HttpContext? context, string room)
        {
            return GetUserRooms(context).Contains(room, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
