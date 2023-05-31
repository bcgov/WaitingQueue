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
namespace BCGov.WaitingQueue.Admin.Server.Controllers
{
    using BCGov.WaitingQueue.Admin.Common.Models;
    using BCGov.WaitingQueue.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to return waiting queue configuration for approved clients.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ConfigurationController : Controller
    {
        /// <summary>
        /// Returns the external Waiting Queue configuration.
        /// </summary>
        /// <param name="configurationService">The injected configuration provider.</param>
        /// <returns>The Health Gateway Configuration.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ExternalConfiguration Index([FromServices] IConfigurationService configurationService)
        {
            ExternalConfiguration externalConfig = configurationService.GetConfiguration();
            externalConfig.ClientIp = this.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            return externalConfig;
        }

        /// <summary>
        /// Returns a sample response to test the authorization.
        /// </summary>
        /// <returns>ProblemDetails.</returns>
        [HttpGet]
        [Authorize]
        [Route("AuthorizeTest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ProblemDetails Get()
        {
            ProblemDetails problem = new()
            {
                Detail = "Authorization successful",
                Instance = "Authorization",
                Title = "Authorization successful",
                Type = "Authorization",
                Status = StatusCodes.Status200OK,
            };

            return problem;
        }
    }
}
