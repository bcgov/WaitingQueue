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
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Admin.Client.Store.Configuration;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.JSInterop;

    /// <summary>
    /// Backing logic for the User Info page.
    /// </summary>
    public partial class UserInfoPage : FluxorComponent
    {
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Inject]
        private IAccessTokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        [Inject]
        private IState<ConfigurationState> ConfigurationState { get; set; } = default!;

        private string? AuthMessage { get; set; } = string.Empty;

        private string? Token { get; set; }

        private IEnumerable<Claim> Claims { get; set; } = Enumerable.Empty<Claim>();

        /// <inheritdoc/>
        protected override async Task OnInitializedAsync()
        {
            await this.GetClaimsPrincipalData().ConfigureAwait(true);
            await this.CreateCookie("HGAdmin", "dark mode", 365).ConfigureAwait(true);
        }

        private async Task CreateCookie(string name, string value, int days)
        {
            string expires;
            if (days > 0)
            {
                DateTime date = DateTime.Now.AddDays(days);
                expires = "; expires=" + date.ToString("r");
            }
            else
            {
                expires = string.Empty;
            }

            string cookieValue = $"{name}={value}{expires}; path=/";
            await this.JsRuntime.InvokeVoidAsync("eval", $@"document.cookie = ""{cookieValue}""").ConfigureAwait(true);
        }

        private async Task GetClaimsPrincipalData()
        {
            AuthenticationState authState = await this.AuthenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(true);
            ClaimsPrincipal user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                AccessTokenResult? tokenResult = await this.TokenProvider.RequestAccessToken().ConfigureAwait(true);
                tokenResult.TryGetToken(out AccessToken? accessToken);
                this.Token = accessToken.Value;

                this.AuthMessage = $"{user.Identity.Name} is authenticated.";
                this.Claims = user.Claims;
            }
            else
            {
                this.AuthMessage = "The user is NOT authenticated.";
            }
        }
    }
}
