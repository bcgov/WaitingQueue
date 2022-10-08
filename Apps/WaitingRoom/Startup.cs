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
using BCGov.WaitingQueue.Common.Delegates;
using BCGov.WaitingQueue.TicketManagement.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

[assembly: FunctionsStartup(typeof(WaitingRoom.Startup))]

namespace WaitingRoom
{
    using System.Reflection;
    using BCGov.WebCommon.Delegates;

    /// <summary>
    /// Azure Function startup.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <inheritdoc />
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();
            builder.ConfigurationBuilder
                .SetBasePath(context.ApplicationRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <summary>
        /// Configure application services for dependency injection.
        /// </summary>
        /// <param name="builder">The function host builder.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration configuration = builder.GetContext().Configuration;
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configuration.GetValue<string>("RedisConnection")));

            builder.Services.AddTransient<ITicketService, RedisTicketService>();
            builder.Services.AddTransient<IDateTimeDelegate, DateTimeDelegate>();
            builder.Services.AddTransient<IWebTicketDelegate, WebTicketDelegate>();
        }
    }
}
