// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="JsonMediaTypeFormatter"/>.  
    /// It is required because users can select formatters by this type.
    /// </summary>
    internal class JsonMediaTypeFormatterTracer : JsonMediaTypeFormatter, IFormatterTracer
    {
        private MediaTypeFormatterTracer _innerTracer;
        public JsonMediaTypeFormatterTracer(JsonMediaTypeFormatter innerFormatter, ITraceWriter traceWriter, HttpRequestMessage request)
        {
            _innerTracer = new MediaTypeFormatterTracer(innerFormatter, traceWriter, request);

            // copy values we cannot override
            _innerTracer.CopyNonOverriableMembersFromInner(this);
            MaxDepth = innerFormatter.MaxDepth;
            Indent = innerFormatter.Indent;
            UseDataContractJsonSerializer = innerFormatter.UseDataContractJsonSerializer;
            SerializerSettings = innerFormatter.SerializerSettings;
        }

        HttpRequestMessage IFormatterTracer.Request
        {
            get { return _innerTracer.Request; }
        }

        public MediaTypeFormatter InnerFormatter
        {
            get { return _innerTracer.InnerFormatter; }
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

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return _innerTracer.ReadFromStreamAsync(type, readStream, content, formatterLogger);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return _innerTracer.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            _innerTracer.SetDefaultContentHeaders(type, headers, mediaType);
        }
    }
}
