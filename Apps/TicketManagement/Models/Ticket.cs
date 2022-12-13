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
    using System.Text.Json.Serialization;
    using BCGov.WaitingQueue.TicketManagement.Constants;

    /// <summary>
    /// Represents the response from a ticket request or check-in.
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Gets or sets a unique guid id for this response.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the room that ticket was requested for.
        /// </summary>
        [JsonPropertyName("room")]
        public string Room { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nonce to be used in future requests.
        /// </summary>
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CreatedTime.
        /// The time is represented as Unix epoch in seconds.
        /// </summary>
        [JsonPropertyName("createdTime")]
        public long CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the time after which the client must check-in.
        /// The time is represented as Unix epoch in seconds.
        /// </summary>
        [JsonPropertyName("checkInAfter")]
        public long CheckInAfter { get; set; }

        /// <summary>
        /// Gets or sets the time after which the issued token will expire.
        /// </summary>
        [JsonPropertyName("tokenExpires")]
        public long TokenExpires { get; set; }

        /// <summary>
        /// Gets or sets the clients queue position (if queued).
        /// </summary>
        [JsonPropertyName("queuePosition")]
        public long QueuePosition { get; set; }

        /// <summary>
        /// Gets or sets the Status of the request.
        /// </summary>
        [JsonPropertyName("status")]
        public TicketStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the token to be used for service calls.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
