using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using System.Web.OData.Builder;
using System.Reflection;

namespace Microsoft.AspNet.OData
{
    internal class DefaultODataModelProvider
    {
        public static IEdmModel BuildEdmModel(Type ApiContextType)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = ApiContextType.Namespace;

            var publicProperties = ApiContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in publicProperties)
            {
                var entityClrType = TypeHelper.GetImplementedIEnumerableType(property.PropertyType);
                EntityTypeConfiguration entity = builder.AddEntityType(entityClrType);
                builder.AddEntitySet(property.Name, entity);
            }

            return builder.GetEdmModel();
        }
    }
}
