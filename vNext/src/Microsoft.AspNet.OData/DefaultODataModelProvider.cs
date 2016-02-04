using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.AspNet.OData.Builder;
using System.Reflection;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData
{
    internal class DefaultODataModelProvider
    {
        public static IEdmModel BuildEdmModel(Type apiContextType, Action<ODataConventionModelBuilder> after = null)
        {
            var builder = new ODataConventionModelBuilder
            {
                Namespace = apiContextType.Namespace
            };
            
            var publicProperties = apiContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in publicProperties)
            {
                var entityClrType = TypeHelper.GetImplementedIEnumerableType(property.PropertyType);
                var entity = builder.AddEntityType(entityClrType);
                builder.AddEntitySet(property.Name, entity);
            }
            after?.Invoke(builder);
            return builder.GetEdmModel();
        }
    }
}
