// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Mvc
{
    /// <summary>
    /// An optional interface for <see cref="ActionDescriptor" /> types which provide a <see cref="MethodInfo" />.
    /// </summary>
    public interface IMethodInfoActionDescriptor
    {
        /// <summary>
        /// Gets the MethodInfo
        /// </summary>
        MethodInfo MethodInfo
        {
            get;
        }
    }
}
