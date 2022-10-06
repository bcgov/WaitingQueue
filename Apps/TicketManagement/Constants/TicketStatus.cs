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
namespace BCGov.WaitingQueue.TicketManagement.Constants
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the status of a ticket request.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TicketStatus
    {
        /// <summary>
        /// The Token request was reviewed and queued.
        /// </summary>
        Queued,

        /// <summary>
        /// Get Token request was reviewed and processed.
        /// </summary>
        Processed,

        /// <summary>
        /// The Token request cannot be processed as the room was not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// The service is too busy, retry.
        /// </summary>
        TooBusy,

        /// <summary>
        /// The request for a check-in was performed too early, try at the specified time.
        /// </summary>
        TooEarly,
    }
}
