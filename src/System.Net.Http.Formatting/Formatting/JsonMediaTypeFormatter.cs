// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle Json.
    /// </summary>
    public class JsonMediaTypeFormatter : MediaTypeFormatter
    {
        private JsonSerializerSettings _jsonSerializerSettings;
        private int _maxDepth = FormattingUtilities.DefaultMaxDepth;

#if !NETFX_CORE
        private ConcurrentDictionary<Type, DataContractJsonSerializer> _dataContractSerializerCache = new ConcurrentDictionary<Type, DataContractJsonSerializer>();
        private readonly IContractResolver _defaultContractResolver;
        private XmlDictionaryReaderQuotas _readerQuotas = FormattingUtilities.CreateDefaultReaderQuotas();
        private RequestHeaderMapping _requestHeaderMapping;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        public JsonMediaTypeFormatter()
        {
            // Set default supported media types
            SupportedMediaTypes.Add(MediaTypeConstants.ApplicationJsonMediaType);
            SupportedMediaTypes.Add(MediaTypeConstants.TextJsonMediaType);

            // Initialize serializer
#if !NETFX_CORE
            _defaultContractResolver = new JsonContractResolver(this);
#endif
            _jsonSerializerSettings = CreateDefaultSerializerSettings();

            // Set default supported character encodings
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

#if !NETFX_CORE
            _requestHeaderMapping = new XmlHttpRequestHeaderMapping();
            MediaTypeMappings.Add(_requestHeaderMapping);
#endif
        }

        /// <summary>
        /// Gets the default media type for Json, namely "application/json".
        /// </summary>
        /// <remarks>
        /// The default media type does not have any <c>charset</c> parameter as 
        /// the <see cref="Encoding"/> can be configured on a per <see cref="JsonMediaTypeFormatter"/> 
        /// instance basis.
        /// </remarks>
        /// <value>
        /// Because <see cref="MediaTypeHeaderValue"/> is mutable, the value
        /// returned will be a new instance every time.
        /// </value>
        public static MediaTypeHeaderValue DefaultMediaType
        {
            get { return MediaTypeConstants.ApplicationJsonMediaType; }
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerSettings"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        public JsonSerializerSettings SerializerSettings
        {
            get { return _jsonSerializerSettings; }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                _jsonSerializerSettings = value;
            }
        }

#if !NETFX_CORE
        /// <summary>
        /// Gets or sets a value indicating whether to use <see cref="DataContractJsonSerializer"/> by default.
        /// </summary>
        /// <value>
        ///     <c>true</c> if use <see cref="DataContractJsonSerializer"/> by default; otherwise, <c>false</c>. The default is <c>false</c>.
        /// </value>
        public bool UseDataContractJsonSerializer { get; set; }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether to indent elements when writing data. 
        /// </summary>
        public bool Indent { get; set; }

#if !NETFX_CORE
        /// <summary>
        /// Gets or sets the maximum depth allowed by this formatter.
        /// </summary>
        public int MaxDepth
        {
            get
            {
                return _maxDepth;
            }
            set
            {
                if (value < FormattingUtilities.DefaultMinDepth)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, FormattingUtilities.DefaultMinDepth);
                }

                _maxDepth = value;
                _readerQuotas.MaxDepth = value;
            }
        }
#endif

        /// <summary>
        /// Creates a <see cref="JsonSerializerSettings"/> instance with the default settings used by the <see cref="JsonMediaTypeFormatter"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This could only be static half the time.")]
        public JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
#if !NETFX_CORE
                ContractResolver = _defaultContractResolver,
#endif
                MissingMemberHandling = MissingMemberHandling.Ignore,

                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
                TypeNameHandling = TypeNameHandling.None
            };
        }

        /// <summary>
        /// Determines whether this <see cref="JsonMediaTypeFormatter"/> can read objects
        /// of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of object that will be read.</param>
        /// <returns><c>true</c> if objects of this <paramref name="type"/> can be read, otherwise <c>false</c>.</returns>
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

#if !NETFX_CORE
            if (UseDataContractJsonSerializer)
            {
                // If there is a registered non-null serializer, we can support this type.
                DataContractJsonSerializer serializer =
                    _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(t, throwOnError: false));

                // Null means we tested it before and know it is not supported
                return serializer != null;
            }
            else
#endif
            {
                return true;
            }
        }

        /// <summary>
        /// Determines whether this <see cref="JsonMediaTypeFormatter"/> can write objects
        /// of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of object that will be written.</param>
        /// <returns><c>true</c> if objects of this <paramref name="type"/> can be written, otherwise <c>false</c>.</returns>
        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

#if !NETFX_CORE
            if (UseDataContractJsonSerializer)
            {
                MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type);

                // If there is a registered non-null serializer, we can support this type.
                object serializer =
                    _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(t, throwOnError: false));

                // Null means we tested it before and know it is not supported
                return serializer != null;
            }
            else
