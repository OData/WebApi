using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Performs content negotiation. 
    /// This is the process of selecting a response writer (formatter) in compliance with header values in the request.
    /// </summary>
    public interface IContentNegotiator
    {
        /// <summary>
        /// Performs content negotiating by selecting the most appropriate <see cref="MediaTypeFormatter"/> out of the passed in
        /// <paramref name="formatters"/> for the given <paramref name="request"/> that can serialize an object of the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// Implementations of this method should call <see cref="MediaTypeFormatter.GetPerRequestFormatterInstance(Type, HttpRequestMessage, MediaTypeHeaderValue)"/>
        /// on the selected <see cref="MediaTypeFormatter">formatter</see> and return the result of that method.
        /// </remarks>
        /// <param name="type">The type to be serialized.</param>
        /// <param name="request">Request message, which contains the header values used to negotiate with.</param>
        /// <param name="formatters">The set of <see cref="MediaTypeFormatter"/> objects from which to choose.</param>
        /// <param name="mediaType">The media type that is associated with the formatter chosen for serialization.</param>
        /// <returns>The <see cref="MediaTypeFormatter"/> chosen for serialization or <c>null</c> if their is no appropriate formatter.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#", Justification = "The out parameter is necessary for this API.")]
        MediaTypeFormatter Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters, out MediaTypeHeaderValue mediaType);
    }
}
