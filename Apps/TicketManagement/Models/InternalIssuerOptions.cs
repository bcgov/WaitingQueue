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
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Provides the configuration for the Internal issuer.
    /// </summary>
    public record InternalIssuerOptions
    {
        /// <summary>
        /// Gets the room based configuration of token requests for the issuer.
        /// </summary>
        public required Dictionary<string, InternalIssuerRoomConfig> RoomConfiguration { get; init; } = new();
    }

    /// <summary>
    /// Provides room configuration for the Internal issuer.
    /// </summary>
    public record InternalIssuerRoomConfig
    {
        /// <summary>
        /// Gets or sets the time to live for the token.
        /// </summary>
        public int TokenTtl { get; set; }

        /// <summary>
        /// Gets the Issuer for the token.
        /// </summary>
        public required string Issuer { get; init; }

        /// <summary>
        /// Gets the certificates to use for signing the tokens.
        /// </summary>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is a configuration object")]
        public InternalIssuerCertificate[] Certificates { get; init; } = Array.Empty<InternalIssuerCertificate>();
    }

    /// <summary>
    /// Simple object to hold the certificate information.
    /// </summary>
    public record InternalIssuerCertificate
    {
        /// <summary>
        /// Gets the path and filename for the certificate.
        /// </summary>
        public required string CertificatePath { get; init; }

        /// <summary>
        /// Gets the password for the certificate.
        /// </summary>
        public string? CertificatePassword { get; init; }
    }

    /// <summary>
    /// Wraps the signing information for the Internal issuer.
    /// </summary>
    public record InternalIssuerSigningInfo
    {
        /// <summary>
        /// Gets the security key for the certificate.
        /// </summary>
        public required X509SecurityKey SecurityKey { get; init; }

        /// <summary>
        /// Gets the header for the token.
        /// </summary>
        public required JwtHeader TokenHeader { get; init; }
    }
}
