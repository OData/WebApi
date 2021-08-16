//-----------------------------------------------------------------------------
// <copyright file="BindingPathHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder
{
    internal static class BindingPathHelper
    {
        /// <summary>
        /// Converts the binding path list to string. like "A.B/C/D.E".
        /// </summary>
        /// <param name="bindingPath">The binding path list.</param>
        /// <returns>The binding path string. like "A.B/C/D.E".</returns>
        public static string ConvertBindingPath(this IEnumerable<MemberInfo> bindingPath)
        {
            if (bindingPath == null)
            {
                throw Error.ArgumentNull("bindingPath");
            }

            return String.Join("/", bindingPath.Select(e => TypeHelper.GetQualifiedName(e)));
        }
    }
}
