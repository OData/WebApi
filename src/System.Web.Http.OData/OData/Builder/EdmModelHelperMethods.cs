// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Expressions;

namespace System.Web.Http.OData.Builder
{
    public static class EdmModelHelperMethods
    {
        internal static IEntitySetLinkBuilder GetEntitySetLinkBuilder(this IEdmModel model, IEdmEntitySet entitySet)
        {
            IEntitySetLinkBuilder annotation = model.GetAnnotationValue<IEntitySetLinkBuilder>(entitySet);
            if (annotation == null)
            {
                throw Error.NotSupported(SRResources.EntitySetHasNoBuildLinkAnnotation, entitySet.Name);
            }

            return annotation;
        }

        internal static void SetEntitySetLinkBuilderAnnotation(this IEdmModel model, IEdmEntitySet entitySet, IEntitySetLinkBuilder entitySetLinkBuilder)
        {
            model.SetAnnotationValue(entitySet, entitySetLinkBuilder);
        }

        internal static string GetEntitySetUrl(this IEdmModel model, IEdmEntitySet entitySet)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            EntitySetUrlAnnotation annotation = model.GetAnnotationValue<EntitySetUrlAnnotation>(entitySet);
            if (annotation == null)
            {
                return entitySet.Name;
            }
            else
            {
                return annotation.Url;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Pending")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Pending")]
        public static IEdmModel BuildEdmModel(ODataModelBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            EdmModel model = new EdmModel();

            Dictionary<string, IStructuralTypeConfiguration> typeConfigurations = builder.StructuralTypes.ToDictionary(t => t.FullName);

            Dictionary<string, IEdmStructuredType> types = EdmTypeBuilder.GetTypes(builder.StructuralTypes)
                .OfType<IEdmStructuredType>()
                .ToDictionary(t => t.ToString());

            foreach (IEdmStructuredType type in types.Values)
            {
                if (type.TypeKind == EdmTypeKind.Complex)
                {
                    model.AddElement(type as IEdmComplexType);
                }
                else if (type.TypeKind == EdmTypeKind.Entity)
                {
                    model.AddElement(type as IEdmEntityType);
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.UnsupportedEntityTypeInModel);
                }
                
                // pre-populate the model with clr-type annotations so that we dont have to scan 
                // all loaded assemblies to find the clr type for an edm type that we build.
                Type mappedClrType = typeConfigurations[(type as IEdmSchemaType).FullName()].ClrType;
                model.SetAnnotationValue<ClrTypeAnnotation>(type, new ClrTypeAnnotation(mappedClrType));
            }

