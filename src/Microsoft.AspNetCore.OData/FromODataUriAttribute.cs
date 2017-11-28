// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An implementation of <see cref="ParameterBindingAttribute"/> that can bind URI parameters using OData conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class FromODataUriAttribute : ModelBinderAttribute
    {
        /// <inheritdoc/>
        public override BindingSource BindingSource
        {
            get
            {
                throw new NotImplementedException();
            }

            protected set
            {
                throw new NotImplementedException();
            }
        }
    }
}
