//-----------------------------------------------------------------------------
// <copyright file="ControllerDescriptorFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create [Http]ControllerDescriptor.
    /// </summary>
    public class ControllerDescriptorFactory
    {
        /// <summary>
        /// Initializes a new instance of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new instance of the [Http]ControllerDescriptor  class.</returns>
        public static HttpControllerDescriptor Create()
        {
            return new HttpControllerDescriptor();
        }

        /// <summary>
        /// Initializes a new instance of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new instance of the [Http]ControllerDescriptor  class.</returns>
        public static IEnumerable<HttpControllerDescriptor> Create(HttpConfiguration configuration, string name, Type controllerType)
        {
            return new[] { new HttpControllerDescriptor(configuration, name, controllerType) };
        }

        /// <summary>
        /// Initializes a new collection of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new collection of the [Http]ControllerDescriptor  class.</returns>
        public static IEnumerable<HttpControllerDescriptor> CreateCollection()
        {
            return new HttpControllerDescriptor[0];
        }
    }
}
