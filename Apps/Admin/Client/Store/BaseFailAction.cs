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
namespace BCGov.WaitingQueue.Admin.Client.Store
{
    using System;

    /// <summary>
    /// The base class for a failed action.
    /// </summary>
    public abstract class BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFailAction"/> class.
        /// </summary>
        /// <param name="e">The request exception.</param>
        protected BaseFailAction(Exception e)
        {
            this.Exception = e;
        }

        /// <summary>
        /// Gets the error associated with the failed action.
        /// </summary>
        public Exception Exception { get; }
    }
}
