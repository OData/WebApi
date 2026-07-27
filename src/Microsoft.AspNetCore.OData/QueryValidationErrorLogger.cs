//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLogger.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
#if !NETSTANDARD2_0
using Microsoft.AspNetCore.Routing;
#else
using System.Text;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Writes structured diagnostics for a query that failed validation, without changing the response.
    /// </summary>
    internal static class QueryValidationErrorLogger
    {
        private const string MessageTemplate =
            "OData query validation failed. Endpoint: {Endpoint}, Type: {QueryType}, Query options: {QueryOptions}. {Reason}";

        /// <summary>
        /// Writes the diagnostic for a failed query validation, or does nothing when the logger or level is disabled.
        /// </summary>
        /// <param name="logger">The logger to write to, or <c>null</c> when none is available.</param>
        /// <param name="logLevel">The level at which the diagnostic is written.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="processedQueryOptions">
        /// The query options captured before validation ran, or <c>null</c> when they could not be built.
        /// </param>
        /// <param name="exception">The exception raised while validating the query.</param>
        internal static void LogQueryValidationFailure(ILogger logger, LogLevel logLevel, HttpContext httpContext, ODataQueryOptions processedQueryOptions, Exception exception)
        {
            if (logger == null || httpContext == null || !logger.IsEnabled(logLevel))
            {
                return;
            }

            try
            {
                // Record the matched endpoint's route template (for example, "odata/Customers({key})") rather than
                // the concrete request path, so the same endpoint is reported consistently. Null when not routed.
                string endpoint = null;
#if NETSTANDARD2_0
                endpoint = BuildRoutedEndpointTemplate(httpContext);
#else
                endpoint = (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern?.RawText;
#endif

                // Use LoggerMessage.Define so the diagnostic is written with named, structured values on every
                // supported target (the generic ILogger.Log overloads are not available on all of them). The
                // dynamic level from configuration is baked into the delegate here.
                Action<ILogger, string, string, string, string, Exception> log =
                    LoggerMessage.Define<string, string, string, string>(logLevel, default(EventId), MessageTemplate);

                log(
                    logger,
                    endpoint,
                    processedQueryOptions?.Context?.ElementType?.FullTypeName(),
                    FormatRequestedQueryOptions(processedQueryOptions?.RawValues),
                    exception?.Message,
                    exception);
            }
            catch (Exception)
            {
                // Recording the diagnostic must never change the request outcome. If the configured logging
                // provider throws while writing this entry, suppress it so the original validation response and
                // the exception raised for the failed query are preserved unchanged.
            }
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Reconstructs the matched OData endpoint's route template from the parsed path on targets without endpoint
        /// routing, using "{key}" placeholders for entity keys (for example, "odata/Customers({key})").
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>The endpoint template, or <c>null</c> when the request is not served by a routed OData endpoint.</returns>
        private static string BuildRoutedEndpointTemplate(HttpContext httpContext)
        {
            IODataFeature odataFeature = httpContext.ODataFeature();
            ODataPath path = odataFeature?.Path;
            if (path == null || path.Segments.Count == 0)
            {
                return null;
            }

            StringBuilder template = new StringBuilder();
            foreach (ODataPathSegment segment in path.Segments)
            {
                if (segment is KeySegment)
                {
                    // Attach the key placeholder to the preceding segment (for example, "Customers({key})"). A
                    // routed OData path never starts with a key; the bare placeholder is a defensive fallback.
                    template.Append(template.Length == 0 ? "{key}" : "({key})");
                }
                else
                {
                    // ODataPathSegment.Identifier is the segment's own name (entity set, navigation property, etc.).
                    if (template.Length != 0)
                    {
                        template.Append('/');
                    }

                    template.Append(segment.Identifier);
                }
            }

            string prefix = odataFeature.RoutePrefix;
            if (!string.IsNullOrEmpty(prefix))
            {
                // The route prefix precedes the first segment (for example, "odata/Customers({key})").
                template.Insert(0, '/').Insert(0, prefix);
            }

            return template.ToString();
        }
#endif

        /// <summary>
        /// Builds a compact description of the supplied <c>$select</c>/<c>$expand</c> options, omitting empty ones.
        /// </summary>
        /// <param name="rawValues">The raw query option values, or <c>null</c> when unavailable.</param>
        /// <returns>The requested query options, or an empty string when none apply.</returns>
        private static string FormatRequestedQueryOptions(ODataRawQueryOptions rawValues)
        {
            if (rawValues == null)
            {
                return string.Empty;
            }

            bool hasSelect = !string.IsNullOrEmpty(rawValues.Select);
            bool hasExpand = !string.IsNullOrEmpty(rawValues.Expand);

            if (hasSelect && hasExpand)
            {
                return string.Concat("$select=", rawValues.Select, "&$expand=", rawValues.Expand);
            }

            if (hasSelect)
            {
                return string.Concat("$select=", rawValues.Select);
            }

            if (hasExpand)
            {
                return string.Concat("$expand=", rawValues.Expand);
            }

            return string.Empty;
        }
    }
}
