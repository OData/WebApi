//-----------------------------------------------------------------------------
// <copyright file="NotSortableAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a property to specify that 
    /// the property cannot be used in the $orderby OData query option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotSortableAttribute : Attribute
    {
    }
}
