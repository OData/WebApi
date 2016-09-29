// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Describes the set of structural properties and navigation properties and actions to select and navigation properties to expand while 
    /// writing an <see cref="ODataResource"/> in the response.
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
            SelectedComplexProperties = new HashSet<IEdmStructuralProperty>();
            SelectedNavigationProperties = new HashSet<IEdmNavigationProperty>();
            ExpandedNavigationProperties = new Dictionary<IEdmNavigationProperty, SelectExpandClause>();
            SelectedActions = new HashSet<IEdmAction>();
            SelectedFunctions = new HashSet<IEdmFunction>();
            SelectedDynamicProperties = new HashSet<string>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class describing the set of structural properties,
        /// nested properties, navigation properties, and actions to select and expand for the given <paramref name="writeContext"/>.
        /// </summary>
        /// <param name="structuredType">The structural type of the resource that would be written.</param>
        /// <param name="writeContext">The serializer context to be used while creating the collection.</param>
        /// <remarks>The default constructor is for unit testing only.</remarks>
        public SelectExpandNode(IEdmStructuredType structuredType, ODataSerializerContext writeContext)
            : this(writeContext.SelectExpandClause, structuredType, writeContext.Model)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class describing the set of structural properties,
        /// nested properties, navigation properties, and actions to select and expand for the given <paramref name="selectExpandClause"/>.
        /// </summary>
        /// <param name="selectExpandClause">The parsed $select and $expand query options.</param>
        /// <param name="structuredType">The structural type of the resource that would be written.</param>
        /// <param name="model">The <see cref="IEdmModel"/> that contains the given structural type.</param>
        public SelectExpandNode(SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmModel model)
            : this()
        {
            if (structuredType == null)
            {
                throw Error.ArgumentNull("structuredType");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            // So far, it includes all properties of primitive, enum and collection of them
            HashSet<IEdmStructuralProperty> allStructuralProperties = new HashSet<IEdmStructuralProperty>();

            // So far, it includes all properties of complex and collection of complex
            HashSet<IEdmStructuralProperty> allComplexStructuralProperties = new HashSet<IEdmStructuralProperty>();
            GetStructuralProperties(structuredType, allStructuralProperties, allComplexStructuralProperties);

            // So far, it includes all navigation properties
            HashSet<IEdmNavigationProperty> allNavigationProperties;
            HashSet<IEdmAction> allActions;
            HashSet<IEdmFunction> allFunctions;

            IEdmEntityType entityType = structuredType as IEdmEntityType;
            if (entityType != null)
            {
                allNavigationProperties = new HashSet<IEdmNavigationProperty>(entityType.NavigationProperties());
                allActions = new HashSet<IEdmAction>(model.GetAvailableActions(entityType));
                allFunctions = new HashSet<IEdmFunction>(model.GetAvailableFunctions(entityType));
            }
            else
            {
                allNavigationProperties = new HashSet<IEdmNavigationProperty>();
                allActions = new HashSet<IEdmAction>();
                allFunctions = new HashSet<IEdmFunction>();
            }

            if (selectExpandClause == null)
            {
                SelectedStructuralProperties = allStructuralProperties;
                SelectedComplexProperties = allComplexStructuralProperties;
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
                    SelectedComplexProperties = allComplexStructuralProperties;
                    SelectedNavigationProperties = allNavigationProperties;
                    SelectedActions = allActions;
                    SelectedFunctions = allFunctions;
                    SelectAllDynamicProperties = true;
                }
                else
                {
                    BuildSelections(selectExpandClause, allStructuralProperties, allComplexStructuralProperties, allNavigationProperties, allActions, allFunctions);
                    SelectAllDynamicProperties = false;
                }

                BuildExpansions(selectExpandClause, allNavigationProperties);

                // remove expanded navigation properties from the selected navigation properties.
                SelectedNavigationProperties.ExceptWith(ExpandedNavigationProperties.Keys);
            }
        }

        /// <summary>
        /// Gets the list of EDM structural properties (primitive, enum or collection of them) to be included in the response.
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
        /// Gets the list of EDM nested properties (complex or collection of complex) to be included in the response.
        /// </summary>
        public ISet<IEdmStructuralProperty> SelectedComplexProperties { get; private set; }

        /// <summary>
        /// Gets the list of dynamic properties to select.
        /// </summary>
        public ISet<string> SelectedDynamicProperties { get; private set; }

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
            HashSet<IEdmStructuralProperty> allNestedProperties,
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
                        else if (allNestedProperties.Contains(structuralProperty))
                        {
                            SelectedComplexProperties.Add(structuralProperty);
                        }
                        continue;
                    }

                    OperationSegment operationSegment = segment as OperationSegment;
                    if (operationSegment != null)
                    {
                        AddOperations(allActions, allFunctions, operationSegment);
                        continue;
                    }

                    DynamicPathSegment dynamicPathSegment = segment as DynamicPathSegment;
                    if (dynamicPathSegment != null)
                    {
                        SelectedDynamicProperties.Add(dynamicPathSegment.Identifier);
                        continue;
                    }
                    throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, segment.GetType().Name));
                }

                WildcardSelectItem wildCardSelectItem = selectItem as WildcardSelectItem;
                if (wildCardSelectItem != null)
                {
                    SelectedStructuralProperties = allStructuralProperties;
                    SelectedComplexProperties = allNestedProperties;
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
                || lastSegment is OperationSegment
                || lastSegment is DynamicPathSegment))
            {
                throw new ODataException(SRResources.UnsupportedSelectExpandPath);
            }
        }

        /// <summary>
        /// Separate the structural properties into two parts:
        /// 1. Complex and collection of complex are nested structural properties.
        /// 2. Others are non-nested structural properties.
        /// </summary>
        /// <param name="structuredType">The structural type of the resource.</param>
        /// <param name="structuralProperties">The non-nested structural properties of the structural type.</param>
        /// <param name="nestedStructuralProperties">The nested structural properties of the structural type.</param>
        public static void GetStructuralProperties(IEdmStructuredType structuredType, HashSet<IEdmStructuralProperty> structuralProperties,
            HashSet<IEdmStructuralProperty> nestedStructuralProperties)
        {
            if (structuredType == null)
            {
                throw Error.ArgumentNull("structuredType");
            }

            if (structuralProperties == null)
            {
                throw Error.ArgumentNull("structuralProperties");
            }

            if (nestedStructuralProperties == null)
            {
                throw Error.ArgumentNull("nestedStructuralProperties");
            }

            foreach (var edmStructuralProperty in structuredType.StructuralProperties())
            {
                if (edmStructuralProperty.Type.IsComplex())
                {
                    nestedStructuralProperties.Add(edmStructuralProperty);
                }
                else if (edmStructuralProperty.Type.IsCollection())
                {
                    if (edmStructuralProperty.Type.AsCollection().ElementType().IsComplex())
                    {
                        nestedStructuralProperties.Add(edmStructuralProperty);
                    }
                    else
                    {
                        structuralProperties.Add(edmStructuralProperty);
                    }
                }
                else
                {
                    structuralProperties.Add(edmStructuralProperty);
                }
            }
        }
    }
}
