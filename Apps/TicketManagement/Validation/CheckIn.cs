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
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using BCGov.WaitingQueue.TicketManagement.ErrorHandling;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using StackExchange.Redis;

    /// <summary>
    /// Rules to check for ticket checkin.
    /// </summary>
    public static class CheckIn
    {
        /// <summary>
        /// Validate redis ticket rules.
        /// </summary>
        /// <param name="redisTicket">The redis ticket to validate.</param>
        public static void ValidateRedisTicket(RedisValue redisTicket)
        {
            if (!redisTicket.HasValue)
            {
                // Not found
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(
                    "The supplied ticket id was invalid.",
                    HttpStatusCode.NotFound,
                    nameof(CheckIn)));
            }
        }

        /// <summary>
        /// Validate ticket rules.
        /// </summary>
        /// <param name="ticket">The ticket to validate.</param>
        /// <param name="nonce">The nonce to validate against.</param>
        /// <param name="utcUnixTime">The utc unix time to validate against.</param>
        public static void ValidateTicket([NotNull]Ticket? ticket, string nonce, long? utcUnixTime = null)
        {
            if (ticket is null)
            {
                // Internal Server Error
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(
                    "Unable to deserialize ticket.",
                    HttpStatusCode.InternalServerError,
                    nameof(CheckIn)));
            }

            if (ticket.Nonce != nonce)
            {
                // Not found
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(
                    "The supplied ticket nonce was invalid.",
                    HttpStatusCode.NotFound,
                    nameof(CheckIn)));
            }

            if (utcUnixTime != null && ticket.CheckInAfter > utcUnixTime)
            {
                // Too early
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(
                    "The check-in request was too early",
                    HttpStatusCode.PreconditionFailed,
                    nameof(CheckIn)));
            }
        }
    }
}
