// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        private const string EdmModelKey = "MS_EdmModel";
        private const string ODataFormatterKey = "MS_ODataFormatter";
        private const string ODataPathHandlerKey = "MS_ODataPathHandler";
        private const string ODataRoutingConventionsKey = "MS_ODataRoutingConventions";

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
        /// Gets the <see cref="IODataPathHandler"/> from the configuration.
        /// </summary>
        /// <param name="configuration">The server's configuration.</param>
        /// <returns>The <see cref="IODataPathHandler"/> for the configuration.</returns>
        public static IODataPathHandler GetODataPathHandler(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            object pathHandler;
            if (configuration.Properties.TryGetValue(ODataPathHandlerKey, out pathHandler))
            {
                return pathHandler as IODataPathHandler;
            }
            return null;            
        }

        /// <summary>
        /// Sets the <see cref="IODataPathHandler"/> on the configuration.
        /// </summary>
        /// <param name="configuration">The server's configuration.</param>
        /// <param name="parser">The <see cref="IODataPathHandler"/> this configuration should use.</param>
        public static void SetODataPathHandler(this HttpConfiguration configuration, IODataPathHandler parser)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (parser == null)
            {
                throw Error.ArgumentNull("parser");
            }

            configuration.Properties[ODataPathHandlerKey] = parser;
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

        /// <summary>
        /// Enables OData support by adding an OData route and enabling OData controller and action selection, querying, and formatter support for OData.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="model">The EDM model to use for the service.</param>
        public static void EnableOData(this HttpConfiguration configuration, IEdmModel model)
        {
            configuration.EnableOData(model, routePrefix: null);
        }

        /// <summary>
        /// Enables OData support by adding an OData route and enabling OData controller and action selection, querying, and formatter support for OData.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="model">The EDM model to use for the service.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        public static void EnableOData(this HttpConfiguration configuration, IEdmModel model, string routePrefix)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            // Querying
            configuration.SetEdmModel(model);
            configuration.EnableQuerySupport();

            // Routing
            string routeTemplate = String.IsNullOrEmpty(routePrefix) ?
                ODataRouteConstants.ODataPathTemplate :
                routePrefix + "/" + ODataRouteConstants.ODataPathTemplate;
            IODataPathHandler pathHandler = configuration.GetODataPathHandler() ?? new DefaultODataPathHandler(model);
            IHttpRouteConstraint routeConstraint = new ODataPathRouteConstraint(pathHandler);

            configuration.Routes.MapHttpRoute(ODataRouteConstants.RouteName, routeTemplate, null, new HttpRouteValueDictionary() { { ODataRouteConstants.ConstraintName, routeConstraint } });
            configuration.Services.Replace(typeof(IHttpControllerSelector), new ODataControllerSelector(configuration));
            configuration.Services.Replace(typeof(IHttpActionSelector), new ODataActionSelector());

            // Formatter
            configuration.Formatters.InsertRange(0, ODataMediaTypeFormatters.Create(model));
        }

        /// <summary>
        /// Gets the OData routing conventions to use for controller and action selection.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <returns>A list of the OData routing conventions for the server.</returns>
        public static IList<IODataRoutingConvention> GetODataRoutingConventions(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            return configuration.Properties.GetOrAdd(ODataRoutingConventionsKey, GetDefaultRoutingConventions) as IList<IODataRoutingConvention>;
        }

        private static IList<IODataRoutingConvention> GetDefaultRoutingConventions(object key)
        {
            return new List<IODataRoutingConvention>()
            {
                new MetadataRoutingConvention(),
                new EntitySetRoutingConvention(),
                new EntityRoutingConvention(),
                new NavigationRoutingConvention(),
                new LinksRoutingConvention(),
                new ActionRoutingConvention(),
                new UnmappedRequestRoutingConvention()
            };
        }
    }
}
