// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer to monitor <see cref="MediaTypeFormatter"/> instances.
    /// </summary>
    internal class MediaTypeFormatterTracer : MediaTypeFormatter, IFormatterTracer
    {
        private const string ReadFromStreamAsyncMethodName = "ReadFromStreamAsync";
        private const string WriteToStreamAsyncMethodName = "WriteToStreamAsync";
        private const string GetPerRequestFormatterInstanceMethodName = "GetPerRequestFormatterInstance";

        public MediaTypeFormatterTracer(MediaTypeFormatter innerFormatter, ITraceWriter traceWriter, HttpRequestMessage request)
        {
            InnerFormatter = innerFormatter;
            TraceWriter = traceWriter;
            Request = request;
        }

        public MediaTypeFormatter InnerFormatter { get; private set; }

        public ITraceWriter TraceWriter { get; set; }

        public HttpRequestMessage Request { get; set; }

        public static MediaTypeFormatter ActualMediaTypeFormatter(MediaTypeFormatter formatter)
        {
            IFormatterTracer tracer = formatter as IFormatterTracer;
            return tracer == null ? formatter : tracer.InnerFormatter;
        }

        public static MediaTypeFormatter CreateTracer(MediaTypeFormatter formatter, ITraceWriter traceWriter, HttpRequestMessage request)
        {
            // If we have been asked to wrap a tracer around a formatter, it could be
            // already wrapped, and there is nothing to do.  But if we see it is a tracer
            // that is not associated with a request, we wrap it into a new tracer that
            // does have a request.  The only formatter tracers without requests are the
            // ones in the default MediaTypeFormatterCollection in the HttpConfiguration.
            IFormatterTracer formatterTracer = formatter as IFormatterTracer;
            if (formatterTracer != null)
            {
                if (formatterTracer.Request == request)
                {
                    return formatter;
                }

                formatter = formatterTracer.InnerFormatter;
            }

            MediaTypeFormatter tracer = null;

            // We special-case Xml, Json and FormUrlEncoded formatters because we expect to be able
            // to find them with IsAssignableFrom in the MediaTypeFormatterCollection.
            if (formatter is XmlMediaTypeFormatter)
            {
                tracer = new XmlMediaTypeFormatterTracer(formatter, traceWriter, request);
            }
            else if (formatter is JsonMediaTypeFormatter)
            {
                tracer = new JsonMediaTypeFormatterTracer(formatter, traceWriter, request);
            }
            else if (formatter is FormUrlEncodedMediaTypeFormatter)
            {
                tracer = new FormUrlEncodedMediaTypeFormatterTracer(formatter, traceWriter, request);
            }
            else if (formatter is BufferedMediaTypeFormatter)
            {
                tracer = new BufferedMediaTypeFormatterTracer(formatter, traceWriter, request);
            }
            else
            {
                tracer = new MediaTypeFormatterTracer(formatter, traceWriter, request);
            }

            // Copy SupportedMediaTypes and MediaTypeMappings and SupportedEncodings because they are publically visible
            tracer.SupportedMediaTypes.Clear();
            foreach (MediaTypeHeaderValue mediaType in formatter.SupportedMediaTypes)
            {
                tracer.SupportedMediaTypes.Add(mediaType);
            }

            tracer.MediaTypeMappings.Clear();
            foreach (MediaTypeMapping mapping in formatter.MediaTypeMappings)
            {
                tracer.MediaTypeMappings.Add(mapping);
            }

            tracer.SupportedEncodings.Clear();
            foreach (var encoding in formatter.SupportedEncodings)
            {
                tracer.SupportedEncodings.Add(encoding);
            }

            // Copy IRequiredMemberSelector
            tracer.RequiredMemberSelector = formatter.RequiredMemberSelector;

            return tracer;
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            MediaTypeFormatter formatter = null;

            TraceWriter.TraceBeginEnd(
                request,
                TraceCategories.FormattingCategory,
                TraceLevel.Info,
                InnerFormatter.GetType().Name,
                GetPerRequestFormatterInstanceMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                        SRResources.TraceGetPerRequestFormatterMessage,
                        InnerFormatter.GetType().Name,
                        type.Name,
                        mediaType);
                },
                execute: () => { formatter = InnerFormatter.GetPerRequestFormatterInstance(type, request, mediaType); },
                endTrace: (tr) =>
                {
                    if (formatter == null)
                    {
                        tr.Message = SRResources.TraceGetPerRequestNullFormatterEndMessage;
                    }
                    else
                    {
                        string formatMessage =
                            Object.ReferenceEquals(MediaTypeFormatterTracer.ActualMediaTypeFormatter(formatter),
                                                   InnerFormatter)
                                ? SRResources.TraceGetPerRequestFormatterEndMessage
                                : SRResources.TraceGetPerRequestFormatterEndMessageNew;

                        tr.Message = Error.Format(formatMessage, formatter.GetType().Name);
                    }
                },
                errorTrace: null);

            if (formatter != null && !(formatter is IFormatterTracer))
            {
                formatter = MediaTypeFormatterTracer.CreateTracer(formatter, TraceWriter, request);
            }

            return formatter;
        }

        public override bool CanReadType(Type type)
        {
            return InnerFormatter.CanReadType(type);
        }

        public override bool CanWriteType(Type type)
        {
            return InnerFormatter.CanWriteType(type);
        }

        public override bool Equals(object obj)
        {
            return InnerFormatter.Equals(obj);
        }

        public override int GetHashCode()
        {
            return InnerFormatter.GetHashCode();
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, string mediaType)
        {
            InnerFormatter.SetDefaultContentHeaders(type, headers, mediaType);
        }

        public override string ToString()
        {
            return InnerFormatter.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "Tracing layer needs to observer all Task completion paths")]
        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            MediaTypeHeaderValue contentType = contentHeaders == null ? null : contentHeaders.ContentType;

            return TraceWriter.TraceBeginEndAsync<object>(
                Request,
                TraceCategories.FormattingCategory,
                TraceLevel.Info,
                InnerFormatter.GetType().Name,
                ReadFromStreamAsyncMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                        SRResources.TraceReadFromStreamMessage,
                                        type.Name,
                                        contentType == null ? SRResources.TraceNoneObjectMessage : contentType.ToString());
                },

                execute: () => InnerFormatter.ReadFromStreamAsync(type, stream, contentHeaders, formatterLogger),

                endTrace: (tr, value) =>
                {
                    tr.Message = Error.Format(
                                        SRResources.TraceReadFromStreamValueMessage,
                                        FormattingUtilities.ValueToString(value, CultureInfo.CurrentCulture));
                },

                errorTrace: null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "Tracing layer needs to observer all Task completion paths")]
        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
        {
            MediaTypeHeaderValue contentType = contentHeaders == null
                                       ? null
                                       : contentHeaders.ContentType;

            return TraceWriter.TraceBeginEndAsync(
                Request,
                TraceCategories.FormattingCategory,
                TraceLevel.Info,
                InnerFormatter.GetType().Name,
                WriteToStreamAsyncMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                        SRResources.TraceWriteToStreamMessage,
                                        FormattingUtilities.ValueToString(value, CultureInfo.CurrentCulture),
                                        type.Name,
                                        contentType == null ? SRResources.TraceNoneObjectMessage : contentType.ToString());
                },
                execute: () => InnerFormatter.WriteToStreamAsync(type, value, stream, contentHeaders, transportContext),
                endTrace: null,
                errorTrace: null);
        }
    }
}
