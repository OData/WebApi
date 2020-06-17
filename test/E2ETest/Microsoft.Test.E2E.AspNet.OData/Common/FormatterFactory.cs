// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Buffers;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
#else
using System.Collections.Generic;
using System.Net.Http.Formatting;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class FormatterFactory
    {
#if NETCORE
        /// <summary>
        /// Create a Json formatter.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A Json formatter.</returns>
        public static OutputFormatter CreateJson(WebRouteConfiguration configuration)
        {
#if NETCORE2x
            var options = configuration.ServiceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value;
            var charPool = configuration.ServiceProvider.GetRequiredService<ArrayPool<char>>();
            return new JsonOutputFormatter(options.SerializerSettings, charPool);
#else
            var options = configuration.ServiceProvider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value;
            var charPool = configuration.ServiceProvider.GetRequiredService<ArrayPool<char>>();
            return new NewtonsoftJsonOutputFormatter(options.SerializerSettings, charPool, new MvcOptions());
#endif
        }

        /// <summary>
        /// Create the OData formatters.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The OData formatters.</returns>
        public static IList<ODataOutputFormatter> CreateOData(WebRouteConfiguration configuration)
        {
            return ODataOutputFormatterFactory.Create();
        }
#else
            /// <summary>
            /// Create a Json formatter.
            /// </summary>
            /// <param name="configuration">The configuration.</param>
            /// <returns>A Json formatter.</returns>
            public static MediaTypeFormatter CreateJson(WebRouteConfiguration configuration)
        {
            return new JsonMediaTypeFormatter();
        }

        /// <summary>
        /// Create the OData formatters.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The OData formatters.</returns>
        public static IList<ODataMediaTypeFormatter> CreateOData(WebRouteConfiguration configuration)
        {
            return ODataMediaTypeFormatters.Create();
        }
#endif
    }
}
