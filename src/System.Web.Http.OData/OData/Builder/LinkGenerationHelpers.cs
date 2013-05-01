// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Contains helper methods for generating OData links that follow OData URL conventions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class LinkGenerationHelpers
    {
        /// <summary>
        /// Generates a self link following the OData URL conventions for the entity represented by <paramref name="entityContext"/>.
        /// </summary>
        /// <param name="entityContext">The <see cref="EntityInstanceContext"/> representing the entity for which the self link needs to be generated.</param>
        /// <param name="includeCast">Represents whether the generated link should have a cast segment representing a type cast.</param>
        /// <returns>The self link following the OData URL conventions.</returns>
        public static string GenerateSelfLink(this EntityInstanceContext entityContext, bool includeCast)
        {
            if (entityContext == null)
            {
                throw Error.ArgumentNull("entityContext");
            }
            if (entityContext.Url == null)
            {
                throw Error.Argument("entityContext", SRResources.UrlHelperNull, typeof(EntityInstanceContext).Name);
            }

            List<ODataPathSegment> idLinkPathSegments = new List<ODataPathSegment>();

            idLinkPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            idLinkPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));

            if (includeCast)
            {
                idLinkPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            string idLink = entityContext.Url.ODataLink(idLinkPathSegments);
            if (idLink == null)
            {
                return null;
            }

            return idLink;
        }

        /// <summary>
        /// Generates a navigation link following the OData URL conventions for the entity represented by <paramref name="entityContext"/> and the given 
        /// navigation property.
        /// </summary>
        /// <param name="entityContext">The <see cref="EntityInstanceContext"/> representing the entity for which the navigation link needs to be generated.</param>
        /// <param name="navigationProperty">The EDM navigation property.</param>
        /// <param name="includeCast">Represents whether the generated link should have a cast segment representing a type cast.</param>
        /// <returns>The navigation link following the OData URL conventions.</returns>
        public static Uri GenerateNavigationPropertyLink(this EntityInstanceContext entityContext, IEdmNavigationProperty navigationProperty, bool includeCast)
        {
            if (entityContext == null)
            {
                throw Error.ArgumentNull("entityContext");
            }
            if (entityContext.Url == null)
            {
                throw Error.Argument("entityContext", SRResources.UrlHelperNull, typeof(EntityInstanceContext).Name);
            }

            List<ODataPathSegment> navigationPathSegments = new List<ODataPathSegment>();
            navigationPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            navigationPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));

            if (includeCast)
            {
                navigationPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            navigationPathSegments.Add(new NavigationPathSegment(navigationProperty));

            string link = entityContext.Url.ODataLink(navigationPathSegments);
            if (link == null)
            {
                return null;
            }

            return new Uri(link);
        }
    }
}
