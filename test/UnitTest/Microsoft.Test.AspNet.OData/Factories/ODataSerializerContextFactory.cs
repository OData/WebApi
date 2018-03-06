// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;
#else
using System.Net.Http;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A class to create ODataSerializerContext.
    /// </summary>
    public class ODataSerializerContextFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if NETCORE
        public static ODataSerializerContext Create(IEdmModel model, IEdmNavigationSource navigationSource, HttpRequest request)
        {
            return new ODataSerializerContext { Model = model, NavigationSource = navigationSource, Request = request };
        }
#else
        public static ODataSerializerContext Create(IEdmModel model, IEdmNavigationSource navigationSource, HttpRequestMessage request)
        {
            return new ODataSerializerContext { Model = model, NavigationSource = navigationSource, Request = request, Url = request.GetUrlHelper() };
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if NETCORE
        public static ODataSerializerContext Create(IEdmModel model, IEdmNavigationSource navigationSource, ODataPath path, HttpRequest request)
        {
            return new ODataSerializerContext { Model = model, NavigationSource = navigationSource, Path = path, Request = request };
        }
#else
        public static ODataSerializerContext Create(IEdmModel model, IEdmNavigationSource navigationSource, ODataPath path, HttpRequestMessage request)
        {
            return new ODataSerializerContext { Model = model, NavigationSource = navigationSource, Path = path, Request = request, Url = request.GetUrlHelper() };
        }
#endif
    }
}
