using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.OData
{
<<<<<<< HEAD
    using System.Runtime.Serialization;

    using Microsoft.AspNet.OData.Formatter;

=======
>>>>>>> refs/remotes/OData/vNext
    internal class DefaultODataModelProvider
    {
        public static IEdmModel BuildEdmModel(Type ApiContextType)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = ApiContextType.Namespace;

            var publicProperties = ApiContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in publicProperties)
            {
                var propertyAttribute = property.GetCustomAttribute<IgnoreDataMemberAttribute>();
                if (propertyAttribute != null)
                {
                    continue;
                }

                var entityClrType = TypeHelper.GetImplementedIEnumerableType(property.PropertyType);
                if (entityClrType != null)
                {
                    var entity = builder.AddEntityType(entityClrType);
                    builder.AddEntitySet(property.Name, entity);
                }
                else
                {
                    if (property.PropertyType.GetTypeInfo().IsPrimitive
                        || EdmLibHelpers.GetEdmPrimitiveTypeOrNull(property.PropertyType) != null)
                    {
                        builder.AddPrimitiveType(property.PropertyType);
                    }
                    else
                    {
                        // The property is not an IEnumerable implementation
                        builder.AddEntityType(property.PropertyType);
                    }
                }
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
<<<<<<< HEAD
=======
                    //method.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(ODataFunctionAttribute));
>>>>>>> refs/remotes/OData/vNext

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

<<<<<<< HEAD
=======

>>>>>>> refs/remotes/OData/vNext
                    if (configuration != null)
                    {
                        if (method.ReturnType.IsCollection())
                        {
                            configuration.ReturnType = new CollectionTypeConfiguration(entityType, method.ReturnType);
                        }
                        else
                        {
                            configuration.ReturnType = entityType;
                        }

                        configuration.IsComposable = true;
                        configuration.NavigationSource =
                            builder.NavigationSources.FirstOrDefault(n => n.EntityType == entityType) as NavigationSourceConfiguration;

                        foreach (var parameterInfo in method.GetParameters())
                        {
<<<<<<< HEAD
                            if (parameterInfo.ParameterType.GetTypeInfo().IsPrimitive || EdmLibHelpers.GetEdmPrimitiveTypeOrNull(parameterInfo.ParameterType) != null)
=======
                            if (parameterInfo.ParameterType.GetTypeInfo().IsPrimitive
                                || parameterInfo.ParameterType == typeof(decimal)
                                || parameterInfo.ParameterType == typeof(string))
>>>>>>> refs/remotes/OData/vNext
                            {
                                var primitiveType = builder.AddPrimitiveType(parameterInfo.ParameterType);
                                configuration.AddParameter(parameterInfo.Name, primitiveType);
                            }
                            else
                            {

                                if (parameterInfo.ParameterType.IsCollection())
                                {
<<<<<<< HEAD
                                    if (parameterInfo.ParameterType.GenericTypeArguments[0].GetTypeInfo().IsPrimitive || EdmLibHelpers.GetEdmPrimitiveTypeOrNull(parameterInfo.ParameterType) != null)
=======
                                    if (parameterInfo.ParameterType.GenericTypeArguments[0].GetTypeInfo().IsPrimitive)
>>>>>>> refs/remotes/OData/vNext
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
