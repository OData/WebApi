﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public static Task ClearRepositoryAsync(this WebHostTestBase test, string entityName)
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
#if !NETCORE // TODO #939: Enable this function for AspNetCore
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
#else
        public static ODataPath CreateODataPath(this HttpRequest request, Uri uri)
        {
            return null;
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
