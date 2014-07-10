// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
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
            SelectedActions = new HashSet<IEdmAction>();
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
            HashSet<IEdmAction> allActions = new HashSet<IEdmAction>(model.GetAvailableActions(entityType));
            HashSet<IEdmFunction> allFunctions = new HashSet<IEdmFunction>(model.GetAvailableFunctions(entityType));

            if (selectExpandClause == null)
            {
                SelectedStructuralProperties = allStructuralProperties;
                SelectedNavigationProperties = allNavigationProperties;
                SelectedActions = allActions;
                SelectedFunctions = allFunctions;
                SelectAllDynamicProperties = true;
            }
            else
            {
                if (selectExpandClause.AllSelected)
                {
                    SelectedStructuralProperties = allStructuralProperties;
                    SelectedNavigationProperties = allNavigationProperties;
                    SelectedActions = allActions;
                    SelectedFunctions = allFunctions;
                    SelectAllDynamicProperties = true;
                }
                else
                {
                    BuildSelections(selectExpandClause, allStructuralProperties, allNavigationProperties, allActions, allFunctions);
                    SelectAllDynamicProperties = false;
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
        /// Gets the flag to indicate the dynamic property to be included in the response or not.
        /// </summary>
        public bool SelectAllDynamicProperties { get; private set; }

        /// <summary>
        /// Gets the list of OData actions to be included in the response.
        /// </summary>
        public ISet<IEdmAction> SelectedActions { get; private set; }

        /// <summary>
        /// Gets the list of OData functions to be included in the response.
        /// </summary>
        public ISet<IEdmFunction> SelectedFunctions { get; private set; }

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

        private void BuildSelections(
            SelectExpandClause selectExpandClause,
            HashSet<IEdmStructuralProperty> allStructuralProperties,
            HashSet<IEdmNavigationProperty> allNavigationProperties,
            HashSet<IEdmAction> allActions,
            HashSet<IEdmFunction> allFunctions)
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
                        AddOperations(allActions, allFunctions, operationSegment);
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

                NamespaceQualifiedWildcardSelectItem wildCardActionSelection = selectItem as NamespaceQualifiedWildcardSelectItem;
                if (wildCardActionSelection != null)
                {
                    SelectedActions = allActions;
                    SelectedFunctions = allFunctions;
                    continue;
                }

                throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, selectItem.GetType().Name));
            }
        }

        private void AddOperations(HashSet<IEdmAction> allActions, HashSet<IEdmFunction> allFunctions, OperationSegment operationSegment)
        {
            foreach (IEdmOperation operation in operationSegment.Operations)
            {
                IEdmAction action = operation as IEdmAction;
                if (action != null && allActions.Contains(action))
                {
                    SelectedActions.Add(action);
                }

                IEdmFunction function = operation as IEdmFunction;
                if (function != null && allFunctions.Contains(function))
                {
                    SelectedFunctions.Add(function);
                }
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
