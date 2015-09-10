using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Formatter;
using System.Web.OData.Query;


namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Represents a container class that contains properties that are grouped by using $apply.
    /// </summary>
    public class GroupByWrapper : IEdmGeneratedObject
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>
        /// Gets Type.
        /// </summary>
        /// <returns></returns>
        public IEdmTypeReference GetEdmType()
        {
            var type = new EdmEntityType(string.Empty, "GroupingWrapper", baseType: null, isAbstract: false, isOpen: true);
            foreach (var prop in this._values)
            {
                type.AddStructuralProperty(prop.Key, EdmPrimitiveTypeKind.String);
            }

            return type.ToEdmTypeReference(true);
        }


        /// <summary>
        /// Get property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            if (this._values.TryGetValue(propertyName, out value))
            {
                // TODO: Refactor ApplyClause by OData team spec and infer type sduring parsing
                if (value != null)
                {
                    value = value.ToString();
                }
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Get property value.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object GetPropertyValue(string propertyName)
        {
            return this._values[propertyName];
        }

        /// <summary>
        /// Set property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetPropertyValue(string propertyName, object value)
        {
            this._values[propertyName] = value;
        }

        /// <summary>
        /// Compares to wrappers
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var compareWith = obj as GroupByWrapper;
            if (compareWith == null)
            {
                return false;
            }

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
            long hash = 1870403278L; //Arbitrary number from Anonymous Type GetHashCode implementation
            foreach (var v in this._values.Values)
            {
                hash = hash * -1521134295L + v.GetHashCode();
            }

            return (int)hash;
        }
    }
}
