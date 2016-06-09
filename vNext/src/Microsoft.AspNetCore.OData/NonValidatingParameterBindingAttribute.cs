// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// An attribute to disable WebApi model validation for a particular type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal sealed class NonValidatingParameterBindingAttribute : Attribute
    {
        // https://github.com/aspnet/Mvc/issues/38

        //public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        //{
        //    IEnumerable<MediaTypeFormatter> formatters = parameter.Configuration.Formatters;

        //    return new NonValidatingParameterBinding(parameter, formatters);
        //}

        //private sealed class NonValidatingParameterBinding : PerRequestParameterBinding
        //{
        //    public NonValidatingParameterBinding(HttpParameterDescriptor descriptor,
        //        IEnumerable<MediaTypeFormatter> formatters)
        //        : base(descriptor, formatters)
        //    {
        //    }

        //    protected override HttpParameterBinding CreateInnerBinding(IEnumerable<MediaTypeFormatter> perRequestFormatters)
        //    {
        //        return Descriptor.BindWithFormatter(perRequestFormatters, bodyModelValidator: null);
        //    }
        //}
    }
}
