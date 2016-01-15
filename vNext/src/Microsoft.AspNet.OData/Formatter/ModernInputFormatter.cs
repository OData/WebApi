using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.AspNet.Mvc.Formatters;

namespace Microsoft.AspNet.OData.Formatter
{
    public class ModernInputFormatter : InputFormatter
    {
        /// <summary>
        /// Returns UTF8 Encoding without BOM and throws on invalid bytes.
        /// </summary>
        //public static readonly new Encoding UTF8EncodingWithoutBOM
        //    = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private JsonSerializerSettings _serializerSettings;

        public ModernInputFormatter()
        {
            _serializerSettings = new JsonSerializerSettings();

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);

            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json"));
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerSettings"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        public JsonSerializerSettings SerializerSettings
        {
            get
            {
                return _serializerSettings;
            }
            [param: NotNull]
            set
            {
                _serializerSettings = value;
            }
        }

        /// <inheritdoc />
        public override Task<InputFormatterResult> ReadRequestBodyAsync([NotNull] InputFormatterContext context)
        {
            var type = context.ModelType;
            var request = context.HttpContext.Request;
            MediaTypeHeaderValue requestContentType = null;
            MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType);

            // Get the character encoding for the content
            // Never non-null since SelectCharacterEncoding() throws in error / not found scenarios
            var effectiveEncoding = SelectCharacterEncoding(context);

            using (var jsonReader = CreateJsonReader(context, request.Body, effectiveEncoding))
            {
                jsonReader.CloseInput = false;

                var jsonSerializer = CreateJsonSerializer();

                EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> errorHandler = null;
                errorHandler = (sender, e) =>
                {
                    var exception = e.ErrorContext.Error;
                    context.ModelState.TryAddModelError(e.ErrorContext.Path, e.ErrorContext.ToString() );

                    // Error must always be marked as handled
                    // Failure to do so can cause the exception to be rethrown at every recursive level and
                    // overflow the stack for x64 CLR processes
                    e.ErrorContext.Handled = true;
                };
                jsonSerializer.Error += errorHandler;

                try
                {
                    return Task.FromResult(
                        InputFormatterResult.Success(
                            jsonSerializer.Deserialize(jsonReader, type)
                        )
                    );
                }
                finally
                {
                    // Clean up the error handler in case CreateJsonSerializer() reuses a serializer
                    if (errorHandler != null)
                    {
                        jsonSerializer.Error -= errorHandler;
                    }
                }
            }
        }

        /// <summary>
        /// Called during deserialization to get the <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="context">The <see cref="InputFormatterContext"/> for the read.</param>
        /// <param name="readStream">The <see cref="Stream"/> from which to read.</param>
        /// <param name="effectiveEncoding">The <see cref="Encoding"/> to use when reading.</param>
        /// <returns>The <see cref="JsonReader"/> used during deserialization.</returns>
        public virtual JsonReader CreateJsonReader([NotNull] InputFormatterContext context,
                                                   [NotNull] Stream readStream,
                                                   [NotNull] Encoding effectiveEncoding)
        {
            return new JsonTextReader(new StreamReader(readStream, effectiveEncoding));
        }

        /// <summary>
        /// Called during deserialization to get the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <returns>The <see cref="JsonSerializer"/> used during serialization and deserialization.</returns>
        public virtual JsonSerializer CreateJsonSerializer()
        {
            return JsonSerializer.Create(SerializerSettings);
        }
    }
}
