// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class for handling HTML form URL-ended data, also known as <c>application/x-www-form-urlencoded</c>. 
    /// </summary>
    public class FormUrlEncodedMediaTypeFormatter : MediaTypeFormatter
    {
        private const int MinBufferSize = 256;
        private const int DefaultBufferSize = 32 * 1024;

        private static readonly MediaTypeHeaderValue[] _supportedMediaTypes = new MediaTypeHeaderValue[]
        {
            MediaTypeConstants.ApplicationFormUrlEncodedMediaType
        };

        private int _readBufferSize = DefaultBufferSize;
        private int _maxDepth = FormattingUtilities.DefaultMaxDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUrlEncodedMediaTypeFormatter"/> class.
        /// </summary>
        public FormUrlEncodedMediaTypeFormatter()
        {
            foreach (MediaTypeHeaderValue value in _supportedMediaTypes)
            {
                SupportedMediaTypes.Add(value);
            }
        }

        /// <summary>
        /// Gets the default media type for HTML Form URL encoded data, namely <c>application/x-www-form-urlencoded</c>.
        /// </summary>
        /// <value>
        /// Because <see cref="MediaTypeHeaderValue"/> is mutable, the value
        /// returned will be a new instance every time.
        /// </value>
        public static MediaTypeHeaderValue DefaultMediaType
        {
            get { return MediaTypeConstants.ApplicationFormUrlEncodedMediaType; }
        }

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
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffer when reading the incoming stream.
        /// </summary>
        /// <value>
        /// The size of the read buffer.
        /// </value>
        public int ReadBufferSize
        {
            get { return _readBufferSize; }

            set
            {
                if (value < MinBufferSize)
                {
                    throw new ArgumentOutOfRangeException("value", value, RS.Format(Properties.Resources.ArgumentMustBeGreaterThanOrEqualTo, MinBufferSize));
                }

                _readBufferSize = value;
            }
        }

        /// <summary>
        /// Determines whether this <see cref="FormUrlEncodedMediaTypeFormatter"/> can read objects
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

            // Can't read arbitrary types. 
            return type == typeof(FormDataCollection) || FormattingUtilities.IsJTokenType(type);
        }

        /// <summary>
        /// Determines whether this <see cref="FormUrlEncodedMediaTypeFormatter"/> can write objects
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

            return false;
        }

        /// <summary>
        /// Called during deserialization to read an object of the specified <paramref name="type"/>
        /// from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="type">The type of object to read.</param>
        /// <param name="stream">The <see cref="Stream"/> from which to read.</param>
        /// <param name="contentHeaders">The <see cref="HttpContentHeaders"/> for the content being read.</param>
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
                IEnumerable<KeyValuePair<string, string>> nameValuePairs = ReadFormUrlEncoded(stream, ReadBufferSize);

                if (type == typeof(FormDataCollection))
                {
                    return new FormDataCollection(nameValuePairs);
                }

                if (FormattingUtilities.IsJTokenType(type))
                {
                    return FormUrlEncodedJson.Parse(nameValuePairs, _maxDepth);
                }
                
                // Passed us an unsupported type. Should have called CanReadType() first.
                throw new InvalidOperationException(
                    RS.Format(Properties.Resources.SerializerCannotSerializeType, GetType().Name, type.Name));
            });
        }

        /// <summary>
        /// Reads all name-value pairs encoded as HTML Form URL encoded data and add them to 
        /// a collection as UNescaped URI strings.
        /// </summary>
        /// <param name="input">Stream to read from.</param>
        /// <param name="bufferSize">Size of the buffer used to read the contents.</param>
        /// <returns>Collection of name-value pairs.</returns>
        private static IEnumerable<KeyValuePair<string, string>> ReadFormUrlEncoded(Stream input, int bufferSize)
        {
            Contract.Assert(input != null, "input stream cannot be null");
            Contract.Assert(bufferSize >= MinBufferSize, "buffer size cannot be less than MinBufferSize");

            byte[] data = new byte[bufferSize];
            
            int bytesRead;
            bool isFinal = false;
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            FormUrlEncodedParser parser = new FormUrlEncodedParser(result, Int64.MaxValue);
            ParserState state;

            while (true)
            {
                try
                {
                    bytesRead = input.Read(data, 0, data.Length);
                    if (bytesRead == 0)
                    {
                        isFinal = true;
                    }
                }
                catch (Exception e)
                {
                    throw new IOException(Properties.Resources.ErrorReadingFormUrlEncodedStream, e);
                }

                int bytesConsumed = 0;
                state = parser.ParseBuffer(data, bytesRead, ref bytesConsumed, isFinal);
                if (state != ParserState.NeedMoreData && state != ParserState.Done)
                {
                    throw new IOException(RS.Format(Properties.Resources.FormUrlEncodedParseError, bytesConsumed));
                }

                if (isFinal)
                {
                    return result;
                }
            }
        }
    }
}
