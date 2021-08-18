//-----------------------------------------------------------------------------
// <copyright file="IEdmDeltaDeletedLink.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmChangedObject"/>.
    /// Holds the properties necessary to create the ODataDeltaDeletedLink.
    /// </summary>
    public interface IEdmDeltaDeletedLink : IEdmDeltaLinkBase, IEdmChangedObject
    {
    }
}
