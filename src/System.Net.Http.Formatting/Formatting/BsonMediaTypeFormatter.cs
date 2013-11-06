// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle Bson.
    /// </summary>
    public class BsonMediaTypeFormatter : BaseJsonMediaTypeFormatter
    {
        private static readonly Type OpenDictionaryType = typeof(Dictionary<,>);

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonMediaTypeFormatter"/> class.
        /// </summary>
        public BsonMediaTypeFormatter()
        {
            // Set default supported media type
            SupportedMediaTypes.Add(MediaTypeConstants.ApplicationBsonMediaType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="BsonMediaTypeFormatter"/> instance to copy settings from.</param>
        protected BsonMediaTypeFormatter(BsonMediaTypeFormatter formatter)
            : base(formatter)
        {
        }

        /// <summary>
        /// Gets the default media type for Json, namely "application/bson".
        /// </summary>
        /// <remarks>
        /// The default media type does not have any <c>charset</c> parameter as
        /// the <see cref="Encoding"/> can be configured on a per <see cref="BsonMediaTypeFormatter"/>
        /// instance basis.
        /// </remarks>
        /// <value>
        /// Because <see cref="MediaTypeHeaderValue"/> is mutable, the value
        /// returned will be a new instance every time.
        /// </value>
        public static MediaTypeHeaderValue DefaultMediaType
        {
            get
            {
                return MediaTypeConstants.ApplicationBsonMediaType;
            }
        }

#if !NETFX_CORE // MaxDepth not supported in portable library; no need to override there
        /// <inheritdoc />
        public sealed override int MaxDepth
        {
            get
            {
                return base.MaxDepth;
            }
            set
            {
                base.MaxDepth = value;
            }
        }
#endif

        /// <inheritdoc />
        public override object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding,
            IFormatterLogger formatterLogger)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            // Special-case for simple types: Deserialize a Dictionary with a single element named Value.
            // Serialization created this Dictionary<string, object> to work around BSON restrictions: BSON cannot
            // handle a top-level simple type.  NewtonSoft.Json throws a JsonWriterException with message "Error
            // writing Binary value. BSON must start with an Object or Array. Path ''." when WriteToStream() is given
            // such a value.
            //
            // Added clause for typeof(byte[]) needed because NewtonSoft.Json sometimes throws above Exception when
            // WriteToStream() is given a byte[] value.  (Not clear where the bug lies and, worse, it doesn't reproduce
            // reliably.)
            if (IsSimpleType(type) || type == typeof(byte[]))
            {
                // Read as exact expected Dictionary<string, T> to ensure NewtonSoft.Json does correct top-level conversion.
                Type dictionaryType = OpenDictionaryType.MakeGenericType(new Type[] { typeof(string), type });
                IDictionary dictionary =
                    base.ReadFromStream(dictionaryType, readStream, effectiveEncoding, formatterLogger) as IDictionary;
                if (dictionary == null)
                {
                    // Not valid since BaseJsonMediaTypeFormatter.ReadFromStream(Type, Stream, HttpContent, IFormatterLogger)
                    // handles empty content and does not call ReadFromStream(Type, Stream, Encoding, IFormatterLogger)
                    // in that case.
                    throw Error.InvalidOperation(Properties.Resources.MediaTypeFormatter_BsonParseError_MissingData,
                        dictionaryType.Name);
                }

                // Unfortunately IDictionary doesn't have TryGetValue()...
                string firstKey = String.Empty;
                foreach (DictionaryEntry item in dictionary)
                {
                    if (dictionary.Count == 1 && (item.Key as string) == "Value")
                    {
                        // Success
                        return item.Value;
                    }
                    else
                    {
                        if (item.Key != null)
                        {
                            firstKey = item.Key.ToString();
                        }

                        break;
                    }
                }

                throw Error.InvalidOperation(Properties.Resources.MediaTypeFormatter_BsonParseError_UnexpectedData,
                    dictionary.Count, firstKey);
            }
            else
            {
                return base.ReadFromStream(type, readStream, effectiveEncoding, formatterLogger);
            }
        }

        /// <inheritdoc />
        public override JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (readStream == null)
            {
                throw Error.ArgumentNull("readStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            BsonReader reader = new BsonReader(new BinaryReader(readStream, effectiveEncoding));

            try
            {
                // Special case discussed at http://stackoverflow.com/questions/16910369/bson-array-deserialization-with-json-net
                // Dispensed with string (aka IEnumerable<char>) case above in ReadFromStream()
                reader.ReadRootValueAsArray =
                    typeof(IEnumerable).IsAssignableFrom(type) && !typeof(IDictionary).IsAssignableFrom(type);
            }
            catch
            {
                // Ensure instance is cleaned up in case of an issue
                ((IDisposable)reader).Dispose();
                throw;
            }

            return reader;
        }

        /// <inheritdoc />
        public override void WriteToStream(Type type, object value, Stream writeStream, Encoding effectiveEncoding)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            if (value == null)
            {
                // See comments in ReadFromStream() above about this special case.
                return;
            }

            // See comments in ReadFromStream() above about this special case and the need to include byte[] in it.
            if (IsSimpleType(type) || type == typeof(byte[]))
            {
                // Wrap value in a Dictionary with a single property named "Value" to provide BSON with an Object.  Is
                // written out as binary equivalent of { "Value": value } JSON.
                Dictionary<string, object> temporaryDictionary = new Dictionary<string, object>
                {
                    { "Value", value },
                };
                base.WriteToStream(typeof(Dictionary<string, object>), temporaryDictionary, writeStream, effectiveEncoding);
            }
            else
            {
                base.WriteToStream(type, value, writeStream, effectiveEncoding);
            }
        }

        /// <inheritdoc />
        public override JsonWriter CreateJsonWriter(Type type, Stream writeStream, Encoding effectiveEncoding)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (writeStream == null)
            {
                throw Error.ArgumentNull("writeStream");
            }

            if (effectiveEncoding == null)
            {
                throw Error.ArgumentNull("effectiveEncoding");
            }

            return new BsonWriter(new BinaryWriter(writeStream, effectiveEncoding));
        }

        private static bool IsSimpleType(Type type)
        {
            bool isSimpleType;
#if NETFX_CORE // TypeDescriptor is not supported in portable library
            // TODO: This could likely be improved
            isSimpleType = type.IsValueType() || type == typeof(string);
#else
            // CanConvertFrom() check is similar to MVC / Web API ModelMetadata.IsComplexType getters.
            isSimpleType = TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
#endif

            return isSimpleType;
        }
    }
}
