// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An attribute to disable WebApi model validation for a particular type.
    /// </summary>
    /// <remarks>
    /// This is essentially a <see cref="ValidateNeverAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed partial class NonValidatingParameterBindingAttribute : ModelBinderAttribute, IPropertyValidationFilter
    {
        /// <inheritdoc />
        public bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry)
        {
            return false;
        }

        /// <inheritdoc/>
        public override BindingSource BindingSource
        {
            get
            {
                return BindingSource.Body;
            }
        }
    }
}
