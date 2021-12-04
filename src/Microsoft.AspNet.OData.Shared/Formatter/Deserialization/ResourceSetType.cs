//-----------------------------------------------------------------------------
// <copyright file="ResourceSetType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Enum to determine the type of Resource Set
    /// </summary>
    internal enum ResourceSetType
    {
        /// <summary>
        /// A normal ResourceSet
        /// </summary>
        ResourceSet,
		
        /// <summary>
        /// A Delta Resource Set
        /// </summary>
        DeltaResourceSet
    }
}
