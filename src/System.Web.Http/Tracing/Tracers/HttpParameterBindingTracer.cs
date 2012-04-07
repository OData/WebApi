// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer to wrap an <see cref="HttpParameterBinding"/>.
    /// Its primary purpose is to monitor <see cref="ExecuteBindingAsync"/>.
    /// </summary>
    internal class HttpParameterBindingTracer : HttpParameterBinding
    {
        private const string ExecuteBindingAsyncMethodName = "ExecuteBindingAsync";

        public HttpParameterBindingTracer(HttpParameterBinding innerBinding, ITraceWriter traceWriter) : base(innerBinding.Descriptor)
        {
            InnerBinding = innerBinding;
            TraceWriter = traceWriter;
        }

        protected HttpParameterBinding InnerBinding { get; private set; }

        protected ITraceWriter TraceWriter { get; private set; }

        public override string ErrorMessage
        {
            get
            {
                return InnerBinding.ErrorMessage;
            }
        }

        public override bool WillReadBody
        {
            get
            {
                return InnerBinding.WillReadBody;
            }
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return TraceWriter.TraceBeginEndAsync(
                actionContext.Request,
                TraceCategories.ModelBindingCategory,
                TraceLevel.Info,
                InnerBinding.GetType().Name,
                ExecuteBindingAsyncMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(SRResources.TraceBeginParameterBind,
                                              InnerBinding.Descriptor.ParameterName);
                },

                execute: () => InnerBinding.ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken),

                endTrace: (tr) =>
                {
                    string parameterName = InnerBinding.Descriptor.ParameterName;

                    // Model binding error for this parameter shows the error
                    if (!actionContext.ModelState.IsValid && actionContext.ModelState.ContainsKey(parameterName))
                    {
                        tr.Message = Error.Format(SRResources.TraceModelStateInvalidMessage,
                                                  FormattingUtilities.ModelStateToString(
                                                       actionContext.ModelState));
                    }
                    else
                    {
                        tr.Message = actionContext.ActionArguments.ContainsKey(parameterName)
                                         ? Error.Format(SRResources.TraceEndParameterBind, parameterName,
                                                        FormattingUtilities.ValueToString(
                                                             actionContext.ActionArguments[parameterName],
                                                             CultureInfo.CurrentCulture))
                                         : Error.Format(SRResources.TraceEndParameterBindNoBind,
                                                        parameterName);
                    }
                },
                errorTrace: null);
        }
    }
}
