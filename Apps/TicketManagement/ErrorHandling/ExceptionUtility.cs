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
namespace BCGov.WaitingQueue.TicketManagement.ErrorHandling
{
    using System.Net;
    using System.Runtime.CompilerServices;
    using BCGov.WaitingQueue.TicketManagement.Services;

    /// <summary>
    /// Utilities for throwing exceptions.
    /// </summary>
    public static class ExceptionUtility
    {
        /// <summary>
        /// Instantiates and throws a cref="ProblemDetailException".
        /// </summary>
        /// <param name="detail">The detail of the exception.</param>
        /// <param name="statusCode">The http status code of the exception.</param>
        /// <param name="additionalInfo">Additional information of the exception.</param>
        /// <param name="memberName">The member name where the exception occurred.</param>
        /// <exception cref="ProblemDetailException">Exception to be thrown.</exception>
        public static void ThrowException(string detail, HttpStatusCode statusCode, string? additionalInfo = null, [CallerMemberName] string memberName = "")
        {
            throw new ProblemDetailException(detail)
            {
                ProblemType = "Waiting Queue Exception",
                Title = "Error during processing",
                Detail = detail,
                StatusCode = statusCode,
                Instance = $"{nameof(RedisTicketService)}.{memberName}",
                AdditionalInfo = additionalInfo,
            };
        }
    }
}
