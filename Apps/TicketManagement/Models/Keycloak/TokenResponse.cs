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
namespace BCGov.WaitingQueue.TicketManagement.Models.Keycloak
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The json web token model.
    /// </summary>
    public record TokenResponse
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// Gets or sets the token expiration in seconds.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token expiration in minutes.
        /// </summary>
        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the token type.
        /// </summary>
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        /// <summary>
        /// Gets or sets the not-before-policy.
        /// </summary>
        [JsonPropertyName("not-before-policy")]
        public int? NotBeforePolicy { get; set; }

        /// <summary>
        /// Gets or sets the session state.
        /// </summary>
        [JsonPropertyName("session_state")]
        public string? SessionState { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
