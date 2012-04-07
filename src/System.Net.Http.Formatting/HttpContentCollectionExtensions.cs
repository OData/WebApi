// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods to provide convenience methods for finding <see cref="HttpContent"/> items  
    /// within a <see cref="IEnumerable{HttpContent}"/> collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpContentCollectionExtensions
    {
        private const string ContentID = @"Content-ID";

        /// <summary>
        /// Returns the first <see cref="HttpContent"/> in a sequence that has a <see cref="ContentDispositionHeaderValue"/> header field
        /// with a <see cref="ContentDispositionHeaderValue.DispositionType"/> property equal to <paramref name="dispositionType"/>.
        /// </summary>
        /// <param name="contents">The contents to evaluate</param>
        /// <param name="dispositionType">The disposition type to look for.</param>
        /// <returns>The first <see cref="HttpContent"/> in the sequence with a matching disposition type.</returns>
        public static HttpContent FirstDispositionType(this IEnumerable<HttpContent> contents, string dispositionType)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            if (String.IsNullOrWhiteSpace(dispositionType))
            {
                throw new ArgumentNullException("dispositionType");
            }

            return contents.First((item) =>
            {
                return HttpContentCollectionExtensions.FirstDispositionType(item, dispositionType);
            });
        }

        /// <summary>
        /// Returns the first <see cref="HttpContent"/> in a sequence that has a <see cref="ContentDispositionHeaderValue"/> header field
        /// with a <see cref="ContentDispositionHeaderValue.DispositionType"/> property equal to <paramref name="dispositionType"/>.
        /// </summary>
        /// <param name="contents">The contents to evaluate</param>
        /// <param name="dispositionType">The disposition type to look for.</param>
        /// <returns>null if source is empty or if no element matches; otherwise the first <see cref="HttpContent"/> in 
        /// the sequence with a matching disposition type.</returns>
        public static HttpContent FirstDispositionTypeOrDefault(this IEnumerable<HttpContent> contents, string dispositionType)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            if (String.IsNullOrWhiteSpace(dispositionType))
            {
                throw new ArgumentNullException("dispositionType");
            }

            return contents.FirstOrDefault((item) =>
            {
                return HttpContentCollectionExtensions.FirstDispositionType(item, dispositionType);
            });
        }

        /// <summary>
        /// Returns the first <see cref="HttpContent"/> in a sequence that has a <see cref="ContentDispositionHeaderValue"/> header field
        /// with a <see cref="ContentDispositionHeaderValue.Name"/> property equal to <paramref name="dispositionName"/>.
        /// </summary>
        /// <param name="contents">The contents to evaluate</param>
        /// <param name="dispositionName">The disposition name to look for.</param>
        /// <returns>The first <see cref="HttpContent"/> in the sequence with a matching disposition name.</returns>
        public static HttpContent FirstDispositionName(this IEnumerable<HttpContent> contents, string dispositionName)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            if (String.IsNullOrWhiteSpace(dispositionName))
            {
                throw new ArgumentNullException("dispositionName");
            }

            return contents.First((item) =>
            {
                return HttpContentCollectionExtensions.FirstDispositionName(item, dispositionName);
            });
        }

        /// <summary>
        /// Returns the first <see cref="HttpContent"/> in a sequence that has a <see cref="ContentDispositionHeaderValue"/> header field
        /// with a <see cref="ContentDispositionHeaderValue.Name"/> property equal to <paramref name="dispositionName"/>.
        /// </summary>
        /// <param name="contents">The contents to evaluate</param>
        /// <param name="dispositionName">The disposition name to look for.</param>
        /// <returns>null if source is empty or if no element matches; otherwise the first <see cref="HttpContent"/> in 
        /// the sequence with a matching disposition name.</returns>
        public static HttpContent FirstDispositionNameOrDefault(this IEnumerable<HttpContent> contents, string dispositionName)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            if (String.IsNullOrWhiteSpace(dispositionName))
            {
                throw new ArgumentNullException("dispositionName");
            }

            return contents.FirstOrDefault((item) =>
            {
                return HttpContentCollectionExtensions.FirstDispositionName(item, dispositionName);
            });
        }

        /// <summary>
        /// Returns the <c>start</c> multipart body part. The <c>start</c> is used to identify the main body 
        /// in <c>multipart/related</c> content (see RFC 2387).
        /// </summary>
        /// <param name="contents">The contents to evaluate.</param>
        /// <param name="start">The <c>start</c> value to look for. 
        /// A match is found if a <see cref="HttpContent"/> has a <c>Content-ID</c> 
        /// header field with the given value.</param>
        /// <returns>The first <see cref="HttpContent"/> in the sequence with a matching value.</returns>
        public static HttpContent FirstStart(this IEnumerable<HttpContent> contents, string start)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            if (String.IsNullOrWhiteSpace(start))
            {
                throw new ArgumentNullException("start");
            }

            return contents.First((item) =>
            {
                return HttpContentCollectionExtensions.FirstStart(item, start);
            });
        }

        /// <summary>
        /// Returns the first <see cref="HttpContent"/> in a sequence that has a <see cref="ContentDispositionHeaderValue"/> header field
        /// parameter equal to <paramref name="start"/>. This parameter is typically used in connection with <c>multipart/related</c>
        /// content (see RFC 2387).
        /// </summary>
        /// <param name="contents">The contents to evaluate.</param>
        /// <param name="start">The start value to look for. A match is found if a <see cref="HttpContent"/> has a <c>Content-ID</c> 
        /// header field with the given value.</param>
        /// <returns>null if source is empty or if no element matches; otherwise the first <see cref="HttpContent"/> in 
        /// the sequence with a matching value.</returns>
        public static HttpContent FirstStartOrDefault(this IEnumerable<HttpContent> contents, string start)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            if (String.IsNullOrWhiteSpace(start))
            {
                throw new ArgumentNullException("start");
            }

            return contents.FirstOrDefault((item) =>
            {
                return HttpContentCollectionExtensions.FirstStart(item, start);
            });
        }

        /// <summary>
        /// Returns all instances of <see cref="HttpContent"/> in a sequence that has a <see cref="MediaTypeHeaderValue"/> header field
        /// with a <see cref="MediaTypeHeaderValue.MediaType"/> property equal to the provided <paramref name="contentType"/>.
        /// </summary>
        /// <param name="contents">The content to evaluate</param>
        /// <param name="contentType">The media type to look for.</param>
        /// <returns>null if source is empty or if no element matches; otherwise the first <see cref="HttpContent"/> in 
        /// the sequence with a matching media type.</returns>
        public static IEnumerable<HttpContent> FindAllContentType(this IEnumerable<HttpContent> contents, string contentType)
        {
            if (String.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentNullException("contentType");
            }

            return HttpContentCollectionExtensions.FindAllContentType(contents, new MediaTypeHeaderValue(contentType));
        }

        /// <summary>
        /// Returns all instances of <see cref="HttpContent"/> in a sequence that has a <see cref="MediaTypeHeaderValue"/> header field
        /// with a <see cref="MediaTypeHeaderValue.MediaType"/> property equal to the provided <paramref name="contentType"/>.
        /// </summary>
        /// <param name="contents">The content to evaluate</param>
        /// <param name="contentType">The media type to look for.</param>
        /// <returns>null if source is empty or if no element matches; otherwise the first <see cref="HttpContent"/> in 
        /// the sequence with a matching media type.</returns>
        public static IEnumerable<HttpContent> FindAllContentType(this IEnumerable<HttpContent> contents, MediaTypeHeaderValue contentType)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }

            return contents.Where((item) =>
            {
                return HttpContentCollectionExtensions.FindAllContentType(item, contentType);
            });
        }

        private static bool FirstStart(HttpContent content, string start)
        {
            Contract.Assert(content != null, "content cannot be null");
            Contract.Assert(start != null, "start cannot be null");
            if (content.Headers != null)
            {
                IEnumerable<string> values;
                if (content.Headers.TryGetValues(ContentID, out values))
                {
                    return String.Equals(
                        FormattingUtilities.UnquoteToken(values.ElementAt(0)),
                        FormattingUtilities.UnquoteToken(start),
                        StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        private static bool FirstDispositionType(HttpContent content, string dispositionType)
        {
            Contract.Assert(content != null, "content cannot be null");
            Contract.Assert(dispositionType != null, "dispositionType cannot be null");
            return content.Headers != null && content.Headers.ContentDisposition != null &&
                   String.Equals(
                       FormattingUtilities.UnquoteToken(content.Headers.ContentDisposition.DispositionType),
                       FormattingUtilities.UnquoteToken(dispositionType),
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool FirstDispositionName(HttpContent content, string dispositionName)
        {
            Contract.Assert(content != null, "content cannot be null");
            Contract.Assert(dispositionName != null, "dispositionName cannot be null");
            return content.Headers != null && content.Headers.ContentDisposition != null &&
                   String.Equals(
                       FormattingUtilities.UnquoteToken(content.Headers.ContentDisposition.Name),
                       FormattingUtilities.UnquoteToken(dispositionName),
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool FindAllContentType(HttpContent content, MediaTypeHeaderValue contentType)
        {
            Contract.Assert(content != null, "content cannot be null");
            Contract.Assert(contentType != null, "contentType cannot be null");
            return content.Headers != null && content.Headers.ContentType != null &&
                   String.Equals(
                       content.Headers.ContentType.MediaType,
                       contentType.MediaType,
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
