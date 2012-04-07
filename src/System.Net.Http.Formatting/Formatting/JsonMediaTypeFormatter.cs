// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Internal;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
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
        private static readonly MediaTypeHeaderValue[] _supportedMediaTypes = new MediaTypeHeaderValue[]
        {
            MediaTypeConstants.ApplicationJsonMediaType,
            MediaTypeConstants.TextJsonMediaType
        };

        private JsonSerializerSettings _jsonSerializerSettings;
        private readonly IContractResolver _defaultContractResolver;
        private int _maxDepth = FormattingUtilities.DefaultMaxDepth;
        private XmlDictionaryReaderQuotas _readerQuotas = FormattingUtilities.CreateDefaultReaderQuotas();

        private ConcurrentDictionary<Type, DataContractJsonSerializer> _dataContractSerializerCache = new ConcurrentDictionary<Type, DataContractJsonSerializer>();
        private RequestHeaderMapping _requestHeaderMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        public JsonMediaTypeFormatter()
        {
            // Set default supported media types
            foreach (MediaTypeHeaderValue value in _supportedMediaTypes)
            {
                SupportedMediaTypes.Add(value);
            }

            // Initialize serializer
            _defaultContractResolver = new JsonContractResolver(this);
            _jsonSerializerSettings = CreateDefaultSerializerSettings();

            // Set default supported character encodings
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

            _requestHeaderMapping = new XHRRequestHeaderMapping();
            MediaTypeMappings.Add(_requestHeaderMapping);
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
                    throw new ArgumentNullException("value");
                }

                _jsonSerializerSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use <see cref="DataContractJsonSerializer"/> by default.
        /// </summary>
        /// <value>
        ///     <c>true</c> if use <see cref="DataContractJsonSerializer"/> by default; otherwise, <c>false</c>. The default is <c>false</c>.
        /// </value>
        public bool UseDataContractJsonSerializer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to indent elements when writing data. 
        /// </summary>
        public bool Indent { get; set; }

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
                    throw new ArgumentOutOfRangeException("value", value, RS.Format(Properties.Resources.ArgumentMustBeGreaterThanOrEqualTo, FormattingUtilities.DefaultMinDepth));
                }

                _maxDepth = value;
                _readerQuotas.MaxDepth = value;
            }
        }

        /// <summary>
        /// Creates a <see cref="JsonSerializerSettings"/> instance with the default settings used by the <see cref="JsonMediaTypeFormatter"/>.
        /// </summary>
        public JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                ContractResolver = _defaultContractResolver,
                MissingMemberHandling = MissingMemberHandling.Ignore,

                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
                TypeNameHandling = TypeNameHandling.None
            };
        }

        internal bool ContainsSerializerForType(Type type)
        {
            return _dataContractSerializerCache.ContainsKey(type);
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
                throw new ArgumentNullException("type");
            }

            if (UseDataContractJsonSerializer)
            {
                // If there is a registered non-null serializer, we can support this type.
                DataContractJsonSerializer serializer =
                    _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(t));

                // Null means we tested it before and know it is not supported
                return serializer != null;
            }
            else
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
                throw new ArgumentNullException("type");
            }

            if (UseDataContractJsonSerializer)
            {
                MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type);

                // If there is a registered non-null serializer, we can support this type.
                object serializer =
                    _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(t));

                // Null means we tested it before and know it is not supported
                return serializer != null;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Called during deserialization to read an object of the specified <paramref name="type"/>
        /// from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="type">The type of object to read.</param>
        /// <param name="stream">The <see cref="Stream"/> from which to read.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> for the content being written.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>A <see cref="Task"/> whose result will be the object instance that has been read.</returns>
        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return TaskHelpers.RunSynchronously<object>(() =>
            {
                // If content length is 0 then return default value for this type
                if (contentHeaders != null && contentHeaders.ContentLength == 0)
                {
                    return GetDefaultValueForType(type);
                }

                // Get the character encoding for the content
                Encoding effectiveEncoding = SelectCharacterEncoding(contentHeaders);

                try
                {
                    if (UseDataContractJsonSerializer)
                    {
                        DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
                        using (XmlReader reader = JsonReaderWriterFactory.CreateJsonReader(new NonClosingDelegatingStream(stream), effectiveEncoding, _readerQuotas, null))
                        {
                            return dataContractSerializer.ReadObject(reader);
                        }
                    }
                    else
                    {
                        using (JsonTextReader jsonTextReader = new SecureJsonTextReader(new StreamReader(stream, effectiveEncoding), _maxDepth) { CloseInput = false })
                        {
                            JsonSerializer jsonSerializer = JsonSerializer.Create(_jsonSerializerSettings);
                            if (formatterLogger != null)
                            {
                                // Error must always be marked as handled
                                // Failure to do so can cause the exception to be rethrown at every recursive level and overflow the stack for x64 CLR processes
                                jsonSerializer.Error += (sender, e) =>
                                {
                                    Exception exception = e.ErrorContext.Error;
                                    formatterLogger.LogError(e.ErrorContext.Path, exception.Message);
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
                    formatterLogger.LogError(String.Empty, e.Message);
                    return GetDefaultValueForType(type);
                }
            });
        }

        /// <summary>
        /// Called during serialization to write an object of the specified <paramref name="type"/>
        /// to the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="type">The type of object to write.</param>
        /// <param name="value">The object to write.</param>
        /// <param name="stream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> for the content being written.</param>
        /// <param name="transportContext">The <see cref="TransportContext"/>.</param>
        /// <returns>A <see cref="Task"/> that will write the value to the stream.</returns>
        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (UseDataContractJsonSerializer && Indent)
            {
                throw new NotSupportedException(RS.Format(Properties.Resources.UnsupportedIndent, typeof(DataContractJsonSerializer)));
            }

            return TaskHelpers.RunSynchronously(() =>
            {
                Encoding effectiveEncoding = SelectCharacterEncoding(contentHeaders);

                if (!UseDataContractJsonSerializer)
                {
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(new StreamWriter(stream, effectiveEncoding)) { CloseOutput = false })
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
                else
                {
                    if (MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type))
                    {
                        if (value != null)
                        {
                            value = MediaTypeFormatter.GetTypeRemappingConstructor(type).Invoke(new object[] { value });
                        }
                    }

                    DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
                    using (XmlWriter writer = JsonReaderWriterFactory.CreateJsonWriter(stream, effectiveEncoding, ownsStream: false))
                    {
                        dataContractSerializer.WriteObject(writer, value);
                    }
                }
            });
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        private static DataContractJsonSerializer CreateDataContractSerializer(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            DataContractJsonSerializer serializer = null;

            try
            {
                if (IsKnownUnserializableType(type))
                {
                    return null;
                }

                //// TODO: CSDMAIN 211321 -- determine the correct algorithm to know what is serializable.
                serializer = new DataContractJsonSerializer(type);
            }
            catch (Exception)
            {
                //// TODO: CSDMain 232171 -- review and fix swallowed exception
            }

            return serializer;
        }

        private static bool IsKnownUnserializableType(Type type)
        {
            if (type.IsGenericType)
            {
                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    return IsKnownUnserializableType(type.GetGenericArguments()[0]);
                }
            }

            if (!type.IsVisible)
            {
                return true;
            }

            if (type.HasElementType && IsKnownUnserializableType(type.GetElementType()))
            {
                return true;
            }

            return false;
        }

        private DataContractJsonSerializer GetDataContractSerializer(Type type)
        {
            Contract.Assert(type != null, "Type cannot be null");

            DataContractJsonSerializer serializer =
                _dataContractSerializerCache.GetOrAdd(type, (t) => CreateDataContractSerializer(type));

            if (serializer == null)
            {
                // A null serializer means the type cannot be serialized
                throw new InvalidOperationException(
                    RS.Format(Properties.Resources.SerializerCannotSerializeType, typeof(DataContractJsonSerializer).Name, type.Name));
            }

            return serializer;
        }
    }
}
