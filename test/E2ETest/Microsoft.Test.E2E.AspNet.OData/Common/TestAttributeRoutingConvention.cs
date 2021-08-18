//-----------------------------------------------------------------------------
// <copyright file="TestAttributeRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public class TestAttributeRoutingConvention : AttributeRoutingConvention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use for figuring out all the controllers to
        /// look for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        /// <remarks>
        /// While this function does not use types that are AspNetCore-specific,
        /// the functionality is due to the way assembly resolution is done in AspNet vs AspnetCore.
        /// </remarks>
        public TestAttributeRoutingConvention(string routeName, IServiceProvider serviceProvider)
            : base(routeName, serviceProvider)
        {
            this.Controllers = new List<Type>();
        }

        /// <summary>
        /// Gets a list of supported controllers.
        /// </summary>
        public IList<Type> Controllers { get; private set; }

        /// <inheritdocs>
        public override bool ShouldMapController(ControllerActionDescriptor controllerAction)
        {
            return this.Controllers.Contains(controllerAction.ControllerTypeInfo.AsType());
        }
    }
}
#endif
