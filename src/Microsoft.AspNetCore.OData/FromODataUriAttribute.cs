//-----------------------------------------------------------------------------
// <copyright file="FromODataUriAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An implementation of <see cref="ModelBinderAttribute"/> that can bind URI parameters using OData conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class FromODataUriAttribute : ModelBinderAttribute
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="FromODataUriAttribute"/> class.
        /// </summary>
        public FromODataUriAttribute()
            : base(typeof(ODataModelBinder))
        {
        }
    }
}
