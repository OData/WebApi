//-----------------------------------------------------------------------------
// <copyright file="ActionOnDeleteAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a navigation property to specify the applied
    /// action whether delete should also remove the associated item on the other end of the association.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ActionOnDeleteAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionOnDeleteAttribute"/> class.
        /// </summary>
        /// <param name="onDeleteAction">The action applied on delete.</param>
        public ActionOnDeleteAttribute(EdmOnDeleteAction onDeleteAction)
        {
            OnDeleteAction = onDeleteAction;
        }

        /// <summary>
        /// Gets the action whether delete should also remove the associated item on the other end of the association.
        /// </summary>
        public EdmOnDeleteAction OnDeleteAction { get; private set; }
    }
}
