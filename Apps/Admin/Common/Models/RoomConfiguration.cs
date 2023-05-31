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

namespace BCGov.WaitingQueue.Admin.Common.Models;
/// <summary>
/// Represents a room configuration values.
/// </summary>
public record RoomConfiguration
{
    /// <summary>
    /// Gets or sets the room's name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the check in frequency in seconds.
    /// </summary>
    public int CheckInFrequency { get; set; } = 180;

    /// <summary>
    /// Gets or sets the check in grace period in seconds.
    /// </summary>
    public int CheckInGrace { get; set; } = 10;

    /// <summary>
    /// Gets or sets the room idle time to live.
    /// </summary>
    public int RoomIdleTtl { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum number of participants.
    /// </summary>
    public int ParticipantLimit { get; set; } = 800;

    /// <summary>
    /// Gets or sets the queue number of participant threshold.
    /// </summary>
    public int QueueThreshold { get; set; } = 600;

    /// <summary>
    /// Gets or sets the queue maximum size.
    /// </summary>
    public int QueueMaxSize { get; set; } = 500;

    /// <summary>
    /// Gets or sets the maximum of remove expired maximum?.
    /// </summary>
    public int RemoveExpiredMax { get; set; } = 250;

    /// <summary>
    /// Gets or sets the last updated timestamp in UTC Epoch format.
    /// </summary>
    public long LastUpdated { get; set; }
}
