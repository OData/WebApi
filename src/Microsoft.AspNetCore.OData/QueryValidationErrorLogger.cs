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
using System.Collections.Generic;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.UriParser;
#endif

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Writes structured diagnostics about a query that failed validation. Shared by the
    /// <see cref="EnableQueryAttribute"/> validation paths, and never changes the response produced for the query.
    /// </summary>
    internal static class QueryValidationErrorLogger
    {
        private const string MessageTemplate =
            "OData query validation failed. Endpoint: {Endpoint}, Type: {QueryType}, Query options: {QueryOptions}. {Reason}";

        /// <summary>
        /// Writes the diagnostic entry for a failed query validation at the specified level. Does nothing when no
        /// logger is available or the level is not enabled.
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
                // the concrete request path. The template identifies the endpoint and keeps the route prefix while
                // representing entity keys as placeholders, so the same endpoint is reported consistently across
                // requests. It is null when the request is not served by a routed endpoint, in which case it is
                // omitted.
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
        /// Builds a route-template-like identifier for the matched OData endpoint on targets that predate ASP.NET
        /// Core endpoint routing. Reconstructs the template from the parsed OData path segments, preserving entity
        /// set, singleton, navigation, and property names while representing entity keys as the "{key}" placeholder,
        /// so the same endpoint is reported consistently regardless of the concrete key value. This mirrors the value
        /// produced from the endpoint's route pattern on endpoint-routing targets (for example, "odata/Customers({key})").
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>
        /// The endpoint template, or <c>null</c> when the request is not served by a routed OData endpoint.
        /// </returns>
        private static string BuildRoutedEndpointTemplate(HttpContext httpContext)
        {
            var odataFeature = httpContext.ODataFeature();
            var path = odataFeature?.Path;
            if (path == null || path.Segments.Count == 0)
            {
                return null;
            }

            List<string> parts = new List<string>(path.Segments.Count);
            foreach (ODataPathSegment segment in path.Segments)
            {
                if (segment is KeySegment)
                {
                    // Represent the key as a placeholder and, matching the endpoint-routing template shape,
                    // attach it to the preceding segment (for example, "Customers({key})").
                    if (parts.Count > 0)
                    {
                        parts[parts.Count - 1] = parts[parts.Count - 1] + "({key})";
                    }
                    else
                    {
                        parts.Add("{key}");
                    }
                }
                else
                {
                    // ODataPathSegment.Identifier is the segment's name (entity set, singleton, navigation
                    // property, property, operation, $count, $value, ...), so the framework's own naming is reused.
                    parts.Add(segment.Identifier);
                }
            }

            string template = string.Join("/", parts);
            string prefix = odataFeature.RoutePrefix;
            return string.IsNullOrEmpty(prefix) ? template : string.Concat(prefix, "/", template);
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
