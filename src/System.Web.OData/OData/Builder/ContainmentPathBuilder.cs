// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    internal class ContainmentPathBuilder
    {
        private List<ODataPathSegment> _segments;

        public ODataPath TryComputeCanonicalContainingPath(ODataPath path)
        {
            Contract.Assert(path != null);
            Contract.Assert(path.Count >= 2);

            _segments = path.ToList();

            RemoveAllTypeCasts();

            // New ODataPath will be extended later to include any final required key or cast.
            RemovePathSegmentsAfterTheLastNavigationProperty();

            RemoveRedundantContainingPathSegments();

            AddTypeCastsIfNecessary();

            // Also remove the last navigation property segment, since it is not part of the containing path segments.
            if (_segments.Count > 0)
            {
                _segments.RemoveAt(_segments.Count - 1);
            }

            return new ODataPath(_segments);
        }

        private void RemovePathSegmentsAfterTheLastNavigationProperty()
        {
            // Find the last navigation property segment.
            ODataPathSegment lastNavigationProperty = _segments.OfType<NavigationPropertySegment>().LastOrDefault();
            List<ODataPathSegment> newSegments = new List<ODataPathSegment>();
            foreach (ODataPathSegment segment in _segments)
            {
                newSegments.Add(segment);
                if (segment == lastNavigationProperty)
                {
                    break;
                }
            }

            _segments = newSegments;
        }

        private void RemoveRedundantContainingPathSegments()
        {
            // Find the last non-contained navigation property segment:
            //   Collection valued: entity set
            //   -or-
            //   Single valued: singleton
            // Copy over other path segments such as: not a navigation path segment, contained navigation property,
            // single valued navigation property with navigation source targetting an entity set (we won't have key
            // information for that navigation property.)
            _segments.Reverse();
            NavigationPropertySegment navigationPropertySegment = null;
            List<ODataPathSegment> newSegments = new List<ODataPathSegment>();
            foreach (ODataPathSegment segment in _segments)
            {
                navigationPropertySegment = segment as NavigationPropertySegment;
                if (navigationPropertySegment != null)
                {
                    EdmNavigationSourceKind navigationSourceKind =
                        navigationPropertySegment.NavigationSource.NavigationSourceKind();
                    if ((navigationPropertySegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many &&
                         navigationSourceKind == EdmNavigationSourceKind.EntitySet) ||
                        (navigationSourceKind == EdmNavigationSourceKind.Singleton))
                    {
                        break;
                    }
                }

                newSegments.Insert(0, segment);
            }

            // Start the path with the navigation source of the navigation property found above.
            if (navigationPropertySegment != null)
            {
                IEdmNavigationSource navigationSource = navigationPropertySegment.NavigationSource;
                Contract.Assert(navigationSource != null);
                if (navigationSource.NavigationSourceKind() == EdmNavigationSourceKind.Singleton)
                {
                    SingletonSegment singletonSegment = new SingletonSegment((IEdmSingleton)navigationSource);
                    newSegments.Insert(0, singletonSegment);
                }
                else
                {
                    Contract.Assert(navigationSource.NavigationSourceKind() == EdmNavigationSourceKind.EntitySet);
                    EntitySetSegment entitySetSegment = new EntitySetSegment((IEdmEntitySet)navigationSource);
                    newSegments.Insert(0, entitySetSegment);
                }
            }

            _segments = newSegments;
        }

        private void RemoveAllTypeCasts()
        {
            List<ODataPathSegment> newSegments = new List<ODataPathSegment>();
            foreach (ODataPathSegment segment in _segments)
            {
                if (!(segment is TypeSegment))
                {
                    newSegments.Add(segment);
                }
            }

            _segments = newSegments;
        }

        private void AddTypeCastsIfNecessary()
        {
            IEdmEntityType owningType = null;
            List<ODataPathSegment> newSegments = new List<ODataPathSegment>();
            foreach (ODataPathSegment segment in _segments)
            {
                NavigationPropertySegment navProp = segment as NavigationPropertySegment;
                if (navProp != null && owningType != null &&
                    owningType.FindProperty(navProp.NavigationProperty.Name) == null)
                {
                    // need a type cast
                    TypeSegment typeCast = new TypeSegment(
                        navProp.NavigationProperty.DeclaringType,
                        navigationSource: null);
                    newSegments.Add(typeCast);
                }

                newSegments.Add(segment);
                IEdmEntityType targetEntityType = GetTargetEntityType(segment);
                if (targetEntityType != null)
                {
                    owningType = targetEntityType;
                }
            }

            _segments = newSegments;
        }

        private static IEdmEntityType GetTargetEntityType(ODataPathSegment segment)
        {
            Contract.Assert(segment != null);

            EntitySetSegment entitySetSegment = segment as EntitySetSegment;
            if (entitySetSegment != null)
            {
                return entitySetSegment.EntitySet.EntityType();
            }

            SingletonSegment singletonSegment = segment as SingletonSegment;
            if (singletonSegment != null)
            {
                return singletonSegment.Singleton.EntityType();
            }

            NavigationPropertySegment navigationPropertySegment = segment as NavigationPropertySegment;
            if (navigationPropertySegment != null)
            {
                return navigationPropertySegment.NavigationSource.EntityType();
            }

            return null;
        }
    }
}
