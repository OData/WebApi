//-----------------------------------------------------------------------------
// <copyright file="ExpandModelPath.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// The navigation property path is a model path with the following restriction:
    ///  A non-null path MUST resolve to a model element whose type is an entity type, or a collection of entity types, e.g. a navigation property.
    ///
    /// If a path segment is a qualified name, it represents a type cast, and the segment MUST be the name of a type in scope.
    /// If a path segment is a simple identifier, it MUST be the name of a child model element of the model element identified by the preceding path part, or a structural or navigation property of the instance identified by the preceding path part.
    /// A model path MAY contain any number of segments representing collection-valued structural or navigation properties.
    /// </summary>
    internal class ExpandModelPath : List<IEdmElement>
    {
        private string _navigationPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpandModelPath" /> class.
        /// </summary>
        /// <param name="nodes">The segment nodes.</param>
        public ExpandModelPath(IEnumerable<IEdmElement> nodes)
           : base(nodes)
        {
            ValidateAndCalculateElementPath();
        }

        /// <summary>
        /// Gets the navigation property resolved from this path.
        /// </summary>
        public IEdmNavigationProperty Navigation { get; private set; }

        /// <summary>
        /// Gets the navigation property path, it doesn't include the navigation property.
        /// </summary>
        public string NavigationPropertyPath => _navigationPath;

        /// <summary>
        /// Gets the whole expand path.
        /// </summary>
        public string ExpandPath => string.IsNullOrEmpty(_navigationPath) ? Navigation.Name : $"{_navigationPath}/{Navigation.Name}";

        private void ValidateAndCalculateElementPath()
        {
            int index = 0;
            int count = Count;
            bool foundNavProp = false;
            IList<string> identifiers = new List<string>();
            foreach (IEdmElement element in this)
            {
                if (element is IEdmStructuredType structuredType)
                {
                    if (index == count - 1)
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidLastSegmentInSelectExpandPath, element.GetType().Name));
                    }

                    identifiers.Add(structuredType.FullTypeName());
                }
                else if (element is IEdmStructuralProperty structuralProperty)
                {
                    if (index == count - 1)
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidLastSegmentInSelectExpandPath, element.GetType().Name));
                    }

                    identifiers.Add(structuralProperty.Name);
                }
                else if (element is IEdmNavigationProperty navigationProperty)
                {
                    if (index < count - 1 || foundNavProp)
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidLastSegmentInSelectExpandPath, element.GetType().Name));
                    }

                    foundNavProp = true;
                    Navigation = navigationProperty;

                    // don't add the navigation property into identifiers
                    // Because the navigation property path (without the last navigation property name) is used to retrieve the navigation
                    // property binding. See "ODL api, FindNavigationTarget".
                    // identifiers.Add(navigationProperty.Name);
                }
                else
                {
                    throw new ODataException(Error.Format(SRResources.InvalidSegmentInSelectExpandPath, element.GetType().Name));
                }

                index++;
            }

            if (!foundNavProp)
            {
                throw new ODataException(Error.Format(SRResources.ShouldHaveNavigationPropertyInNavigationExpandPath));
            }

            _navigationPath = string.Join("/", identifiers);
        }
    }
}