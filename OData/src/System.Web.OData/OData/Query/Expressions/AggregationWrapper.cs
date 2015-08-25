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
    public class AggregationWrapper<TElement> : IEdmGeneratedObject
    {
        private readonly Dictionary<string, int> _values = new Dictionary<string, int>();

        /// <summary>
        /// Creates aggregation results for predefined property
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="value"></param>
        public AggregationWrapper( string alias, int value)
        {
            this._values.Add(alias, value);
        }

        /// <summary>
        /// An ID to uniquely identify the model in the <see cref="ModelContainer"/>.
        /// </summary>
        public string ModelID { get; private set; }

        /// <summary>
        /// Gets Type.
        /// </summary>
        /// <returns></returns>
        public IEdmTypeReference GetEdmType()
        {
            var type = new EdmEntityType(string.Empty, "AggregationWrapper", baseType: null ,isAbstract: false, isOpen: true);
            foreach (var prop in this._values)
            {
                var structProp = type.AddStructuralProperty(prop.Key, EdmPrimitiveTypeKind.Int32);
            }

            return type.ToEdmTypeReference(true);
            //Type elementType = GetElementType();
            //return GetModel().GetEdmTypeReference(elementType);
        }

        /// <summary>
        /// Get property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            int intValue;
            if (this._values.TryGetValue(propertyName, out intValue))
            {
                value = intValue;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Temp method to get property value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public int GetProperty(string propertyName)
        {
            return this._values[propertyName];
        }

        private Type GetElementType()
        {
            return typeof(TElement);
        }

        private IEdmModel GetModel()
        {
            return ModelContainer.GetModel(ModelID);
        }
    }
}
