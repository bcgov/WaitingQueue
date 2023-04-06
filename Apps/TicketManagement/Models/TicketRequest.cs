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
namespace BCGov.WaitingQueue.TicketManagement.Models
{
    using System;

    /// <summary>
    /// Simple object to wrap properties required for a Ticket request.
    /// </summary>
    public class TicketRequest
    {
        /// <summary>
        /// Gets or sets the Id used to retrieve the Token Response.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the room.
        /// </summary>
        public string Room { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Nonce that is used to validate the request.
        /// </summary>
        public string Nonce { get; set; } = null!;
    }
}
