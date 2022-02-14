//-----------------------------------------------------------------------------
// <copyright file="ODataPathParameterBindingAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Implementation of <see cref="ParameterBindingAttribute"/> used to bind an instance of <see cref="ODataPath"/> as an action parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed partial class ODataPathParameterBindingAttribute : ParameterBindingAttribute
    {
        /// <summary>
        /// Gets the parameter binding.
        /// </summary>
        /// <param name="parameter">The parameter description.</param>
        /// <returns>
        /// The parameter binding.
        /// </returns>
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            return new ODataPathParameterBinding(parameter);
        }

        internal class ODataPathParameterBinding : HttpParameterBinding
        {
            public ODataPathParameterBinding(HttpParameterDescriptor parameterDescriptor)
                : base(parameterDescriptor)
            {
            }

            public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
            {
                if (actionContext == null)
                {
                    throw Error.ArgumentNull("actionContext");
                }

                HttpRequestMessage request = actionContext.Request;

                if (request == null)
                {
                    throw Error.Argument("actionContext", SRResources.ActionContextMustHaveRequest);
                }

                SetValue(actionContext, request.ODataProperties().Path);

                return TaskHelpers.Completed();
            }
        }
    }
}
