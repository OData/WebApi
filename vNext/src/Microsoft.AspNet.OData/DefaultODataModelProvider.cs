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

                    var entityType = builder.AddEntityType(entityClrType);

                    var functionAttribute = method.GetCustomAttribute<ODataFunctionAttribute>();
                        //method.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(ODataFunctionAttribute));

                    if (functionAttribute != null)
                    {
                        configuration = builder.Function(method.Name);
                        if (functionAttribute.IsBound)
                        {
                            configuration.SetBindingParameterImplementation(functionAttribute.BindingName, entityType);
                        }
                    }

                    var actionAttribute = method.GetCustomAttribute<ODataActionAttribute>();
                    //method.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(ODataActionAttribute));
                    if (actionAttribute != null)
                    {
                        configuration = builder.Action(method.Name);
                        if (actionAttribute.IsBound)
                        {
                            configuration.SetBindingParameterImplementation(actionAttribute.BindingName, entityType);
                        }
                    }

                    
                    if (configuration != null)
                    {
                        configuration.ReturnType = entityType;
                        configuration.IsComposable = true;
                        configuration.NavigationSource =
                            builder.NavigationSources.FirstOrDefault(n => n.EntityType == entityType) as NavigationSourceConfiguration;

                        foreach (var parameterInfo in method.GetParameters())
                        {
                            if (parameterInfo.ParameterType.GetTypeInfo().IsPrimitive)
                            {
                                var primitiveType = builder.AddPrimitiveType(parameterInfo.ParameterType);
                                configuration.AddParameter(parameterInfo.Name, primitiveType);
                            }
                            else
                            {
                                var parameterType = builder.AddEntityType(parameterInfo.ParameterType);
                                configuration.AddParameter(parameterInfo.Name, parameterType);
                            }
                        }
                    }
                }
            }

            return builder.GetEdmModel();
        }
    }
}
