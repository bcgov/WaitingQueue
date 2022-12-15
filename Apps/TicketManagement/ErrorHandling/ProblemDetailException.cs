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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a custom problem detail exception.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class ProblemDetailException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailException"/> class.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="innerException">The inner exception associated with the exception.</param>
        public ProblemDetailException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailException"/> class.
        /// </summary>
        /// <param name="message">The message associated with exception.</param>
        public ProblemDetailException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailException"/> class.
        /// </summary>
        public ProblemDetailException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info associated with the exception.</param>
        /// <param name="streamingContext">The streaming context associated with the exception.</param>
        protected ProblemDetailException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(
                serializationInfo,
                streamingContext)
        {
        }

        /// <summary>
        /// Gets or sets additional info.
        /// </summary>
        required public string? AdditionalInfo { get; set; }

        /// <summary>
        /// Gets or sets problem type.
        /// </summary>
        required public string ProblemType { get; set; }

        /// <summary>
        /// Gets or sets detail.
        /// </summary>
        required public string Detail { get; set; }

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        required public string Title { get; set; }

        /// <summary>
        /// Gets or sets instance.
        /// </summary>
        required public string Instance { get; set; }

        /// <summary>
        /// Gets or sets status code.
        /// </summary>
        required public HttpStatusCode StatusCode { get; set; }
    }
}
