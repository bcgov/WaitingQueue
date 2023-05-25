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
    [Authorize]
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
        /// Returns a list of of all known rooms.
        /// </summary>
        /// <returns>The Health Gateway Configuration.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IEnumerable<string>> Index()
        {
            return await this.roomService.GetRoomsAsync().ConfigureAwait(true);
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
            RoomConfiguration? roomConfig = await this.roomService.ReadConfigurationAsync(room).ConfigureAwait(true);
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
            bool exists = await this.roomService.RoomExists(room).ConfigureAwait(true);
            return exists;
        }

        /// <summary>
        /// Creates the room configuration.
        /// </summary>
        /// <param name="room">The room to create.</param>
        /// <param name="roomConfig">The new room configuration to create.</param>
        /// <returns>The room configuration that was created.</returns>
        [HttpPost]
        [Route("{room}")]
        [ProducesResponseType(typeof(RoomConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateRoom(string room, RoomConfiguration roomConfig)
        {
            roomConfig.Name = room;
            (bool committed, RoomConfiguration config) = await this.roomService.WriteConfigurationAsync(roomConfig, true).ConfigureAwait(true);
            if (committed)
            {
                return new JsonResult(config);
            }

            return new ConflictResult();
        }

        /// <summary>
        /// Updates the room configuration.
        /// </summary>
        /// <param name="room">The room to create.</param>
        /// <param name="roomConfig">The room configuration to update.</param>
        /// <returns>The updated room configuration or.</returns>
        [HttpPut]
        [Route("{room}")]
        [ProducesResponseType(typeof(RoomConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateRoom(string room, RoomConfiguration roomConfig)
        {
            roomConfig.Name = room;
            (bool committed, RoomConfiguration config) = await this.roomService.WriteConfigurationAsync(roomConfig).ConfigureAwait(true);
            if (committed)
            {
                return new JsonResult(config);
            }

            return new ConflictResult();
        }
    }
}
