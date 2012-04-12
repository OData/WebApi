// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Base class to handle serializing and deserializing strongly-typed objects using <see cref="ObjectContent"/>.
    /// </summary>
    public abstract class MediaTypeFormatter
    {
        private const int DefaultMinHttpCollectionKeys = 1;
        private const int DefaultMaxHttpCollectionKeys = 1000; // same default as ASPNET
        private const string IWellKnownComparerTypeName = "System.IWellKnownStringEqualityComparer, mscorlib, Version=4.0.0.0, PublicKeyToken=b77a5c561934e089";

        private static readonly ConcurrentDictionary<Type, Type> _delegatingEnumerableCache = new ConcurrentDictionary<Type, Type>();
        private static ConcurrentDictionary<Type, ConstructorInfo> _delegatingEnumerableConstructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static Lazy<int> _defaultMaxHttpCollectionKeys = new Lazy<int>(InitializeDefaultCollectionKeySize, true); // Max number of keys is 1000
        private static int _maxHttpCollectionKeys = -1;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeFormatter"/> class.
        /// </summary>
        protected MediaTypeFormatter()
        {
            SupportedMediaTypes = new MediaTypeHeaderValueCollection();
            SupportedEncodings = new Collection<Encoding>();
            MediaTypeMappings = new Collection<MediaTypeMapping>();
        }

        /// <summary>
        /// Gets or sets the maximum number of keys stored in a NameValueCollection. 
        /// </summary>
        public static int MaxHttpCollectionKeys
        {
            get 
            {
                if (_maxHttpCollectionKeys < 0)
                {
                    _maxHttpCollectionKeys = _defaultMaxHttpCollectionKeys.Value;
                }

                return _maxHttpCollectionKeys;
            }
            set
            {
                if (value < DefaultMinHttpCollectionKeys)
                {
                    throw new ArgumentOutOfRangeException("value", value, RS.Format(Properties.Resources.ArgumentMustBeGreaterThanOrEqualTo, DefaultMinHttpCollectionKeys));
                }

                _maxHttpCollectionKeys = value;
            }
        }

        /// <summary>
        /// Gets the mutable collection of <see cref="MediaTypeHeaderValue"/> elements supported by
        /// this <see cref="MediaTypeFormatter"/> instance.
        /// </summary>
        public Collection<MediaTypeHeaderValue> SupportedMediaTypes { get; private set; }

        /// <summary>
        /// Gets the mutable collection of character encodings supported by
        /// this <see cref="MediaTypeFormatter"/> instance. The encodings are
        /// used when reading or writing data. 
        /// </summary>
        public Collection<Encoding> SupportedEncodings { get; private set; }

        /// <summary>
        /// Gets the mutable collection of <see cref="MediaTypeMapping"/> elements used
        /// by this <see cref="MediaTypeFormatter"/> instance to determine the
        /// <see cref="MediaTypeHeaderValue"/> of requests or responses.
        /// </summary>
        public Collection<MediaTypeMapping> MediaTypeMappings { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IRequiredMemberSelector"/> used to determine required members.
        /// </summary>
        public IRequiredMemberSelector RequiredMemberSelector { get; set; }

        /// <summary>
        /// Returns a <see cref="Task"/> to deserialize an object of the given <paramref name="type"/> from the given <paramref name="stream"/>
        /// </summary>
        /// <remarks>
        /// <para>This implementation throws a <see cref="NotSupportedException"/>. Derived types should override this method if the formatter
        /// supports reading.</para>
        /// <para>An implementation of this method should NOT close <paramref name="stream"/> upon completion. The stream will be closed independently when
        /// the <see cref="HttpContent"/> instance is disposed.
        /// </para>
        /// </remarks>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="stream">The <see cref="Stream"/> to read.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> if available. It may be <c>null</c>.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>A <see cref="Task"/> whose result will be an object of the given type.</returns>
        /// <exception cref="NotSupportedException">Derived types need to support reading.</exception>
        /// <seealso cref="CanWriteType(Type)"/>
        public virtual Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            throw new NotSupportedException(
                RS.Format(Properties.Resources.MediaTypeFormatterCannotRead, GetType().Name));
        }

        /// <summary>
        /// Returns a <see cref="Task"/> that serializes the given <paramref name="value"/> of the given <paramref name="type"/>
        /// to the given <paramref name="stream"/>.
        /// </summary>
        /// <remarks>
        /// <para>This implementation throws a <see cref="NotSupportedException"/>. Derived types should override this method if the formatter
        /// supports reading.</para>
        /// <para>An implementation of this method should NOT close <paramref name="stream"/> upon completion. The stream will be closed independently when
        /// the <see cref="HttpContent"/> instance is disposed.
        /// </para>
        /// </remarks>
        /// <param name="type">The type of the object to write.</param>
        /// <param name="value">The object value to write.  It may be <c>null</c>.</param>
        /// <param name="stream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> if available. It may be <c>null</c>.</param>
        /// <param name="transportContext">The <see cref="TransportContext"/> if available. It may be <c>null</c>.</param>
        /// <returns>A <see cref="Task"/> that will perform the write.</returns>
        /// <exception cref="NotSupportedException">Derived types need to support writing.</exception>
        /// <seealso cref="CanReadType(Type)"/>
        public virtual Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
        {
            throw new NotSupportedException(
                RS.Format(Properties.Resources.MediaTypeFormatterCannotWrite, GetType().Name));
        }

        private static bool TryGetDelegatingType(Type interfaceType, ref Type type)
        {
            if (type != null
                && type.IsInterface
                && type.IsGenericType
                && (type.GetInterface(interfaceType.FullName) != null || type.GetGenericTypeDefinition().Equals(interfaceType)))
            {
                type = GetOrAddDelegatingType(type);
                return true;
            }

            return false;
        }

        private static int InitializeDefaultCollectionKeySize()
        {
            // we first detect if we are running on 4.5, return Max value if we are.
            Type comparerType = Type.GetType(IWellKnownComparerTypeName, throwOnError: false);

            if (comparerType != null)
            {
                return Int32.MaxValue;
            }

            // we should try to read it from the AppSettings 
            // if we found the aspnet settings configured, we will use that. Otherwise, we used the default 
            NameValueCollection settings = ConfigurationManager.AppSettings;
            int result;

            if (settings == null || !Int32.TryParse(settings["aspnet:MaxHttpCollectionKeys"], out result) || result < 0)
            {
                result = DefaultMaxHttpCollectionKeys;
            }

            return result;
        }

        /// <summary>
        /// This method converts <see cref="IEnumerable{T}"/> (and interfaces that mandate it) to a <see cref="DelegatingEnumerable{T}"/> for serialization purposes.
        /// </summary>
        /// <param name="type">The type to potentially be wrapped. If the type is wrapped, it's changed in place.</param>
        /// <returns>Returns <c>true</c> if the type was wrapped; <c>false</c>, otherwise</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Justification = "This API is designed to morph the type parameter appropriately")]
        internal static bool TryGetDelegatingTypeForIEnumerableGenericOrSame(ref Type type)
        {
            return TryGetDelegatingType(FormattingUtilities.EnumerableInterfaceGenericType, ref type);
        }

        /// <summary>
        /// This method converts <see cref="IQueryable{T}"/> (and interfaces that mandate it) to a <see cref="DelegatingEnumerable{T}"/> for serialization purposes.
        /// </summary>
        /// <param name="type">The type to potentially be wrapped. If the type is wrapped, it's changed in place.</param>
        /// <returns>Returns <c>true</c> if the type was wrapped; <c>false</c>, otherwise</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Justification = "This API is designed to morph the type parameter appropriately")]
        internal static bool TryGetDelegatingTypeForIQueryableGenericOrSame(ref Type type)
        {
            return TryGetDelegatingType(FormattingUtilities.QueryableInterfaceGenericType, ref type);
        }

        internal static ConstructorInfo GetTypeRemappingConstructor(Type type)
        {
            ConstructorInfo constructorInfo;
            _delegatingEnumerableConstructorCache.TryGetValue(type, out constructorInfo);
            return constructorInfo;
        }

        /// <summary>
        /// Determines the best <see cref="Encoding"/> amongst the supported encodings
        /// for reading or writing an HTTP entity body based on the provided <paramref name="contentHeaders"/>.
        /// </summary>
        /// <param name="contentHeaders">The content headers provided as part of the request or response.</param>
        /// <returns>The <see cref="Encoding"/> to use when reading the request or writing the response.</returns>
        protected Encoding SelectCharacterEncoding(HttpContentHeaders contentHeaders)
        {
            Encoding encoding = null;
            if (contentHeaders != null && contentHeaders.ContentType != null)
            {
                // Find encoding based on content type charset parameter
                string charset = contentHeaders.ContentType.CharSet;
                if (!String.IsNullOrWhiteSpace(charset))
                {
                    encoding =
                        SupportedEncodings.FirstOrDefault(
                            enc => charset.Equals(enc.WebName, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (encoding == null)
            {
                // We didn't find a character encoding match based on the content headers.
                // Instead we try getting the default character encoding.
                encoding = SupportedEncodings.FirstOrDefault();
            }

            if (encoding == null)
            {
                // No supported encoding was found so there is no way for us to start reading or writing.
                throw new InvalidOperationException(RS.Format(Properties.Resources.MediaTypeFormatterNoEncoding, GetType().Name));
            }

            return encoding;
        }

        internal bool CanReadAs(Type type, MediaTypeHeaderValue mediaType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            if (!CanReadType(type))
            {
                return false;
            }

            MediaTypeMatch mediaTypeMatch;
            return TryMatchSupportedMediaType(mediaType, out mediaTypeMatch);
        }

        internal bool CanWriteAs(Type type, MediaTypeHeaderValue mediaType, out MediaTypeHeaderValue matchedMediaType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            if (!CanWriteType(type))
            {
                matchedMediaType = null;
                return false;
            }

            MediaTypeMatch mediaTypeMatch;
            if (TryMatchSupportedMediaType(mediaType, out mediaTypeMatch))
            {
                matchedMediaType = mediaTypeMatch.MediaType;
                return true;
            }

            matchedMediaType = null;
            return false;
        }

        internal ResponseMediaTypeMatch SelectResponseMediaType(Type type, HttpRequestMessage request)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!CanWriteType(type))
            {
                return null;
            }

            // Determine the best character encoding if we have any registered encoders.
            // Note that it is ok for a formatter not to register any encoders in case it doesn't 
            // do any structured reading or writing.
            Encoding characterEncodingMatch = SupportedEncodings.Any() ? SelectResponseCharacterEncoding(request) : null;

            // Determine the best media type
            MediaTypeMatch mediaTypeMatch = null;

            // Match against media type mapping first
            if (TryMatchMediaTypeMapping(request, out mediaTypeMatch))
            {
                mediaTypeMatch.SetEncoding(characterEncodingMatch);
                return new ResponseMediaTypeMatch(
                    mediaTypeMatch,
                    ResponseFormatterSelectionResult.MatchOnRequestWithMediaTypeMapping);
            }

            // Match against the accept header.
            if (TryMatchSupportedMediaType(request, out mediaTypeMatch))
            {
                mediaTypeMatch.SetEncoding(characterEncodingMatch);
                return new ResponseMediaTypeMatch(
                    mediaTypeMatch,
                    ResponseFormatterSelectionResult.MatchOnRequestAcceptHeader);
            }

            // Match against request's content type
            HttpContent requestContent = request.Content;
            if (requestContent != null)
            {
                MediaTypeHeaderValue requestContentType = requestContent.Headers.ContentType;
                if (requestContentType != null && TryMatchSupportedMediaType(requestContentType, out mediaTypeMatch))
                {
                    mediaTypeMatch.SetEncoding(characterEncodingMatch);
                    return new ResponseMediaTypeMatch(
                        mediaTypeMatch,
                        ResponseFormatterSelectionResult.MatchOnRequestContentType);
                }
            }

            // No match at all.
            // Pick the first supported media type and indicate we've matched only on type
            MediaTypeHeaderValue mediaType = SupportedMediaTypes.FirstOrDefault();

            mediaTypeMatch = new MediaTypeMatch(mediaType);
            mediaTypeMatch.SetEncoding(characterEncodingMatch);
            return new ResponseMediaTypeMatch(
                mediaTypeMatch,
                ResponseFormatterSelectionResult.MatchOnCanWriteType);
        }

        /// <summary>
        /// Determine the best character encoding for writing the response. First we look
        /// for accept-charset headers and if not found then we try to match
        /// any charset encoding in the request (in case of PUT, POST, etc.)
        /// If no encoding is found then we use the default for the formatter.
        /// </summary>
        /// <returns>The <see cref="Encoding"/> determined to be the best match.</returns>
        internal Encoding SelectResponseCharacterEncoding(HttpRequestMessage request)
        {
            // Sort accept-charset headers in descending order based on q factor
            IEnumerable<StringWithQualityHeaderValue> acceptCharsetValues =
                request.Headers.AcceptCharset.OrderByDescending(m => m, StringWithQualityHeaderValueComparer.QualityComparer);

            // Check for match based on accept-charset headers
            foreach (StringWithQualityHeaderValue acceptCharset in acceptCharsetValues)
            {
                foreach (Encoding encoding in SupportedEncodings)
                {
                    if (acceptCharset.Value.Equals(encoding.WebName, StringComparison.OrdinalIgnoreCase) ||
                        acceptCharset.Value.Equals("*", StringComparison.OrdinalIgnoreCase))
                    {
                        return encoding;
                    }
                }
            }

            // Check for match based on any request entity body
            return SelectCharacterEncoding(request.Content != null ? request.Content.Headers : null);
        }

        internal bool TryMatchSupportedMediaType(MediaTypeHeaderValue mediaType, out MediaTypeMatch mediaTypeMatch)
        {
            Contract.Assert(mediaType != null);

            foreach (MediaTypeHeaderValue supportedMediaType in SupportedMediaTypes)
            {
                if (supportedMediaType.IsSubsetOf(mediaType))
                {
                    // If the incoming media type had an associated quality factor, propagate it to the match
                    MediaTypeWithQualityHeaderValue mediaTypeWithQualityHeaderValue = mediaType as MediaTypeWithQualityHeaderValue;
                    double quality = mediaTypeWithQualityHeaderValue != null && mediaTypeWithQualityHeaderValue.Quality.HasValue
                                         ? mediaTypeWithQualityHeaderValue.Quality.Value
                                         : MediaTypeMatch.Match;

                    mediaTypeMatch = new MediaTypeMatch(supportedMediaType, quality);
                    return true;
                }
            }

            mediaTypeMatch = null;
            return false;
        }

        internal bool TryMatchSupportedMediaType(HttpRequestMessage request, out MediaTypeMatch mediaTypeMatch)
        {
            Contract.Assert(request != null);

            IEnumerable<MediaTypeWithQualityHeaderValue> acceptMediaTypeValues = SortByQFactor(request.Headers.Accept);

            foreach (MediaTypeHeaderValue acceptMediaTypeValue in acceptMediaTypeValues)
            {
                if (TryMatchSupportedMediaType(acceptMediaTypeValue, out mediaTypeMatch))
                {
                    return true;
                }
            }

            mediaTypeMatch = null;
            return false;
        }

        private static IEnumerable<MediaTypeWithQualityHeaderValue> SortByQFactor(HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> acceptHeaders)
        {
            if (acceptHeaders.Count > 1)
            {
                // Sort accept headers (if more than 1) in descending order based on q factor
                // Use OrderBy() instead of Array.Sort() as it performs fewer comparisons. In this case the comparisons
                // are quite expensive so OrderBy() performs better.
                return acceptHeaders.OrderByDescending(m => m, MediaTypeWithQualityHeaderValueComparer.QualityComparer);
            }
            else
            {
                return acceptHeaders;
            }
        }

        internal bool TryMatchMediaTypeMapping(HttpRequestMessage request, out MediaTypeMatch mediaTypeMatch)
        {
            Contract.Assert(request != null, "request cannot be null.");

            foreach (MediaTypeMapping mapping in MediaTypeMappings)
            {
                // Collection<T> is not protected against null, so avoid them
                double quality;
                if (mapping != null && ((quality = mapping.TryMatchMediaType(request)) > 0.0))
                {
                    mediaTypeMatch = new MediaTypeMatch(mapping.MediaType, quality);
                    return true;
                }
            }

            mediaTypeMatch = null;
            return false;
        }

        /// <summary>
        /// Sets the default headers for content that will be formatted using this formatter. This method
        /// is called from the <see cref="ObjectContent"/> constructor.
        /// This implementation sets the Content-Type header to the value of <paramref name="mediaType"/> if it is
        /// not <c>null</c>. If it is <c>null</c> it sets the Content-Type to the default media type of this formatter.
        /// If the Content-Type does not specify a charset it will set it using this formatters configured
        /// <see cref="Encoding"/>.
        /// </summary>
        /// <remarks>
        /// Subclasses can override this method to set content headers such as Content-Type etc. Subclasses should
        /// call the base implementation. Subclasses should treat the passed in <paramref name="mediaType"/> (if not <c>null</c>)
        /// as the authoritative media type and use that as the Content-Type.
        /// </remarks>
        /// <param name="type">The type of the object being serialized. See <see cref="ObjectContent"/>.</param>
        /// <param name="headers">The content headers that should be configured.</param>
        /// <param name="mediaType">The authoritative media type. Can be <c>null</c>.</param>
        public virtual void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, string mediaType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            if (!String.IsNullOrEmpty(mediaType))
            {
                var parsedMediaType = MediaTypeHeaderValue.Parse(mediaType);
                headers.ContentType = parsedMediaType;
            }

            // If content type is not set then set it based on supported media types.
            if (headers.ContentType == null)
            {
                MediaTypeHeaderValue defaultMediaType = SupportedMediaTypes.FirstOrDefault();
                if (defaultMediaType != null)
                {
                    headers.ContentType = defaultMediaType.Clone();
                }
            }

            // If content type charset parameter is not set then set it based on the supported encodings.
            if (headers.ContentType != null && headers.ContentType.CharSet == null)
            {
                Encoding defaultEncoding = SupportedEncodings.FirstOrDefault();
                if (defaultEncoding != null)
                {
                    headers.ContentType.CharSet = defaultEncoding.WebName;
                }
            }
        }

        /// <summary>
        /// Returns a specialized instance of the <see cref="MediaTypeFormatter"/> that can handle formatting a response for the given
        /// parameters. This method is called by <see cref="DefaultContentNegotiator"/> after a formatter has been selected through content
        /// negotiation.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <c>this</c> instance. Derived classes can choose to return a new instance if
        /// they need to close over any of the parameters.
        /// </remarks>
        /// <param name="type">The type being serialized.</param>
        /// <param name="request">The request.</param>
        /// <param name="mediaType">The media type chosen for the serialization. Can be <c>null</c>.</param>
        /// <returns>An instance that can format a response to the given <paramref name="request"/>.</returns>
        public virtual MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            return this;
        }

        /// <summary>
        /// Determines whether this <see cref="MediaTypeFormatter"/> can deserialize
        /// an object of the specified type.
        /// </summary>
        /// <remarks>
        /// Derived classes must implement this method and indicate if a type can or cannot be deserialized.
        /// </remarks>
        /// <param name="type">The type of object that will be deserialized.</param>
        /// <returns><c>true</c> if this <see cref="MediaTypeFormatter"/> can deserialize an object of that type; otherwise <c>false</c>.</returns>
        public abstract bool CanReadType(Type type);

        /// <summary>
        /// Determines whether this <see cref="MediaTypeFormatter"/> can serialize
        /// an object of the specified type.
        /// </summary>
        /// <remarks>
        /// Derived classes must implement this method and indicate if a type can or cannot be serialized.
        /// </remarks>
        /// <param name="type">The type of object that will be serialized.</param>
        /// <returns><c>true</c> if this <see cref="MediaTypeFormatter"/> can serialize an object of that type; otherwise <c>false</c>.</returns>
        public abstract bool CanWriteType(Type type);

        private static Type GetOrAddDelegatingType(Type type)
        {
            return _delegatingEnumerableCache.GetOrAdd(
                type,
                (typeToRemap) =>
                {
                    // The current method is called by methods that already checked the type for is not null, is generic and is or implements IEnumerable<T>
                    // This retrieves the T type of the IEnumerable<T> interface.
                    Type elementType;
                    if (typeToRemap.GetGenericTypeDefinition().Equals(FormattingUtilities.EnumerableInterfaceGenericType))
                    {
                        elementType = typeToRemap.GetGenericArguments()[0];
                    }
                    else
                    {
                        elementType = typeToRemap.GetInterface(FormattingUtilities.EnumerableInterfaceGenericType.FullName).GetGenericArguments()[0];
                    }

                    Type delegatingType = FormattingUtilities.DelegatingEnumerableGenericType.MakeGenericType(elementType);
                    ConstructorInfo delegatingConstructor = delegatingType.GetConstructor(new Type[] { FormattingUtilities.EnumerableInterfaceGenericType.MakeGenericType(elementType) });
                    _delegatingEnumerableConstructorCache.TryAdd(delegatingType, delegatingConstructor);

                    return delegatingType;
                });
        }

        /// <summary>
        /// Gets the default value for the specified type.
        /// </summary>
        protected internal static object GetDefaultValueForType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Collection class that validates it contains only <see cref="MediaTypeHeaderValue"/> instances
        /// that are not null and not media ranges.
        /// </summary>
        internal class MediaTypeHeaderValueCollection : Collection<MediaTypeHeaderValue>
        {
            private static readonly Type _mediaTypeHeaderValueType = typeof(MediaTypeHeaderValue);

            /// <summary>
            /// Inserts the <paramref name="item"/> into the collection at the specified <paramref name="index"/>.
            /// </summary>
            /// <param name="index">The zero-based index at which item should be inserted.</param>
            /// <param name="item">The object to insert. It cannot be <c>null</c>.</param>
            protected override void InsertItem(int index, MediaTypeHeaderValue item)
            {
                ValidateMediaType(item);
                base.InsertItem(index, item);
            }

            /// <summary>
            /// Replaces the element at the specified <paramref name="index"/>.
            /// </summary>
            /// <param name="index">The zero-based index of the item that should be replaced.</param>
            /// <param name="item">The new value for the element at the specified index.  It cannot be <c>null</c>.</param>
            protected override void SetItem(int index, MediaTypeHeaderValue item)
            {
                ValidateMediaType(item);
                base.SetItem(index, item);
            }

            private static void ValidateMediaType(MediaTypeHeaderValue item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(item);
                if (parsedMediaType.IsAllMediaRange || parsedMediaType.IsSubTypeMediaRange)
                {
                    throw new ArgumentException(
                        RS.Format(Properties.Resources.CannotUseMediaRangeForSupportedMediaType, _mediaTypeHeaderValueType.Name, item.MediaType),
                        "item");
                }
            }
        }
    }
}
