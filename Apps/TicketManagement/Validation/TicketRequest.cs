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
namespace BCGov.WaitingQueue.TicketManagement.Validation
{
    using System.Net;
    using BCGov.WaitingQueue.TicketManagement.ErrorHandling;
    using BCGov.WaitingQueue.TicketManagement.Models;

    /// <summary>
    /// Rules to check for ticket request.
    /// </summary>
    public static class TicketRequest
    {
        /// <summary>
        /// Validate room configuration.
        /// </summary>
        /// <param name="roomConfig">The room configuration to validate.</param>
        public static void ValidateRoomConfig(RoomConfiguration? roomConfig)
        {
            if (roomConfig is null)
            {
                // Not found
                ExceptionUtility.ThrowException(
                    "The requested room: {room} was not found.",
                    HttpStatusCode.NotFound,
                    nameof(TicketRequest));
            }
        }

        /// <summary>
        /// Validate waiting room count.
        /// </summary>
        /// <param name="waitingCount">The waiting room count to validate.</param>
        /// <param name="queueMaxSize">The queue maximum size to validate against.</param>
        public static void ValidateWaitingCount(long waitingCount, int queueMaxSize)
        {
            if (waitingCount >= queueMaxSize)
            {
                // Too busy
                ExceptionUtility.ThrowException(
                    "The waiting queue has exceeded maximum capacity, try again later",
                    HttpStatusCode.ServiceUnavailable,
                    nameof(TicketRequest));
            }
        }
    }
}