            if (builder.EntitySets.Any() || builder.Procedures.Any())
            {
                EdmEntityContainer container = new EdmEntityContainer(builder.Namespace, builder.ContainerName);

                Dictionary<string, EdmEntitySet> edmEntitySetMap = new Dictionary<string, EdmEntitySet>();
                foreach (IEntitySetConfiguration entitySet in builder.EntitySets)
                {
                    EdmEntitySet edmEntitySet = container.AddEntitySet(entitySet.Name, (IEdmEntityType)types[entitySet.EntityType.FullName]);
                    EntitySetLinkBuilderAnnotation entitySetLinkBuilderAnnotation = new EntitySetLinkBuilderAnnotation(entitySet);

                    model.SetEntitySetLinkBuilderAnnotation(edmEntitySet, entitySetLinkBuilderAnnotation);
                    model.SetAnnotationValue<EntitySetUrlAnnotation>(edmEntitySet, new EntitySetUrlAnnotation { Url = entitySet.GetUrl() });
                    edmEntitySetMap.Add(edmEntitySet.Name, edmEntitySet);
                }

                foreach (IEntitySetConfiguration entitySet in builder.EntitySets)
                {
                    EdmEntitySet edmEntitySet = edmEntitySetMap[entitySet.Name];
                    EntitySetLinkBuilderAnnotation entitySetLinkBuilderAnnotation = model.GetEntitySetLinkBuilder(edmEntitySet) as EntitySetLinkBuilderAnnotation;
                    foreach (NavigationPropertyConfiguration navigation in entitySet.EntityType.NavigationProperties)
                    {
                        NavigationPropertyBinding binding = entitySet.FindBinding(navigation);
                        if (binding != null)
                        {
                            EdmEntityType edmEntityType = types[entitySet.EntityType.FullName] as EdmEntityType;
                            IEdmNavigationProperty edmNavigationProperty = edmEntityType.NavigationProperties().Single(np => np.Name == navigation.Name);

                            edmEntitySet.AddNavigationTarget(edmNavigationProperty, edmEntitySetMap[binding.EntitySet.Name]);
                            entitySetLinkBuilderAnnotation.AddNavigationPropertyLinkBuilder(edmNavigationProperty, entitySet.GetNavigationPropertyLink(edmNavigationProperty.Name));
                        }
                    }
                }

                foreach (ProcedureConfiguration procedure in builder.Procedures)
                {                
                    switch (procedure.Kind)
                    {
                        case ProcedureKind.Action:
                            ActionConfiguration action = procedure as ActionConfiguration;
                            IEdmTypeReference returnReference = GetEdmTypeReference(types, action.ReturnType, nullable: true);
                            IEdmExpression expression = GetEdmEntitySetExpression(edmEntitySetMap, action);

                            EdmFunctionImport functionImport = new EdmFunctionImport(container, action.Name, returnReference, expression, action.IsSideEffecting, action.IsComposable, action.IsBindable);
                            foreach (ParameterConfiguration parameter in action.Parameters)
                            {
                                // TODO: need so support configuring nullability of parameters. Currently we default to nullable.
                                IEdmTypeReference parameterTypeReference = GetEdmTypeReference(types, parameter.TypeConfiguration, nullable: true);
                                EdmFunctionParameter functionParameter = new EdmFunctionParameter(functionImport, parameter.Name, parameterTypeReference, EdmFunctionParameterMode.In);
                                functionImport.AddParameter(functionParameter);
                            }
                            container.AddElement(functionImport);
                            break;
                        
                        case ProcedureKind.Function:
                            Contract.Assert(false, "Functions are not supported.");
                            break;
                        
                        case ProcedureKind.ServiceOperation:
                            Contract.Assert(false, "ServiceOperations are not supported.");
                            break;
                    }
                }
                model.AddElement(container);
            }
            return model;
        }

        private static IEdmExpression GetEdmEntitySetExpression(Dictionary<string, EdmEntitySet> entitySets, ActionConfiguration action)
        {
            if (action.EntitySet != null)
            {
                if (entitySets.ContainsKey(action.EntitySet.Name))
                {
                    EdmEntitySet entitySet = entitySets[action.EntitySet.Name];
                    return new EdmEntitySetReferenceExpression(entitySet);
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.EntitySetNotFoundForName, action.EntitySet.Name);
                }
            }
            return null;
        }

        private static IEdmTypeReference GetEdmTypeReference(Dictionary<string, IEdmStructuredType> availableTypes, IEdmTypeConfiguration configuration, bool nullable)
        {
            Contract.Assert(availableTypes != null);
            
            if (configuration == null)
            {
                return null;
            }

            EdmTypeKind kind = configuration.Kind;
            if (kind == EdmTypeKind.Collection)
            {
                ICollectionTypeConfiguration collectionType = configuration as ICollectionTypeConfiguration;
                EdmCollectionType edmCollectionType = new EdmCollectionType(GetEdmTypeReference(availableTypes, collectionType.ElementType, false));
                return new EdmCollectionTypeReference(edmCollectionType, nullable);
            }
            else if (availableTypes.ContainsKey(configuration.FullName))
            {
                IEdmStructuredType structuralType = availableTypes[configuration.FullName];
                if (kind == EdmTypeKind.Complex)
                {
                    return new EdmComplexTypeReference(structuralType as IEdmComplexType, nullable);
                }
                else if (kind == EdmTypeKind.Entity)
                {
                    return new EdmEntityTypeReference(structuralType as IEdmEntityType, nullable);
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.UnsupportedEdmTypeKind, kind.ToString());
                }
            }
            else if (configuration.Kind == EdmTypeKind.Primitive)
            {
                PrimitiveTypeConfiguration primitiveTypeConfiguration = configuration as PrimitiveTypeConfiguration;
                return new EdmPrimitiveTypeReference(primitiveTypeConfiguration.EdmPrimitiveType, nullable);
            }
            else
            {
                throw Error.InvalidOperation(SRResources.NoMatchingIEdmTypeFound, configuration.FullName);
            }
        }
    }
}
