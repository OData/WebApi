using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.OData.Formatter
{
    public class ModernOutputFormatter : OutputFormatter
    {
        /// <summary>
        /// Returns UTF8 Encoding without BOM and throws on invalid bytes.
        /// </summary>
        public static readonly Encoding UTF8EncodingWithoutBOM
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        public ModernOutputFormatter()
        {
            // TODO: JC: Restore this
            //SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));

            //foreach (var mediaType in SupportedMediaTypes)
            //{
            //    mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
            //}
        }

		public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;
            //var selectedEncoding = context.ContentType.Encoding == null ? Encoding.UTF8 : context.ContentType.Encoding;
            var selectedEncoding = Encoding.UTF8;

			var value = context.Object;
			if (value is IEdmModel)
			{
				using (var delegatingStream = new NonDisposableStream(response.Body))
				using (var writer = new StreamWriter(delegatingStream, selectedEncoding, 1024, leaveOpen: true))
				{
					WriteMetadata(writer, (IEdmModel)value);
					WriteObject(delegatingStream, context);
				}
			}
			else {
				using (var delegatingStream = new NonDisposableStream(response.Body))
            //using (var writer = new StreamWriter(delegatingStream, selectedEncoding, 1024, leaveOpen: true))
            {
                WriteObject(delegatingStream, context);
            }
			}

            return Task.FromResult(true);
        }

        public override void WriteResponseHeaders(OutputFormatterWriteContext context)
        {
            if (context.Object is IEdmModel)
            {
                context.ContentType = new StringSegment(SupportedMediaTypes[2]);
            }

            context.HttpContext.Response.Headers.Add("OData-Version", new[] { "4.0" });
            base.WriteResponseHeaders(context);
        }

        // In the future, should convert to ODataEntry and use ODL to write out.
        // Or use ODL to build a JObject and use Json.NET to write out.
        public void WriteObject(Stream stream, OutputFormatterWriteContext context)
        {
			new ODataJsonSerializer(context).WriteJson(context.Object, stream);
            //using (var jsonWriter = CreateJsonWriter(writer))
            //{
            //    var jsonSerializer = CreateJsonSerializer(context);
            //    jsonSerializer.Serialize(jsonWriter, value);
            //}
        }

        private JsonSerializer CreateJsonSerializer(OutputFormatterWriteContext context)
        {
            var serializerSettings = new JsonSerializerSettings();
	        serializerSettings.Converters.Add(
		        new ODataJsonConverter(
			        new Uri("http://localhost:58888/"),
					context));
            var jsonSerializer = JsonSerializer.Create(serializerSettings);
            return jsonSerializer;
        }

        private JsonWriter CreateJsonWriter(TextWriter writer)
        {
            var jsonWriter = new JsonTextWriter(writer);
            jsonWriter.CloseOutput = false;

            return jsonWriter;
        }

        private void WriteMetadata(TextWriter writer, IEdmModel model)
        {
            using (var xmlWriter = XmlWriter.Create(writer))
            {
                IEnumerable<EdmError> errors;
                EdmxWriter.TryWriteEdmx(model, xmlWriter, EdmxTarget.OData, out errors);
            }
        }
    }
}