using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

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

        public MediaTypeFormatter Find(string mediaType)
        {
            MediaTypeHeaderValue val = MediaTypeHeaderValue.Parse(mediaType);
            return Find(val);
        }

        /// <summary>
        /// Find a formatter in this collection that matches the requested media type.
        /// </summary>
        /// <returns>Returns a formatter or null if not found.</returns>
        public MediaTypeFormatter Find(MediaTypeHeaderValue mediaType)
        {
            var comparer = MediaTypeHeaderValueEqualityComparer.EqualityComparer;
            MediaTypeFormatter formatter = Items.FirstOrDefault(f => f.SupportedMediaTypes.Any(mt => comparer.Equals(mt, mediaType)));
            return formatter;
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
                throw new ArgumentNullException("type");
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
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
        /// Returns true if the type should be excluded from validation.
        /// </summary>
        /// <param name="type">.NET type to validate</param>
        /// <returns>True if the type should be excluded.</returns>
        public static bool IsTypeExcludedFromValidation(Type type)
        {
            return (type.IsAssignableFrom(typeof(JToken)) || type == typeof(XElement) || type == typeof(XElement));
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
