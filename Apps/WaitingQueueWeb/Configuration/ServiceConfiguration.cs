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
    using BCGov.WaitingQueue.Common.Delegates;
    using BCGov.WaitingQueue.TicketManagement.Services;
    using BCGov.WebCommon.Delegates;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides ASP.Net Services related to controllers.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Adds and configures services.
        /// </summary>
        /// <param name="builder">The web application builder to use.</param>
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<ITicketService, RedisTicketService>();
            builder.Services.AddTransient<IDateTimeDelegate, DateTimeDelegate>();
            builder.Services.AddTransient<IWebTicketDelegate, WebTicketDelegate>();
        }
    }
}
