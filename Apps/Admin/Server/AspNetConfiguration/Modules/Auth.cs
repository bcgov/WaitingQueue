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
namespace BCGov.WaitingQueue.Admin.Server.AspNetConfiguration.Modules
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Admin.Server.Authorization;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using AuthenticationFailedContext = Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext;

    /// <summary>
    /// Provides ASP.Net Services related to Authentication and Authorization services.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
    public static class Auth
    {
        /// <summary>
        /// Configures the auth services for json web token bearer.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use for values.</param>
        /// <param name="environment">The environment to use.</param>
        public static void ConfigureAuthServicesForJwtBearer(IServiceCollection services, ILogger logger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            bool debugEnabled = environment.IsDevelopment() || configuration.GetValue("EnableDebug", true);
            logger.LogDebug("Debug configuration is {DebugEnabled}", debugEnabled);

            // Displays sensitive data from the jwt if the environment is development only
            IdentityModelEventSource.ShowPII = debugEnabled;
            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    options =>
                    {
                        options.SaveToken = true;
                        options.RequireHttpsMetadata = true;
                        options.IncludeErrorDetails = true;
                        configuration.GetSection("OpenIdConnect").Bind(options);

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            ValidateAudience = true,
                            ValidateIssuer = true,
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = ctx => OnAuthenticationFailed(logger, ctx),
                        };
                    });

            services.AddScoped<IAuthorizationHandler, RoomAccessHandler>();
            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy(
                        RoomPolicy.RoomAccess,
                        policy =>
                        {
                            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.RequireRole(Roles.Admin);
                            policy.Requirements.Add(new RoomAccessRequirement());
                        });
                });
        }

        /// <summary>
        /// Configures the app to use auth.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="logger">The logger to use.</param>
        public static void UseAuth(IApplicationBuilder app, ILogger logger)
        {
            logger.LogDebug("Use Auth...");

            // Enable jwt authentication
            app.UseAuthentication();
            app.UseAuthorization();
        }

        /// <summary>
        /// Handles Bearer Token authentication failures.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="context">The JWT authentication failed context.</param>
        /// <returns>An async task.</returns>
        private static Task OnAuthenticationFailed(ILogger logger, AuthenticationFailedContext context)
        {
            logger.LogDebug("OnAuthenticationFailed...{Exception}", context.Exception);
            return Task.CompletedTask;
        }
    }
}
