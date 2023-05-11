//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace BCGov.WaitingQueue.Admin.Server.AspNetConfiguration
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The program configuration class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ProgramConfiguration
    {
        private const string EnvironmentPrefix = "WaitingQueue_";

        /// <summary>
        /// Creates a WebApplicationBuilder with configuration set and open telemetry.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Returns the configured WebApplicationBuilder.</returns>
        public static WebApplicationBuilder CreateWebAppBuilder(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddSimpleConsole(
                options =>
                {
                    options.TimestampFormat = "[yyyy/MM/dd HH:mm:ss]";
                    options.IncludeScopes = true;
                });

            // OpenTelemetry
            builder.Logging.AddOpenTelemetry();

            // Additional configuration sources
            builder.Configuration.AddJsonFile("appsettings.local.json", true, true);
            builder.Configuration.AddEnvironmentVariables(prefix: EnvironmentPrefix);

            return builder;
        }

        /// <summary>
        /// Create an intiial logger to use during Program startup.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <returns>An instance of a logger.</returns>
        public static ILogger GetInitialLogger(IConfiguration configuration)
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(
                builder =>
                {
                    builder.AddSimpleConsole(
                        options =>
                        {
                            options.TimestampFormat = "[yyyy/MM/dd HH:mm:ss]";
                            options.IncludeScopes = true;
                        });

                    builder.AddConfiguration(configuration);
                });

            return loggerFactory.CreateLogger("Startup");
        }
    }
}
