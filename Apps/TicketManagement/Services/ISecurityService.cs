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
namespace BCGov.WaitingQueue.TicketManagement.Services
{
    using System;
    using System.Text.Json.Serialization;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Provides a mechanism to fetch the security keys for a given room.
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// Gets the X509SecurityKeys for the given room.
        /// </summary>
        /// <param name="room">The room to lookup.</param>
        /// <returns>The list of X509SecurityKeys current and expired.</returns>
        X509SecurityKey[] GetSecurityKeys(string room);

        /// <summary>
        /// Gets the OIDC Configuration for the supplied room.
        /// </summary>
        /// <param name="room">The room to lookup.</param>
        /// <returns>The OidcConfiguration.</returns>
        /// <exception cref="BCGov.WaitingQueue.TicketManagement.ErrorHandling.ConfigurationException">Thrown when the supplied room doesn't resolve.</exception>
        OidcConfiguration? GetOidcConfiguration(string room);
    }

    /// <summary>
    /// Provides the configuration for token validation.
    /// </summary>
    public record OidcConfiguration
    {
        /// <summary>
        /// Gets the JWKS Endpoint for validation.
        /// </summary>
        [JsonPropertyName("jwks_uri")]
        public Uri? JwksUri { get; init; }
    }
}
