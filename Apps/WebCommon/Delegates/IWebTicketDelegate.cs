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
namespace BCGov.WebCommon.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Services;

    /// <summary>
    /// Wraps Ticket Management responses into reusable web responses.
    /// </summary>
    public interface IWebTicketDelegate
    {
        /// <summary>
        /// Gets the ticket for the given Ticket request.
        /// </summary>
        /// <returns>The updated Ticket.</returns>
        /// <param name="room">The room to query.</param>
        /// <param name="ticketId">The ticket id to query.</param>
        /// <param name="nonce">The nonce for the given ticket.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="400">The request was invalid.</response>
        /// <response code="404">The requested ticket was not found.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        Task<Ticket> GetTicket(string room, Guid ticketId, string nonce);

        /// <summary>
        /// Request a ticket which either creates a ticket or puts the user in a waiting room.
        /// </summary>
        /// <returns>A ticket response when successful.</returns>
        /// <param name="room">The room for which the client is requesting a ticket.</param>
        /// <response code="200">Ticket returned.</response>
        /// <response code="400">The request was invalid.</response>
        /// <response code="404">The requested room was not found.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        /// <response code="503">The service is too busy, retry after the amount of time specified in retry-after.</response>
        Task<Ticket> CreateTicket(string room);

        /// <summary>
        /// Performs a check-in on the Ticket which will update the state and/or ticket associated.
        /// </summary>
        /// <returns>The updated Ticket.</returns>
        /// <param name="ticketRequest">The ticket request to check-in.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="400">The request was invalid.</response>
        /// <response code="404">The requested ticket was not found.</response>
        /// <response code="412">The service is unable to complete the request, review the error.</response>
        /// <response code="429">The user has made too many requests in the given timeframe.</response>
        Task<Ticket> CheckInAsync(TicketRequest ticketRequest);

        /// <summary>
        /// Removes a ticket from the system.
        /// </summary>
        /// <returns>Ok or NotFound Result.</returns>
        /// <param name="ticketRequest">The ticket request to check-in.</param>
        /// <response code="200">The ticket returned.</response>
        /// <response code="404">The requested ticket was not found.</response>
        Task RemoveTicketAsync(TicketRequest ticketRequest);

        /// <summary>
        /// Gets the OIDC configuration for the given room.
        /// </summary>
        /// <param name="room">The room to lookup.</param>
        /// <returns>The OIDC configuration.</returns>
        OidcConfiguration? GetOidcConfiguration(string room);

        /// <summary>
        /// Returns a the JSON Web Keys for the given room.
        /// </summary>
        /// <param name="room">The room to lookup.</param>
        /// <returns>The list of JsonWebTokens.</returns>
        JsonWebKeys GetJsonWebKeys(string room);
    }

    /// <summary>
    /// Represents the list of JSON Web Keys.
    /// </summary>
    public record JsonWebKeys
    {
        /// <summary>
        /// Gets the list of keys.
        /// </summary>
        public IEnumerable<JsonWebKey> Keys { get; init; } = Array.Empty<JsonWebKey>();
    }

    /// <summary>
    /// Represents a Json Web Key.
    /// </summary>
    public record JsonWebKey
    {
        /// <summary>
        /// Gets the 'kid' (Key ID).
        /// </summary>
        public string Kid { get; init; } = string.Empty;

        /// <summary>
        /// Gets the 'kty' (Key Type).
        /// </summary>
        public string Kty { get; init; } = string.Empty;

        /// <summary>
        /// Gets the 'alg' (KeyType)..
        /// </summary>
        public string Alg { get; init; } = string.Empty;

        /// <summary>
        /// Gets the 'use' (Public Key Use).
        /// </summary>
        public string Use { get; init; } = string.Empty;

        /// <summary>
        /// Gets the 'n' (RSA - Modulus).
        /// </summary>
        public string N { get; init; } = string.Empty;

        /// <summary>
        /// Gets the 'e' (RSA - Exponent).
        /// </summary>
        public string E { get; init; } = string.Empty;

        /// <summary>
        /// Gets the 'x5c' collection (X.509 Certificate Chain).
        /// </summary>
        public IList<string> X5c { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets the 'x5t' (X.509 Certificate SHA-1 thumbprint)..
        /// </summary>
        public string X5t { get; init; } = string.Empty;

        /// <summary>
        /// Gets the 'x5t#S256' (X.509 Certificate SHA-1 thumbprint).
        /// </summary>
        [JsonPropertyName("x5t#S256")]
        public string X5tS256 { get; init; } = string.Empty;
    }
}
