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
namespace BCGov.WaitingQueue.Configuration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using BCGov.WaitingQueue.Common.Delegates;
    using BCGov.WaitingQueue.TicketManagement.Api;
    using BCGov.WaitingQueue.TicketManagement.ErrorHandling;
    using BCGov.WaitingQueue.TicketManagement.Issuers;
    using BCGov.WaitingQueue.TicketManagement.Models;
    using BCGov.WaitingQueue.TicketManagement.Services;
    using BCGov.WebCommon.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    /// <summary>
    /// Provides ASP.Net Services related to controllers.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Adds and configures services.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITicketService, RedisTicketService>();
            services.AddTransient<IRoomService, RedisRoomService>();
            services.AddTransient<IDateTimeDelegate, DateTimeDelegate>();
            services.AddTransient<IWebTicketDelegate, WebTicketDelegate>();

            string ticketIssuer = configuration.GetValue("TokenIssuer", ITokenIssuer.DefaultIssuer)!;
            string issuerTypeFullname = $"BCGov.WaitingQueue.TicketManagement.Issuers.{ticketIssuer}";
            Type issuerType = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName != null && issuerTypeFullname.Equals(t.FullName, StringComparison.OrdinalIgnoreCase)) ?? throw new ConfigurationException($"Unknown token issuer: {issuerTypeFullname}");
            services.AddSingleton(typeof(ITokenIssuer), issuerType);

            // Dynamically add IOptions and other required dependencies
            switch (ticketIssuer)
            {
                case ITokenIssuer.DefaultIssuer:
                    KeycloakIssuerOptions issuerOptions = configuration.GetSection(ticketIssuer).Get<KeycloakIssuerOptions>() ?? throw new ConfigurationException($"Missing configuration for {ticketIssuer}");
                    services.Configure<KeycloakIssuerOptions>(configuration.GetSection(ticketIssuer));

                    // Add Keycloak Refit API
                    services.AddRefitClient<IKeycloakApi>()
                        .ConfigureHttpClient(c => c.BaseAddress = issuerOptions.BaseUri);
                    break;
                case "InternalIssuer":
                    services.AddMemoryCache();
                    services.AddSingleton(typeof(ISecurityService), issuerType);
                    services.Configure<InternalIssuerOptions>(configuration.GetSection(ticketIssuer));
                    break;
                default:
                    throw new ConfigurationException($"Unknown token issuer: {ticketIssuer}");
            }
        }
    }
}
