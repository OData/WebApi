// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Constants related to media types.
    /// </summary>
    internal static class MediaTypeConstants
    {
        private static readonly MediaTypeHeaderValue _defaultApplicationXmlMediaType = new MediaTypeHeaderValue("application/xml");
        private static readonly MediaTypeHeaderValue _defaultTextXmlMediaType = new MediaTypeHeaderValue("text/xml");
        private static readonly MediaTypeHeaderValue _defaultApplicationJsonMediaType = new MediaTypeHeaderValue("application/json");
        private static readonly MediaTypeHeaderValue _defaultTextJsonMediaType = new MediaTypeHeaderValue("text/json");
        private static readonly MediaTypeHeaderValue _defaultApplicationOctetStreamMediaType = new MediaTypeHeaderValue("application/octet-stream");
        private static readonly MediaTypeHeaderValue _defaultApplicationFormUrlEncodedMediaType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        /// <summary>
        /// Gets a <see cref="MediaTypeHeaderValue"/> instance representing <c>application/octet-stream</c>.
        /// </summary>
        /// <value>
        /// A new <see cref="MediaTypeHeaderValue"/> instance representing <c>application/octet-stream</c>.
        /// </value>
        public static MediaTypeHeaderValue ApplicationOctetStreamMediaType
        {
            get { return _defaultApplicationOctetStreamMediaType.Clone(); }
        }

        /// <summary>
        /// Gets a <see cref="MediaTypeHeaderValue"/> instance representing <c>application/xml</c>.
        /// </summary>
        /// <value>
        /// A new <see cref="MediaTypeHeaderValue"/> instance representing <c>application/xml</c>.
        /// </value>
        public static MediaTypeHeaderValue ApplicationXmlMediaType
        {
            get { return _defaultApplicationXmlMediaType.Clone(); }
        }

        /// <summary>
        /// Gets a <see cref="MediaTypeHeaderValue"/> instance representing <c>application/json</c>.
        /// </summary>
        /// <value>
        /// A new <see cref="MediaTypeHeaderValue"/> instance representing <c>application/json</c>.
        /// </value>
        public static MediaTypeHeaderValue ApplicationJsonMediaType
        {
            get { return _defaultApplicationJsonMediaType.Clone(); }
        }

        /// <summary>
        /// Gets a <see cref="MediaTypeHeaderValue"/> instance representing <c>text/xml</c>.
        /// </summary>
        /// <value>
        /// A new <see cref="MediaTypeHeaderValue"/> instance representing <c>text/xml</c>.
        /// </value>
        public static MediaTypeHeaderValue TextXmlMediaType
        {
            get { return _defaultTextXmlMediaType.Clone(); }
        }

        /// <summary>
        /// Gets a <see cref="MediaTypeHeaderValue"/> instance representing <c>text/json</c>.
        /// </summary>
        /// <value>
        /// A new <see cref="MediaTypeHeaderValue"/> instance representing <c>text/json</c>.
        /// </value>
        public static MediaTypeHeaderValue TextJsonMediaType
        {
            get { return _defaultTextJsonMediaType.Clone(); }
        }

        /// <summary>
        /// Gets a <see cref="MediaTypeHeaderValue"/> instance representing <c>application/x-www-form-urlencoded</c>.
        /// </summary>
        /// <value>
        /// A new <see cref="MediaTypeHeaderValue"/> instance representing <c>application/x-www-form-urlencoded</c>.
        /// </value>
        public static MediaTypeHeaderValue ApplicationFormUrlEncodedMediaType
        {
            get { return _defaultApplicationFormUrlEncodedMediaType.Clone(); }
        }
    }
}
