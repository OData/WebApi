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

            // Get the actions and functions into the model
            var publicMethods = ApiContextType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in publicMethods)
            {
                if (!method.IsSpecialName)
                {
                    var entityClrType = TypeHelper.GetImplementedIEnumerableType(method.ReturnType) ?? method.ReturnType;
                    ProcedureConfiguration configuration;

                    if (entityClrType.Name != "Void")
                    {
                        var returnType = builder.AddEntityType(entityClrType);
                        configuration = builder.Function(method.Name);
                        configuration.ReturnType = returnType;
                    }
                    else
                    {
                        configuration = builder.Action(method.Name);
                    }

                    foreach (var parameterInfo in method.GetParameters())
                    {
                        var parameterType = builder.AddEntityType(parameterInfo.ParameterType);
                        configuration.AddParameter(parameterInfo.Name, parameterType);
                    }
                }
            }

            return builder.GetEdmModel();
        }
    }
}
