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
namespace BCGov.WaitingQueue.TicketManagement.Models.Keycloak
{
    using Refit;

    /// <summary>
    /// Properties used for OIDC Authentication flows.
    /// </summary>
    public record TokenRequest
    {
        /// <summary>
        /// Gets or sets the client id to use for authentication.
        /// </summary>
        [AliasAs("client_id")]
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the type type of grant type being used.
        /// </summary>
        [AliasAs("grant_type")]
        public string? GrantType { get; set; }

        /// <summary>
        /// Gets or sets the optional client secret.
        /// </summary>
        [AliasAs("client_secret")]
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the optional audience to request.
        /// </summary>
        [AliasAs("audience")]
        public string? Audience { get; set; }

        /// <summary>
        /// Gets or sets the optional comma seperated list of scopes.
        /// </summary>
        [AliasAs("scope")]
        public string? Scope { get; set; }

        /// <summary>
        /// Gets or sets the username if required for the grant type.
        /// </summary>
        [AliasAs("username")]
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the password if required by the grant type.
        /// </summary>
        [AliasAs("password")]
        public string? Password { get; set; }
    }
}
