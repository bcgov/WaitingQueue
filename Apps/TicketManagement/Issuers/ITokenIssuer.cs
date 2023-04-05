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
    using System.Threading.Tasks;

    /// <summary>
    /// Generic mechanism to generate signed tokens.
    /// </summary>
    public interface ITokenIssuer
    {
        /// <summary>
        /// The default issuer to use.
        /// </summary>
        public const string DefaultIssuer = "KeycloakIssuer";

        /// <summary>
        /// Generates a signed token for the given room.
        /// </summary>
        /// <param name="room">The room for token configuration.</param>
        /// <returns>The encoded token and when it expires.</returns>
        Task<(string Token, long Expires)> CreateTokenAsync(string room);
    }
}
