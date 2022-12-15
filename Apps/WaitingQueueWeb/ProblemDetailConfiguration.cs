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
    using System.Diagnostics.CodeAnalysis;
    using BCGov.WaitingQueue.ErrorHandling;
    using BCGov.WaitingQueue.TicketManagement.ErrorHandling;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using ProblemDetailsException = BCGov.WaitingQueue.TicketManagement.ErrorHandling.ProblemDetailsException;

    /// <summary>
    /// Provides ASP.Net Services for exception handling.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ProblemDetailConfiguration
    {
        /// <summary>
        /// Adds and configures the services required to use problem details.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="environment">The environment the services are associated with.</param>
        public static void ConfigureProblemDetails(IServiceCollection services, IWebHostEnvironment environment)
        {
            services.AddProblemDetails(
                setup =>
                {
                    setup.IncludeExceptionDetails = (_, _) => environment.IsDevelopment();

                    setup.Map<ProblemDetailsException>(
                        exception => new WaitingQueueProblemDetails
                        {
                            Title = exception.Title,
                            Detail = exception.Detail,
                            Status = (int)exception.StatusCode,
                            Type = exception.ProblemType,
                            Instance = exception.Instance,
                            AdditionalInfo = exception.AdditionalInfo,
                        });
                });
        }

        /// <summary>
        /// Configures the app to use problem details middleware.
        /// </summary>
        /// <param name="app">The application builder where modules are specified to be used.</param>
        public static void UseProblemDetails(IApplicationBuilder app)
        {
            app.UseProblemDetails();
        }
    }
}
