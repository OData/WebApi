//-----------------------------------------------------------------------------
// <copyright file="TestActionDescriptorCollectionProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// Test version of an <see cref="IActionDescriptorCollectionProvider"/>
    /// </summary>
    internal class TestActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
    {
        /// <summary>
        /// A list of <see cref="ActionDescriptor"/> that we can modify.
        /// </summary>
        public List<ActionDescriptor> TestActionDescriptors { get; } = new List<ActionDescriptor>();

        /// <summary>
        /// The action descriptors collection from <see cref="IActionDescriptorCollectionProvider"/>
        /// which is immutable.
        /// </summary>
        public ActionDescriptorCollection ActionDescriptors
        {
            get
            {
                return new ActionDescriptorCollection(TestActionDescriptors, 0);
            }
        }
    }
}
