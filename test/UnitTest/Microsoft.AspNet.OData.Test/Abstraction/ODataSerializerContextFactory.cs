//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerContextFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test.Abstraction
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
        public static ODataSerializerContext Create(IEdmModel model, IEdmNavigationSource navigationSource, HttpRequestMessage request)
        {
            return new ODataSerializerContext { Model = model, NavigationSource = navigationSource, Request = request, Url = request.GetUrlHelper() };
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static ODataSerializerContext Create(IEdmModel model, IEdmNavigationSource navigationSource, ODataPath path, HttpRequestMessage request)
        {
            return new ODataSerializerContext { Model = model, NavigationSource = navigationSource, Path = path, Request = request, Url = request.GetUrlHelper() };
        }
    }
}
