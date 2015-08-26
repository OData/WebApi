using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Formatter;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Represents a container class that contains properties that are either aggregared or grouped by using $apply.
    /// </summary>
    internal class GroupingWrapper<TElement> : IEdmGeneratedObject
    {
        private readonly Dictionary<string, int?> _values = new Dictionary<string, int?>();
        protected static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();


        /// <summary>
        /// Create aggregation result
        /// </summary>
        public GroupingWrapper()
        {

        }

        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
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
                    var structProp = type.AddStructuralProperty(prop.Key, EdmPrimitiveTypeKind.Int32);
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
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            return this.GroupByContainer.ToDictionary(DefaultPropertyMapper).TryGetValue(propertyName, out value);
        }

        /// <summary>
        /// Temp method to get property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public int? GetProperty(string propertyName)
        {
            return this._values[propertyName];
        }
    }

    internal class AggregationWrapper<TElement> : GroupingWrapper<TElement>
    {
        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public virtual PropertyContainer Container { get; set; }

        /// <summary>
        /// Gets Type.
        /// </summary>
        /// <returns></returns>
        public override IEdmTypeReference GetEdmType()
        {
            var type = new EdmEntityType(string.Empty, "AggregationWrapper", baseType: null, isAbstract: false, isOpen: true);
            if (this.GroupByContainer != null)
            {
                foreach (var prop in this.GroupByContainer.ToDictionary(DefaultPropertyMapper))
                {
                    var structProp = type.AddStructuralProperty(prop.Key, EdmPrimitiveTypeKind.Int32);
                }
            }

            if (this.Container != null)
            {
                foreach (var prop in this.Container.ToDictionary(DefaultPropertyMapper))
                {
                    var structProp = type.AddStructuralProperty(prop.Key, EdmPrimitiveTypeKind.Int32);
                }
            }

            return type.ToEdmTypeReference(true);
        }
    }
}
