//-----------------------------------------------------------------------------
// <copyright file="ODataNullValueMessageHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Net;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see href="HttpMessageHandler" /> that converts null values in OData responses to
    /// HTTP NotFound responses or NoContent responses following the OData specification.
    /// </summary>
    public partial class ODataNullValueMessageHandler
    {
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
            // preceded by a property access segment.
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
