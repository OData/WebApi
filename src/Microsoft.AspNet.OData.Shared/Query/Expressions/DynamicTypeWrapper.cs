// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
[module: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Extra needed to workaorund EF issue with expression shape.")]

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// Represents a container class that contains properties that are grouped by using $apply.
    /// </summary>
    public abstract class DynamicTypeWrapper
    {
        /// <summary>
        /// Gets values stored in the wrapper
        /// </summary>
        public abstract Dictionary<string, object> Values { get; }

        /// <summary>
        /// Attempts to get the value of the Property called <paramref name="propertyName"/> from the underlying Entity.
        /// </summary>
        /// <param name="propertyName">The name of the Property</param>
        /// <param name="value">The new value of the Property</param>
        /// <returns>True if successful</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Generics not appropriate here")]
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            return this.Values.TryGetValue(propertyName, out value);
        }
    }

    [JsonConverter(typeof(DynamicTypeWrapperConverter))]
    internal class GroupByWrapper : DynamicTypeWrapper
    {
        private Dictionary<string, object> _values;
        protected static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();

        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public virtual AggregationPropertyContainer GroupByContainer { get; set; }

        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public virtual AggregationPropertyContainer Container { get; set; }

        public override Dictionary<string, object> Values
        {
            get
            {
                EnsureValues();
                return this._values;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var compareWith = obj as GroupByWrapper;
            if (compareWith == null)
            {
                return false;
            }
            var dictionary1 = this.Values;
            var dictionary2 = compareWith.Values;
            return dictionary1.Count() == dictionary2.Count() && !dictionary1.Except(dictionary2).Any();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            EnsureValues();
            long hash = 1870403278L; //Arbitrary number from Anonymous Type GetHashCode implementation
            foreach (var v in this.Values.Values)
            {
                hash = (hash * -1521134295L) + (v == null ? 0 : v.GetHashCode());
            }

            return (int)hash;
        }

        private void EnsureValues()
        {
            if (_values == null)
            {
                if (this.GroupByContainer != null)
                {
                    this._values = this.GroupByContainer.ToDictionary(DefaultPropertyMapper);
                }
                else
                {
                    this._values = new Dictionary<string, object>();
                }

                if (this.Container != null)
                {
                    _values.MergeWithReplace(this.Container.ToDictionary(DefaultPropertyMapper));
                }
            }
        }
    }

    internal class FlatteningWrapper<T> : GroupByWrapper
    {
        public T Source { get; set; }
    }

    internal class NoGroupByWrapper : GroupByWrapper
    {
    }

    internal class AggregationWrapper : GroupByWrapper
    {
    }

    internal class NoGroupByAggregationWrapper : GroupByWrapper
    {
    }

    internal class EntitySetAggregationWrapper : GroupByWrapper
    {
    }

    internal class ComputeWrapper<T> : GroupByWrapper, IEdmEntityObject
    {
        public T Instance { get; set; }

        /// <summary>
        /// An ID to uniquely identify the model in the <see cref="ModelContainer"/>.
        /// </summary>
        public string ModelID { get; set; }

        public override Dictionary<string, object> Values
        {
            get
            {
                EnsureValues();
                return base.Values;
            }
        }

        private bool _merged;
        private void EnsureValues()
        {
            if (!this._merged)
            {
                // Base properties available via Instance can be real OData properties or generated in previous transformations

                var instanceContainer = this.Instance as DynamicTypeWrapper;
                if (instanceContainer != null)
                {
                    // Add proeprties generated in previous transformations to the collection
                    base.Values.MergeWithReplace(instanceContainer.Values);
                }
                else
                {
                    // Add real OData properties to the collection
                    // We need to use injected Model to real property names
                    var edmType = GetEdmType() as IEdmEntityTypeReference;
                    _typedEdmEntityObject = _typedEdmEntityObject ??
                        new TypedEdmEntityObject(Instance, edmType, GetModel());

                    var props = edmType.DeclaredStructuralProperties().Where(p => p.Type.IsPrimitive()).Select(p => p.Name);
                    foreach (var propertyName in props)
                    {
                        object value;
                        if (_typedEdmEntityObject.TryGetPropertyValue(propertyName, out value))
                        {
                            base.Values[propertyName] = value;
                        }
                    }
                }
                this._merged = true;
            }
        }
        private TypedEdmEntityObject _typedEdmEntityObject;

        private IEdmModel GetModel()
        {
            Contract.Assert(ModelID != null);

            return ModelContainer.GetModel(ModelID);
        }

        public IEdmTypeReference GetEdmType()
        {
            IEdmModel model = GetModel();
            return model.GetEdmTypeReference(typeof(T));
        }
    }
}
