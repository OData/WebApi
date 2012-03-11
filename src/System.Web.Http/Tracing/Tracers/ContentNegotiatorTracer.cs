using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.Common;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IContentNegotiator"/>.
    /// </summary>
    internal class ContentNegotiatorTracer : IContentNegotiator
    {
        private const string NegotiateMethodName = "Negotiate";

        private readonly IContentNegotiator _innerNegotiator;
        private readonly ITraceWriter _traceWriter;

        public ContentNegotiatorTracer(IContentNegotiator innerNegotiator, ITraceWriter traceWriter)
        {
            _innerNegotiator = innerNegotiator;
            _traceWriter = traceWriter;
        }

        public MediaTypeFormatter Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters, out MediaTypeHeaderValue mediaType)
        {
            mediaType = null;
            MediaTypeHeaderValue selectedMediaType = null;
            MediaTypeFormatter selectedFormatter = null;

            _traceWriter.TraceBeginEnd(
                request,
                TraceCategories.FormattingCategory,
                TraceLevel.Info,
                _innerNegotiator.GetType().Name,
                NegotiateMethodName,

                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                        SRResources.TraceNegotiateFormatter,
                        type.Name,
                        FormattingUtilities.FormattersToString(formatters));
                },

                execute: () =>
                {
                    selectedFormatter = _innerNegotiator.Negotiate(type, request, formatters, out selectedMediaType);
                },

                endTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                        SRResources.TraceSelectedFormatter,
                        selectedFormatter == null
                            ? SRResources.TraceNoneObjectMessage
                            : MediaTypeFormatterTracer.ActualMediaTypeFormatter(selectedFormatter).GetType().Name,
                        selectedMediaType == null
                            ? SRResources.TraceNoneObjectMessage
                            : selectedMediaType.ToString());
                },

        errorTrace: null);

            mediaType = selectedMediaType;

            if (selectedFormatter != null)
            {
                selectedFormatter = MediaTypeFormatterTracer.CreateTracer(selectedFormatter, _traceWriter, request);
            }

            return selectedFormatter;
        }
    }
}
