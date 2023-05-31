//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace BCGov.WaitingQueue.Admin.Server.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to maintain the Waiting Queue Room configuration.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "AdminUser")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RoomController : Controller
    {
        private readonly IRoomService roomService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomController"/> class.
        /// </summary>
        /// <param name="roomService">The injected room service to use.</param>
        public RoomController(IRoomService roomService)
        {
            this.roomService = roomService;
        }

        /// <summary>
        /// Returns a dictionary of rooms along with their associated configuration.
        /// </summary>
        /// <returns>The key/value pairs of room configurations.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RoomConfiguration>), StatusCodes.Status200OK)]
        public async Task<IDictionary<string, RoomConfiguration>> Index()
        {
            return await this.roomService.GetRoomsAsync();
        }

        /// <summary>
        /// Returns the Configuration for the supplied Room name.
        /// </summary>
        /// <param name="room">The room to lookup the configuration for.</param>
        /// <returns>The Health Gateway Configuration.</returns>
        [HttpGet]
        [Route("{room}")]
        [ProducesResponseType(typeof(RoomConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfig(string room)
        {
            RoomConfiguration? roomConfig = await this.roomService.ReadConfigurationAsync(room);
            if (roomConfig is not null)
            {
                return new JsonResult(roomConfig);
            }

            return new NotFoundResult();
        }

        /// <summary>
        /// Returns a boolean indicating the existence of the room at the point in time.
        /// </summary>
        /// <param name="room">The room to query existence.</param>
        /// <returns>A boolean indicating the existence.</returns>
        [HttpGet]
        [Route("{room}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<bool> Exists(string room)
        {
            bool exists = await this.roomService.RoomExists(room);
            return exists;
        }

        /// <summary>
        /// Creates or updates the room configuration.
        /// </summary>
        /// <param name="roomConfig">The new room configuration.</param>
        /// <returns>The newly updated/created room configuration.</returns>
        [HttpPut]
        [ProducesResponseType(typeof(RoomConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpsertRoom(RoomConfiguration roomConfig)
        {
            (bool committed, RoomConfiguration config) = await this.roomService.WriteConfigurationAsync(roomConfig);
            if (committed)
            {
                return new JsonResult(config);
            }

            return new ConflictResult();
        }
    }
}
