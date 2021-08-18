//-----------------------------------------------------------------------------
// <copyright file="SingletonAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a property to specify
    /// that the property must bind to a singleton. It's used in convention model builder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SingletonAttribute : Attribute
    {
    }
}
