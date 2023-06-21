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
namespace BCGov.WaitingQueue.Admin.Server.Authorization
{
    /// <summary>
    /// The roles available to the application.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// A user allowed to administrate the rooms.
        /// </summary>
        public const string Admin = "AdminUser";

        /// <summary>
        /// A user allowed to view the statistics page.
        /// </summary>
        public const string Stats = "StatsUser";
    }
}
