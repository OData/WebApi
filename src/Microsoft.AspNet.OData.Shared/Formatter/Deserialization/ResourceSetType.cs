//-----------------------------------------------------------------------------
// <copyright file="ResourceSetType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Specifies the type of resource set.
    /// </summary>
    internal enum ResourceSetType
    {
        /// <summary>
        /// A normal resource set.
        /// </summary>
        ResourceSet,

        /// <summary>
        /// A delta resource set.
        /// </summary>
        DeltaResourceSet
    }
}
