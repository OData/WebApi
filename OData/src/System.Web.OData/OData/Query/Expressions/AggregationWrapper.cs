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
    /// Represents a container class that contains properties that are either aggregated  using $apply.
    /// </summary>
    internal class AggregationWrapper<TElement> : GroupByWrapper<TElement>
    {
        /// <summary>
        /// Gets or sets the property container that contains the properties being aggregated. 
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
                    type.AddStructuralProperty(prop.Key, EdmPrimitiveTypeKind.String);
                }
            }

            if (this.Container != null)
            {
                foreach (var prop in this.Container.ToDictionary(DefaultPropertyMapper))
                {
                    type.AddStructuralProperty(prop.Key, EdmPrimitiveTypeKind.String);
                }
            }

            return type.ToEdmTypeReference(true);
        }

        public override bool TryGetPropertyValue(string propertyName, out object value)
        {
            if (base.TryGetPropertyValue(propertyName, out value)
                || this.Container.ToDictionary(DefaultPropertyMapper).TryGetValue(propertyName, out value))
            {
                // TODO: Refactor ApplyClause by OData team spec and infer type sduring parsing
                if (value != null)
                {
                    value = value.ToString();
                }
                return true;
            }

            return false;
        }

    }
}
