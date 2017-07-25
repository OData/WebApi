﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData
{
    /// <summary>
    /// Represents an <see href="HttpMessageHandler" /> that converts null values in OData responses to
    /// HTTP NotFound responses or NoContent responses following the OData specification.
    /// </summary>
    public class ODataNullValueMessageHandler : DelegatingHandler
    {
        /// <inheritdoc />
        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // This message handler is intended for helping with queries that return a null value, for example in a
            // get request for a particular entity on an entity set, for a single valued navigation property or for
            // a structural property of a given entity. The only case in which a data modification request will result
            // in a 204 response status code, is when a primitive property is set to null through a PUT request to the
            // property URL and in that case, the user can return the right status code himself.
            ObjectContent content = response == null ? null : response.Content as ObjectContent;
            if (request.Method == HttpMethod.Get && content != null && content.Value == null &&
                response.StatusCode == HttpStatusCode.OK)
            {
                HttpStatusCode? newStatusCode = GetUpdatedResponseStatusCodeOrNull(request.ODataProperties().Path);
                if (newStatusCode.HasValue)
                {
                    response = request.CreateResponse(newStatusCode.Value);
                }
            }

            return response;
        }

        // This method is intended for unit testing purposes only.
        internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return SendAsync(request, CancellationToken.None);
        }

        // Determines if a status code needs to be changed based on the path of the request and returns the new
        // status code or null if no change is required.
        internal static HttpStatusCode? GetUpdatedResponseStatusCodeOrNull(ODataPath oDataPath)
        {
            if (oDataPath == null || oDataPath.Segments == null || oDataPath.Segments.Count == 0)
            {
                return null;
            }

            // Skip any sequence of cast segments at the end of the path.
            int currentIndex = oDataPath.Segments.Count - 1;
            ReadOnlyCollection<ODataPathSegment> segments = oDataPath.Segments;
            while (currentIndex >= 0 && segments[currentIndex] is TypeSegment)
            {
                currentIndex--;
            }

            // Null value properties should be treated in the same way independent of whether the user asked for the
            // raw value of the property or a specific format, so we skip the $value segment as it can only be
            // preceeded by a property access segment.
            if (currentIndex >= 0 && segments[currentIndex] is ValueSegment)
            {
                currentIndex--;
            }

            // Protect ourselves against malformed path segments.
            if (currentIndex < 0)
            {
                return null;
            }

            KeySegment keySegment = segments[currentIndex] as KeySegment;
            if (keySegment != null)
            {
                // Look at the previous segment to decide, but skip any possible sequence of cast segments in 
                // between.
                currentIndex--;
                while (currentIndex >= 0 && segments[currentIndex] is TypeSegment)
                {
                    currentIndex--;
                }
                if (currentIndex < 0)
                {
                    return null;
                }

                if (segments[currentIndex] is EntitySetSegment)
                {
                    // Return 404 if we were trying to retrieve a specific entity from an entity set.
                    return HttpStatusCode.NotFound;
                }

                if (segments[currentIndex] is NavigationPropertySegment)
                {
                    // Return 204 if we were trying to retrieve a related entity via a navigation property.
                    return HttpStatusCode.NoContent;
                }

                return null;
            }

            PropertySegment propertySegment = segments[currentIndex] as PropertySegment;
            if (propertySegment != null)
            {
                // Return 204 only if the property is single valued (not a collection of values).
                return GetChangedStatusCodeForProperty(propertySegment);
            }

            NavigationPropertySegment navigationSegment = segments[currentIndex] as NavigationPropertySegment;
            if (navigationSegment != null)
            {
                // Return 204 only if the navigation property is a single related entity and not a collection
                // of entities.
                return GetChangedStatusCodeForNavigationProperty(navigationSegment);
            }

            SingletonSegment singletonSegment = segments[currentIndex] as SingletonSegment;
            if (singletonSegment != null)
            {
                // Return 404 for a singleton with a null value.
                return HttpStatusCode.NotFound;
            }

            return null;
        }

        private static HttpStatusCode? GetChangedStatusCodeForNavigationProperty(NavigationPropertySegment navigation)
        {
            EdmMultiplicity multiplicity = navigation.NavigationProperty.TargetMultiplicity();
            return multiplicity == EdmMultiplicity.ZeroOrOne || multiplicity == EdmMultiplicity.One ?
                HttpStatusCode.NoContent :
                (HttpStatusCode?)null;
        }

        private static HttpStatusCode? GetChangedStatusCodeForProperty(PropertySegment propertySegment)
        {
            IEdmTypeReference type = propertySegment.Property.Type;
            return type.IsPrimitive() || type.IsComplex() ? HttpStatusCode.NoContent : (HttpStatusCode?)null;
        }
    }
}
