//-----------------------------------------------------------------------------
// <copyright file="SelectExpandNode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query.Expressions;
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
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class.
        /// </summary>
        /// <remarks>The default constructor is for unit testing only.</remarks>
        public SelectExpandNode()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SelectExpandNode"/> class by copying the state of another instance. This is
        /// intended for scenarios that wish to modify state without updating the values cached within ODataResourceSerializer.
        /// </summary>
        /// <param name="selectExpandNodeToCopy">The instance from which the state for the new instance will be copied.</param>
        public SelectExpandNode(SelectExpandNode selectExpandNodeToCopy)
        {
            SelectedStructuralProperties = selectExpandNodeToCopy.SelectedStructuralProperties == null ?
                null : new HashSet<IEdmStructuralProperty>(selectExpandNodeToCopy.SelectedStructuralProperties);

            SelectedComplexTypeProperties = selectExpandNodeToCopy.SelectedComplexTypeProperties == null ?
                null : new Dictionary<IEdmStructuralProperty, PathSelectItem>(selectExpandNodeToCopy.SelectedComplexTypeProperties);

            SelectedNavigationProperties = selectExpandNodeToCopy.SelectedNavigationProperties == null ?
                null : new HashSet<IEdmNavigationProperty>(selectExpandNodeToCopy.SelectedNavigationProperties);

            ExpandedProperties = selectExpandNodeToCopy.ExpandedProperties == null ?
                null : new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>(selectExpandNodeToCopy.ExpandedProperties);

            ReferencedProperties = selectExpandNodeToCopy.ReferencedProperties == null ?
                null : new Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem>();

            SelectAllDynamicProperties = selectExpandNodeToCopy.SelectAllDynamicProperties;

            SelectedDynamicProperties = selectExpandNodeToCopy.SelectedDynamicProperties == null ?
                null : new HashSet<string>(selectExpandNodeToCopy.SelectedDynamicProperties);

            SelectedActions = selectExpandNodeToCopy.SelectedActions == null ?
                null : new HashSet<IEdmAction>(selectExpandNodeToCopy.SelectedActions);

            SelectedFunctions = selectExpandNodeToCopy.SelectedFunctions == null ?
                null : new HashSet<IEdmFunction>(selectExpandNodeToCopy.SelectedFunctions);
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
            : this()
        {
            Initialize(selectExpandClause, structuredType, model, false);
        }

        /// <summary>
        /// Gets the list of EDM navigation properties to be expand referenced in the response.
        /// keeping this is only for non-breaking changes, This should be replaced by "ReferencedProperties" later.
        /// </summary>
        [Obsolete("This property will be removed later, please use ReferencedProperties.")]
        public ISet<IEdmNavigationProperty> ReferencedNavigationProperties
        {
            get
            {
                if (ReferencedProperties == null)
                {
                    return null;
                }

                return new HashSet<IEdmNavigationProperty>(ReferencedProperties.Keys);
            }
        }

        /// <summary>
        /// Gets the list of EDM nested properties (complex or collection of complex) to be included in the response.
        /// keeping this is only for non-breaking changes, This should be replaced by "SelectedComplexes".
        /// </summary>
        [Obsolete("This property will be removed later, please use SelectedComplexTypeProperties.")]
        public ISet<IEdmStructuralProperty> SelectedComplexProperties
        {
            get
            {
                if (SelectedComplexTypeProperties == null)
                {
                    return null;
                }

                return new HashSet<IEdmStructuralProperty>(SelectedComplexTypeProperties.Keys);
            }
        }

        /// <summary>
        /// Gets the list of EDM structural properties (primitive, enum or collection of them) to be included in the response.
        /// It could be null if there's no property selected.
        /// </summary>
        public ISet<IEdmStructuralProperty> SelectedStructuralProperties { get; internal set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be included as links in the response. It could be null.
        /// </summary>
        public ISet<IEdmNavigationProperty> SelectedNavigationProperties { get; internal set; }

        /// <summary>
        /// Gets the list of Edm structural properties (complex or complex collection) to be included in the response.
        /// The key is the Edm structural property.
        /// The value is the potential sub select item.
        /// </summary>
        public IDictionary<IEdmStructuralProperty, PathSelectItem> SelectedComplexTypeProperties { get; internal set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be expanded in the response along with the nested query options embedded in the expand.
        /// It could be null if no navigation property to expand.
        /// </summary>
        public IDictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem> ExpandedProperties { get; internal set; }

        /// <summary>
        /// Gets the list of EDM navigation properties odata.count values to be added in the response.
        /// It could be null if no navigation property with a $count segment.
        /// </summary>
        public IDictionary<IEdmNavigationProperty, ExpandedCountSelectItem> ExpandedCountProperties { get; internal set; }

        /// <summary>
        /// Gets the list of EDM navigation properties to be referenced in the response along with the nested query options embedded in the expand.
        /// It could be null if no navigation property to reference.
        /// </summary>
        public IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> ReferencedProperties { get; internal set; }

        /// <summary>s
        /// Gets the list of dynamic properties to select. It could be null.
        /// </summary>
        public ISet<string> SelectedDynamicProperties { get; internal set; }

        /// <summary>
        /// Gets the flag to indicate the dynamic property to be included in the response or not.
        /// </summary>
        public bool SelectAllDynamicProperties { get; internal set; }

        /// <summary>
        /// Gets the list of OData actions to be included in the response. It could be null.
        /// </summary>
        public ISet<IEdmAction> SelectedActions { get; internal set; }

        /// <summary>
        /// Gets the list of OData functions to be included in the response. It could be null.
        /// </summary>
        public ISet<IEdmFunction> SelectedFunctions { get; internal set; }

        /// <summary>
        /// Initialize the Node from <see cref="SelectExpandClause"/> for the given <see cref="IEdmStructuredType"/>.
        /// </summary>
        /// <param name="selectExpandClause">The input select and expand clause ($select and $expand).</param>
        /// <param name="structuredType">The related structural type to select and expand.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="expandedReference">Is expanded reference.</param>
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
                EdmStructuralTypeInfo structuralTypeInfo = new EdmStructuralTypeInfo(model, structuredType);

                if (selectExpandClause == null)
                {
                    SelectAllDynamicProperties = true;

                    // includes navigation properties
                    SelectedNavigationProperties = structuralTypeInfo.AllNavigationProperties;

                    // includes all bound actions
                    SelectedActions = structuralTypeInfo.AllActions;

                    // includes all bound functions
                    SelectedFunctions = structuralTypeInfo.AllFunctions;

                    // includes all structural properties
                    if (structuralTypeInfo.AllStructuralProperties != null)
                    {
                        foreach (var property in structuralTypeInfo.AllStructuralProperties)
                        {
                            AddStructuralProperty(property, null);
                        }
                    }
                }
                else
                {
                    BuildSelectExpand(selectExpandClause, structuralTypeInfo);
                }

                AdjustSelectNavigationProperties();
            }
        }

        /// <summary>
        /// Build $select and $expand clause
        /// </summary>
        /// <param name="selectExpandClause">The select expand clause</param>
        /// <param name="structuralTypeInfo">The structural type properties.</param>
        private void BuildSelectExpand(SelectExpandClause selectExpandClause, EdmStructuralTypeInfo structuralTypeInfo)
        {
            Contract.Assert(selectExpandClause != null);
            Contract.Assert(structuralTypeInfo != null);

            var currentLevelPropertiesInclude = new Dictionary<IEdmStructuralProperty, SelectExpandIncludedProperty>();

            // Explicitly set SelectAllDynamicProperties as false,
            // Below will re-set it as true if it meets the select all condition.
            SelectAllDynamicProperties = false;
            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                // $expand=...
                ExpandedReferenceSelectItem expandReferenceItem = selectItem as ExpandedReferenceSelectItem;
                if (expandReferenceItem != null)
                {
                    BuildExpandItem(expandReferenceItem, currentLevelPropertiesInclude, structuralTypeInfo);
                    continue;
                }

                PathSelectItem pathSelectItem = selectItem as PathSelectItem;
                if (pathSelectItem != null)
                {
                    // $select=abc/.../xyz
                    BuildSelectItem(pathSelectItem, currentLevelPropertiesInclude, structuralTypeInfo);
                    continue;
                }

                WildcardSelectItem wildCardSelectItem = selectItem as WildcardSelectItem;
                if (wildCardSelectItem != null)
                {
                    // $select=*
                    MergeAllStructuralProperties(structuralTypeInfo.AllStructuralProperties, currentLevelPropertiesInclude);
                    MergeSelectedNavigationProperties(structuralTypeInfo.AllNavigationProperties);
                    SelectAllDynamicProperties = true;
                    continue;
                }

                NamespaceQualifiedWildcardSelectItem wildCardActionSelection = selectItem as NamespaceQualifiedWildcardSelectItem;
                if (wildCardActionSelection != null)
                {
                    // $select=NS.*
                    AddNamespaceWildcardOperation(wildCardActionSelection, structuralTypeInfo.AllActions, structuralTypeInfo.AllFunctions);
                    continue;
                }

                throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, selectItem.GetType().Name));
            }

            if (selectExpandClause.AllSelected)
            {
                MergeAllStructuralProperties(structuralTypeInfo.AllStructuralProperties, currentLevelPropertiesInclude);
                MergeSelectedNavigationProperties(structuralTypeInfo.AllNavigationProperties);
                MergeSelectedAction(structuralTypeInfo.AllActions);
                MergeSelectedFunction(structuralTypeInfo.AllFunctions);
                SelectAllDynamicProperties = true;
            }

            // to make sure the structural properties are in the same order defined in the type.
            if (structuralTypeInfo.AllStructuralProperties != null)
            {
                foreach (var structuralProperty in structuralTypeInfo.AllStructuralProperties)
                {
                    SelectExpandIncludedProperty includeProperty;
                    if (!currentLevelPropertiesInclude.TryGetValue(structuralProperty, out includeProperty))
                    {
                        continue;
                    }

                    PathSelectItem pathSelectItem = includeProperty == null ? null : includeProperty.ToPathSelectItem();
                    AddStructuralProperty(structuralProperty, pathSelectItem);
                }
            }
        }

        /// <summary>
        /// Build the $expand item, it maybe $expand=nav, $expand=complex/nav, $expand=nav/$ref, etc.
        /// </summary>
        /// <param name="expandReferenceItem">The expanded reference select item.</param>
        /// <param name="currentLevelPropertiesInclude">The current properties to include at current level.</param>
        /// <param name="structuralTypeInfo">The structural type properties.</param>
        private void BuildExpandItem(ExpandedReferenceSelectItem expandReferenceItem,
            IDictionary<IEdmStructuralProperty, SelectExpandIncludedProperty> currentLevelPropertiesInclude,
            EdmStructuralTypeInfo structuralTypeInfo)
        {
            Contract.Assert(expandReferenceItem != null && expandReferenceItem.PathToNavigationProperty != null);
            Contract.Assert(currentLevelPropertiesInclude != null);
            Contract.Assert(structuralTypeInfo != null);

            // Verify and process the $expand=abc/xyz/nav.
            ODataExpandPath expandPath = expandReferenceItem.PathToNavigationProperty;
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment segment = expandPath.GetFirstNonTypeCastSegment(out remainingSegments);

            PropertySegment firstPropertySegment = segment as PropertySegment;
            if (firstPropertySegment != null)
            {
                // for example: $expand=abc/xyz/nav, the remaining segment can't be null
                // because at least the last navigation property segment is there.
                Contract.Assert(remainingSegments != null);

                if (structuralTypeInfo.IsStructuralPropertyDefined(firstPropertySegment.Property))
                {
                    SelectExpandIncludedProperty newPropertySelectItem;
                    if (!currentLevelPropertiesInclude.TryGetValue(firstPropertySegment.Property, out newPropertySelectItem))
                    {
                        newPropertySelectItem = new SelectExpandIncludedProperty(firstPropertySegment);
                        currentLevelPropertiesInclude[firstPropertySegment.Property] = newPropertySelectItem;
                    }

                    newPropertySelectItem.AddSubExpandItem(remainingSegments, expandReferenceItem);
                }
            }
            else
            {
                // for example: $expand=nav, or $expand=NS.SubType/nav, the navigation property segment should be the last segment.
                // So, the remaining segments should be null.
                Contract.Assert(remainingSegments == null);

                NavigationPropertySegment firstNavigationSegment = segment as NavigationPropertySegment;
                Contract.Assert(firstNavigationSegment != null);

                if (structuralTypeInfo.IsNavigationPropertyDefined(firstNavigationSegment.NavigationProperty))
                {
                    // $expand=..../nav/$count
                    ExpandedCountSelectItem expandedCount = expandReferenceItem as ExpandedCountSelectItem;
                    if (expandedCount != null)
                    {
                        if (ExpandedCountProperties == null)
                        {
                            ExpandedCountProperties = new Dictionary<IEdmNavigationProperty, ExpandedCountSelectItem>();
                        }

                        ExpandedCountProperties[firstNavigationSegment.NavigationProperty] = expandedCount;
                        return;
                    }

                    // It's not allowed to have mulitple navigation expanded or referenced.
                    // for example: "$expand=nav($top=2),nav($skip=3)" is not allowed and will be merged (or throw exception) at ODL side.
                    ExpandedNavigationSelectItem expanded = expandReferenceItem as ExpandedNavigationSelectItem;
                    if (expanded != null)
                    {
                        if (ExpandedProperties == null)
                        {
                            ExpandedProperties = new Dictionary<IEdmNavigationProperty, ExpandedNavigationSelectItem>();
                        }

                        ExpandedProperties[firstNavigationSegment.NavigationProperty] = expanded;
                    }
                    else
                    {
                        // $expand=..../nav/$ref
                        if (ReferencedProperties == null)
                        {
                            ReferencedProperties = new Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem>();
                        }

                        ReferencedProperties[firstNavigationSegment.NavigationProperty] = expandReferenceItem;
                    }
                }
            }
        }

        /// <summary>
        /// Build the $select item, it maybe $select=complex/abc, $select=abc, $select=nav, etc.
        /// </summary>
        /// <param name="pathSelectItem">The expanded reference select item.</param>
        /// <param name="currentLevelPropertiesInclude">The current properties to include at current level.</param>
        /// <param name="structuralTypeInfo">The structural type properties.</param>
        private void BuildSelectItem(PathSelectItem pathSelectItem,
            IDictionary<IEdmStructuralProperty, SelectExpandIncludedProperty> currentLevelPropertiesInclude,
            EdmStructuralTypeInfo structuralTypeInfo)
        {
            Contract.Assert(pathSelectItem != null && pathSelectItem.SelectedPath != null);
            Contract.Assert(currentLevelPropertiesInclude != null);
            Contract.Assert(structuralTypeInfo != null);

            // Verify and process the $select=abc/xyz/....
            ODataSelectPath selectPath = pathSelectItem.SelectedPath;
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment segment = selectPath.GetFirstNonTypeCastSegment(out remainingSegments);

            PropertySegment firstPropertySegment = segment as PropertySegment;
            if (firstPropertySegment != null)
            {
                if (structuralTypeInfo.IsStructuralPropertyDefined(firstPropertySegment.Property))
                {
                    // $select=abc/xyz/...
                    SelectExpandIncludedProperty newPropertySelectItem;
                    if (!currentLevelPropertiesInclude.TryGetValue(firstPropertySegment.Property, out newPropertySelectItem))
                    {
                        newPropertySelectItem = new SelectExpandIncludedProperty(firstPropertySegment);
                        currentLevelPropertiesInclude[firstPropertySegment.Property] = newPropertySelectItem;
                    }

                    newPropertySelectItem.AddSubSelectItem(remainingSegments, pathSelectItem);
                }

                return;
            }

            // If the first segment is not a property segment,
            // that segment must be the last segment, so the remainging segments should be null.
            Contract.Assert(remainingSegments == null);

            NavigationPropertySegment navigationSegment = segment as NavigationPropertySegment;
            if (navigationSegment != null)
            {
                // for example: $select=NavigationProperty or $select=NS.VipCustomer/VipNav
                if (structuralTypeInfo.IsNavigationPropertyDefined(navigationSegment.NavigationProperty))
                {
                    if (SelectedNavigationProperties == null)
                    {
                        SelectedNavigationProperties = new HashSet<IEdmNavigationProperty>();
                    }

                    SelectedNavigationProperties.Add(navigationSegment.NavigationProperty);
                }

                return;
            }

            OperationSegment operationSegment = segment as OperationSegment;
            if (operationSegment != null)
            {
                // for example: $select=NS.Operation, or, $select=NS.VipCustomer/NS.Operation
                AddOperations(operationSegment, structuralTypeInfo.AllActions, structuralTypeInfo.AllFunctions);
                return;
            }

            DynamicPathSegment dynamicPathSegment = segment as DynamicPathSegment;
            if (dynamicPathSegment != null)
            {
                if (SelectedDynamicProperties == null)
                {
                    SelectedDynamicProperties = new HashSet<string>();
                }

                SelectedDynamicProperties.Add(dynamicPathSegment.Identifier);
                return;
            }

            // In fact, we should never be here, because it's verified above
            throw new ODataException(Error.Format(SRResources.SelectionTypeNotSupported, segment.GetType().Name));
        }

        private static void MergeAllStructuralProperties(ISet<IEdmStructuralProperty> allStructuralProperties,
            IDictionary<IEdmStructuralProperty, SelectExpandIncludedProperty> currentLevelPropertiesInclude)
        {
            if (allStructuralProperties == null)
            {
                return;
            }

            Contract.Assert(currentLevelPropertiesInclude != null);

            foreach (var property in allStructuralProperties)
            {
                if (!currentLevelPropertiesInclude.ContainsKey(property))
                {
                    // Set the value as null is safe, because this property should not further process.
                    // Besides, if there's "WildcardSelectItem", there's no other property selection items.
                    currentLevelPropertiesInclude[property] = null;
                }
            }
        }

        private void MergeSelectedNavigationProperties(ISet<IEdmNavigationProperty> allNavigationProperties)
        {
            if (allNavigationProperties == null)
            {
                return;
            }

            if (SelectedNavigationProperties == null)
            {
                SelectedNavigationProperties = allNavigationProperties;
            }
            else
            {
                SelectedNavigationProperties.UnionWith(allNavigationProperties);
            }
        }

        private void MergeSelectedAction(ISet<IEdmAction> allActions)
        {
            if (allActions == null)
            {
                return;
            }

            if (SelectedActions == null)
            {
                SelectedActions = allActions;
            }
            else
            {
                SelectedActions.UnionWith(allActions);
            }
        }

        private void MergeSelectedFunction(ISet<IEdmFunction> allFunctions)
        {
            if (allFunctions == null)
            {
                return;
            }

            if (SelectedFunctions == null)
            {
                SelectedFunctions = allFunctions;
            }
            else
            {
                SelectedFunctions.UnionWith(allFunctions);
            }
        }

        private void AddStructuralProperty(IEdmStructuralProperty structuralProperty, PathSelectItem pathSelectItem)
        {
            bool isComplexOrCollectComplex = IsComplexOrCollectionComplex(structuralProperty);

            if (isComplexOrCollectComplex)
            {
                if (SelectedComplexTypeProperties == null)
                {
                    SelectedComplexTypeProperties = new Dictionary<IEdmStructuralProperty, PathSelectItem>();
                }

                SelectedComplexTypeProperties[structuralProperty] = pathSelectItem;
            }
            else
            {
                if (SelectedStructuralProperties == null)
                {
                    SelectedStructuralProperties = new HashSet<IEdmStructuralProperty>();
                }

                // for primitive, enum and collection them, needn't care about the nested query options now.
                // So, skip the path select item.
                SelectedStructuralProperties.Add(structuralProperty);
            }
        }

        private void AddNamespaceWildcardOperation(NamespaceQualifiedWildcardSelectItem namespaceSelectItem, ISet<IEdmAction> allActions,
            ISet<IEdmFunction> allFunctions)
        {
            if (allActions == null)
            {
                SelectedActions = null;
            }
            else
            {
                SelectedActions = new HashSet<IEdmAction>(allActions.Where(a => a.Namespace == namespaceSelectItem.Namespace));
            }

            if (allFunctions == null)
            {
                SelectedFunctions = null;
            }
            else
            {
                SelectedFunctions = new HashSet<IEdmFunction>(allFunctions.Where(a => a.Namespace == namespaceSelectItem.Namespace));
            }
        }

        private void AddOperations(OperationSegment operationSegment, ISet<IEdmAction> allActions, ISet<IEdmFunction> allFunctions)
        {
            foreach (IEdmOperation operation in operationSegment.Operations)
            {
                IEdmAction action = operation as IEdmAction;
                if (action != null && allActions.Contains(action))
                {
                    if (SelectedActions == null)
                    {
                        SelectedActions = new HashSet<IEdmAction>();
                    }

                    SelectedActions.Add(action);
                }

                IEdmFunction function = operation as IEdmFunction;
                if (function != null && allFunctions.Contains(function))
                {
                    if (SelectedFunctions == null)
                    {
                        SelectedFunctions = new HashSet<IEdmFunction>();
                    }

                    SelectedFunctions.Add(function);
                }
            }
        }

        private void AdjustSelectNavigationProperties()
        {
            if (SelectedNavigationProperties != null)
            {
                // remove expanded navigation properties from the selected navigation properties.
                if (ExpandedProperties != null)
                {
                    SelectedNavigationProperties.ExceptWith(ExpandedProperties.Keys);
                }

                // remove referenced navigation properties from the selected navigation properties.
                if (ReferencedProperties != null)
                {
                    SelectedNavigationProperties.ExceptWith(ReferencedProperties.Keys);
                }
            }

            if (SelectedNavigationProperties != null && !SelectedNavigationProperties.Any())
            {
                SelectedNavigationProperties = null;
            }
        }

        /// <summary>
        /// Test whether the input structural property is complex property or collection of complex property.
        /// </summary>
        /// <param name="structuralProperty">The test structural property.</param>
        /// <returns>True/false.</returns>
        internal static bool IsComplexOrCollectionComplex(IEdmStructuralProperty structuralProperty)
        {
            if (structuralProperty == null)
            {
                return false;
            }

            if (structuralProperty.Type.IsComplex())
            {
                return true;
            }

            if (structuralProperty.Type.IsCollection())
            {
                if (structuralProperty.Type.AsCollection().ElementType().IsComplex())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Separate the structural properties into two parts:
        /// 1. Complex and collection of complex are nested structural properties.
        /// 2. Others are non-nested structural properties.
        /// </summary>
        /// <param name="structuredType">The structural type of the resource.</param>
        /// <param name="structuralProperties">The non-nested structural properties of the structural type.</param>
        /// <param name="nestedStructuralProperties">The nested structural properties of the structural type.</param>
        [Obsolete("This public method is not used anymore. It will be removed later.")]
        public static void GetStructuralProperties(IEdmStructuredType structuredType, HashSet<IEdmStructuralProperty> structuralProperties,
            HashSet<IEdmStructuralProperty> nestedStructuralProperties)
        {
            // Be noted: this method is not used anymore. Keeping it unremoved is only for non-breaking changes.
            // If any new requirement for such method, it's better to move to "EdmLibHelpers" class.

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

        /// <summary>
        /// An internal cache class used to provide the propert, operations
        /// and do verification on the given <see cref="IEdmStructuredType"/>.
        /// </summary>
        internal class EdmStructuralTypeInfo
        {
            /// <summary>
            /// Gets all structural properties defined on the structure type.
            /// </summary>
            public ISet<IEdmStructuralProperty> AllStructuralProperties { get; }

            /// <summary>
            /// Gets all navigation properties defined on the structure type.
            /// </summary>
            public ISet<IEdmNavigationProperty> AllNavigationProperties { get; }

            /// <summary>
            /// Gets all actions bonding to the structure type.
            /// </summary>
            public ISet<IEdmAction> AllActions { get; }

            /// <summary>
            /// Gets all function bonding to the structure type.
            /// </summary>
            public ISet<IEdmFunction> AllFunctions { get; }

            /// <summary>
            /// Creates a new instance of the <see cref="EdmStructuralTypeInfo"/> class
            /// </summary>
            /// <param name="model">The Edm model.</param>
            /// <param name="structuredType">The Edm structured Type.</param>
            public EdmStructuralTypeInfo(IEdmModel model, IEdmStructuredType structuredType)
            {
                Contract.Assert(model != null);
                Contract.Assert(structuredType != null);

                foreach (var edmProperty in structuredType.Properties())
                {
                    switch (edmProperty.PropertyKind)
                    {
                        case EdmPropertyKind.Structural:
                            if (AllStructuralProperties == null)
                            {
                                AllStructuralProperties = new HashSet<IEdmStructuralProperty>();
                            }

                            AllStructuralProperties.Add((IEdmStructuralProperty)edmProperty);
                            break;

                        case EdmPropertyKind.Navigation:
                            if (AllNavigationProperties == null)
                            {
                                AllNavigationProperties = new HashSet<IEdmNavigationProperty>();
                            }

                            AllNavigationProperties.Add((IEdmNavigationProperty)edmProperty);
                            break;
                    }
                }

                IEdmEntityType entityType = structuredType as IEdmEntityType;
                if (entityType != null)
                {
                    var actions = model.GetAvailableActions(entityType);
                    AllActions = actions.Any() ? new HashSet<IEdmAction>(actions) : null;

                    var functions = model.GetAvailableFunctions(entityType);
                    AllFunctions = functions.Any() ? new HashSet<IEdmFunction>(functions) : null;
                }
            }

            /// <summary>
            /// Tests whether a <see cref="IEdmStructuralProperty"/> is defined on this type.
            /// </summary>
            /// <param name="property">The test property.</param>
            /// <returns>True/false</returns>
            public bool IsStructuralPropertyDefined(IEdmStructuralProperty property)
            {
                return AllStructuralProperties != null && AllStructuralProperties.Contains(property);
            }

            /// <summary>
            /// Tests whether a <see cref="IEdmNavigationProperty"/> is defined on this type.
            /// </summary>
            /// <param name="property">The test property.</param>
            /// <returns>True/false</returns>
            public bool IsNavigationPropertyDefined(IEdmNavigationProperty property)
            {
                return AllNavigationProperties != null && AllNavigationProperties.Contains(property);
            }
        }
    }
}
