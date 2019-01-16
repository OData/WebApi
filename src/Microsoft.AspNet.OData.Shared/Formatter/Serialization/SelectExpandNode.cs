// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Describes the set of structural properties and navigation properties and actions to select and navigation properties to expand while
    /// writing an <see cref="ODataResource"/> in the response.
    /// </summary>
    public class SelectExpandNode
    {
        /// <summary>
        /// Exists to support backward compatibility as we introduced ExpandedProperties.
        /// </summary>
        private Dictionary<IEdmNavigationProperty, SelectExpandClause> cachedExpandedClauses;

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class.
        /// </summary>
        /// <remarks>The default constructor is for unit testing only.</remarks>
        public SelectExpandNode()
        {
            SelectedStructuralProperties = new HashSet<IEdmStructuralProperty>();
            SelectedComplexProperties = new HashSet<IEdmStructuralProperty>();
            SelectedNavigationProperties = new HashSet<IEdmNavigationProperty>();
            ExpandedNavigationPropertiesOnComplexTypes = new Dictionary<IEdmStructuralProperty, ExpandedNavigationSelectItem>();
            ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>();
            ReferencedNavigationProperties = new HashSet<IEdmNavigationProperty>();
            SelectedActions = new HashSet<IEdmAction>();
            SelectedFunctions = new HashSet<IEdmFunction>();
            SelectedDynamicProperties = new HashSet<string>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class by copying the state of another instance. This is
        /// intended for scenarios that wish to modify state without updating the values cached within ODataResourceSerializer.
        /// </summary>
        /// <param name="selectExpandNodeToCopy">The instance from which the state for the new instance will be copied.</param>
        public SelectExpandNode(SelectExpandNode selectExpandNodeToCopy)
        {
            ExpandedNavigationPropertiesOnComplexTypes = new Dictionary<IEdmStructuralProperty, ExpandedNavigationSelectItem>(selectExpandNodeToCopy.ExpandedNavigationPropertiesOnComplexTypes);
            ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>(selectExpandNodeToCopy.ExpandedProperties);
            ReferencedNavigationProperties = new HashSet<IEdmNavigationProperty>(selectExpandNodeToCopy.ReferencedNavigationProperties);

            SelectedActions = new HashSet<IEdmAction>(selectExpandNodeToCopy.SelectedActions);
            SelectAllDynamicProperties = selectExpandNodeToCopy.SelectAllDynamicProperties;
            SelectedComplexProperties = new HashSet<IEdmStructuralProperty>(selectExpandNodeToCopy.SelectedComplexProperties);
            SelectedDynamicProperties = new HashSet<string>(selectExpandNodeToCopy.SelectedDynamicProperties);
            SelectedFunctions = new HashSet<IEdmFunction>(selectExpandNodeToCopy.SelectedFunctions);
            SelectedNavigationProperties = new HashSet<IEdmNavigationProperty>(selectExpandNodeToCopy.SelectedNavigationProperties);
            SelectedStructuralProperties = new HashSet<IEdmStructuralProperty>(selectExpandNodeToCopy.SelectedStructuralProperties);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class describing the set of structural properties,
        /// nested properties, navigation properties, and actions to select and expand for the given <paramref name="writeContext"/>.
        /// </summary>
        /// <param name="structuredType">The structural type of the resource that would be written.</param>
        /// <param name="writeContext">The serializer context to be used while creating the collection.</param>
        /// <remarks>The default constructor is for unit testing only.</remarks>
        public SelectExpandNode(IEdmStructuredType structuredType, ODataSerializerContext writeContext)
            : this()
        {
            Property = writeContext.EdmProperty;
            PropertiesInPath = writeContext.PropertiesInPath;
            Initialize(writeContext.SelectExpandClause, structuredType, writeContext.Model, writeContext.ExpandReference);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class describing the set of structural properties,
        /// nested properties, navigation properties, and actions to select and expand for the given <paramref name="selectExpandClause"/>.
        /// </summary>
        /// <param name="selectExpandClause">The parsed $select and $expand query options.</param>
        /// <param name="structuredType">The structural type of the resource that would be written.</param>
        /// <param name="model">The <see cref="IEdmModel"/> that contains the given structural type.</param>
        public SelectExpandNode(SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmModel model)
            : this(selectExpandClause, structuredType, model, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class describing the set of structural properties,
        /// nested properties, navigation properties, and actions to select and expand for the given <paramref name="selectExpandClause"/>.
        /// </summary>
        /// <param name="selectExpandClause">The parsed $select and $expand query options.</param>
        /// <param name="structuredType">The structural type of the resource that would be written.</param>
        /// <param name="model">The <see cref="IEdmModel"/> that contains the given structural type.</param>
        /// <param name="expandedReference">a boolean value indicating whether it's expanded reference.</param>
        internal SelectExpandNode(SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmModel model, bool expandedReference)
            : this()
        {
            Initialize(selectExpandClause, structuredType, model, false);
        }

        private void Initialize(SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmModel model, bool expandedReference)
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

            IEdmEntityType entityType = structuredType as IEdmEntityType;
            if (expandedReference)
            {
                SelectAllDynamicProperties = false;
                if (entityType != null)
                {
                    // only need to include the key properties.
                    SelectedStructuralProperties = new HashSet<IEdmStructuralProperty>(entityType.Key());
                }
            }
            else
            {
                // So far, it includes all properties of complex and collection of complex
                HashSet<IEdmStructuralProperty> allComplexStructuralProperties = new HashSet<IEdmStructuralProperty>();
                GetStructuralProperties(structuredType, allStructuralProperties, allComplexStructuralProperties);

                // So far, it includes all navigation properties
                HashSet<IEdmNavigationProperty> allNavigationProperties;
                HashSet<IEdmAction> allActions;
                HashSet<IEdmFunction> allFunctions;
                IEnumerable<SelectItem> selectItems = new List<SelectItem>();

                if (entityType != null)
                {
                    allNavigationProperties = new HashSet<IEdmNavigationProperty>(entityType.NavigationProperties());
                    allActions = new HashSet<IEdmAction>(model.GetAvailableActions(entityType));
                    allFunctions = new HashSet<IEdmFunction>(model.GetAvailableFunctions(entityType));
                }
                else if (structuredType != null)
                {
                    allNavigationProperties = new HashSet<IEdmNavigationProperty>(structuredType.NavigationProperties());
                    
                    // Currently, the library does not support for bounded operations on complex type. 
                    allActions = new HashSet<IEdmAction>();
                    allFunctions = new HashSet<IEdmFunction>();
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
                        // Explicitly set SelectAllDynamicProperties as false, while the BuildSelections method will set it as true
                        // if it meets the select all condition.
                        SelectAllDynamicProperties = false;
                        BuildSelections(selectExpandClause, allStructuralProperties, allComplexStructuralProperties, allNavigationProperties, allActions, allFunctions);
                    }

                    selectItems = selectExpandClause.SelectedItems;
                }

                BuildExpansions(selectItems, allNavigationProperties);

                // remove expanded navigation properties from the selected navigation properties.
                SelectedNavigationProperties.ExceptWith(ExpandedProperties.Keys);

                // remove referenced navigation properties from the selected navigation properties.
                SelectedNavigationProperties.ExceptWith(ReferencedNavigationProperties);
            }
        }

        /// <summary>
        /// Gets the list of EDM structural properties (primitive, enum or collection of them) to be included in the response.
        /// </summary>
        public ISet<IEdmStructuralProperty> SelectedStructuralProperties { get; private set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be included as links in the response. It is deprecated in favor of ExpandedProperties
        /// </summary>
        public ISet<IEdmNavigationProperty> SelectedNavigationProperties { get; private set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be expanded in the response.
        /// </summary>
        [Obsolete("This property is deprecated in favor of ExpandedProperties as this property only contains a subset of the information.")]
        public IDictionary<IEdmNavigationProperty, SelectExpandClause> ExpandedNavigationProperties
        {
            get
            {
                if (this.cachedExpandedClauses == null)
                {
                    this.cachedExpandedClauses = ExpandedProperties.ToDictionary(item => item.Key,
                        item => item.Value != null ? item.Value.SelectAndExpand : null);
                }

                return this.cachedExpandedClauses;
            }
        }

        /// <summary>
        /// Gets the list of EDM navigation properties to be expanded in the response along with the nested query options embedded in the expand.
        /// </summary>
        public IDictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem> ExpandedProperties { get; private set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be expand referenced in the response.
        /// </summary>
        public ISet<IEdmNavigationProperty> ReferencedNavigationProperties { get; private set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be expanded on ComplexTypes in the response.
        /// </summary>
        public IDictionary<IEdmStructuralProperty, ExpandedNavigationSelectItem> ExpandedNavigationPropertiesOnComplexTypes { get; private set; }

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

        /// <summary>
        /// Gets the path to property corresponding to the SelectExpandNode. Null for a top-level select expand.
        /// </summary>
        public Queue<IEdmProperty> PropertiesInPath { get; private set; }

        /// <summary>
        /// Gets the property corresponding to the SelectExpandNode. Null for a top-level select expand.
        /// </summary>
        public IEdmProperty Property { get; private set; }

        private void BuildExpansions(IEnumerable<SelectItem> selectedItems, HashSet<IEdmNavigationProperty> allNavigationProperties)
        {
            foreach (SelectItem selectItem in selectedItems)
            {
                ExpandedReferenceSelectItem expandReferenceItem = selectItem as ExpandedReferenceSelectItem;
                if (expandReferenceItem != null)
                {
                    ValidatePathIsSupportedForExpand(expandReferenceItem.PathToNavigationProperty);
                    NavigationPropertySegment navigationSegment = (NavigationPropertySegment)expandReferenceItem.PathToNavigationProperty.LastSegment;
                    IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                    if (allNavigationProperties.Contains(navigationProperty))
                    {
                        ExpandedNavigationSelectItem expandItem = selectItem as ExpandedNavigationSelectItem;

                        if (expandItem != null)
                        {
                            if (!ExpandedProperties.ContainsKey(navigationProperty))
                            {
                                ExpandedProperties.Add(navigationProperty, expandItem);
                            }
                            else
                            {
                                ExpandedProperties[navigationProperty] = expandItem;
                            }
                        }
                        else
                        {
                            ReferencedNavigationProperties.Add(navigationProperty);
                        }
                    }
                    else
                    {
                        //This is the case where the navigation property is not on the current type. We need to propagate the expand item to deeper SelectExpandNode.
                        IEdmStructuralProperty complexProperty = FindNextPropertySegment(expandReferenceItem.PathToNavigationProperty);

                        if (complexProperty != null)
                        {
                            SelectExpandClause newClause;
                            if (ExpandedNavigationPropertiesOnComplexTypes.ContainsKey(complexProperty))
                            {
                                SelectExpandClause oldClause = ExpandedNavigationPropertiesOnComplexTypes[complexProperty].SelectAndExpand;
                                newClause = new SelectExpandClause(
                                    oldClause.SelectedItems.Concat(new SelectItem[] { expandReferenceItem }), false);
                            }
                            else
                            {
                                newClause = new SelectExpandClause(new SelectItem[] { expandReferenceItem }, false);
                            }
                           
                            ExpandedNavigationSelectItem newItem = new ExpandedNavigationSelectItem(expandReferenceItem.PathToNavigationProperty, navigationSegment.NavigationSource, newClause);
                            ExpandedNavigationPropertiesOnComplexTypes.Add(complexProperty, newItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the appropriate property segment which should be responsible for propagating the expand item.
        /// For instance, if we are creating the SelectExpandNode for property p2 for the query ~/EnitySet/Key/p1/p2/p3?$expand=NP, we want to return property p3 here.
        /// </summary>
        private IEdmStructuralProperty FindNextPropertySegment(ODataPath path)
        {
            IEdmStructuralProperty complexProperty = null;
            // If the current SelectExpandNode is not top-level and has a property associated with it then return the next property segment from the path.
            if (Property != null)
            {
                Debug.Assert(PropertiesInPath != null, "PropertiesInPath should not be null if Property is not null");
                Queue<IEdmProperty> propertyQueue = new Queue<IEdmProperty>(PropertiesInPath);

                foreach (ODataPathSegment segment in path)
                {
                    PropertySegment propertySegment = segment as PropertySegment;
                    if (propertySegment != null)
                    {
                        complexProperty = propertySegment.Property;
                        if (propertyQueue.Count == 0)
                        {
                            break;
                        }

                        if (propertyQueue.Peek().Name == complexProperty.Name)
                        {
                            propertyQueue.Dequeue();
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                // Return the first property if top-level resource
                PropertySegment segment = path.OfType<PropertySegment>().FirstOrDefault();
                if (segment != null)
                {
                    complexProperty = segment.Property;
                }
            }

            return complexProperty;
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
                    ValidatePathIsSupportedForSelect(pathSelectItem.SelectedPath);
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
                    SelectAllDynamicProperties = true;
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
        internal static void ValidatePathIsSupportedForSelect(ODataPath path)
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

        // we support paths of type 'cast/structuralOrNavPropertyOrAction', 'ComplexObject/cast/StructuralOrNavPropertyOnAction', 'ComplexObject/structuralOrNavPropertyOnAction' and 'structuralOrNavPropertyOrAction'.
        internal static void ValidatePathIsSupportedForExpand(ODataPath path)
        {
            ODataPathSegment lastSegment = path.LastSegment;
            foreach (ODataPathSegment segment in path)
            {
                if (!(segment is TypeSegment || segment is PropertySegment || (segment == lastSegment)))
                {
                    throw new ODataException(SRResources.UnsupportedSelectExpandPath);
                }
            }

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
