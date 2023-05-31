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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using BCGov.WaitingQueue.Admin.Client.Api;
using BCGov.WaitingQueue.Admin.Client.Components.RoomConfiguration;
using BCGov.WaitingQueue.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Refit;

/// <summary>
/// Backing logic for the Room Config page.
/// </summary>
public partial class RoomConfigPage : ComponentBase
{
    /// <summary>
    /// Gets the configured rooms.
    /// </summary>
    public IDictionary<string, RoomConfiguration> Rooms { get; private set; } = new Dictionary<string, RoomConfiguration>();

    private bool IsDialogOpen { get; set; }

    [Inject]
    private IDialogService Dialog { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IRoomApi RoomApi { get; set; } = default!;

    private string? ErrorMessage { get; set; }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            this.Rooms = await this.RoomApi.GetRoomsAsync();
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            this.ErrorMessage = "Unable to load room configurations, please try refreshing the page or contact support";
        }
    }

    private async Task HandleClickNewAsync()
    {
        await this.OpenRoomConfigurationDialog(new RoomConfiguration());
    }

    private async Task HandleClickEditAsync(RoomConfiguration roomConfiguration)
    {
        await this.OpenRoomConfigurationDialog(roomConfiguration);
    }

    private async Task HandleCloseError()
    {
        this.ErrorMessage = null;
        await Task.CompletedTask;
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

        DialogResult result = await dialog.Result;
        this.IsDialogOpen = false;
        if (!result.Canceled)
        {
            this.ErrorMessage = null;
            try
            {
                this.Rooms[item.Name] = await this.RoomApi.UpsertConfiguration(item);
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.ErrorMessage = "Unable to save room configuration, please try refreshing the page or contact support";
            }

            this.StateHasChanged();
        }

        return;
    }
}
