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
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a custom api patient exception.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class WaitingQueueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaitingQueueException"/> class.
        /// </summary>
        /// <param name="detail">The detail associated with the exception.</param>
        /// <param name="statusCode">The HTTP status code associated with the exception.</param>
        /// <param name="typeName">The name of the type where the exception was generated.</param>
        /// <param name="memberName">The type of the method or property where the exception was generated.</param>
        public WaitingQueueException(string detail, HttpStatusCode statusCode, string typeName, [CallerMemberName] string memberName = "")
        {
            this.ProblemType = "waiting-queue-exception";
            this.Detail = detail;
            this.Title = "Custom Exception Handling";
            this.AdditionalInfo = "Please try again at a later time.";
            this.Instance = $"{typeName}.{memberName}";
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitingQueueException"/> class.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="innerException">The inner exception associated with the exception.</param>
        public WaitingQueueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitingQueueException"/> class.
        /// </summary>
        /// <param name="message">The message associated with exception.</param>
        public WaitingQueueException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitingQueueException"/> class.
        /// </summary>
        public WaitingQueueException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitingQueueException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info associated with the exception.</param>
        /// <param name="streamingContext">The streaming context associated with the exception.</param>
        protected WaitingQueueException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(
                serializationInfo,
                streamingContext)
        {
        }

        /// <summary>
        /// Gets or sets additional info.
        /// </summary>
        public string AdditionalInfo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets problem type.
        /// </summary>
        public string ProblemType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets detail.
        /// </summary>
        public string Detail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets instance.
        /// </summary>
        public string Instance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
    }
}
