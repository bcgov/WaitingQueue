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
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides ASP.Net Services related to cors.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class CorsConfiguration
    {
        /// <summary>
        /// Adds and configures the services required to use cors.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        public static void ConfigureCors(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("allowAny", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        }

        /// <summary>
        /// Configures the app to use cors.
        /// </summary>
        /// <param name="builder">The web application builder to use.</param>
        /// <param name="app">The application to use.</param>
        public static void UseCors(WebApplicationBuilder builder, IApplicationBuilder app)
        {
            // Enable CORS
            string? enableCors = builder.Configuration.GetValue<string>("AllowOrigins");
            if (!string.IsNullOrEmpty(enableCors))
            {
                app.UseCors(
                    build =>
                    {
                        build
                            .WithOrigins(enableCors)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            }
        }
    }
}
