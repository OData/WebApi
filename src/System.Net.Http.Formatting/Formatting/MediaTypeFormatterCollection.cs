// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Collection class that contains <see cref="MediaTypeFormatter"/> instances.
    /// </summary>
    public class MediaTypeFormatterCollection : Collection<MediaTypeFormatter>
    {
        private static readonly Type _mediaTypeFormatterType = typeof(MediaTypeFormatter);

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeFormatterCollection"/> class.
        /// </summary>
        /// <remarks>
        /// This collection will be initialized to contain default <see cref="MediaTypeFormatter"/>
        /// instances for Xml, JsonValue and Json.
        /// </remarks>
        public MediaTypeFormatterCollection()
            : this(CreateDefaultFormatters())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeFormatterCollection"/> class.
        /// </summary>
        /// <param name="formatters">A collection of <see cref="MediaTypeFormatter"/> instances to place in the collection.</param>
        public MediaTypeFormatterCollection(IEnumerable<MediaTypeFormatter> formatters)
        {
            VerifyAndSetFormatters(formatters);
        }

        /// <summary>
        /// Gets the <see cref="MediaTypeFormatter"/> to use for Xml.
        /// </summary>
        public XmlMediaTypeFormatter XmlFormatter
        {
            get { return Items.OfType<XmlMediaTypeFormatter>().FirstOrDefault(); }
        }

        /// <summary>
        /// Gets the <see cref="MediaTypeFormatter"/> to use for Json.
        /// </summary>
        public JsonMediaTypeFormatter JsonFormatter
        {
            get { return Items.OfType<JsonMediaTypeFormatter>().FirstOrDefault(); }
        }

        /// <summary>
        /// Gets the <see cref="MediaTypeFormatter"/> to use for <c>application/x-www-form-urlencoded</c> data.
        /// </summary>
        public FormUrlEncodedMediaTypeFormatter FormUrlEncodedFormatter
        {
            get { return Items.OfType<FormUrlEncodedMediaTypeFormatter>().FirstOrDefault(); }
        }

        /// <summary>
        /// Helper to search a collection for a formatter that can read the .NET type in the given mediaType.
        /// </summary>
        /// <param name="type">.NET type to read</param>
        /// <param name="mediaType">media type to match on.</param>
        /// <returns>Formatter that can read the type. Null if no formatter found.</returns>
        public MediaTypeFormatter FindReader(Type type, MediaTypeHeaderValue mediaType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (mediaType == null)
            {
                throw Error.ArgumentNull("mediaType");
            }

            foreach (MediaTypeFormatter formatter in this.Items)
            {
                if (formatter.CanReadAs(type, mediaType))
                {
                    return formatter;
                }
            }

            return null;
        }

        /// <summary>
        /// Helper to search a collection for a formatter that can write the .NET type in the given mediaType.
        /// </summary>
        /// <param name="type">.NET type to read</param>
        /// <param name="mediaType">media type to match on.</param>
        /// <returns>Formatter that can write the type. Null if no formatter found.</returns>
        public MediaTypeFormatter FindWriter(Type type, MediaTypeHeaderValue mediaType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (mediaType == null)
            {
                throw Error.ArgumentNull("mediaType");
            }

            foreach (MediaTypeFormatter formatter in Items)
            {
                MediaTypeHeaderValue match;
                if (formatter.CanWriteAs(type, mediaType, out match))
                {
                    return formatter;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the type is one of those loosely defined types that should be excluded from validation
        /// </summary>
        /// <param name="type">.NET <see cref="Type"/> to validate</param>
        /// <returns><c>true</c> if the type should be excluded.</returns>
        public static bool IsTypeExcludedFromValidation(Type type)
        {
            return FormattingUtilities.IsJTokenType(type) || typeof(XObject).IsAssignableFrom(type) || typeof(XmlNode).IsAssignableFrom(type) 
                || typeof(FormDataCollection).IsAssignableFrom(type);
        }

        /// <summary>
        /// Creates a collection of new instances of the default <see cref="MediaTypeFormatter"/>s.
        /// </summary>
        /// <returns>The collection of default <see cref="MediaTypeFormatter"/> instances.</returns>
        private static IEnumerable<MediaTypeFormatter> CreateDefaultFormatters()
        {
            return new MediaTypeFormatter[]
            {
                new JsonMediaTypeFormatter(),
                new XmlMediaTypeFormatter(),
                new FormUrlEncodedMediaTypeFormatter()
            };
        }

        private void VerifyAndSetFormatters(IEnumerable<MediaTypeFormatter> formatters)
        {
            if (formatters == null)
            {
                throw new ArgumentNullException("formatters");
            }

            foreach (MediaTypeFormatter formatter in formatters)
            {
                if (formatter == null)
                {
                    throw new ArgumentException(RS.Format(Properties.Resources.CannotHaveNullInList, _mediaTypeFormatterType.Name), "formatters");
                }

                Add(formatter);
            }
        }
    }
}
