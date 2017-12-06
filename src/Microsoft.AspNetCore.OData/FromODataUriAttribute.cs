// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Formatter;

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
