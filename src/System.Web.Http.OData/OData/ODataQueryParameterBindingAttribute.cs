// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ODataQueryParameterBindingAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            return new ODataQueryParameterBinding(parameter);
        }

        internal class ODataQueryParameterBinding : HttpParameterBinding
        {
            public ODataQueryParameterBinding(HttpParameterDescriptor parameterDescriptor)
                : base(parameterDescriptor)
            {
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
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

                HttpActionDescriptor actionDescriptor = actionContext.ActionDescriptor;

                if (actionDescriptor == null)
                {
                    throw Error.Argument("actionContext", SRResources.ActionContextMustHaveDescriptor);
                }

                HttpConfiguration configuration = request.GetConfiguration();

                if (configuration == null)
                {
                    throw Error.Argument("actionContext", SRResources.RequestMustContainConfiguration);
                }

                IEdmModel model = configuration.GetEdmModel();

                if (actionDescriptor.ReturnType == null)
                {
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                                 Error.Format(SRResources.FailedToBuildEdmModelBecauseReturnTypeIsNull,
                               actionDescriptor.ActionName, actionDescriptor.ControllerDescriptor.ControllerName)));
                }

                Type entityClrType = TypeHelper.GetImplementedIEnumerableType(actionDescriptor.ReturnType);

                if (entityClrType == null)
                {
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                           Error.Format(SRResources.FailedToRetrieveTypeToBuildEdmModel, actionDescriptor.ReturnType.FullName,
                               actionDescriptor.ActionName, actionDescriptor.ControllerDescriptor.ControllerName)));
                }

                if (model == null)
                {
                    model = actionDescriptor.GetEdmModel(entityClrType);
                }

                ODataQueryOptions parameterValue = null;
                ODataQueryContext entitySetContext = new ODataQueryContext(model, entityClrType);

                try
                {
                    parameterValue = new ODataQueryOptions(entitySetContext, request);
                    SetValue(actionContext, parameterValue);
                }
                catch (ODataException exception)
                {
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest, exception));
                }

                return TaskHelpers.FromResult(0);
            }
        }
    }
}
