//-----------------------------------------------------------------------------
// <copyright file="IEdmDeltaLinkBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmChangedObject"/>.
    /// Holds the properties necessary to create either ODataDeltaLink or ODataDeltaDeletedLink.
    /// </summary>
    public interface IEdmDeltaLinkBase : IEdmChangedObject
    {
        /// <summary>
        /// The Uri of the entity from which the relationship is defined, which may be absolute or relative.
        /// </summary>
        Uri Source { get; set; }

        /// <summary>
        /// The Uri of the related entity, which may be absolute or relative.
        /// </summary>
        Uri Target { get; set; }

        /// <summary>
        /// The name of the relationship property on the parent object.
        /// </summary>
        string Relationship { get; set; }
    }
}
