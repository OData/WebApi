// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer to wrap a <see cref="FormatterParameterBinding"/>.
    /// Its primary purpose is to intercept binding requests so that it can create tracers for the formatters.
    /// </summary>
    internal class FormatterParameterBindingTracer : FormatterParameterBinding
    {
        private const string ExecuteBindingAsyncMethodName = "ExecuteBindingAsync";

        private FormatterParameterBinding _innerBinding;
        private ITraceWriter _traceWriter;

        public FormatterParameterBindingTracer(FormatterParameterBinding innerBinding, ITraceWriter traceWriter) : base(innerBinding.Descriptor, innerBinding.Formatters, innerBinding.BodyModelValidator)
        {
            _innerBinding = innerBinding;
            _traceWriter = traceWriter;
        }

        protected override Task<object> ReadContentAsync(HttpRequestMessage request, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            // Intercept this method solely to wrap request-knowledgable formatter tracers
            return base.ReadContentAsync(request, type, CreateFormatterTracers(request, formatters), formatterLogger);
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync(
                actionContext.Request,
                TraceCategories.ModelBindingCategory,
                TraceLevel.Info,
                _innerBinding.GetType().Name,
                ExecuteBindingAsyncMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(SRResources.TraceBeginParameterBind,
                                              _innerBinding.Descriptor.ParameterName);
                },

                execute: () => _innerBinding.ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken),

                endTrace: (tr) =>
                {
                    string parameterName = _innerBinding.Descriptor.ParameterName;
                    tr.Message = actionContext.ActionArguments.ContainsKey(parameterName)
                                    ? Error.Format(SRResources.TraceEndParameterBind, parameterName,
                                                    FormattingUtilities.ValueToString(actionContext.ActionArguments[parameterName], CultureInfo.CurrentCulture))
                                    : Error.Format(SRResources.TraceEndParameterBindNoBind,
                                                    parameterName);
                },
                errorTrace: null);
        }

        private IEnumerable<MediaTypeFormatter> CreateFormatterTracers(HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            List<MediaTypeFormatter> formatterTracers = new List<MediaTypeFormatter>();
            foreach (MediaTypeFormatter formatter in formatters)
            {
                formatterTracers.Add(MediaTypeFormatterTracer.CreateTracer(formatter, _traceWriter, request));
            }

            return formatterTracers;
        }
    }
}
