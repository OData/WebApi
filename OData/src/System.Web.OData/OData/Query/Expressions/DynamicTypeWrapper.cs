// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Represents a container class that contains properties that are grouped by using $apply.
    /// </summary>
    internal class DynamicTypeWrapper
    {
        private Dictionary<string, object> _values;// = new Dictionary<string, object>();
        protected static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();


        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public virtual AggregationPropertyContainer GroupByContainer { get; set; }

        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public virtual AggregationPropertyContainer Container { get; set; }

        public Dictionary<string, object> Values
        {
            get
            {
                EnsureValues();
                return this._values;
            }
        }


        /// <summary>
        /// Get property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Justification = "Generics not appropriate here")]
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            EnsureValues();
            return this._values.TryGetValue(propertyName, out value);
        }

        /// <summary>
        /// Compares to wrappers
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            EnsureValues();
            var compareWith = obj as DynamicTypeWrapper;
            if (compareWith == null)
            {
                return false;
            }
            compareWith.EnsureValues();
            var dictionary1 = this._values;
            var dictionary2 = compareWith._values;
            return dictionary1.Count() == dictionary2.Count() && !dictionary1.Except(dictionary2).Any();
        }

        /// <summary>
        /// Gets hashcode.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            EnsureValues();
            long hash = 1870403278L; //Arbitrary number from Anonymous Type GetHashCode implementation
            foreach (var v in this._values.Values)
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
                    _values = _values.Concat(this.Container.ToDictionary(DefaultPropertyMapper)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
        }
    }
    internal class NoGroupByWrapper : DynamicTypeWrapper
    {

    }
    internal class AggregationWrapper : DynamicTypeWrapper
    {
    }

    internal class NoGroupByAggregationWrapper : DynamicTypeWrapper
    {
    }

}
