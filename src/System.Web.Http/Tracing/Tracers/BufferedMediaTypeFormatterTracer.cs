// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.Properties;
using System.Web.Http.Services;

namespace System.Web.Http.Tracing.Tracers
{
    internal class BufferedMediaTypeFormatterTracer : BufferedMediaTypeFormatter, IFormatterTracer, IDecorator<BufferedMediaTypeFormatter>
    {
        private const string OnReadFromStreamMethodName = "ReadFromStream";
        private const string OnWriteToStreamMethodName = "WriteToStream";

        private readonly BufferedMediaTypeFormatter _inner;
        private MediaTypeFormatterTracer _innerTracer;

        public BufferedMediaTypeFormatterTracer(BufferedMediaTypeFormatter innerFormatter, ITraceWriter traceWriter, HttpRequestMessage request)
            : base(innerFormatter)
        {
            _inner = innerFormatter;
            _innerTracer = new MediaTypeFormatterTracer(innerFormatter, traceWriter, request);
        }

        HttpRequestMessage IFormatterTracer.Request
        {
            get { return _innerTracer.Request; }
        }

        public BufferedMediaTypeFormatter Inner
        {
            get { return _inner; }
        }

        public MediaTypeFormatter InnerFormatter
        {
            get { return _innerTracer.InnerFormatter; }
        }

        public override IRequiredMemberSelector RequiredMemberSelector
        {
            get
            {
                return _innerTracer.RequiredMemberSelector;
            }
            set
            {
                _innerTracer.RequiredMemberSelector = value;
            }
        }

        public override bool CanReadType(Type type)
        {
            return _innerTracer.CanReadType(type);
        }

        public override bool CanWriteType(Type type)
        {
            return _innerTracer.CanWriteType(type);
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            return _innerTracer.GetPerRequestFormatterInstance(type, request, mediaType);
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            _innerTracer.SetDefaultContentHeaders(type, headers, mediaType);
        }

        public override object ReadFromStream(Type type, Stream stream, HttpContent content, IFormatterLogger formatterLogger)
        {
            BufferedMediaTypeFormatter innerFormatter = InnerFormatter as BufferedMediaTypeFormatter;
            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
            MediaTypeHeaderValue contentType = contentHeaders == null ? null : contentHeaders.ContentType;
            object value = null;

            _innerTracer.TraceWriter.TraceBeginEnd(
                _innerTracer.Request,
                TraceCategories.FormattingCategory,
                TraceLevel.Info,
                _innerTracer.InnerFormatter.GetType().Name,
                OnReadFromStreamMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                            SRResources.TraceReadFromStreamMessage,
                            type.Name,
                            contentType == null ? SRResources.TraceNoneObjectMessage : contentType.ToString());
                },
                execute: () =>
                {
                    value = innerFormatter.ReadFromStream(type, stream, content, formatterLogger);
                },
                endTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                SRResources.TraceReadFromStreamValueMessage,
                                FormattingUtilities.ValueToString(value, CultureInfo.CurrentCulture));
                },
                errorTrace: null);

            return value;
        }

        public override void WriteToStream(Type type, object value, Stream stream, HttpContent content)
        {
            BufferedMediaTypeFormatter innerFormatter = InnerFormatter as BufferedMediaTypeFormatter;

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;
            MediaTypeHeaderValue contentType = contentHeaders == null
                           ? null
                           : contentHeaders.ContentType;

            _innerTracer.TraceWriter.TraceBeginEnd(
                _innerTracer.Request,
                TraceCategories.FormattingCategory,
                TraceLevel.Info,
                _innerTracer.InnerFormatter.GetType().Name,
                OnWriteToStreamMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                            SRResources.TraceWriteToStreamMessage,
                            FormattingUtilities.ValueToString(value, CultureInfo.CurrentCulture),
                            type.Name,
                            contentType == null ? SRResources.TraceNoneObjectMessage : contentType.ToString());
                },
                execute: () =>
                {
                    innerFormatter.WriteToStream(type, value, stream, content);
                },
                endTrace: null,
                errorTrace: null);
        }
    }
}
