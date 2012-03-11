using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Web.Http.Common;
using System.Web.Http.Properties;

namespace System.Web.Http
{
    /// <summary>
    /// An exception that allows for a given <see cref="HttpResponseMessage"/>
    /// to be returned to the client.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2240:Implement ISerializable correctly", Justification = "This type has no additional serializable state")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "HttpResponseException is not a real exception and is just an easy way to return HttpResponseMessage")]
    [Serializable]
    public class HttpResponseException : Exception
    {
        private const string ResponsePropertyName = "Response";

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        public HttpResponseException()
            : this(HttpStatusCode.InternalServerError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>        
        public HttpResponseException(string message)
            : this(message, HttpStatusCode.InternalServerError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="statusCode">The status code to use with the <see cref="HttpResponseMessage"/>.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
        public HttpResponseException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            HttpResponseMessage response = new HttpResponseMessage(statusCode)
            {
                Content = new ObjectContent<string>(message, new JsonMediaTypeFormatter())
            };
            InitializeResponse(response);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param name="statusCode">The status code to use with the <see cref="HttpResponseMessage"/>.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
        public HttpResponseException(HttpStatusCode statusCode)
            : this(new HttpResponseMessage(statusCode))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param name="response">The response message.</param>
        public HttpResponseException(HttpResponseMessage response)
            : base(Error.Format(SRResources.HttpResponseExceptionMessage, ResponsePropertyName))
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            InitializeResponse(response);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="streamingContext">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "disposed later.")]
        protected HttpResponseException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            InitializeResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }

        /// <summary>
        /// Gets the <see cref="HttpResponseMessage"/> to return to the client.
        /// </summary>
        public HttpResponseMessage Response { get; private set; }

        private void InitializeResponse(HttpResponseMessage response)
        {
            Contract.Assert(response != null, "Response cannot be null!");
            Response = response;
        }
    }
}
