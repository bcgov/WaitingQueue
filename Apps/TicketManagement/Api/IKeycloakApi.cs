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
namespace BCGov.WaitingQueue.TicketManagement.Api
{
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models.Keycloak;
    using Refit;

    /// <summary>
    /// Provides access to the Keycloak API for authentication.
    /// </summary>
    public interface IKeycloakApi
    {
        /// <summary>
        /// Authenticates to the token endpoint.
        /// </summary>
        /// <param name="tokenRequest">The parameters to post to Keycloak to obtain the token.</param>
        /// <returns>The token response containing meta-data and the token.</returns>
        [Post("/protocol/openid-connect/token")]
        Task<TokenResponse> Authenticate([Body(BodySerializationMethod.UrlEncoded)] TokenRequest tokenRequest);
    }
}
