// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Describes the set of structural properties and navigation properties and actions to select and navigation properties to expand while 
    /// writing an <see cref="ODataEntry"/> in the response.
    /// </summary>
    public class SelectExpandNode
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class.
        /// </summary>
        /// <remarks>The default constructor is for unit testing only.</remarks>
        public SelectExpandNode()
        {
            SelectedStructuralProperties = new HashSet<IEdmStructuralProperty>();
            SelectedNavigationProperties = new HashSet<IEdmNavigationProperty>();
            ExpandedNavigationProperties = new Dictionary<IEdmNavigationProperty, SelectExpandClause>();
            SelectedActions = new HashSet<IEdmFunctionImport>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class describing the set of structural properties,
        /// navigation properties, and actions to select and expand for the given <paramref name="selectExpandClause"/>.
        /// </summary>
        /// <param name="selectExpandClause">The parsed $select and $expand query options.</param>
        /// <param name="entityType">The entity type of the entry that would be written.</param>
        /// <param name="model">The <see cref="IEdmModel"/> that contains the given entity type.</param>
        public SelectExpandNode(SelectExpandClause selectExpandClause, IEdmEntityType entityType, IEdmModel model)
            : this()
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            HashSet<IEdmStructuralProperty> allStructuralProperties = new HashSet<IEdmStructuralProperty>(entityType.StructuralProperties());
            HashSet<IEdmNavigationProperty> allNavigationProperties = new HashSet<IEdmNavigationProperty>(entityType.NavigationProperties());
            HashSet<IEdmFunctionImport> allActions = new HashSet<IEdmFunctionImport>(model.GetAvailableProcedures(entityType));

            if (selectExpandClause == null)
            {
                SelectedStructuralProperties = allStructuralProperties;
                SelectedNavigationProperties = allNavigationProperties;
                SelectedActions = allActions;
            }
            else
            {
                if (selectExpandClause.AllSelected)
                {
                    SelectedStructuralProperties = allStructuralProperties;
                    SelectedNavigationProperties = allNavigationProperties;
                    SelectedActions = allActions;
                }
                else
                {
                    BuildSelections(selectExpandClause, allStructuralProperties, allNavigationProperties, allActions);
                }

                BuildExpansions(selectExpandClause, allNavigationProperties);

                // remove expanded navigation properties from the selected navigation properties.
                SelectedNavigationProperties.ExceptWith(ExpandedNavigationProperties.Keys);
            }
        }

        /// <summary>
        /// Gets the list of EDM structural properties to be included in the response.
        /// </summary>
        public ISet<IEdmStructuralProperty> SelectedStructuralProperties { get; private set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be included as links in the response.
        /// </summary>
        public ISet<IEdmNavigationProperty> SelectedNavigationProperties { get; private set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be expanded in the response.
        /// </summary>
        public IDictionary<IEdmNavigationProperty, SelectExpandClause> ExpandedNavigationProperties { get; private set; }

        /// <summary>
        /// Gets the list of OData actions to be included in the response.
        /// </summary>
        public ISet<IEdmFunctionImport> SelectedActions { get; private set; }

        private void BuildExpansions(SelectExpandClause selectExpandClause, HashSet<IEdmNavigationProperty> allNavigationProperties)
        {
            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                ExpandedNavigationSelectItem expandItem = selectItem as ExpandedNavigationSelectItem;
                if (expandItem != null)
                {
                    ValidatePathIsSupported(expandItem.PathToNavigationProperty);
                    NavigationPropertySegment navigationSegment = (NavigationPropertySegment)expandItem.PathToNavigationProperty.LastSegment;
                    IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                    if (allNavigationProperties.Contains(navigationProperty))
                    {
                        ExpandedNavigationProperties.Add(navigationProperty, expandItem.SelectAndExpand);
                    }
                }
            }
        }

        private void BuildSelections(SelectExpandClause selectExpandClause, HashSet<IEdmStructuralProperty> allStructuralProperties, HashSet<IEdmNavigationProperty> allNavigationProperties, HashSet<IEdmFunctionImport> allActions)
        {
            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                if (selectItem is ExpandedNavigationSelectItem)
                {
                    continue;
                }

                PathSelectItem pathSelectItem = selectItem as PathSelectItem;

                if (pathSelectItem != null)
                {
                    ValidatePathIsSupported(pathSelectItem.SelectedPath);
                    ODataPathSegment segment = pathSelectItem.SelectedPath.LastSegment;

                    NavigationPropertySegment navigationPropertySegment = segment as NavigationPropertySegment;
                    if (navigationPropertySegment != null)
                    {
                        IEdmNavigationProperty navigationProperty = navigationPropertySegment.NavigationProperty;
                        if (allNavigationProperties.Contains(navigationProperty))
                        {
                            SelectedNavigationProperties.Add(navigationProperty);
                        }
                        continue;
                    }

                    PropertySegment structuralPropertySegment = segment as PropertySegment;
                    if (structuralPropertySegment != null)
                    {
                        IEdmStructuralProperty structuralProperty = structuralPropertySegment.Property;
                        if (allStructuralProperties.Contains(structuralProperty))
                        {
                            SelectedStructuralProperties.Add(structuralProperty);
                        }
                        continue;
                    }

                    OperationSegment operationSegment = segment as OperationSegment;
                    if (operationSegment != null)
                    {
                        foreach (IEdmFunctionImport action in operationSegment.Operations)
                        {
                            if (allActions.Contains(action))
                            {
                                SelectedActions.Add(action);
                            }
                        }
                        continue;
                    }

                    throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, segment.GetType().Name));
                }

                WildcardSelectItem wildCardSelectItem = selectItem as WildcardSelectItem;
                if (wildCardSelectItem != null)
                {
                    SelectedStructuralProperties = allStructuralProperties;
                    SelectedNavigationProperties = allNavigationProperties;
                    continue;
                }

                ContainerQualifiedWildcardSelectItem wildCardActionSelection = selectItem as ContainerQualifiedWildcardSelectItem;
                if (wildCardActionSelection != null)
                {
                    IEnumerable<IEdmFunctionImport> actionsInThisContainer = allActions.Where(a => a.Container == wildCardActionSelection.Container);
                    foreach (IEdmFunctionImport action in actionsInThisContainer)
                    {
                        SelectedActions.Add(action);
                    }
                    continue;
                }

                throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, selectItem.GetType().Name));
            }
        }

        // we only support paths of type 'cast/structuralOrNavPropertyOrAction' and 'structuralOrNavPropertyOrAction'.
        internal static void ValidatePathIsSupported(ODataPath path)
        {
            int segmentCount = path.Count();

            if (segmentCount > 2)
            {
                throw new ODataException(SRResources.UnsupportedSelectExpandPath);
            }

            if (segmentCount == 2)
            {
                if (!(path.FirstSegment is TypeSegment))
                {
                    throw new ODataException(SRResources.UnsupportedSelectExpandPath);
                }
            }

            ODataPathSegment lastSegment = path.LastSegment;
            if (!(lastSegment is NavigationPropertySegment
                || lastSegment is PropertySegment
                || lastSegment is OperationSegment))
            {
                throw new ODataException(SRResources.UnsupportedSelectExpandPath);
            }
        }
    }
}
