//-----------------------------------------------------------------------------
// <copyright file="MediaTypeAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Marks this entity type as media type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MediaTypeAttribute : Attribute
    {
    }
}