#endif
            {
                return true;
            }
        }

        /// <summary>
        /// Called during deserialization to read an object of the specified <paramref name="type"/>
        /// from the specified <paramref name="readStream"/>.
        /// </summary>
        /// <param name="type">The type of object to read.</param>
        /// <param name="readStream">The <see cref="Stream"/> from which to read.</param>
        /// <param name="content">The <see cref="HttpContent"/> for the content being written.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>A <see cref="Task"/> whose result will be the object instance that has been read.</returns>
        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            return TaskHelpers.RunSynchronously<object>(() =>
            {
                HttpContentHeaders contentHeaders = content == null ? null : content.Headers;

                // If content length is 0 then return default value for this type
                if (contentHeaders != null && contentHeaders.ContentLength == 0)
                {
                    return GetDefaultValueForType(type);
                }

                // Get the character encoding for the content
                Encoding effectiveEncoding = SelectCharacterEncoding(contentHeaders);

                try
                {
#if !NETFX_CORE
                    if (UseDataContractJsonSerializer)
                    {
                        DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
                        using (XmlReader reader = JsonReaderWriterFactory.CreateJsonReader(new NonClosingDelegatingStream(readStream), effectiveEncoding, _readerQuotas, null))
                        {
                            return dataContractSerializer.ReadObject(reader);
                        }
                    }
                    else
#endif
                    {
                        using (JsonTextReader jsonTextReader = new JsonTextReader(new StreamReader(readStream, effectiveEncoding)) { CloseInput = false, MaxDepth = _maxDepth })
                        {
                            JsonSerializer jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings);
                            if (formatterLogger != null)
                            {
                                // Error must always be marked as handled
                                // Failure to do so can cause the exception to be rethrown at every recursive level and overflow the stack for x64 CLR processes
                                jsonSerializer.Error += (sender, e) =>
                                {
                                    Exception exception = e.ErrorContext.Error;
                                    formatterLogger.LogError(e.ErrorContext.Path, exception);
                                    e.ErrorContext.Handled = true;
                                };
                            }
                            return jsonSerializer.Deserialize(jsonTextReader, type);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (formatterLogger == null)
                    {
                        throw;
                    }
                    formatterLogger.LogError(String.Empty, e);
                    return GetDefaultValueForType(type);
                }
            });
        }

        /// <summary>
        /// Called during serialization to write an object of the specified <paramref name="type"/>
        /// to the specified <paramref name="writeStream"/>.
        /// </summary>
        /// <param name="type">The type of object to write.</param>
        /// <param name="value">The object to write.</param>
        /// <param name="writeStream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="content">The <see cref="HttpContent"/> for the content being written.</param>
        /// <param name="transportContext">The <see cref="TransportContext"/>.</param>
        /// <returns>A <see cref="Task"/> that will write the value to the stream.</returns>
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

#if !NETFX_CORE
            if (UseDataContractJsonSerializer && Indent)
            {
                throw Error.NotSupported(Properties.Resources.UnsupportedIndent, typeof(DataContractJsonSerializer));
            }
#endif

            return TaskHelpers.RunSynchronously(() =>
            {
                Encoding effectiveEncoding = SelectCharacterEncoding(content == null ? null : content.Headers);

#if !NETFX_CORE
                if (UseDataContractJsonSerializer)
                {
                    if (MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type))
                    {
                        if (value != null)
                        {
                            value = MediaTypeFormatter.GetTypeRemappingConstructor(type).Invoke(new object[] { value });
                        }
                    }

                    DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
                    using (XmlWriter writer = JsonReaderWriterFactory.CreateJsonWriter(writeStream, effectiveEncoding, ownsStream: false))
                    {
                        dataContractSerializer.WriteObject(writer, value);
                    }
                }
                else
#endif
                {
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(new StreamWriter(writeStream, effectiveEncoding)) { CloseOutput = false })
                    {
                        if (Indent)
                        {
                            jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                        }
                        JsonSerializer jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings);
                        jsonSerializer.Serialize(jsonTextWriter, value);
                        jsonTextWriter.Flush();
                    }
                }
            });
        }

#if !NETFX_CORE
        private static DataContractJsonSerializer CreateDataContractSerializer(Type type, bool throwOnError)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            DataContractJsonSerializer serializer = null;
            Exception exception = null;

            try
            {
                // Verify that type is a valid data contract by forcing the serializer to try to create a data contract
                FormattingUtilities.XsdDataContractExporter.GetRootElementName(type);
                serializer = new DataContractJsonSerializer(type);
            }
            catch (InvalidDataContractException invalidDataContractException)
            {
                exception = invalidDataContractException;
            }

            if (exception != null)
            {
                if (throwOnError)
                {
                    throw Error.InvalidOperation(exception, Properties.Resources.SerializerCannotSerializeType,
                                  typeof(DataContractJsonSerializer).Name,
                                  type.Name);
                }
            }

            return serializer;
        }

        private DataContractJsonSerializer GetDataContractSerializer(Type type)
        {
            Contract.Assert(type != null, "Type cannot be null");

            DataContractJsonSerializer serializer =
                _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(type, throwOnError: true));

            if (serializer == null)
            {
                // A null serializer means the type cannot be serialized
                throw Error.InvalidOperation(Properties.Resources.SerializerCannotSerializeType, typeof(DataContractJsonSerializer).Name, type.Name);
            }

            return serializer;
        }
#endif
    }
}
