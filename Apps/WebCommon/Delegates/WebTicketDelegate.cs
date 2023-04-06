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
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Services;
    using Microsoft.IdentityModel.Tokens;

    /// <inheritdoc />
    public class WebTicketDelegate : IWebTicketDelegate
    {
        private readonly ITicketService ticketService;
        private readonly ISecurityService? securityService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTicketDelegate"/> class.
        /// </summary>
        /// <param name="ticketService">The injected ticket service.</param>
        /// <param name="securityService">The service to fetch the security keys.</param>
        public WebTicketDelegate(ITicketService ticketService, ISecurityService securityService)
        {
            this.ticketService = ticketService;
            this.securityService = securityService;
        }

        /// <inheritdoc />
        public async Task<Ticket> GetTicket(string room, Guid ticketId, string nonce)
        {
            TicketRequest ticketRequest = new()
            {
                Room = room,
                Id = ticketId,
                Nonce = nonce,
            };
            return await this.ticketService.GetTicketAsync(ticketRequest).ConfigureAwait(true);
        }

        /// <inheritdoc />
        public async Task<Ticket> CreateTicket(string room)
        {
            return await this.ticketService.RequestTicketAsync(room).ConfigureAwait(true);
        }

        /// <inheritdoc />
        public async Task<Ticket> CheckInAsync(TicketRequest ticketRequest)
        {
            return await this.ticketService.CheckInAsync(ticketRequest).ConfigureAwait(true);
        }

        /// <inheritdoc />
        public async Task RemoveTicketAsync(TicketRequest ticketRequest)
        {
            await Task.CompletedTask.ConfigureAwait(true);
        }

        /// <inheritdoc />
        public OidcConfiguration? GetOidcConfiguration(string room)
        {
            if (this.securityService is null)
            {
                return null;
            }

            return this.securityService.GetOidcConfiguration(room);
        }

        /// <inheritdoc />
        public JsonWebKeys GetJsonWebKeys(string room)
        {
            if (this.securityService is null)
            {
                return new JsonWebKeys();
            }

            X509SecurityKey[] securityKeys = this.securityService.GetSecurityKeys(room);
            return new JsonWebKeys()
            {
                Keys = securityKeys.Select(
                        key =>
                        {
                            RSAParameters rsaParameters = key.Certificate.GetRSAPublicKey()!.ExportParameters(false);
                            return new JsonWebKey
                            {
                                Kid = key.KeyId,
                                Kty = "RSA",
                                Alg = "RS256",
                                Use = "sig",
                                N = Base64UrlEncoder.Encode(rsaParameters.Modulus),
                                E = Base64UrlEncoder.Encode(rsaParameters.Exponent),
                                X5c = new List<string> { Convert.ToBase64String(key.Certificate.RawData) },
                                X5t = Base64UrlEncoder.Encode(key.X5t),
                                X5tS256 = ComputeSha256Thumbprint(key.Certificate),
                            };
                        })
                    .ToList(),
            };
        }

        private static string ComputeSha256Thumbprint(X509Certificate2 certificate)
        {
            byte[] hash = SHA256.HashData(certificate.RawData);
            return Base64UrlEncoder.Encode(hash);
        }
    }
}
