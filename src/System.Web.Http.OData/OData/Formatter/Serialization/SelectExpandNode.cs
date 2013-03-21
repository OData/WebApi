// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
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

        /// <summary>
        /// Builds the <see cref="SelectExpandNode"/> describing the set of structural properties and navigation properties and actions to select
        /// and navigation properties to expand while writing an entry of type <paramref name="entityType"/> for the given 
        /// <paramref name="selectExpandClause"/>.
        /// </summary>
        /// <param name="selectExpandClause">The parsed $select and $expand query options.</param>
        /// <param name="entityType">The entity type of the entry that would be written.</param>
        /// <param name="model">The <see cref="IEdmModel"/> that contains the given entity type.</param>
        /// <returns>The built <see cref="SelectExpandNode"/>.</returns>
        public static SelectExpandNode BuildSelectExpandNode(SelectExpandClause selectExpandClause, IEdmEntityTypeReference entityType, IEdmModel model)
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            SelectExpandNode selectExpandNode = new SelectExpandNode();
            if (selectExpandClause != null && selectExpandClause.Expansion != null)
            {
                selectExpandNode.BuildExpansions(selectExpandClause.Expansion, entityType);
            }
            selectExpandNode.BuildSelections(selectExpandClause == null ? null : selectExpandClause.Selection, entityType, model);

            // remove expanded navigation properties from the selected navigation properties.
            IEnumerable<IEdmNavigationProperty> expandedNavigationProperties = selectExpandNode.ExpandedNavigationProperties.Keys;
            selectExpandNode.SelectedNavigationProperties.ExceptWith(expandedNavigationProperties);

            return selectExpandNode;
        }

        private void BuildExpansions(Expansion expansion, IEdmEntityTypeReference entityType)
        {
            IEnumerable<IEdmNavigationProperty> allNavigationProperties = entityType.NavigationProperties();
            ExpandedNavigationProperties = new Dictionary<IEdmNavigationProperty, SelectExpandClause>();
            foreach (ExpandItem expandItem in expansion.ExpandItems)
            {
                ValidatePathIsSupported(expandItem.PathToNavigationProperty);
                NavigationPropertySegment navigationSegment = (NavigationPropertySegment)expandItem.PathToNavigationProperty.LastSegment;
                if (allNavigationProperties.Contains(navigationSegment.NavigationProperty))
                {
                    ExpandedNavigationProperties.Add(navigationSegment.NavigationProperty, expandItem.SelectExpandOption);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "SelectExpandNode deals with SelectExpandClaus from ODataLib. Class coupling acceptable here.")]
        private void BuildSelections(Selection selection, IEdmEntityTypeReference entityType, IEdmModel model)
        {
            Contract.Assert(entityType != null);
            Contract.Assert(model != null);

            HashSet<IEdmStructuralProperty> allStructuralProperties = new HashSet<IEdmStructuralProperty>(entityType.StructuralProperties());
            HashSet<IEdmNavigationProperty> allNavigationProperties = new HashSet<IEdmNavigationProperty>(entityType.NavigationProperties());
            HashSet<IEdmFunctionImport> allActions = new HashSet<IEdmFunctionImport>(model.GetAvailableProcedures(entityType.EntityDefinition()));

            if (selection == null || selection == AllSelection.Instance)
            {
                SelectedStructuralProperties = allStructuralProperties;
                SelectedNavigationProperties = allNavigationProperties;
                SelectedActions = allActions;
            }
            else if (selection == ExpansionsOnly.Instance)
            {
                // nothing to select.
            }
            else
            {
                PartialSelection partialSelection = selection as PartialSelection;
                if (partialSelection == null)
                {
                    throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, selection.GetType().Name));
                }

                HashSet<IEdmStructuralProperty> selectedStructuralProperties = new HashSet<IEdmStructuralProperty>();
                HashSet<IEdmNavigationProperty> selectedNavigationProperties = new HashSet<IEdmNavigationProperty>();
                HashSet<IEdmFunctionImport> selectedActions = new HashSet<IEdmFunctionImport>();

                foreach (SelectionItem selectionItem in partialSelection.SelectedItems)
                {
                    PathSelectionItem pathSelection = selectionItem as PathSelectionItem;

                    if (pathSelection != null)
                    {
                        ValidatePathIsSupported(pathSelection.SelectedPath);
                        Segment segment = pathSelection.SelectedPath.LastSegment;

                        NavigationPropertySegment navigationPropertySegment = segment as NavigationPropertySegment;
                        if (navigationPropertySegment != null)
                        {
                            if (allNavigationProperties.Contains(navigationPropertySegment.NavigationProperty))
                            {
                                selectedNavigationProperties.Add(navigationPropertySegment.NavigationProperty);
                            }
                            continue;
                        }

                        PropertySegment structuralPropertySegment = segment as PropertySegment;
                        if (structuralPropertySegment != null)
                        {
                            if (allStructuralProperties.Contains(structuralPropertySegment.Property))
                            {
                                selectedStructuralProperties.Add(structuralPropertySegment.Property);
                            }
                            continue;
                        }

                        throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, segment.GetType().Name));
                    }

                    WildcardSelectionItem wildCardSelection = selectionItem as WildcardSelectionItem;
                    if (wildCardSelection != null)
                    {
                        selectedStructuralProperties = allStructuralProperties;
                        selectedNavigationProperties = allNavigationProperties;
                        continue;
                    }

                    ContainerQualifiedWildcardSelectionItem wildCardActionSelection = selectionItem as ContainerQualifiedWildcardSelectionItem;
                    if (wildCardActionSelection != null)
                    {
                        IEnumerable<IEdmFunctionImport> actionsInThisContainer = allActions.Where(a => a.Container == wildCardActionSelection.Container);
                        foreach (IEdmFunctionImport action in actionsInThisContainer)
                        {
                            selectedActions.Add(action);
                        }
                        continue;
                    }

                    throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, selectionItem.GetType().Name));
                }

                SelectedStructuralProperties = selectedStructuralProperties;
                SelectedNavigationProperties = selectedNavigationProperties;
                SelectedActions = selectedActions;
            }
        }

        // we only support paths of type 'cast/structuralOrNavProperty' and 'structuralOrNavProperty'.
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

            if (!(path.LastSegment is NavigationPropertySegment || path.LastSegment is PropertySegment))
            {
                throw new ODataException(SRResources.UnsupportedSelectExpandPath);
            }
        }
    }
}
