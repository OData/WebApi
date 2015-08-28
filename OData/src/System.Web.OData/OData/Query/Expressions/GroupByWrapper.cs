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
    internal class GroupByWrapper<TElement> : IEdmGeneratedObject
    {
        private readonly Dictionary<string, int?> _values = new Dictionary<string, int?>();
        protected static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();

        /// <summary>
        /// Create group by result
        /// </summary>
        public GroupByWrapper()
        {
        }

        /// <summary>
        /// Gets or sets the property container that contains the properties being grouped by. 
        /// </summary>
        public virtual PropertyContainer GroupByContainer { get; set; }

        /// <summary>
        /// Gets Type.
        /// </summary>
        /// <returns></returns>
        public virtual IEdmTypeReference GetEdmType()
        {
            var type = new EdmEntityType(string.Empty, "GroupingWrapper", baseType: null, isAbstract: false, isOpen: true);
            if (this.GroupByContainer != null)
            {
                foreach (var prop in this.GroupByContainer.ToDictionary(DefaultPropertyMapper))
                {
                    type.AddStructuralProperty(prop.Key, EdmPrimitiveTypeKind.String);
                }
            }

            return type.ToEdmTypeReference(true);
        }

        /// <summary>
        /// Get property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool TryGetPropertyValue(string propertyName, out object value)
        {
            if (this.GroupByContainer != null)
            {
                return this.GroupByContainer.ToDictionary(DefaultPropertyMapper).TryGetValue(propertyName, out value);
            }
            else
            {
                value = null;
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            var compareWith = obj as GroupByWrapper<TElement>;
            if (compareWith == null)
            {
                return false;
            }
            else if (this.GroupByContainer == null && compareWith.GroupByContainer == null)
            {
                return true;
            }
            else if (this.GroupByContainer != null && compareWith.GroupByContainer != null)
            {
                var dictionary1 = this.GroupByContainer.ToDictionary(DefaultPropertyMapper);
                var dictionary2 = compareWith.GroupByContainer.ToDictionary(DefaultPropertyMapper);
                return dictionary1.Count() == dictionary2.Count() && !dictionary1.Except(dictionary2).Any();
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (this.GroupByContainer == null)
            {
                return 0;
            }

            long hash = 1870403278L; //Arbitrary number from Anonymous Type GetHashCode implementation
            var dictionary = this.GroupByContainer.ToDictionary(DefaultPropertyMapper);
            foreach (var v in dictionary.Values)
            {
                hash = hash * -1521134295L + v.GetHashCode();
            }

            return (int)hash;
        }
    }
}
