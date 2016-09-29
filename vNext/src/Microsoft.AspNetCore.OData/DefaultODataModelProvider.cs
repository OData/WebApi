using Microsoft.AspNetCore.OData.Builder;
using Microsoft.OData.Edm;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.OData
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
                    OperationConfiguration configuration = null;
                    PrimitiveTypeConfiguration primitiveEntityType = null;
                    EntityTypeConfiguration entityType = null;

                    if (entityClrType.GetTypeInfo().IsPrimitive
                               || entityClrType.GetType() == typeof(decimal)
                               || entityClrType.GetType() == typeof(string))
                    {
                        primitiveEntityType = builder.AddPrimitiveType(entityClrType);
                    }
                    else
                    {
                        entityType = builder.AddEntityType(entityClrType);
                    }

                    var functionAttribute = method.GetCustomAttribute<ODataFunctionAttribute>();
                    if (functionAttribute != null)
                    {
                        configuration = builder.Function(method.Name);
                        if (functionAttribute.IsBound)
                        {
                            configuration.SetBindingParameterImplementation(functionAttribute.BindingName, entityType);
                        }
                    }

                    var actionAttribute = method.GetCustomAttribute<ODataActionAttribute>();
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
                        if (primitiveEntityType == null)
                        {
                            configuration.ReturnType = entityType;
                        }
                        else
                        {
                            configuration.ReturnType = primitiveEntityType;
                        }

                        configuration.IsComposable = true;
                        configuration.NavigationSource =
                            builder.NavigationSources.FirstOrDefault(n => n.EntityType == entityType) as NavigationSourceConfiguration;

                        foreach (var parameterInfo in method.GetParameters())
                        {
                            if (parameterInfo.ParameterType.GetTypeInfo().IsPrimitive
                                || parameterInfo.ParameterType == typeof(decimal)
                                || parameterInfo.ParameterType == typeof(string))
                            {
                                var primitiveType = builder.AddPrimitiveType(parameterInfo.ParameterType);
                                configuration.AddParameter(parameterInfo.Name, primitiveType);
                            }
                            else
                            {

                                if (parameterInfo.ParameterType.IsCollection())
                                {
                                    if (parameterInfo.ParameterType.GenericTypeArguments[0].GetTypeInfo().IsPrimitive)
                                    {
                                        var parameterType = builder.AddPrimitiveType(parameterInfo.ParameterType.GenericTypeArguments[0]);
                                        var collectionTypeConfig = new CollectionTypeConfiguration(parameterType, parameterInfo.ParameterType.GenericTypeArguments[0]);
                                        configuration.AddParameter(parameterInfo.Name, collectionTypeConfig);
                                    }
                                    else
                                    {
                                        var parameterType = builder.AddEntityType(parameterInfo.ParameterType.GenericTypeArguments[0]);
                                        var collectionTypeConfig = new CollectionTypeConfiguration(parameterType, parameterInfo.ParameterType.GenericTypeArguments[0]);
                                        configuration.AddParameter(parameterInfo.Name, collectionTypeConfig);
                                    }
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
            }

            return builder.GetEdmModel();
        }
    }
}
