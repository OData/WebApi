// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi options to OData WebApi.
    /// </summary>
    internal class WebApiOptions : IWebApiOptions
    {
        /// <summary>
        /// Initializes a new instance of the WebApiOptions class.
        /// </summary>
        /// <param name="options">The inner options.</param>
        public WebApiOptions(ODataOptions options)
        {
            if (options == null)
            {
                throw Error.ArgumentNull("options");
            }

            this.NullDynamicPropertyIsEnabled = options.NullDynamicPropertyIsEnabled;
            this.UrlKeyDelimiter = options.UrlKeyDelimiter;
            this.EnableContinueOnErrorHeader = options.EnableContinueOnErrorHeader;
        }

        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// </summary>
        public ODataUrlKeyDelimiter UrlKeyDelimiter { get; private set; }

        /// <summary>
        /// Gets or Sets a value indicating if value should be emitted for dynamic properties which are null.
        /// </summary>
        public bool NullDynamicPropertyIsEnabled { get; private set; }

        /// <summary>
        /// Check the continue-on-error header is enable or not.
        /// </summary>
        /// <returns>True if continue-on-error header is enable; false otherwise</returns>
        public bool EnableContinueOnErrorHeader { get; private set; }
    }
}
