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
    /// Configuration for a Room.
    /// </summary>
    public class RoomConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the Room.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the signing private key.
        /// </summary>
        public string PrivateKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the issuer of the ticket.
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the frequency in seconds that the user must checkin.
        /// </summary>
        public int CheckInFrequency { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to keep the data after the checkin period has been met.
        /// </summary>
        public int CheckInGrace { get; set; }

        /// <summary>
        /// Gets or sets the amount of time in seconds that an issued ticket is valid for.
        /// </summary>
        public int TicketTtl { get; set; }

        /// <summary>
        /// Gets or sets the amount of time in seconds that the room will live if idle.
        /// </summary>
        public int RoomIdleTtl { get; set; }

        /// <summary>
        /// Gets or sets the maximum participants to be admitted to the room.
        /// </summary>
        public int ParticipantLimit { get; set; }

        /// <summary>
        /// Gets or sets the threshold after which the queue activates.
        /// </summary>
        public int QueueThreshold { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed users in the queue.
        /// </summary>
        public int QueueMaxSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum expired entries to remove at a time.
        /// </summary>
        public int RemoveExpiredMax { get; set; }
    }
}
