using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle Json.
    /// </summary>
    public class JsonMediaTypeFormatter : MediaTypeFormatter
    {
        private const int DefaultMaxDepth = 1024;
        private static readonly MediaTypeHeaderValue[] _supportedMediaTypes = new MediaTypeHeaderValue[]
        {
            MediaTypeConstants.ApplicationJsonMediaType,
            MediaTypeConstants.TextJsonMediaType
        };
        private JsonSerializer _jsonSerializer = CreateDefaultSerializer();
        private int _maxDepth = DefaultMaxDepth;
        private XmlDictionaryReaderQuotas _readerQuotas = CreateDefaultReaderQuotas();

        // Encoders used for reading data based on charset parameter and default encoder doesn't match
        private readonly Dictionary<string, Encoding> _decoders = new Dictionary<string, Encoding>(StringComparer.OrdinalIgnoreCase)
        {
            { Encoding.UTF8.WebName, new UTF8Encoding(false, true) },
            { Encoding.Unicode.WebName, new UnicodeEncoding(false, true, true) },
        };

        private ConcurrentDictionary<Type, DataContractJsonSerializer> _dataContractSerializerCache = new ConcurrentDictionary<Type, DataContractJsonSerializer>();
        private RequestHeaderMapping _requestHeaderMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        public JsonMediaTypeFormatter()
        {
            InitializeEncoding();
            foreach (MediaTypeHeaderValue value in _supportedMediaTypes)
            {
                SupportedMediaTypes.Add(value);
            }

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
        /// Gets or sets the <see cref="Encoding"/> to use when writing data.
        /// </summary>
        /// <remarks>The default encoding is <see cref="UTF8Encoding"/>.</remarks>
        /// <value>
        /// The <see cref="Encoding"/> to use when writing data.
        /// </value>
        public Encoding CharacterEncoding
        {
            get { return Encoding; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                Type valueType = value.GetType();
                if (FormattingUtilities.Utf8EncodingType.IsAssignableFrom(valueType) || FormattingUtilities.Utf16EncodingType.IsAssignableFrom(valueType))
                {
                    Encoding = value;
                    return;
                }

                throw new ArgumentException(
                    RS.Format(Properties.Resources.UnsupportedEncoding, typeof(JsonMediaTypeFormatter).Name, FormattingUtilities.Utf8EncodingType.Name, FormattingUtilities.Utf16EncodingType.Name), "value");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializer"/> used for Json.
        /// </summary>
        public JsonSerializer Serializer
        {
            get
            {
                return _jsonSerializer;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _jsonSerializer = value;
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
                _maxDepth = value;
                _readerQuotas.MaxDepth = value;
            }
        }

        /// <summary>
        /// Creates a <see cref="JsonSerializer"/> with the default settings used by the <see cref="JsonMediaTypeFormatter"/>.
        /// </summary>
        public static JsonSerializer CreateDefaultSerializer()
        {
            JsonSerializer defaultSerializer = new JsonSerializer();
            defaultSerializer.ContractResolver = new JsonContractResolver();

            // Do not change this setting
            // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
            defaultSerializer.TypeNameHandling = TypeNameHandling.None;
            return defaultSerializer;
        }

        private static XmlDictionaryReaderQuotas CreateDefaultReaderQuotas()
        {
            return new XmlDictionaryReaderQuotas()
            {
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxDepth = DefaultMaxDepth,
                MaxNameTableCharCount = int.MaxValue,
                MaxStringContentLength = int.MaxValue
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
                Encoding effectiveEncoding = Encoding;

                if (contentHeaders != null && contentHeaders.ContentType != null)
                {
                    string charset = contentHeaders.ContentType.CharSet;
                    if (!String.IsNullOrWhiteSpace(charset) &&
                        !String.Equals(charset, Encoding.WebName) &&
                        !_decoders.TryGetValue(charset, out effectiveEncoding))
                    {
                        effectiveEncoding = Encoding;
                    }
                }

                if (!UseDataContractJsonSerializer)
                {
                    using (JsonTextReader jsonTextReader = new SecureJsonTextReader(new StreamReader(stream, effectiveEncoding), _maxDepth))
                    {
                        return _jsonSerializer.Deserialize(jsonTextReader, type);
                    }
                }
                else
                {
                    DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
                    using (XmlReader reader = JsonReaderWriterFactory.CreateJsonReader(stream, effectiveEncoding, _readerQuotas, null))
                    {
                        return dataContractSerializer.ReadObject(reader);
                    }
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
                if (!UseDataContractJsonSerializer)
                {
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(new StreamWriter(stream, Encoding)) { CloseOutput = false })
                    {
                        if (Indent)
                        {
                            jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                        }

                        Serializer.Serialize(jsonTextWriter, value);
                        jsonTextWriter.Flush();
                    }
                }
                else
                {
                    if (MediaTypeFormatter.TryGetDelegatingTypeForIQueryableGenericOrSame(ref type))
                    {
                        value = MediaTypeFormatter.GetTypeRemappingConstructor(type).Invoke(new object[] { value });
                    }

                    DataContractJsonSerializer dataContractSerializer = GetDataContractSerializer(type);
                    // TODO: CSDMain 235508: Should formatters close write stream on completion or leave that to somebody else?
                    using (XmlWriter writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding, ownsStream: false))
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
