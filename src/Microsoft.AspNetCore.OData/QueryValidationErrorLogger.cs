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
#if !NETSTANDARD2_0
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
