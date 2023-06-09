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
namespace BCGov.WaitingQueue.Admin.Server
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using BCGov.WaitingQueue.Admin.Server.AspNetConfiguration;
    using BCGov.WaitingQueue.Admin.Server.AspNetConfiguration.Modules;
    using BCGov.WaitingQueue.Admin.Server.Services;
    using BCGov.WaitingQueue.Common.Delegates;
    using BCGov.WaitingQueue.TicketManagement.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The entry point for the project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        /// <summary>
        /// The entry point for the class.
        /// </summary>
        /// <param name="args">The command line arguments to be passed in.</param>
        /// <returns>A task which represents the exit of the application.</returns>
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = ProgramConfiguration.CreateWebAppBuilder(args);

            IServiceCollection services = builder.Services;
            IConfiguration configuration = builder.Configuration;
            ILogger logger = ProgramConfiguration.GetInitialLogger(configuration);
            IWebHostEnvironment environment = builder.Environment;

            AddModules(services, configuration, logger, environment);
            AddServices(services);
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();

            ExceptionHandling.UseProblemDetails(app);
            HttpWeb.UseForwardHeaders(app, logger, configuration);
            HttpWeb.UseHttp(app, logger, configuration, environment);
            HttpWeb.UseContentSecurityPolicy(app, configuration);
            SwaggerDoc.UseSwagger(app, logger);
            Auth.UseAuth(app, logger);

            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            await app.RunAsync();
        }

        private static void AddModules(IServiceCollection services, IConfiguration configuration, ILogger logger, IWebHostEnvironment environment)
        {
            HttpWeb.ConfigureForwardHeaders(services, logger, configuration);
            HttpWeb.ConfigureHttpServices(services, logger);
            Auth.ConfigureAuthServicesForJwtBearer(services, logger, configuration, environment);
            SwaggerDoc.ConfigureSwaggerServices(services, configuration);
            ExceptionHandling.ConfigureProblemDetails(services, environment);
            RedisConfiguration.ConfigureRedis(services, configuration);
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IDateTimeDelegate, DateTimeDelegate>();
            services.AddTransient<IRoomService, RedisRoomService>();
            services.AddTransient<ITicketService, RedisTicketService>();
        }
    }
}
