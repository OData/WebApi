// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public static class ODataTestExtension
    {
        public static Task ClearRepositoryAsync<TTest>(this WebHostTestBase<TTest> test, string entityName)
        {
            return test.Client.DeleteAsync(test.BaseAddress + "/" + entityName);
        }

        public static void EnableODataSupport(this WebRouteConfiguration configuration, IEdmModel model, string routePrefix)
        {
            var conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new PropertyRoutingConvention());
            conventions.Insert(0, new NavigationRoutingConvention2());

            configuration.MapODataServiceRoute(
                ODataTestConstants.DefaultRouteName, 
                routePrefix, model, 
                new DefaultODataPathHandler(), 
                conventions);

            configuration.AddODataQueryFilter();
        }

        public static void EnableODataSupport(this WebRouteConfiguration configuration, IEdmModel model)
        {
            configuration.EnableODataSupport(model, routePrefix: null);
        }

        /// <summary>
        /// Helper method to get the odata path for an arbitrary odata uri.
        /// </summary>
        /// <param name="request">The request instance in current context</param>
        /// <param name="uri">OData uri</param>
        /// <returns>The parsed odata path</returns>
#if NETCORE
        public static ODataPath CreateODataPath(this HttpRequest request, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            // Clone the features so that a new set is used for each context.
            // The features themselves will be reused but not the collection. We
            // store the request container as a feature of the request and we don't want
            // the features added to one context/request to be visible on another.
            //
            // Note that just about everything in the HttpContext and HttpRequest is
            // backed by one of these features. So reusing the features means the HttContext
            // and HttpRequests are the same without needing to copy properties. To make them
            // different, we need to avoid copying certain features to that the objects don't
            // share the same storage/
            IFeatureCollection features = new FeatureCollection();
            foreach (KeyValuePair<Type, object> kvp in request.HttpContext.Features)
            {
                // Don't include the OData features. They may already
                // be present. This will get re-created later.
                //
                // Also, clear out the items feature, which is used
                // to store a few object, the one that is an issue here is the Url
                // helper, which has an affinity to the context. If we leave it,
                // the context of the helper no longer matches the new context and
                // the resulting url helper doesn't have access to the OData feature
                // because it's looking in the wrong context.
                //
                // Because we need a different request and response, leave those features
                // out as well.
                if (kvp.Key == typeof(IODataBatchFeature) ||
                    kvp.Key == typeof(IODataFeature) ||
                    kvp.Key == typeof(IItemsFeature) ||
                    kvp.Key == typeof(IHttpRequestFeature) ||
                    kvp.Key == typeof(IHttpResponseFeature))
                {
                    continue;
                }

                features[kvp.Key] = kvp.Value;
            }

            // Add in an items, request and response feature.
            features[typeof(IItemsFeature)] = new ItemsFeature();
            features[typeof(IHttpRequestFeature)] = new HttpRequestFeature();
            features[typeof(IHttpResponseFeature)] = new HttpResponseFeature();

            // Create a context from the factory or use the default context.
            HttpContext context = new DefaultHttpContext(features);

            // Clone parts of the request. All other parts of the request will be 
            // populated during batch processing.
            context.Request.Cookies = request.HttpContext.Request.Cookies;
            foreach (KeyValuePair<string, StringValues> header in request.HttpContext.Request.Headers)
            {
                context.Request.Headers.Add(header);
            }

            // Copy the Uri.
            context.Request.Scheme = uri.Scheme;
            context.Request.Host = uri.IsDefaultPort ?
                new HostString(uri.Host) :
                new HostString(uri.Host, uri.Port);
            context.Request.QueryString = new QueryString(uri.Query);
            context.Request.Path = new PathString(uri.AbsolutePath);

            // Get the existing OData route
            IRoutingFeature routingFeature = context.Features[typeof(IRoutingFeature)] as IRoutingFeature;
            ODataRoute route = routingFeature.RouteData.Routers.OfType<ODataRoute>().FirstOrDefault();

            // Attempt to route the new request and extract the path.
            RouteContext routeContext = new RouteContext(context);
            route.RouteAsync(routeContext).Wait();
            return context.Request.ODataFeature().Path;
        }
#else
        public static ODataPath CreateODataPath(this HttpRequestMessage request, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var newRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            newRequest.SetConfiguration(request.GetConfiguration());
            var route = request.GetRouteData().Route;

            var newRoute = new HttpRoute(
                route.RouteTemplate,
                new HttpRouteValueDictionary(route.Defaults),
                new HttpRouteValueDictionary(route.Constraints),
                new HttpRouteValueDictionary(route.DataTokens),
                route.Handler);
            var routeData = newRoute.GetRouteData(request.GetConfiguration().VirtualPathRoot, newRequest);
            if (routeData == null)
            {
                throw new InvalidOperationException("The link is not a valid odata link.");
            }

            return newRequest.ODataProperties().Path;
        }
#endif

        /// <summary>
        /// Helper method to get the key value from a uri.
        /// Usually used by $link action to extract the key value from the url in body.
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <param name="request">The request instance in current context</param>
        /// <param name="uri">OData uri that contains the key value</param>
        /// <returns>The key value</returns>
#if NETCORE
        public static TKey GetKeyValue<TKey>(this HttpRequest request, Uri uri)
#else
        public static TKey GetKeyValue<TKey>(this HttpRequestMessage request, Uri uri)
#endif
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            //get the odata path Ex: ~/entityset/key/$links/navigation
            var odataPath = request.CreateODataPath(uri);
            var keySegment = odataPath.Segments.OfType<KeySegment>().FirstOrDefault();
            if (keySegment == null)
            {
                throw new InvalidOperationException("The link does not contain a key.");
            }

            return (TKey)keySegment.Keys.First().Value;
        }

        /// <summary>
        /// Convert model state errors into string value.
        /// </summary>
        /// <param name="modelState">Model state</param>
        /// <returns>String value which contains all model errors</returns>
        public static string GetModelStateErrorInformation(ModelStateDictionary modelState)
        {
            StringBuilder errorMessageBuilder = new StringBuilder();
            errorMessageBuilder.AppendLine("Invalid request received.");

            if (modelState != null)
            {
                foreach (var key in modelState.Keys)
                {
                    if (modelState[key].Errors.Count > 0)
                    {
#if NETCORE
                        errorMessageBuilder.AppendLine(key + ":" + ((modelState[key] != null) ? modelState[key].RawValue : "null"));
#else
                        errorMessageBuilder.AppendLine(key + ":" + ((modelState[key].Value != null) ? modelState[key].Value.RawValue : "null"));
#endif
                    }
                }
            }

            return errorMessageBuilder.ToString();
        }
    }

#if NOTHING
    public class EntityTypeConstraint : IHttpRouteConstraint
    {
        public bool Match(System.Net.Http.HttpRequestMessage request, IHttpRoute route, string parameterName, System.Collections.Generic.IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (routeDirection == HttpRouteDirection.UriGeneration)
            {
                return true;
            }

            return ((string)values[parameterName]).Contains(".");
        }
    }
#endif
}
