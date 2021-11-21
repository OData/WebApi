//-----------------------------------------------------------------------------
// <copyright file="AutoSelectExpandHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query
{
    internal static class AutoSelectExpandHelper
    {
        #region Auto Select and Expand Test
        /// <summary>
        /// Tests whether there are auto select properties.
        /// So far, we only test one depth for auto select, shall we go through the deeper depth?
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="structuredType">The type from value or from path.</param>
        /// <param name="property">The property from path, it can be null.</param>
        /// <returns>true if the structured type has auto select properties; otherwise false.</returns>
        public static bool HasAutoSelectProperty(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty property)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            foreach (IEdmStructuredType edmStructuredType in new SelfAndDerivedEnumerator(structuredType, edmModel))
            {
                // for top type, let's retrieve its properties and the properties from base type of top type if has.
                // for derived type, let's retrieve the declared properties.
                IEnumerable<IEdmStructuralProperty> properties = edmStructuredType == structuredType
                        ? edmStructuredType.StructuralProperties()
                        : edmStructuredType.DeclaredStructuralProperties();

                foreach (IEdmStructuralProperty subProperty in properties)
                {
                    if (IsAutoSelect(subProperty, property, edmStructuredType, edmModel))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tests whether there are auto expand properties.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <param name="property">The property from path, it can be null.</param>
        /// <returns>true if the structured type has auto expand properties; otherwise false.</returns>
        public static bool HasAutoExpandProperty(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty property)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            return edmModel.HasAutoExpandProperty(structuredType, property, new HashSet<IEdmStructuredType>());
        }

        private static bool HasAutoExpandProperty(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty pathProperty, ISet<IEdmStructuredType> visited)
        {
            if (visited.Contains(structuredType))
            {
                return false;
            }
            visited.Add(structuredType);


            foreach (IEdmStructuredType edmStructuredType in new SelfAndDerivedEnumerator(structuredType, edmModel))
            {
                // for top type, let's retrieve its properties and the properties from base type of top type if has.
                // for derived type, let's retrieve the declared properties.
                IEnumerable<IEdmProperty> properties = edmStructuredType == structuredType
                        ? edmStructuredType.Properties()
                        : edmStructuredType.DeclaredProperties;

                foreach (IEdmProperty property in properties)
                {
                    switch (property.PropertyKind)
                    {
                        case EdmPropertyKind.Structural:
                            IEdmStructuralProperty structuralProperty = (IEdmStructuralProperty)property;
                            IEdmTypeReference typeRef = property.Type.GetElementTypeOrSelf();
                            if (typeRef.IsComplex() && edmModel.CanExpand(typeRef.AsComplex().ComplexDefinition(), structuralProperty))
                            {
                                IEdmStructuredType subStrucutredType = typeRef.AsStructured().StructuredDefinition();
                                if (edmModel.HasAutoExpandProperty(subStrucutredType, structuralProperty, visited))
                                {
                                    return true;
                                }
                            }
                            break;

                        case EdmPropertyKind.Navigation:
                            IEdmNavigationProperty navigationProperty = (IEdmNavigationProperty)property;
                            if (IsAutoExpand(navigationProperty, pathProperty, edmStructuredType, edmModel))
                            {
                                return true; // find an auto-expand navigation property path
                            }
                            break;
                    }
                }
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Gets the auto select paths.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <param name="pathProperty">The property from path, it can be null.</param>
        /// <param name="querySettings">The query settings.</param>
        /// <returns>The auto select paths.</returns>
        public static IEnumerable<SelectModelPath> GetAutoSelectPaths(this IEdmModel edmModel, IEdmStructuredType structuredType,
            IEdmProperty pathProperty, ModelBoundQuerySettings querySettings = null)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            List<SelectModelPath> autoSelectProperties = null;
            foreach (IEdmStructuredType edmStructuredType in new SelfAndDerivedEnumerator(structuredType, edmModel))
            {
                // for top type, let's retrieve its properties and the properties from base type of top type if has.
                // for derived type, let's retrieve the declared properties.
                IEnumerable<IEdmStructuralProperty> properties = (edmStructuredType == structuredType) ?
                    edmStructuredType.StructuralProperties() :
                    properties = edmStructuredType.DeclaredStructuralProperties();

                foreach (IEdmStructuralProperty property in properties)
                {
                    if (IsAutoSelect(property, pathProperty, edmStructuredType, edmModel, querySettings))
                    {
                        if (autoSelectProperties == null)
                        {
                            autoSelectProperties = new List<SelectModelPath>(1);
                        }

                        if (edmStructuredType == structuredType)
                        {
                            autoSelectProperties.Add(new SelectModelPath(new[] { property }));
                        }
                        else
                        {
                            autoSelectProperties.Add(new SelectModelPath(new IEdmElement[] { edmStructuredType, property }));
                        }
                    }
                }
            }

            return autoSelectProperties ?? Enumerable.Empty<SelectModelPath>();
        }

        /// <summary>
        /// Gets the auto expand paths.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <param name="property">The property starting from, it can be null.</param>
        /// <param name="isSelectPresent">Is $select presented.</param>
        /// <param name="querySettings">The query settings.</param>
        /// <returns>The auto expand paths.</returns>
        public static IEnumerable<ExpandModelPath> GetAutoExpandPaths(this IEdmModel edmModel, IEdmStructuredType structuredType,
            IEdmProperty property, bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            Stack<IEdmElement> nodes = new Stack<IEdmElement>();
            ISet<IEdmStructuredType> visited = new HashSet<IEdmStructuredType>();
            IList<ExpandModelPath> results = new List<ExpandModelPath>();

            // type and property from path is higher priority
            edmModel.GetAutoExpandPaths(structuredType, property, nodes, visited, results, isSelectPresent, querySettings);

            Contract.Assert(nodes.Count == 0);
            return results;
        }

        public static bool IsAutoExpand(IEdmProperty navigationProperty,
            IEdmProperty pathProperty, IEdmStructuredType pathStructuredType, IEdmModel edmModel,
            bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            QueryableRestrictionsAnnotation annotation = EdmLibHelpers.GetPropertyRestrictions(navigationProperty, edmModel);
            if (annotation != null && annotation.Restrictions.AutoExpand)
            {
                return !annotation.Restrictions.DisableAutoExpandWhenSelectIsPresent || !isSelectPresent;
            }

            if (querySettings == null)
            {
                querySettings = EdmLibHelpers.GetModelBoundQuerySettings(pathProperty, pathStructuredType, edmModel);
            }

            if (querySettings != null && querySettings.IsAutomaticExpand(navigationProperty.Name))
            {
                return true;
            }

            return false;
        }

        public static bool IsAutoSelect(IEdmProperty property, IEdmProperty pathProperty,
            IEdmStructuredType pathStructuredType, IEdmModel edmModel, ModelBoundQuerySettings querySettings = null)
        {
            if (querySettings == null)
            {
                querySettings = EdmLibHelpers.GetModelBoundQuerySettings(pathProperty, pathStructuredType, edmModel);
            }

            if (querySettings != null && querySettings.IsAutomaticSelect(property.Name))
            {
                return true;
            }

            return false;
        }

        private static bool CanExpand(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty property)
        {
            // first for back-compability, check the queryable restriction
            QueryableRestrictionsAnnotation annotation = EdmLibHelpers.GetPropertyRestrictions(property, edmModel);
            if (annotation != null && annotation.Restrictions.NotExpandable)
            {
                return false;
            }

            ModelBoundQuerySettings settings = edmModel.GetModelBoundQuerySettingsOrNull(structuredType, property);
            if (settings != null && !settings.Expandable(property.Name))
            {
                return false;
            }

            return true;
        }

        private static void GetAutoExpandPaths(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty pathProperty,
            Stack<IEdmElement> nodes, ISet<IEdmStructuredType> visited, IList<ExpandModelPath> results,
            bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            if (visited.Contains(structuredType))
            {
                return;
            }
            visited.Add(structuredType);

            foreach (IEdmStructuredType edmStructuredType in new SelfAndDerivedEnumerator(structuredType, edmModel))
            {
                IEnumerable<IEdmProperty> properties;

                if (edmStructuredType == structuredType)
                {
                    // for base type, let's retrieve its properties and the properties from base type of base type if have.
                    properties = edmStructuredType.Properties();
                }
                else
                {
                    // for derived type, let's retrieve the declared properties.
                    properties = edmStructuredType.DeclaredProperties;
                    nodes.Push(edmStructuredType); // add a type cast for derived type
                }

                foreach (IEdmProperty property in properties)
                {
                    switch (property.PropertyKind)
                    {
                        case EdmPropertyKind.Structural:
                            IEdmStructuralProperty structuralProperty = (IEdmStructuralProperty)property;
                            IEdmTypeReference typeRef = property.Type.GetElementTypeOrSelf();
                            if (typeRef.IsComplex() && edmModel.CanExpand(typeRef.AsComplex().ComplexDefinition(), structuralProperty))
                            {
                                IEdmStructuredType subStructuredType = typeRef.AsStructured().StructuredDefinition();

                                nodes.Push(structuralProperty);

                                edmModel.GetAutoExpandPaths(subStructuredType, structuralProperty, nodes, visited, results, isSelectPresent, querySettings);

                                nodes.Pop();
                            }
                            break;

                        case EdmPropertyKind.Navigation:
                            IEdmNavigationProperty navigationProperty = (IEdmNavigationProperty)property;
                            if (IsAutoExpand(navigationProperty, pathProperty, edmStructuredType, edmModel, isSelectPresent, querySettings))
                            {
                                nodes.Push(navigationProperty);
                                results.Add(new ExpandModelPath(nodes.Reverse())); // found  an auto-expand navigation property path
                                nodes.Pop();
                            }
                            break;
                    }
                }

                if (edmStructuredType != structuredType)
                {
                    nodes.Pop(); // pop the type cast for derived type
                }
            }
        }

        /// <summary>
        /// This is a helper that allows us to avoid inefficiencies in the previous pattern:
        ///     var structuredTypes = new List&lt;IEdmStructuredType&gt;();
        ///     structuredTypes.Add(structuredType);
        ///     structuredTypes.AddRange(edmModel.FindAllDerivedTypes(structuredType));
        ///
        /// Specifically, the allocation of the list and the resizing driven by the "AddRange" call are
        /// avoided by leveraging a struct with a simple state machine for enumerating over a type
        /// and its derived types.
        /// </summary>
        private struct SelfAndDerivedEnumerator : IEnumerator<IEdmStructuredType>
        {
            private enum Stage : byte
            {
                Initial,
                Self,
                Derived,
            }

            private readonly IEnumerator<IEdmStructuredType> derivedEnumerator;
            private readonly IEdmStructuredType structuredType;

            private Stage stage;

            public SelfAndDerivedEnumerator(IEdmStructuredType structuredType, IEdmModel edmModel)
            {
                if (structuredType == null)
                {
                    throw new ArgumentNullException(nameof(structuredType));
                }

                if (edmModel == null)
                {
                    throw new ArgumentNullException(nameof(edmModel));
                }

                this.stage = Stage.Initial;
                this.derivedEnumerator = edmModel.FindAllDerivedTypes(structuredType).GetEnumerator();
                this.structuredType = structuredType;
            }

            public IEdmStructuredType Current
            {
                get
                {
                    switch (stage)
                    {
                        case Stage.Self:
                            return this.structuredType;

                        case Stage.Derived:
                            return this.derivedEnumerator.Current;

                        default:
                            throw new InvalidOperationException("Enumeration is an invalid state");
                    }
                }
            }

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                this.derivedEnumerator.Dispose();
            }

            public SelfAndDerivedEnumerator GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                switch (this.stage)
                {
                    case Stage.Initial:
                        this.stage = Stage.Self;
                        return true;

                    case Stage.Self:
                        this.stage = Stage.Derived;
                        goto case Stage.Derived;

                    case Stage.Derived:
                        return this.derivedEnumerator.MoveNext();

                    default:
                        return false;
                }
            }

            public void Reset()
            {
                this.stage = 0;
                this.derivedEnumerator.Reset();
            }
        }
    }
}