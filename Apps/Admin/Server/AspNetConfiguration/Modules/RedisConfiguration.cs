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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using StackExchange.Redis;

    /// <summary>
    /// Provides ASP.Net Services related to redis.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class RedisConfiguration
    {
        /// <summary>
        /// Adds and configures redis.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="configuration">The configuration values from.</param>
        public static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configuration.GetValue<string>("RedisConnection")));
        }
    }
}
