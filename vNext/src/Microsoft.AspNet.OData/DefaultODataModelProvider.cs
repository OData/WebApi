using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.AspNet.OData.Builder;
using System.Reflection;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData
{
    using System.Linq;

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
                    ProcedureConfiguration configuration = null;
                    var functionAttribute =
                        method.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(ODataFunctionAttribute));

                    if (functionAttribute != null)
                    {
                        configuration = builder.Function(method.Name);
                    }

                    var actionAttribute =
                        method.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(ODataActionAttribute));
                    if (actionAttribute != null)
                    {
                        configuration = builder.Action(method.Name);
                    }

                    var entityType = builder.AddEntityType(entityClrType);
                    if (configuration != null)
                    {
                        configuration.ReturnType = entityType;
                        configuration.IsComposable = true;
                        configuration.NavigationSource =
                            builder.NavigationSources.FirstOrDefault(n => n.EntityType == entityType) as NavigationSourceConfiguration;

                        foreach (var parameterInfo in method.GetParameters())
                        {
                            var parameterType = builder.AddEntityType(parameterInfo.ParameterType);
                            configuration.AddParameter(parameterInfo.Name, parameterType);
                        }
                    }
                }
            }

            return builder.GetEdmModel();
        }
    }
}
