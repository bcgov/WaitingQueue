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
namespace BCGov.WaitingQueue.TicketManagement.Issuers
{
    using System;
    using System.Diagnostics;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Api;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Models.Keycloak;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Generates signed tokens using Keycloak APIs.
    /// </summary>
    public class KeycloakIssuer : ITokenIssuer
    {
        private readonly ILogger<KeycloakIssuer> logger;
        private readonly KeycloakIssuerOptions configuration;
        private readonly IKeycloakApi keycloakApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeycloakIssuer"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="options">Injected IOptions/configuration.</param>
        /// <param name="keycloakApi">The Keycloak API.</param>
        public KeycloakIssuer(ILogger<KeycloakIssuer> logger, IOptions<KeycloakIssuerOptions> options, IKeycloakApi keycloakApi)
        {
            this.logger = logger;
            this.configuration = options.Value;
            this.keycloakApi = keycloakApi;
        }

        /// <inheritdoc />
        public async Task<(string Token, long Expires)> CreateTokenAsync(string room, string ticketId)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            TokenRequest tokenRequest = this.configuration.RoomConfiguration[room];
            TokenResponse tokenResponse = await this.keycloakApi.AuthenticateAsync(tokenRequest);
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken token = handler.ReadJwtToken(tokenResponse.AccessToken);
            DateTimeOffset ticketExpiry = token.ValidTo;
            stopwatch.Stop();
            this.logger.LogDebug("CreateToken Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return (tokenResponse.AccessToken, ticketExpiry.ToUnixTimeSeconds());
        }
    }
}
