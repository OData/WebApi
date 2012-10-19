// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Query;
using Microsoft.Data.Edm;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        private const string EdmModelKey = "MS_EdmModel";
        private const string ODataFormatterKey = "MS_ODataFormatter";
        private const string ODataActionResolverKey = "MS_ODataActionResolver";

        /// <summary>
        /// Retrieve the EdmModel from the configuration Properties collection. Null if user has not set it.
        /// </summary>
        /// <param name="configuration">Configuration to look into.</param>
        /// <returns>Returns an EdmModel for this configuration</returns>
        public static IEdmModel GetEdmModel(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // returns one if user sets one, null otherwise
            object result;
            if (configuration.Properties.TryGetValue(EdmModelKey, out result))
            {
                return result as IEdmModel;
            }

            return null;
        }

        /// <summary>
        /// Sets the given EdmModel with the configuration.
        /// </summary>
        /// <param name="configuration">Configuration to be updated.</param>
        /// <param name="model">The EdmModel to update.</param>
        public static void SetEdmModel(this HttpConfiguration configuration, IEdmModel model)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            configuration.Properties.AddOrUpdate(EdmModelKey, model, (a, b) =>
                {
                    return model;
                });
        }

        /// <summary>
        /// Retrieve the <see cref="MediaTypeFormatter" /> from the configuration. Null if user has not set it.
        /// </summary>
        /// <param name="configuration">Configuration to look into.</param>
        /// <param name="edmModel">The EDM model.</param>
        /// <returns>
        /// Returns an <see cref="MediaTypeFormatter" /> for this configuration.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Calling the formatter only to identify the ODataFormatter; exceptions can be ignored")]
        internal static MediaTypeFormatter GetODataFormatter(this HttpConfiguration configuration, out IEdmModel edmModel)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            foreach (MediaTypeFormatter formatter in configuration.Formatters)
            {
                ODataMediaTypeFormatter odataFormatter = formatter as ODataMediaTypeFormatter;
                if (odataFormatter != null)
                {
                    edmModel = odataFormatter.Model;
                    return odataFormatter;
                }
            }

            // Detects ODataFormatters that are wrapped by tracing
            // Creates a dummy request message and sees if the formatter adds a model to the request properties
            // This is a workaround until tracing provides information about the wrapped inner formatter
            foreach (MediaTypeFormatter formatter in configuration.Formatters)
            {
                using (HttpRequestMessage request = new HttpRequestMessage())
                {
                    try
                    {
                        formatter.GetPerRequestFormatterInstance(typeof(IEdmModel), request, mediaType: null);
                        object model;
                        if (request.Properties.TryGetValue(ODataMediaTypeFormatter.EdmModelKey, out model))
                        {
                            edmModel = model as IEdmModel;
                            if (edmModel != null)
                            {
                                return formatter;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore exceptions - it isn't the OData formatter we're looking for
                    }
                }
            }

            edmModel = null;
            return null;
        }

        /// <summary>
        /// Gets the <see cref="IODataActionResolver"/> from the configuration.
        /// </summary>
        /// <remarks>
        /// If an <see cref="IODataActionResolver"/> is not found, this method registers and returns a <see cref="DefaultODataActionResolver"/> 
        /// </remarks>
        /// <param name="configuration">Configuration to check.</param>
        /// <returns>Returns an <see cref="IODataActionResolver"/></returns>
        public static IODataActionResolver GetODataActionResolver(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // returns one if user sets one, null otherwise
            object result = configuration.Properties.GetOrAdd(ODataActionResolverKey, (key) => new DefaultODataActionResolver());
            return result as IODataActionResolver;
        }

        /// <summary>
        /// Sets the <see cref="IODataActionResolver"/> on the configuration
        /// </summary>
        /// <param name="configuration">Configuration to be updated.</param>
        /// <param name="resolver">The <see cref="IODataActionResolver"/> this configuration should use.</param>
        public static void SetODataActionResolver(this HttpConfiguration configuration, IODataActionResolver resolver)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (resolver == null)
            {
                throw Error.ArgumentNull("resolver");
            }
            configuration.Properties[ODataActionResolverKey] = resolver;
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return type.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static void EnableQuerySupport(this HttpConfiguration configuration)
        {
            configuration.EnableQuerySupport(new QueryableAttribute());
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return type.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public static void EnableQuerySupport(this HttpConfiguration configuration, IActionFilter queryFilter)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Services.Add(typeof(IFilterProvider), new QueryFilterProvider(queryFilter));
        }
    }
}
