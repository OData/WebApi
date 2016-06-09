﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Implementation of <see cref="ParameterBindingAttribute"/> used to bind an instance of <see cref="ODataPath"/> as an action parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ODataPathParameterBindingAttribute : Attribute
    {
        ///// <summary>
        ///// Gets the parameter binding.
        ///// </summary>
        ///// <param name="parameter">The parameter description.</param>
        ///// <returns>
        ///// The parameter binding.
        ///// </returns>
        //public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        //{
        //    return new ODataPathParameterBinding(parameter);
        //}

        //internal class ODataPathParameterBinding : HttpParameterBinding
        //{
        //    public ODataPathParameterBinding(HttpParameterDescriptor parameterDescriptor)
        //        : base(parameterDescriptor)
        //    {
        //    }

        //    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        //    public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        //    {
        //        if (actionContext == null)
        //        {
        //            throw Error.ArgumentNull("actionContext");
        //        }

        //        HttpRequestMessage request = actionContext.Request;

        //        if (request == null)
        //        {
        //            throw Error.Argument("actionContext", SRResources.ActionContextMustHaveRequest);
        //        }

        //        SetValue(actionContext, request.ODataProperties().Path);

        //        return TaskHelpers.Completed();
        //    }
        //}
    }
}
