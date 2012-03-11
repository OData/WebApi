using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s from path extensions appearing
    /// in a <see cref="Uri"/>.
    /// </summary>
    public sealed class UriPathExtensionMapping : MediaTypeMapping
    {
        private static readonly Type _uriPathExtensionMappingType = typeof(UriPathExtensionMapping);

        /// <summary>
        /// Initializes a new instance of the <see cref="UriPathExtensionMapping"/> class.
        /// </summary>
        /// <param name="uriPathExtension">The extension corresponding to <paramref name="mediaType"/>.
        /// This value should not include a dot or wildcards.</param>
        /// <param name="mediaType">The media type that will be returned
        /// if <paramref name="uriPathExtension"/> is matched.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "There is no meaningful System.Uri representation for a path suffix such as '.xml'")]
        public UriPathExtensionMapping(string uriPathExtension, string mediaType)
            : base(mediaType)
        {
            Initialize(uriPathExtension);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UriPathExtensionMapping"/> class.
        /// </summary>
        /// <param name="uriPathExtension">The extension corresponding to <paramref name="mediaType"/>.
        /// This value should not include a dot or wildcards.</param>
        /// <param name="mediaType">The <see cref="MediaTypeHeaderValue"/> that will be returned
        /// if <paramref name="uriPathExtension"/> is matched.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "There is no meaningful System.Uri representation for a path suffix such as '.xml'")]
        public UriPathExtensionMapping(string uriPathExtension, MediaTypeHeaderValue mediaType)
            : base(mediaType)
        {
            Initialize(uriPathExtension);
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> path extension.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "There is no meaningful System.Uri representation for a path suffix such as '.xml'")]
        public string UriPathExtension { get; private set; }

        /// <summary>
        /// Returns a value indicating whether this <see cref="UriPathExtensionMapping"/>
        /// instance can provide a <see cref="MediaTypeHeaderValue"/> for the <see cref="Uri"/> 
        /// of <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to check.</param>
        /// <returns>If this instance can match a file extension in <paramref name="request"/>
        /// it returns <c>1.0</c> otherwise <c>0.0</c>.</returns>
        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string extension = GetUriPathExtensionOrNull(request.RequestUri);
            return String.Equals(extension, UriPathExtension, StringComparison.OrdinalIgnoreCase) ? MediaTypeMatch.Match : MediaTypeMatch.NoMatch;
        }

        private static string GetUriPathExtensionOrNull(Uri uri)
        {
            if (uri == null)
            {
                throw new InvalidOperationException(RS.Format(Properties.Resources.NonNullUriRequiredForMediaTypeMapping, _uriPathExtensionMappingType.Name));
            }

            string uriPathExtension = null;
            int numberOfSegments = uri.Segments.Length;
            if (numberOfSegments > 0)
            {
                string lastSegment = uri.Segments[numberOfSegments - 1];
                int indexAfterFirstPeriod = lastSegment.IndexOf('.') + 1;
                if (indexAfterFirstPeriod > 0 && indexAfterFirstPeriod < lastSegment.Length)
                {
                    uriPathExtension = lastSegment.Substring(indexAfterFirstPeriod);
                }
            }

            return uriPathExtension;
        }

        private void Initialize(string uriPathExtension)
        {
            if (String.IsNullOrWhiteSpace(uriPathExtension))
            {
                throw new ArgumentNullException("uriPathExtension");
            }

            UriPathExtension = uriPathExtension.Trim().TrimStart('.');
        }
    }
}
