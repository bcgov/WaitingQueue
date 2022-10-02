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
namespace BCGov.WaitingQueue.Common.Delegates
{
    using System;

    /// <summary>
    /// Provides an abstraction layer to the system clock.
    /// </summary>
    public interface IDateTimeDelegate
    {
        /// <summary>
        /// Gets the current date/time in UTC as a DateTimeOffset.
        /// </summary>
        DateTimeOffset UtcNow { get; }

        /// <summary>
        /// Gets the current date/time in URC as a DateTime.
        /// </summary>
        DateTime UtcNowDateTime { get; }

        /// <summary>
        /// Gets the current date/time in UTC represented as seconds from the Unix Epoch.
        /// </summary>
        long UtcUnixTime { get; }

        /// <summary>
        /// Gets the current date/time in UTC represented as milliseconds from the Unix Epoch.
        /// </summary>
        long UtcUnixTimeMilliseconds { get; }
    }
}
