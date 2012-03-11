using System.IO;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods for <see cref="ContentDispositionHeaderValue"/>.
    /// </summary>
    internal static class ContentDispositionHeaderValueExtensions
    {
        private static readonly Type _contentDispositionHeaderValueType = typeof(ContentDispositionHeaderValue);

        /// <summary>
        /// Returns a file name suitable for use on the local file system. The file name is extracted from 
        /// <see cref="ContentDispositionHeaderValue.FileNameStar"/> and <see cref="ContentDispositionHeaderValue.FileName"/>
        /// in that order.
        /// </summary>
        /// <param name="contentDisposition">The content disposition to extract a local file name from.</param>
        /// <returns>A file name (without any path components) suitable for use on local file system.</returns>
        public static string ExtractLocalFileName(this ContentDispositionHeaderValue contentDisposition)
        {
            if (contentDisposition == null)
            {
                throw new ArgumentNullException("contentDisposition");
            }

            string candidate = contentDisposition.FileNameStar;
            if (String.IsNullOrEmpty(candidate))
            {
                candidate = contentDisposition.FileName;
            }

            if (String.IsNullOrWhiteSpace(candidate))
            {
                throw new ArgumentException(
                    RS.Format(Properties.Resources.ContentDispositionInvalidFileName, _contentDispositionHeaderValueType.Name, candidate),
                    "contentDisposition");
            }

            string unquotedFileName = FormattingUtilities.UnquoteToken(candidate);
            if (String.IsNullOrWhiteSpace(unquotedFileName))
            {
                throw new ArgumentException(
                    RS.Format(Properties.Resources.ContentDispositionInvalidFileName, _contentDispositionHeaderValueType.Name, unquotedFileName),
                    "contentDisposition");
            }

            // Get rid of all path components
            return Path.GetFileName(unquotedFileName);
        }
    }
}
