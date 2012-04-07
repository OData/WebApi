// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods to allow strongly typed objects to be read from <see cref="HttpContent"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpContentExtensions
    {
        /// <summary>
        /// Returns a <see cref="Task"/> that will yield an object of the specified <paramref name="type"/>
        /// from the <paramref name="content"/> instance.
        /// </summary>
        /// <remarks>This override use the built-in collection of formatters.</remarks>
        /// <param name="content">The <see cref="HttpContent"/> instance from which to read.</param>
        /// <param name="type">The type of the object to read.</param>
        /// <returns>A <see cref="Task"/> that will yield an object instance of the specified type.</returns>
        public static Task<object> ReadAsAsync(this HttpContent content, Type type)
        {
            return content.ReadAsAsync(type, new MediaTypeFormatterCollection());
        }

        /// <summary>
        /// Returns a <see cref="Task"/> that will yield an object of the specified <paramref name="type"/>
        /// from the <paramref name="content"/> instance using one of the provided <paramref name="formatters"/>
        /// to deserialize the content.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> instance from which to read.</param>
        /// <param name="type">The type of the object to read.</param>
        /// <param name="formatters">The collection of <see cref="MediaTypeFormatter"/> instances to use.</param>
        /// <returns>An object instance of the specified type.</returns>
        public static Task<object> ReadAsAsync(this HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters)
        {
            return ReadAsAsync<object>(content, type, formatters, null);
        }

        /// <summary>
        /// Returns a <see cref="Task"/> that will yield an object of the specified <paramref name="type"/>
        /// from the <paramref name="content"/> instance using one of the provided <paramref name="formatters"/>
        /// to deserialize the content.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> instance from which to read.</param>
        /// <param name="type">The type of the object to read.</param>
        /// <param name="formatters">The collection of <see cref="MediaTypeFormatter"/> instances to use.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>An object instance of the specified type.</returns>
        public static Task<object> ReadAsAsync(this HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            return ReadAsAsync<object>(content, type, formatters, formatterLogger);
        }

        /// <summary>
        /// Returns a <see cref="Task"/> that will yield an object of the specified
        /// type <typeparamref name="T"/> from the <paramref name="content"/> instance.
        /// </summary>
        /// <remarks>This override use the built-in collection of formatters.</remarks>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <param name="content">The <see cref="HttpContent"/> instance from which to read.</param>
        /// <returns>An object instance of the specified type.</returns>
        public static Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            return content.ReadAsAsync<T>(new MediaTypeFormatterCollection());
        }

        /// <summary>
        /// Returns a <see cref="Task"/> that will yield an object of the specified
        /// type <typeparamref name="T"/> from the <paramref name="content"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <param name="content">The <see cref="HttpContent"/> instance from which to read.</param>
        /// <param name="formatters">The collection of <see cref="MediaTypeFormatter"/> instances to use.</param>
        /// <returns>An object instance of the specified type.</returns>
        public static Task<T> ReadAsAsync<T>(this HttpContent content, IEnumerable<MediaTypeFormatter> formatters)
        {
            return ReadAsAsync<T>(content, typeof(T), formatters, null);
        }

        /// <summary>
        /// Returns a <see cref="Task"/> that will yield an object of the specified
        /// type <typeparamref name="T"/> from the <paramref name="content"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <param name="content">The <see cref="HttpContent"/> instance from which to read.</param>
        /// <param name="formatters">The collection of <see cref="MediaTypeFormatter"/> instances to use.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// <returns>An object instance of the specified type.</returns>
        public static Task<T> ReadAsAsync<T>(this HttpContent content, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            return ReadAsAsync<T>(content, typeof(T), formatters, formatterLogger);
        }

        // There are many helper overloads for ReadAs*(). Provide one worker function to ensure the logic is shared.
        //
        // For loosely typed, T = Object, type = specific class.
        // For strongly typed, T == type.GetType()
        private static Task<T> ReadAsAsync<T>(HttpContent content, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (formatters == null)
            {
                throw new ArgumentNullException("formatters");
            }

            ObjectContent objectContent = content as ObjectContent;
            if (objectContent != null && objectContent.Value != null && type.IsAssignableFrom(objectContent.Value.GetType()))
            {
                return TaskHelpers.FromResult((T)objectContent.Value);
            }

            MediaTypeFormatter formatter = null;
            MediaTypeHeaderValue mediaType = content.Headers.ContentType;
            if (mediaType != null)
            {
                formatter = new MediaTypeFormatterCollection(formatters).FindReader(type, mediaType);
            }

            if (formatter == null)
            {
                string mediaTypeAsString = mediaType != null ? mediaType.MediaType : Properties.Resources.UndefinedMediaType;
                throw new InvalidOperationException(
                    RS.Format(Properties.Resources.NoReadSerializerAvailable, type.Name, mediaTypeAsString));
            }

            return content.ReadAsStreamAsync()
                          .Then(stream => formatter.ReadFromStreamAsync(type, stream, content.Headers, formatterLogger)
                          .Then(value => (T)value));
        }
    }
}
