// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
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

            if (configuration.GetODataFormatter() != null)
            {
                throw Error.NotSupported(
                    SRResources.EdmModelMismatch,
                    typeof(IEdmModel).Name,
                    typeof(ODataMediaTypeFormatter).Name,
                    "SetODataFormatter");
            }

            configuration.Properties.AddOrUpdate(EdmModelKey, model, (a, b) =>
                {
                    return model;
                });
        }

        /// <summary>
        /// Retrieve the <see cref="ODataMediaTypeFormatter"/> from the configuration Properties collection. Null if user has not set it.
        /// </summary>
        /// <param name="configuration">Configuration to look into.</param>
        /// <returns>Returns an <see cref="ODataMediaTypeFormatter"/> for this configuration.</returns>
        public static ODataMediaTypeFormatter GetODataFormatter(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // returns one if user sets one, else null.
            object result;
            if (configuration.Properties.TryGetValue(ODataFormatterKey, out result))
            {
                return result as ODataMediaTypeFormatter;
            }

            // Instead of trying to get the odata formatter from the formatter collection which works only if tracing is not enabled
            // fail here so that the user doesn't have a surprise when he enables tracing.
            return null;
        }

        /// <summary>
        /// Sets the given <see cref="ODataMediaTypeFormatter"/> on the configuration and adds it to the formatter collection.
        /// </summary>
        /// <param name="configuration">Configuration to be updated.</param>
        /// <param name="formatter">The <see cref="ODataMediaTypeFormatter"/> to update.</param>
        public static void SetODataFormatter(this HttpConfiguration configuration, ODataMediaTypeFormatter formatter)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (formatter == null)
            {
                throw Error.ArgumentNull("formatter");
            }

            if (configuration.GetODataFormatter() != null || configuration.Formatters.OfType<ODataMediaTypeFormatter>().Any())
            {
                throw Error.NotSupported(SRResources.ResetODataFormatterNotSupported, typeof(ODataMediaTypeFormatter).Name, typeof(HttpConfiguration).Name);
            }
            else if (configuration.GetEdmModel() != null && configuration.GetEdmModel() != formatter.Model)
            {
                throw Error.NotSupported(
                    SRResources.EdmModelOnConfigurationMismatch,
                    typeof(IEdmModel).Name,
                    typeof(ODataMediaTypeFormatter).Name,
                    "SetODataFormatter");
            }
            else
            {
                // This is a workaround for Bug 464640 where the formatter tracer wraps on to the formatter and there is no 
                // easy way to retrieve the ODataFormatter from configuration afterwards.
                configuration.SetEdmModel(formatter.Model);
                configuration.Properties.TryAdd(ODataFormatterKey, formatter);
                configuration.Formatters.Insert(0, formatter);
            }
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
        /// Enables query support for actions with an <see cref="IQueryable"/> or <see cref="IQueryable{T}"/> return type.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static void EnableQuerySupport(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Services.Add(typeof(IFilterProvider), new QueryableFilterProvider());
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable"/> or <see cref="IQueryable{T}"/> return type.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="resultLimit">The maximum number of results to return.</param>
        public static void EnableQuerySupport(this HttpConfiguration configuration, int resultLimit)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (resultLimit <= 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("resultLimit", resultLimit, 1);
            }

            configuration.Services.Add(typeof(IFilterProvider), new QueryableFilterProvider() { ResultLimit = resultLimit });
        }
    }
}
