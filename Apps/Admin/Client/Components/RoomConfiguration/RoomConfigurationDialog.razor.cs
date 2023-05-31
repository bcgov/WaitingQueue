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

namespace BCGov.WaitingQueue.Admin.Client.Components.RoomConfiguration;

using System.Threading.Tasks;
using BCGov.WaitingQueue.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Show dialog to add/edit room configuration.
/// </summary>
public partial class RoomConfigurationDialog
{
    /// <summary>
    /// Gets or sets the room configuration instance to edit.
    /// </summary>
    [Parameter]
    public RoomConfiguration RoomConfiguration { get; set; } = new();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private MudForm Form { get; set; } = default!;

    private async Task HandleClickCancelAsync()
    {
        await Task.CompletedTask;
        this.MudDialog.Cancel();
    }

    private async Task HandleClickSaveAsync()
    {
        await Task.CompletedTask;
        this.MudDialog.Close(DialogResult.Ok(true));
    }
}
