//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerContextFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
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
        public static ODataSerializerContext Create(IEdmModel model, IEdmNavigationSource navigationSource, HttpRequest request)
        {
            return new ODataSerializerContext { Model = model, NavigationSource = navigationSource, Request = request };
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static ODataSerializerContext Create(IEdmModel model, IEdmNavigationSource navigationSource, ODataPath path, HttpRequest request)
        {
            return new ODataSerializerContext { Model = model, NavigationSource = navigationSource, Path = path, Request = request };
        }
    }
}
