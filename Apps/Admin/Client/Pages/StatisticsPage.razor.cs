//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace BCGov.WaitingQueue.Admin.Client.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Admin.Client.Api;
    using BCGov.WaitingQueue.Admin.Client.Authorization;
    using BCGov.WaitingQueue.Admin.Common.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Refit;

    /// <summary>
    /// Backing logic for the Asdmin page.
    /// </summary>
    public partial class StatisticsPage : ComponentBase
    {
        /// <summary>
        /// Gets the rooms' statistics.
        /// </summary>
        public IEnumerable<RoomStatistics> Rooms { get; private set; } = Array.Empty<RoomStatistics>();

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IRoomApi RoomApi { get; set; } = default!;

        private string? ErrorMessage { get; set; }

        private async Task HandleCloseError()
        {
            this.ErrorMessage = null;
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            try
            {
                this.Rooms = await this.RoomApi.GetRoomStatistics();
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.ErrorMessage = "Unable to load rooms statistics, please try refreshing the page or contact support";
            }
        }
    }
}
