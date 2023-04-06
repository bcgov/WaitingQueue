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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Common.Delegates;
    using BCGov.WaitingQueue.TicketManagement.ErrorHandling;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Services;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Generates signed tokens using C# APIs.
    /// </summary>
    public class InternalIssuer : ITokenIssuer, ISecurityService
    {
        private readonly ILogger<InternalIssuer> logger;
        private readonly InternalIssuerOptions configuration;
        private readonly IDateTimeDelegate dateTimeDelegate;
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalIssuer"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="options">Injected IOptions/configuration.</param>
        /// <param name="dateTimeDelegate">The datetime delegate.</param>
        /// <param name="memoryCache">The memory cache.</param>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "MemoryCache is disposed by the DI container.")]
        public InternalIssuer(ILogger<InternalIssuer> logger, IOptions<InternalIssuerOptions> options, IDateTimeDelegate dateTimeDelegate, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.configuration = options.Value;
            this.dateTimeDelegate = dateTimeDelegate;
            this.memoryCache = memoryCache;

            foreach (KeyValuePair<string, InternalIssuerRoomConfig> room in this.configuration.RoomConfiguration)
            {
                SortedList<long, InternalIssuerSigningInfo> signingInfos = new();
                this.logger.LogDebug("Loading certificates for {Room}", room.Key);
                foreach (InternalIssuerCertificate cert in room.Value.Certificates)
                {
                    X509Certificate2 certificate = new(cert.CertificatePath, cert.CertificatePassword);
                    X509SecurityKey securityKey = new(certificate);
                    SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.RsaSha256);
                    InternalIssuerSigningInfo signingInfo = new()
                    {
                        SecurityKey = securityKey,
                        TokenHeader = new(signingCredentials),
                    };
                    DateTime expiry = certificate.NotAfter;
                    this.logger.LogDebug("Certificate for {Room} expires on {Expiry}", room.Key, expiry);
                    signingInfos.Add(((DateTimeOffset)expiry).ToUnixTimeSeconds(), signingInfo);

                    this.logger.LogDebug("Loaded certificate with kid {Kid} for {Room}", securityKey.KeyId, room.Key);
                }

                this.logger.LogInformation("Caching {Count} certificates for {Room}", signingInfos.Count, room.Key);
                this.memoryCache.Set($"{room.Key}.SigningInfos", signingInfos);
            }
        }

        /// <inheritdoc />
        public Task<(string Token, long Expires)> CreateTokenAsync(string room, string ticketId)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            InternalIssuerRoomConfig roomConfig = this.configuration.RoomConfiguration[room];
            DateTimeOffset ticketExpiry = this.dateTimeDelegate.UtcNow.AddMinutes(roomConfig.TokenTtl);
            this.logger.LogDebug("Creating token for room {Room} with expiry {Expiry}", room, ticketExpiry);
            JwtPayload payload = new(
                issuer: roomConfig.Issuer,
                audience: room,
                claims: new[]
                {
                    new Claim(
                        JwtRegisteredClaimNames.Iat,
                        this.dateTimeDelegate.UtcUnixTime.ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Jti, ticketId),
                    new Claim(JwtRegisteredClaimNames.Sub, ticketId),
                    new Claim(JwtRegisteredClaimNames.Azp, room),
                },
                notBefore: this.dateTimeDelegate.UtcNowDateTime,
                expires: ticketExpiry.DateTime);

            JwtSecurityToken jwt = new(this.GetJwtHeader(room), payload);
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);
            stopwatch.Stop();

            this.logger.LogDebug("CreateToken Execution Time: {Duration} ms", stopwatch.ElapsedMilliseconds);
            return Task.FromResult((token, ticketExpiry.ToUnixTimeSeconds()));
        }

        /// <inheritdoc />
        public X509SecurityKey[] GetSecurityKeys(string room)
        {
            if (this.configuration.RoomConfiguration.ContainsKey(room))
            {
                this.logger.LogDebug("Fetching Security Keys from cache for {Room}", room);
                SortedList<long, InternalIssuerSigningInfo>? signingInfos = this.memoryCache.Get<SortedList<long, InternalIssuerSigningInfo>>($"{room}.SigningInfos")!;
                return signingInfos.Select(p => p.Value.SecurityKey).ToArray();
            }

            return Array.Empty<X509SecurityKey>();
        }

        /// <inheritdoc />
        public OidcConfiguration? GetOidcConfiguration(string room)
        {
            if (this.configuration.RoomConfiguration.TryGetValue(room, out InternalIssuerRoomConfig? roomConfig))
            {
                Uri jwksUri = new($"{roomConfig.Issuer}/protocol/openid-connect/jwks");
                OidcConfiguration config = new()
                {
                    JwksUri = jwksUri,
                };

                return config;
            }

            return null;
        }

        private JwtHeader GetJwtHeader(string room)
        {
            this.logger.LogDebug("Fetching Token Header from cache for {Room}", room);
            SortedList<long, InternalIssuerSigningInfo>? signingInfos = this.memoryCache.Get<SortedList<long, InternalIssuerSigningInfo>>($"{room}.SigningInfos")!;
            return signingInfos.First(p => this.dateTimeDelegate.UtcUnixTime < p.Key).Value.TokenHeader;
        }
    }
}
