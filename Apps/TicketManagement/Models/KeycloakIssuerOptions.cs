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
namespace BCGov.WaitingQueue.TicketManagement.Models
{
    using System;
    using System.Collections.Generic;
    using BCGov.WaitingQueue.TicketManagement.Models.Keycloak;

    /// <summary>
    /// Provides the configuration for the Keycloak issuer.
    /// </summary>
    public record KeycloakIssuerOptions
    {
        /// <summary>
        /// Gets or sets the base URI to use to connect to the Keycloak server.
        /// </summary>
        public required Uri BaseUri { get; set; }

        /// <summary>
        /// Gets the room based configuration of token requests for the issuer.
        /// </summary>
        public required Dictionary<string, TokenRequest> RoomConfiguration { get; init; }
    }
}
