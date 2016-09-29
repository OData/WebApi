// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Builder
{
    internal static class BindingPathHelper
    {
        /// <summary>
        /// Converts the binding path list to string. like "A.B/C/D.E".
        /// </summary>
        /// <param name="bindingPath">The binding path list.</param>
        /// <returns>The binding path string. like "A.B/C/D.E".</returns>
        public static string ConvertBindingPath(this IEnumerable<object> bindingPath)
        {
            if (bindingPath == null)
            {
                throw Error.ArgumentNull("bindingPath");
            }

            return String.Join("/", bindingPath.Select(e => e.GetQualifiedName()));
        }

        private static string GetQualifiedName(this object memberInfo)
        {
            Contract.Assert(memberInfo != null);

            Type type = memberInfo as Type;
            if (type != null)
            {
                return (type.Namespace + "." + type.Name);
            }

            MemberInfo memInfo = memberInfo as MemberInfo;
            if (memInfo != null)
            {
                return memInfo.Name;
            }

            return String.Empty;
        }
    }
}
