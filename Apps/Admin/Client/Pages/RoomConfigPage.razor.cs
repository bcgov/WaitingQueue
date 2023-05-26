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
namespace BCGov.WaitingQueue.Admin.Client.Pages;

using BCGov.WaitingQueue.Admin.Client.Components.RoomConfiguration;
using BCGov.WaitingQueue.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

/// <summary>
/// Backing logic for the Room Config page.
/// </summary>
public partial class RoomConfigPage : ComponentBase
{
    /// <summary>
    /// Gets the configured rooms.
    /// </summary>
    public ReadOnlyCollection<RoomConfiguration> Rooms { get; private set; } = null!;

    private bool IsDialogOpen { get; set; }

    [Inject]
    private IDialogService Dialog { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(true);
        var user = authState.User;

        this.Rooms = new ReadOnlyCollection<RoomConfiguration>(
            new List<RoomConfiguration>()
            {
                new RoomConfiguration
                {
                    Id = "room1",
                    Name = "test room 1",
                },
                new RoomConfiguration
                {
                    Id = "room2",
                    Name = "test room 2",
                },
            });
    }

    private async Task HandleClickNewAsync()
    {
        await OpenRoomConfigurationDialog(new RoomConfiguration());
    }

    private async Task HandleClickEditAsync(RoomConfiguration roomConfiguration)
    {
        await OpenRoomConfigurationDialog(roomConfiguration);
    }

    private async Task OpenRoomConfigurationDialog(RoomConfiguration item)
    {
        if (this.IsDialogOpen)
        {
            return;
        }

        this.IsDialogOpen = true;

        DialogParameters parameters = new() { ["roomConfiguration"] = item };
        DialogOptions options = new() { DisableBackdropClick = true };
        IDialogReference dialog = await this.Dialog.ShowAsync<RoomConfigurationDialog>("Room Configuration", parameters, options);

        var result = await dialog.Result;
        this.IsDialogOpen = false;
        if (!result.Canceled)
        {
            //TODO: save config
        }

        return;
    }
}
