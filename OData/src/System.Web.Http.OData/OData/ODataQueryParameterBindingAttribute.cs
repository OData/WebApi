// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// A <see cref="ParameterBindingAttribute"/> to bind parameters of type <see cref="ODataQueryOptions"/> to the OData query from the incoming request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ODataQueryParameterBindingAttribute : ParameterBindingAttribute
    {
        /// <inheritdoc />
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            return new ODataQueryParameterBinding(parameter);
        }

        internal class ODataQueryParameterBinding : HttpParameterBinding
        {
            private static MethodInfo _createODataQueryOptions = typeof(ODataQueryParameterBinding).GetMethod("CreateODataQueryOptions");
            private const string CreateODataQueryOptionsCtorKey = "MS_CreateODataQueryOptionsOfT";

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

                // Get the entity type from the parameter type if it is ODataQueryOptions<T>.
                // Fall back to the return type if not. Also, note that the entity type from the return type and ODataQueryOptions<T> 
                // can be different (example implementing $select or $expand).
                Type entityClrType = GetEntityClrTypeFromParameterType(Descriptor) ?? GetEntityClrTypeFromActionReturnType(actionDescriptor);

                IEdmModel model = request.ODataProperties().Model ?? actionDescriptor.GetEdmModel(entityClrType);
                ODataQueryContext entitySetContext = new ODataQueryContext(model, entityClrType);

                Func<ODataQueryContext, HttpRequestMessage, ODataQueryOptions> createODataQueryOptions =
                    (Func<ODataQueryContext, HttpRequestMessage, ODataQueryOptions>)Descriptor.Properties.GetOrAdd(CreateODataQueryOptionsCtorKey, _ =>
                    {
                        return Delegate.CreateDelegate(typeof(Func<ODataQueryContext, HttpRequestMessage, ODataQueryOptions>), _createODataQueryOptions.MakeGenericMethod(entityClrType));
                    });

                ODataQueryOptions parameterValue = createODataQueryOptions(entitySetContext, request);
                SetValue(actionContext, parameterValue);

                return TaskHelpers.Completed();
            }

            public static ODataQueryOptions<T> CreateODataQueryOptions<T>(ODataQueryContext context, HttpRequestMessage request)
            {
                return new ODataQueryOptions<T>(context, request);
            }

            internal static Type GetEntityClrTypeFromActionReturnType(HttpActionDescriptor actionDescriptor)
            {
                // It is a developer programming error to use this binding attribute
                // on actions that return void.
                if (actionDescriptor.ReturnType == null)
                {
                    throw Error.InvalidOperation(
                                    SRResources.FailedToBuildEdmModelBecauseReturnTypeIsNull,
                                    actionDescriptor.ActionName,
                                    actionDescriptor.ControllerDescriptor.ControllerName);
                }

                Type entityClrType = TypeHelper.GetImplementedIEnumerableType(actionDescriptor.ReturnType);

                if (entityClrType == null)
                {
                    // It is a developer programming error to use this binding attribute
                    // on actions that return a collection whose element type cannot be
                    // determined, such as a non-generic IQueryable or IEnumerable.
                    throw Error.InvalidOperation(
                                    SRResources.FailedToRetrieveTypeToBuildEdmModel,
                                    actionDescriptor.ActionName,
                                    actionDescriptor.ControllerDescriptor.ControllerName,
                                    actionDescriptor.ReturnType.FullName);
                }

                return entityClrType;
            }

            internal static Type GetEntityClrTypeFromParameterType(HttpParameterDescriptor parameterDescriptor)
            {
                Contract.Assert(parameterDescriptor != null);

                Type parameterType = parameterDescriptor.ParameterType;
                Contract.Assert(parameterType != null);

                if (parameterType.IsGenericType &&
                    parameterType.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>))
                {
                    return parameterType.GetGenericArguments().Single();
                }

                return null;
            }
        }
    }
}
