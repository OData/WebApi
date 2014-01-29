// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;
using Extensions = System.Web.Http.OData.Extensions;
using Routing = System.Web.Http.OData.Routing;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataHttpRequestMessageExtensions
    {
        /// <summary>
        /// Gets the EDM model associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The EDM model associated with this request or <c>null</c> if there isn't one.</returns>
        [Obsolete("This method is obsolete; use the ODataProperties().Model property from the " + 
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static IEdmModel GetEdmModel(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).Model;
        }

        /// <summary>
        /// Sets the EDM model associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="model">The EDM model to associate with the request.</param>
        [Obsolete("This method is obsolete; use the ODataProperties().Model property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void SetEdmModel(this HttpRequestMessage request, IEdmModel model)
        {
            Extensions.HttpRequestMessageExtensions.ODataProperties(request).Model = model;
        }

        /// <summary>
        /// Gets the route name for generating OData links.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The route name for generating OData links or <c>null</c> if there isn't one.</returns>
        [Obsolete("This method is obsolete; use the ODataProperties().RouteName property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static string GetODataRouteName(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).RouteName;
        }

        /// <summary>
        /// Sets the route name for generating OData links.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The route name for generating OData links.</param>
        [Obsolete("This method is obsolete; use the ODataProperties().RouteName property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void SetODataRouteName(this HttpRequestMessage request, string routeName)
        {
            Extensions.HttpRequestMessageExtensions.ODataProperties(request).RouteName = routeName;
        }

        /// <summary>
        /// Gets the OData routing conventions for controller and action selection.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The OData routing conventions for controller and action selection or <c>null</c> if there aren't any.
        /// </returns>
        [Obsolete("This method is obsolete; use the ODataProperties().RoutingConventions property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static IEnumerable<IODataRoutingConvention> GetODataRoutingConventions(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).RoutingConventions;
        }

        /// <summary>
        /// Sets the OData routing conventions for controller and action selection.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routingConventions">The OData routing conventions for controller and action selection.</param>
        [Obsolete("This method is obsolete; use the ODataProperties().RoutingConventions property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void SetODataRoutingConventions(this HttpRequestMessage request,
            IEnumerable<IODataRoutingConvention> routingConventions)
        {
            Extensions.HttpRequestMessageExtensions.ODataProperties(request).RoutingConventions = routingConventions;
        }

        /// <summary>
        /// Gets the <see cref="IODataPathHandler"/> for generating links. Creates a default
        /// <see cref="IODataPathHandler"/> if value is currently <c>null</c>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataPathHandler"/> for generating links.</returns>
        [Obsolete("This method is obsolete; use the ODataProperties().PathHandler property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static IODataPathHandler GetODataPathHandler(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).PathHandler;
        }

        /// <summary>
        /// Sets the <see cref="IODataPathHandler"/> for generating links.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> for generating links.</param>
        [Obsolete("This method is obsolete; use the ODataProperties().PathHandler property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void SetODataPathHandler(this HttpRequestMessage request, IODataPathHandler pathHandler)
        {
            Extensions.HttpRequestMessageExtensions.ODataProperties(request).PathHandler = pathHandler;
        }

        /// <summary>
        /// <para>Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/>
        /// representing an error with an instance of <see cref="ObjectContent{T}"/> wrapping
        /// <paramref name="oDataError"/> as the content. If no formatter is found, this method returns a response with
        /// status 406 NotAcceptable.</para>
        ///
        /// <para>This method requires that <paramref name="request"/> has been associated with an instance of
        /// <see cref="HttpConfiguration"/>.</para>
        /// </summary>
        /// <param name="request">The request of interest.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="oDataError">The OData error to wrap.</param>
        /// <returns>
        /// An error response wrapping <paramref name="oDataError"/> with status code <paramref name="statusCode"/>.
        /// </returns>
        [Obsolete("This method is obsolete; use the CreateErrorResponse method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "o",
            Justification = "oDataError is spelled correctly.")]
        public static HttpResponseMessage CreateODataErrorResponse(this HttpRequestMessage request,
            HttpStatusCode statusCode, ODataError oDataError)
        {
            return Extensions.HttpRequestMessageExtensions.CreateErrorResponse(request, statusCode, oDataError);
        }

        /// <summary>
        /// Gets the OData path of the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The OData path of the request or <c>null</c> if there isn't one</returns>
        [Obsolete("This method is obsolete; use the ODataProperties().Path property from the " + 
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static Routing.ODataPath GetODataPath(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).Path;
        }

        /// <summary>
        /// Sets the OData path of the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="odataPath">The OData path of the request.</param>
        [Obsolete("This method is obsolete; use the ODataProperties().Path property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata",
            Justification = "odata is spelled correctly")]
        public static void SetODataPath(this HttpRequestMessage request, Routing.ODataPath odataPath)
        {
            Extensions.HttpRequestMessageExtensions.ODataProperties(request).Path = odataPath;
        }

        /// <summary>
        /// Gets the inline count for the OData response. Returns <c>null</c> if no count should be sent back to the
        /// client.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The inline count to send back to the client or <c>null</c> if no count should be sent back to the client.
        /// </returns>
        [Obsolete("This method is obsolete; use the ODataProperties().TotalCount property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static long? GetInlineCount(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).TotalCount;
        }

        /// <summary>
        /// Sets the inline count for the OData response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="inlineCount">The inline count to send back to the client.</param>
        [Obsolete("This method is obsolete; use the ODataProperties().TotalCount property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void SetInlineCount(this HttpRequestMessage request, long inlineCount)
        {
            Extensions.HttpRequestMessageExtensions.ODataProperties(request).TotalCount = inlineCount;
        }

        /// <summary>
        /// Gets the next page link for the OData response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The next page link to send back to the client or <c>null</c> if if no next page link should be sent back to
        /// the client.
        /// </returns>
        [Obsolete("This method is obsolete; use the ODataProperties().NextLink property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static Uri GetNextPageLink(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).NextLink;
        }

        /// <summary>
        /// Sets the next page link for the OData response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="nextPageLink">The next page link to send back to the client.</param>
        [Obsolete("This method is obsolete; use the ODataProperties().NextLink property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void SetNextPageLink(this HttpRequestMessage request, Uri nextPageLink)
        {
            Extensions.HttpRequestMessageExtensions.ODataProperties(request).NextLink = nextPageLink;
        }

        /// <summary>
        /// Gets the parsed OData <see cref="SelectExpandClause"/> of the request. The
        /// <see cref="ODataMediaTypeFormatter"/> will use this information (if any) while writing the response for
        /// this request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The parsed OData <see cref="SelectExpandClause"/> of the request or <c>null</c> if there isn't one.
        /// </returns>
        [Obsolete("This method is obsolete; use the ODataProperties().SelectExpandClause property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static SelectExpandClause GetSelectExpandClause(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).SelectExpandClause;
        }

        /// <summary>
        /// Sets the parsed OData <see cref="SelectExpandClause"/> of the request. The
        /// <see cref="ODataMediaTypeFormatter"/> will use this information (if any) while writing the response for
        /// this request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="selectExpandClause">
        /// The parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </param>
        [Obsolete("This method is obsolete; use the ODataProperties().SelectExpandClause property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void SetSelectExpandClause(this HttpRequestMessage request,
            SelectExpandClause selectExpandClause)
        {
            // Use correct argument name in ArgumentNullException, if any.
            if (selectExpandClause == null)
            {
                throw Error.ArgumentNull("selectExpandClause");
            }

            Extensions.HttpRequestMessageExtensions.ODataProperties(request).SelectExpandClause = selectExpandClause;
        }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data. Creates
        /// a new <c>IDictionary&lt;string, object&gt;</c> the first time it is called.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </returns>
        [Obsolete("This method is obsolete; use the ODataProperties().RoutingConventionsStore property from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static IDictionary<string, object> GetRoutingConventionsDataStore(this HttpRequestMessage request)
        {
            return Extensions.HttpRequestMessageExtensions.ODataProperties(request).RoutingConventionsStore;
        }
    }
}
