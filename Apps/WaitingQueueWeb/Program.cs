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
namespace BCGov.WaitingQueue
{
    using System;
    using BCGov.WaitingQueue.Configuration;
    using BCGov.WaitingQueue.TicketManagement.Api;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    /// <summary>
    /// Main entry.
    /// </summary>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Configure problem details
            ExceptionHandlingConfiguration.ConfigureProblemDetails(builder.Services, builder.Environment);

            // Add controllers
            ControllerConfiguration.ConfigureControllers(builder);

            // Add swagger
            SwaggerConfiguration.ConfigureSwagger(builder);

            // Add redis
            RedisConfiguration.ConfigureRedis(builder);

            // Add cors
            CorsConfiguration.ConfigureCors(builder.Services);

            // Add services
            ServiceConfiguration.ConfigureServices(builder);

            // Add Refit APIs
            Uri? baseUri = builder.Configuration.GetValue<Uri>("Keycloak:BaseUri");
            builder.Services.AddRefitClient<IKeycloakApi>()
                .ConfigureHttpClient(c => c.BaseAddress = baseUri);

            WebApplication app = builder.Build();

            // Use problem details
            ExceptionHandlingConfiguration.UseProblemDetails(app, builder.Environment);

            // Use Swagger
            SwaggerConfiguration.UseSwagger(app);

            app.MapControllers();

            // Enable CORS
            CorsConfiguration.UseCors(builder, app);

            app.Run();
        }
    }
}
