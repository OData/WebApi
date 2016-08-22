// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http;

namespace System.Web.OData.Builder
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

            return String.Join("/", bindingPath.Select(e => e.GetQualifiedName()));
        }

        private static string GetQualifiedName(this MemberInfo memberInfo)
        {
            Contract.Assert(memberInfo != null);

            Type type = memberInfo as Type;
            return type != null ? (type.Namespace + "." + type.Name) : memberInfo.Name;
        }
    }
}
