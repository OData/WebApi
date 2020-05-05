// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Defines a contract to generate absolute and related URIs based on OData endpoint routing.
    /// </summary>
    internal class ODataEndpointLinkGenerator : LinkGenerator
    {
        private static readonly string _escapedHashMark = Uri.EscapeDataString("#");
        private static readonly string _escapedQuestionMark = Uri.EscapeDataString("?");

        private LinkGenerator _innerGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEndpointLinkGenerator"/> class.
        /// </summary>
        /// <param name="generator">The inner Link generator</param>
        public ODataEndpointLinkGenerator(LinkGenerator generator)
        {
            if (generator == null)
            {
                throw Error.ArgumentNull(nameof(generator));
            }

            _innerGenerator = generator;
        }

        /// <inheritdoc/>
        public override string GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary ambientValues = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions options = null)
        {
            object odataPathValue;
            if (values.TryGetValue("odataPath", out odataPathValue))
            {
                string odataPath = odataPathValue as string;
                if (odataPath != null)
                {
                    IPerRouteContainer perRouteContainer = httpContext.RequestServices.GetRequiredService<IPerRouteContainer>();
                    string routePrefix = perRouteContainer.GetRoutePrefix(httpContext.Request.ODataFeature().RouteName);

                    bool canGenerateDirectLink = routePrefix == null || routePrefix.IndexOf('{') == -1;
                    if (!canGenerateDirectLink)
                    {
                        routePrefix = BindPrefixTemplate(routePrefix, values, ambientValues);
                    }
                    string link = CombinePathSegments(routePrefix, odataPath);
                    link = UriEncode(link);

                    // A workaround to include the PathBase, a good solution is to use ASP.NET Core provided the APIs
                    // at https://github.com/dotnet/aspnetcore/blob/master/src/Http/Http.Extensions/src/UriHelper.cs#L48
                    // to build the absolute Uri. But, here only needs the "PathBase + Path (without OData path)",
                    HttpRequest request = httpContext.Request;
                    if (request != null && request.PathBase != null && request.PathBase.HasValue)
                    {
                        return request.PathBase.Value + "/" + link;
                    }

                    return link;
                }
            }

            return _innerGenerator.GetPathByAddress(httpContext, address, values, ambientValues, pathBase, fragment, options);
        }

        /// <inheritdoc/>
        public override string GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = default, FragmentString fragment = default, LinkOptions options = null)
        {
            return _innerGenerator.GetPathByAddress<TAddress>(address, values, pathBase, fragment, options);
        }

        /// <inheritdoc/>
        public override string GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary ambientValues = null, string scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions options = null)
        {
            return _innerGenerator.GetUriByAddress(httpContext, address, values, ambientValues, scheme, host, pathBase, fragment, options);
        }

        /// <inheritdoc/>
        public override string GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions options = null)
        {
            return _innerGenerator.GetUriByAddress(address, values, scheme, host, pathBase, fragment, options);
        }

        // Noted: simple workaround to bind the value for the prefix template
        // Should replace it using the standard ASP.NET Core way later.
        internal static string BindPrefixTemplate(string prefix, RouteValueDictionary values, RouteValueDictionary ambientValues)
        {
            IList<string> templates = new List<string>();
            int startIndex = 0;
            while (true)
            {
                int start = prefix.IndexOf('{', startIndex);
                int end = prefix.IndexOf('}', start + 1); // start == -1 is ok
                if (start == -1 || end == -1)
                {
                    break;
                }

                startIndex = end + 1;

                string subStr = prefix.Substring(start, end - start + 1);
                templates.Add(subStr);
            }

            foreach (var item in templates)
            {
                string variable = item.Substring(1, item.Length - 2); // remove { and }

                if (values != null && values.TryGetValue(variable, out object valueFromValues))
                {
                    prefix = prefix.Replace(item, valueFromValues.ToString());
                }
                else if (ambientValues != null && ambientValues.TryGetValue(variable, out object valueFromAmbient))
                {
                    prefix = prefix.Replace(item, valueFromAmbient.ToString());
                }
            }

            return prefix;
        }

        private static string CombinePathSegments(string routePrefix, string odataPath)
        {
            if (String.IsNullOrEmpty(routePrefix))
            {
                return odataPath;
            }
            else
            {
                return String.IsNullOrEmpty(odataPath) ? routePrefix : routePrefix + '/' + odataPath;
            }
        }

        private static string UriEncode(string str)
        {
            string escape = Uri.EscapeUriString(str);
            escape = escape.Replace("#", _escapedHashMark);
            escape = escape.Replace("?", _escapedQuestionMark);
            return escape;
        }
    }
}
#endif
