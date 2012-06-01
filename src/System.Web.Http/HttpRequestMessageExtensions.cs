// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Gets the <see cref="HttpConfiguration"/> for the given request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="HttpConfiguration"/>.</returns>
        public static HttpConfiguration GetConfiguration(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetProperty<HttpConfiguration>(HttpPropertyKeys.HttpConfigurationKey);
        }

        /// <summary>
        /// Gets the dependency resolver scope associated with this <see cref="HttpRequestMessage"/>.
        /// Services which are retrieved from this scope will be released when the request is
        /// cleaned up by the framework.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="IDependencyScope"/> for the given request.</returns>
        public static IDependencyScope GetDependencyScope(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IDependencyScope result;
            if (!request.Properties.TryGetValue<IDependencyScope>(HttpPropertyKeys.DependencyScope, out result))
            {
                result = request.GetConfiguration().DependencyResolver.BeginScope();
                request.Properties[HttpPropertyKeys.DependencyScope] = result;
                request.RegisterForDispose(result);
            }

            return result;
        }

        /// <summary>
        /// Gets the <see cref="System.Threading.SynchronizationContext"/> for the given request or null if not available.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="System.Threading.SynchronizationContext"/> or null.</returns>
        public static SynchronizationContext GetSynchronizationContext(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetProperty<SynchronizationContext>(HttpPropertyKeys.SynchronizationContextKey);
        }

        /// <summary>
        /// Gets the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2"/> or null if not available.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2"/> or null.</returns>
        public static X509Certificate2 GetClientCertificate(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            X509Certificate2 result = null;

            if (!request.Properties.TryGetValue(HttpPropertyKeys.ClientCertificateKey, out result))
            {
                // now let us get out the delegate and try to invoke it
                Func<HttpRequestMessage, X509Certificate2> retrieveCertificate;

                if (request.Properties.TryGetValue(HttpPropertyKeys.RetrieveClientCertificateDelegateKey, out retrieveCertificate))
                {
                    result = retrieveCertificate(request);

                    if (result != null)
                    {
                        request.Properties.Add(HttpPropertyKeys.ClientCertificateKey, result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the <see cref="System.Web.Http.Routing.IHttpRouteData"/> for the given request or null if not available.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="System.Web.Http.Routing.IHttpRouteData"/> or null.</returns>
        public static IHttpRouteData GetRouteData(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetProperty<IHttpRouteData>(HttpPropertyKeys.HttpRouteDataKey);
        }

        private static T GetProperty<T>(this HttpRequestMessage request, string key)
        {
            T value;
            request.Properties.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> representing an error 
        /// with an instance of <see cref="ObjectContent{T}"/> wrapping an <see cref="HttpError"/> with message <paramref name="message"/>.
        /// If no formatter is found, this method returns a response with status 406 NotAcceptable.
        /// </summary>
        /// <remarks>
        /// This method requires that <paramref name="request"/> has been associated with an instance of 
        /// <see cref="HttpConfiguration"/>.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="message">The error message.</param>
        /// <returns>An error response with error message <paramref name="message"/> and status code <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, string message)
        {
            return request.CreateErrorResponse(statusCode, new HttpError(message));
        }

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> representing an error 
        /// with an instance of <see cref="ObjectContent{T}"/> wrapping an <see cref="HttpError"/> with message <paramref name="message"/>
        /// and message detail <paramref name="messageDetail"/>.If no formatter is found, this method returns a response with 
        /// status 406 NotAcceptable.
        /// </summary>
        /// <remarks>
        /// This method requires that <paramref name="request"/> has been associated with an instance of 
        /// <see cref="HttpConfiguration"/>.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="message">The error message. This message will always be seen by clients.</param>
        /// <param name="messageDetail">The error message detail. This message will only be seen by clients if we should include error detail.</param>
        /// <returns>An error response with error message <paramref name="message"/> and message detail <paramref name="messageDetail"/>
        /// and status code <paramref name="statusCode"/>.</returns>
        internal static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, string message, string messageDetail)
        {
            return request.CreateErrorResponse(statusCode, includeErrorDetail => includeErrorDetail ? new HttpError(message, messageDetail) : new HttpError(message));
        }

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> representing an error 
        /// with an instance of <see cref="ObjectContent{T}"/> wrapping an <see cref="HttpError"/> with error message <paramref name="message"/>
        /// for exception <paramref name="exception"/>. If no formatter is found, this method returns a response with status 406 NotAcceptable.
        /// </summary>
        /// <remarks>
        /// This method requires that <paramref name="request"/> has been associated with an instance of
        /// <see cref="HttpConfiguration"/>.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="message">The error message.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>An error response for <paramref name="exception"/> with error message <paramref name="message"/>
        /// and status code <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, string message, Exception exception)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.CreateErrorResponse(statusCode, includeErrorDetail => new HttpError(exception, includeErrorDetail) { Message = message });
        }

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> representing an error 
        /// with an instance of <see cref="ObjectContent{T}"/> wrapping an <see cref="HttpError"/> for exception <paramref name="exception"/>.
        /// If no formatter is found, this method returns a response with status 406 NotAcceptable.
        /// </summary>
        /// <remarks>
        /// This method requires that <paramref name="request"/> has been associated with an instance of
        /// <see cref="HttpConfiguration"/>.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>An error response for <paramref name="exception"/> with status code <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, Exception exception)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.CreateErrorResponse(statusCode, includeErrorDetail => new HttpError(exception, includeErrorDetail));
        }

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> representing an error 
        /// with an instance of <see cref="ObjectContent{T}"/> wrapping an <see cref="HttpError"/> for model state <paramref name="modelState"/>.
        /// If no formatter is found, this method returns a response with status 406 NotAcceptable.
        /// </summary>
        /// <remarks>
        /// This method requires that <paramref name="request"/> has been associated with an instance of
        /// <see cref="HttpConfiguration"/>.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="modelState">The model state.</param>
        /// <returns>An error response for <paramref name="modelState"/> with status code <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, ModelStateDictionary modelState)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.CreateErrorResponse(statusCode, includeErrorDetail => new HttpError(modelState, includeErrorDetail));
        }

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> representing an error 
        /// with an instance of <see cref="ObjectContent{T}"/> wrapping <paramref name="error"/> as the content. If no formatter 
        /// is found, this method returns a response with status 406 NotAcceptable.
        /// </summary>
        /// <remarks>
        /// This method requires that <paramref name="request"/> has been associated with an instance of
        /// <see cref="HttpConfiguration"/>.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="error">The error to wrap.</param>
        /// <returns>An error response wrapping <paramref name="error"/> with status code <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, HttpError error)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.CreateErrorResponse(statusCode, includeErrorDetail => error);
        }

        private static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, Func<bool, HttpError> errorCreator)
        {
            HttpConfiguration configuration = request.GetConfiguration();

            // CreateErrorResponse should never fail, even if there is no configuration associated with the request
            // In that case, use the default HttpConfiguration to con-neg the response media type
            if (configuration == null)
            {
                using (HttpConfiguration defaultConfig = new HttpConfiguration())
                {
                    HttpError error = errorCreator(defaultConfig.ShouldIncludeErrorDetail(request));
                    return request.CreateResponse<HttpError>(statusCode, error, defaultConfig);
                }
            }
            else
            {
                HttpError error = errorCreator(configuration.ShouldIncludeErrorDetail(request));
                return request.CreateResponse<HttpError>(statusCode, error, configuration);
            }
        }

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> with an instance
        /// of <see cref="ObjectContent{T}"/> as the content if a formatter can be found. If no formatter is found, this
        /// method returns a response with status 406 NotAcceptable. This forwards the call to
        /// <see cref="CreateResponse{T}(HttpRequestMessage, HttpStatusCode, T, HttpConfiguration)"/> with a <c>null</c>
        /// configuration.
        /// </summary>
        /// <remarks>
        /// This method requires that <paramref name="request"/> has been associated with an instance of
        /// <see cref="HttpConfiguration"/>.
        /// </remarks>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="value">The value to wrap. Can be <c>null</c>.</param>
        /// <returns>A response wrapping <paramref name="value"/> with <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value)
        {
            return request.CreateResponse<T>(statusCode, value, configuration: null);
        }

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> with an instance
        /// of <see cref="ObjectContent{T}"/> as the content if a formatter can be found. If no formatter is found, this
        /// method returns a response with status 406 NotAcceptable.
        /// </summary>
        /// <remarks>
        /// This method will use the provided <paramref name="configuration"/> or it will get the 
        /// <see cref="HttpConfiguration"/> instance associated with <paramref name="request"/>.
        /// </remarks>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="value">The value to wrap. Can be <c>null</c>.</param>
        /// <param name="configuration">The configuration to use. Can be <c>null</c>.</param>
        /// <returns>A response wrapping <paramref name="value"/> with <paramref name="statusCode"/>.</returns>   
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller will dispose")]
        public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value, HttpConfiguration configuration)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            configuration = configuration ?? request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.HttpRequestMessageExtensions_NoConfiguration);
            }

            IContentNegotiator contentNegotiator = configuration.Services.GetContentNegotiator();
            if (contentNegotiator == null)
            {
                throw Error.InvalidOperation(SRResources.HttpRequestMessageExtensions_NoContentNegotiator, typeof(IContentNegotiator).FullName);
            }

            IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;

            // Run content negotiation
            ContentNegotiationResult result = contentNegotiator.Negotiate(typeof(T), request, formatters);

            if (result == null)
            {
                // no result from content negotiation indicates that 406 should be sent.
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    RequestMessage = request,
                };
            }
            else
            {
                MediaTypeHeaderValue mediaType = result.MediaType;
                return new HttpResponseMessage
                {
                    // At this point mediaType should be a cloned value (the content negotiator is responsible for returning a new copy)
                    Content = new ObjectContent<T>(value, result.Formatter, mediaType),
                    StatusCode = statusCode,
                    RequestMessage = request
                };
            }
        }

        /// <summary>
        /// Helper method that creates a <see cref="HttpResponseMessage"/> with an <see cref="ObjectContent{T}"/> instance containing the provided
        /// <paramref name="value"/>. The given <paramref name="mediaType"/> is used to find an instance of <see cref="MediaTypeFormatter"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="value">The value to wrap. Can be <c>null</c>.</param>
        /// <param name="mediaType">The media type used to look up an instance of <see cref="MediaTypeFormatter"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if the <paramref name="request"/> does not have an associated
        /// <see cref="HttpConfiguration"/> instance or if the configuration does not have a formatter matching <paramref name="mediaType"/>.</exception>
        /// <returns>A response wrapping <paramref name="value"/> with <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value, string mediaType)
        {
            return request.CreateResponse(statusCode, value, new MediaTypeHeaderValue(mediaType));
        }

        /// <summary>
        /// Helper method that creates a <see cref="HttpResponseMessage"/> with an <see cref="ObjectContent{T}"/> instance containing the provided
        /// <paramref name="value"/>. The given <paramref name="mediaType"/> is used to find an instance of <see cref="MediaTypeFormatter"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="value">The value to wrap. Can be <c>null</c>.</param>
        /// <param name="mediaType">The media type used to look up an instance of <see cref="MediaTypeFormatter"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if the <paramref name="request"/> does not have an associated
        /// <see cref="HttpConfiguration"/> instance or if the configuration does not have a formatter matching <paramref name="mediaType"/>.</exception>
        /// <returns>A response wrapping <paramref name="value"/> with <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value, MediaTypeHeaderValue mediaType)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }
            if (mediaType == null)
            {
                throw Error.ArgumentNull("mediaType");
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.HttpRequestMessageExtensions_NoConfiguration);
            }

            MediaTypeFormatter formatter = configuration.Formatters.FindWriter(typeof(T), mediaType);
            if (formatter == null)
            {
                throw Error.InvalidOperation(SRResources.HttpRequestMessageExtensions_NoMatchingFormatter, mediaType, typeof(T).Name);
            }

            return request.CreateResponse(statusCode, value, formatter, mediaType);
        }

        /// <summary>
        /// Helper method that creates a <see cref="HttpResponseMessage"/> with an <see cref="ObjectContent{T}"/> instance containing the provided
        /// <paramref name="value"/> and the given <paramref name="formatter"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="value">The value to wrap. Can be <c>null</c>.</param>
        /// <param name="formatter">The formatter to use.</param>
        /// <returns>A response wrapping <paramref name="value"/> with <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value, MediaTypeFormatter formatter)
        {
            return request.CreateResponse(statusCode, value, formatter, (MediaTypeHeaderValue)null);
        }

        /// <summary>
        /// Helper method that creates a <see cref="HttpResponseMessage"/> with an <see cref="ObjectContent{T}"/> instance containing the provided
        /// <paramref name="value"/> and the given <paramref name="formatter"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="value">The value to wrap. Can be <c>null</c>.</param>
        /// <param name="formatter">The formatter to use.</param>
        /// <param name="mediaType">The media type override to set on the response's content. Can be <c>null</c>.</param>
        /// <returns>A response wrapping <paramref name="value"/> with <paramref name="statusCode"/>.</returns>
        public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value, MediaTypeFormatter formatter, string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeader = mediaType != null ? new MediaTypeHeaderValue(mediaType) : null;
            return request.CreateResponse(statusCode, value, formatter, mediaTypeHeader);
        }

        /// <summary>
        /// Helper method that creates a <see cref="HttpResponseMessage"/> with an <see cref="ObjectContent{T}"/> instance containing the provided
        /// <paramref name="value"/> and the given <paramref name="formatter"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="value">The value to wrap. Can be <c>null</c>.</param>
        /// <param name="formatter">The formatter to use.</param>
        /// <param name="mediaType">The media type override to set on the response's content. Can be <c>null</c>.</param>
        /// <returns>A response wrapping <paramref name="value"/> with <paramref name="statusCode"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller will dispose")]
        public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }
            if (formatter == null)
            {
                throw Error.ArgumentNull("formatter");
            }

            HttpResponseMessage response = request.CreateResponse(statusCode);
            response.Content = new ObjectContent<T>(value, formatter, mediaType);
            return response;
        }

        /// <summary>
        /// Adds the given <paramref name="resource"/> to a list of resources that will be disposed by a host once
        /// the <paramref name="request"/> is disposed.
        /// </summary>
        /// <param name="request">The request controlling the lifecycle of <paramref name="resource"/>.</param>
        /// <param name="resource">The resource to dispose when <paramref name="request"/> is being disposed. Can be <c>null</c>.</param>
        public static void RegisterForDispose(this HttpRequestMessage request, IDisposable resource)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (resource == null)
            {
                return;
            }

            List<IDisposable> trackedResources;
            if (!request.Properties.TryGetValue(HttpPropertyKeys.DisposableRequestResourcesKey, out trackedResources))
            {
                trackedResources = new List<IDisposable>();
                request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey] = trackedResources;
            }

            trackedResources.Add(resource);
        }

        /// <summary>
        /// Disposes of all tracked resources associated with the <paramref name="request"/> which were added via the
        /// <see cref="RegisterForDispose(HttpRequestMessage, IDisposable)"/> method.
        /// </summary>
        /// <param name="request">The request.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to ignore all exceptions.")]
        public static void DisposeRequestResources(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            List<IDisposable> resourcesToDispose;
            if (request.Properties.TryGetValue(HttpPropertyKeys.DisposableRequestResourcesKey, out resourcesToDispose))
            {
                foreach (IDisposable resource in resourcesToDispose)
                {
                    try
                    {
                        resource.Dispose();
                    }
                    catch
                    {
                        // ignore exceptions
                    }
                }
                resourcesToDispose.Clear();
            }
        }

        /// <summary>
        /// Retrieves the <see cref="Guid"/> which has been assigned as the
        /// correlation id associated with the given <paramref name="request"/>.
        /// The value will be created and set the first time this method is called.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>The <see cref="Guid"/> associated with that request.</returns>
        public static Guid GetCorrelationId(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Guid correlationId;
            if (!request.Properties.TryGetValue<Guid>(HttpPropertyKeys.RequestCorrelationKey, out correlationId))
            {
                correlationId = Guid.NewGuid();
                request.Properties.Add(HttpPropertyKeys.RequestCorrelationKey, correlationId);
            }

            return correlationId;
        }

        /// <summary>
        /// Retrieves the parsed query string as a collection of key-value pairs.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>The query string as a collection of key-value pairs.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "NameValuePairsValueProvider takes an IEnumerable<KeyValuePair<string, string>>")]
        public static IEnumerable<KeyValuePair<string, string>> GetQueryNameValuePairs(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Uri uri = request.RequestUri;

            // Unit tests may not always provide a Uri in the request
            if (uri == null || String.IsNullOrEmpty(uri.Query))
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }

            IEnumerable<KeyValuePair<string, string>> queryString;
            if (!request.Properties.TryGetValue<IEnumerable<KeyValuePair<string, string>>>(HttpPropertyKeys.RequestQueryNameValuePairsKey, out queryString))
            {
                // Uri --> FormData --> NVC
                FormDataCollection formData = new FormDataCollection(uri);
                queryString = formData.GetJQueryNameValuePairs();
                request.Properties.Add(HttpPropertyKeys.RequestQueryNameValuePairsKey, queryString);
            }

            return queryString;
        }

        public static UrlHelper GetUrlHelper(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return new UrlHelper(request);
        }
    }
}
