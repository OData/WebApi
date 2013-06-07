// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Cors;
using System.Web.Http.Controllers;
using System.Web.Http.Cors.Properties;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace System.Web.Http.Cors
{
    /// <summary>
    /// An implementation of <see cref="ICorsPolicyProviderFactory"/> that returns the <see cref="ICorsPolicyProvider"/> from the controller or action attribute.
    /// </summary>
    public class AttributeBasedPolicyProviderFactory : ICorsPolicyProviderFactory
    {
        private const string HttpContextBaseKey = "MS_HttpContext";

        /// <summary>
        /// Gets or sets the default <see cref="ICorsPolicyProvider"/>.
        /// </summary>
        public ICorsPolicyProvider DefaultPolicyProvider { get; set; }

        /// <summary>
        /// Gets the <see cref="ICorsPolicyProvider" /> for the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The <see cref="ICorsPolicyProvider" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The object is registered for disposal when the request message is disposed.")]
        public virtual ICorsPolicyProvider GetCorsPolicyProvider(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            CorsRequestContext corsRequestContext = request.GetCorsRequestContext();
            HttpActionDescriptor actionDescriptor = null;
            if (corsRequestContext.IsPreflight)
            {
                HttpRequestMessage targetRequest = new HttpRequestMessage(new HttpMethod(corsRequestContext.AccessControlRequestMethod), request.RequestUri);
                try
                {
                    foreach (var property in request.Properties)
                    {
                        // The RouteData and HttpContext from the preflight request properties contain information
                        // relevant to the preflight request and not the actual request, therefore we need to exclude them.
                        if (property.Key != HttpPropertyKeys.HttpRouteDataKey &&
                            property.Key != HttpContextBaseKey)
                        {
                            targetRequest.Properties.Add(property.Key, property.Value);
                        }
                    }
                    actionDescriptor = SelectAction(targetRequest);
                }
                catch
                {
                    if (DefaultPolicyProvider != null)
                    {
                        return DefaultPolicyProvider;
                    }
                    throw;
                }
                finally
                {
                    if (targetRequest != null)
                    {
                        request.RegisterForDispose(targetRequest);
                    }
                }
            }
            else
            {
                actionDescriptor = request.GetActionDescriptor();
            }

            return GetCorsPolicyProvider(actionDescriptor);
        }

        private static void RemoveOptionalRoutingParameters(IDictionary<string, object> routeValueDictionary)
        {
            Contract.Assert(routeValueDictionary != null);

            // Get all keys for which the corresponding value is 'Optional'.
            // Having a separate array is necessary so that we don't manipulate the dictionary while enumerating.
            // This is on a hot-path and linq expressions are showing up on the profile, so do array manipulation.
            int max = routeValueDictionary.Count;
            int i = 0;
            string[] matching = new string[max];
            foreach (KeyValuePair<string, object> kv in routeValueDictionary)
            {
                if (kv.Value == RouteParameter.Optional)
                {
                    matching[i] = kv.Key;
                    i++;
                }
            }
            for (int j = 0; j < i; j++)
            {
                string key = matching[j];
                routeValueDictionary.Remove(key);
            }
        }

        private ICorsPolicyProvider GetCorsPolicyProvider(HttpActionDescriptor actionDescriptor)
        {
            ICorsPolicyProvider policyProvider = null;
            if (actionDescriptor != null)
            {
                HttpControllerDescriptor controllerDescriptor = actionDescriptor.ControllerDescriptor;
                policyProvider = actionDescriptor.GetCustomAttributes<ICorsPolicyProvider>().FirstOrDefault();
                if (policyProvider == null && controllerDescriptor != null)
                {
                    policyProvider = controllerDescriptor.GetCustomAttributes<ICorsPolicyProvider>().FirstOrDefault();
                }
            }

            if (policyProvider == null)
            {
                policyProvider = DefaultPolicyProvider;
            }

            return policyProvider;
        }

        private static HttpActionDescriptor SelectAction(HttpRequestMessage request)
        {
            HttpConfiguration config = request.GetConfiguration();
            if (config == null)
            {
                throw new InvalidOperationException(SRResources.NoConfiguration);
            }

            IHttpRouteData routeData = config.Routes.GetRouteData(request);
            if (routeData == null)
            {
                throw new InvalidOperationException(SRResources.NoRouteData);
            }
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            RemoveOptionalRoutingParameters(routeData.Values);

            HttpControllerDescriptor controllerDescriptor = config.Services.GetHttpControllerSelector().SelectController(request);

            // Get the per-controller configuration
            config = controllerDescriptor.Configuration;
            request.SetConfiguration(config);
            HttpControllerContext controllerContext = new HttpControllerContext(config, routeData, request)
            {
                ControllerDescriptor = controllerDescriptor
            };

            return config.Services.GetActionSelector().SelectAction(controllerContext);
        }
    }
}