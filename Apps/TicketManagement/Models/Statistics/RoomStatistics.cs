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

namespace BCGov.WaitingQueue.TicketManagement.Models.Statistics
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a room's statistics.
    /// </summary>
    /// <param name="Counters">The statistical counters.</param>
    public record RoomStatistics(IEnumerable<Counter> Counters);

    /// <summary>
    /// Represents a single statistics counter.
    /// </summary>
    /// <param name="Name">The counter's name.</param>
    /// <param name="Value">The counter's value.</param>
    public record Counter(string Name, long Value);
}
