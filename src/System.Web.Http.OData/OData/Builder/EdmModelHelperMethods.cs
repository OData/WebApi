// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Expressions;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Builder
{
    public static class EdmModelHelperMethods
    {
        public static IEdmModel BuildEdmModel(ODataModelBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer(builder.Namespace, builder.ContainerName);

            // add types and sets, building an index on the way.
            Dictionary<string, IEdmStructuredType> edmTypeMap = model.AddTypes(builder.StructuralTypes);
            Dictionary<string, EdmEntitySet> edmEntitySetMap = model.AddEntitySets(builder, container, edmTypeMap);

            // add procedures
            model.AddProcedures(builder.Procedures, container, edmTypeMap, edmEntitySetMap);

            // finish up
            model.AddElement(container);

            // build the map from IEdmEntityType to IEdmFunctionImport
            model.SetAnnotationValue<BindableProcedureFinder>(model, new BindableProcedureFinder(model));

            return model;
        }

        private static void AddTypes(this EdmModel model, IEnumerable<IEdmStructuredType> structuredTypes)
        {
            Contract.Assert(model != null);
            Contract.Assert(structuredTypes != null);

            foreach (IEdmStructuredType type in structuredTypes)
            {
                model.AddType(type);
            }
        }

        private static Dictionary<string, EdmEntitySet> AddEntitySets(this EdmModel model, ODataModelBuilder builder, EdmEntityContainer container, Dictionary<string, IEdmStructuredType> edmTypeMap)
        {
            IEnumerable<IEntitySetConfiguration> configurations = builder.EntitySets;

            // build the entitysets and their annotations
            IEnumerable<Tuple<EdmEntitySet, IEntitySetConfiguration>> entitySets = AddEntitySets(configurations, container, edmTypeMap);
            var entitySetAndAnnotations = entitySets.Select(e => new
            {
                EntitySet = e.Item1,
                Configuration = e.Item2,
                Annotations = new
                {
                    LinkBuilder = new EntitySetLinkBuilderAnnotation(e.Item2),
                    Url = new EntitySetUrlAnnotation { Url = e.Item2.GetUrl() }
                }
            }).ToArray();

            // index the entitySets by name
            Dictionary<string, EdmEntitySet> edmEntitySetMap = entitySetAndAnnotations.ToDictionary(e => e.EntitySet.Name, e => e.EntitySet);

            // apply the annotations
            foreach (var iter in entitySetAndAnnotations)
            {
                EdmEntitySet entitySet = iter.EntitySet;
                model.SetAnnotationValue<EntitySetUrlAnnotation>(entitySet, iter.Annotations.Url);
                model.SetAnnotationValue<IEntitySetLinkBuilder>(entitySet, iter.Annotations.LinkBuilder);

                AddNavigationBindings(iter.Configuration, iter.EntitySet, iter.Annotations.LinkBuilder, builder, edmTypeMap, edmEntitySetMap);
            }
            return edmEntitySetMap;
        }

        private static void AddNavigationBindings(IEntitySetConfiguration configuration, EdmEntitySet entitySet, EntitySetLinkBuilderAnnotation linkBuilder, ODataModelBuilder builder,
            Dictionary<string, IEdmStructuredType> edmTypeMap, Dictionary<string, EdmEntitySet> edmEntitySetMap)
                {
            foreach (IEntityTypeConfiguration entity in builder.ThisAndBaseAndDerivedTypes(configuration.EntityType))
            {
                foreach (NavigationPropertyConfiguration navigation in entity.NavigationProperties)
                {
                    NavigationPropertyBinding binding = configuration.FindBinding(navigation);
                    if (binding != null)
                    {
                        EdmEntityType edmEntityType = edmTypeMap[entity.FullName] as EdmEntityType;
                        IEdmNavigationProperty edmNavigationProperty = edmEntityType.NavigationProperties().Single(np => np.Name == navigation.Name);

                        entitySet.AddNavigationTarget(edmNavigationProperty, edmEntitySetMap[binding.EntitySet.Name]);
                        linkBuilder.AddNavigationPropertyLinkBuilder(edmNavigationProperty, configuration.GetNavigationPropertyLink(navigation));
                    }
                }
            }
        }

        private static void AddProcedures(this IEdmModel model, IEnumerable<ProcedureConfiguration> configurations, EdmEntityContainer container, Dictionary<string, IEdmStructuredType> edmTypeMap, Dictionary<string, EdmEntitySet> edmEntitySetMap)
        {
            foreach (ProcedureConfiguration procedure in configurations)
            {
                switch (procedure.Kind)
                {
                    case ProcedureKind.Action:
                        ActionConfiguration action = procedure as ActionConfiguration;
                        IEdmTypeReference returnReference = GetEdmTypeReference(edmTypeMap, action.ReturnType, nullable: true);
                        IEdmExpression expression = GetEdmEntitySetExpression(edmEntitySetMap, action);

                        EdmFunctionImport functionImport = new EdmFunctionImport(container, action.Name, returnReference, expression, action.IsSideEffecting, action.IsComposable, action.IsBindable);
                        if (action.IsBindable)
                        {
                            model.SetIsAlwaysBindable(functionImport, action.IsAlwaysBindable);
                            if (action.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity)
                            {
                                Func<EntityInstanceContext, Uri> actionFactory = action.GetActionLink();
                                if (actionFactory != null)
                                {
                                    model.SetAnnotationValue<ActionLinkBuilder>(functionImport, new ActionLinkBuilder(actionFactory));
                                }
                            }
                        }

                        foreach (ParameterConfiguration parameter in action.Parameters)
                        {
                            // TODO: http://aspnetwebstack.codeplex.com/workitem/417
                            IEdmTypeReference parameterTypeReference = GetEdmTypeReference(edmTypeMap, parameter.TypeConfiguration, nullable: true);
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
        }

        private static Dictionary<string, IEdmStructuredType> AddTypes(this EdmModel model, IEnumerable<IStructuralTypeConfiguration> types)
        {
            IStructuralTypeConfiguration[] configTypes = types.ToArray();

            // build types
            IEdmStructuredType[] edmTypes = EdmTypeBuilder.GetTypes(configTypes).ToArray();

            // index types
            Dictionary<string, IEdmStructuredType> edmTypeMap = edmTypes.ToDictionary(t => (t as IEdmSchemaType).FullName());

            // Add an annotate types
            model.AddTypes(edmTypes);
            model.AddClrTypeAnnotations(GenerateEdmToClrMap(configTypes, edmTypeMap));
            return edmTypeMap;
        }

        private static Dictionary<IEdmStructuredType, Type> GenerateEdmToClrMap(IEnumerable<IStructuralTypeConfiguration> configTypes, Dictionary<string, IEdmStructuredType> edmTypeMap)
        {
            return configTypes.ToDictionary(c => edmTypeMap[c.FullName], c => c.ClrType);
        }

        private static void AddType(this EdmModel model, IEdmStructuredType type)
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
                Contract.Assert(false, "Only ComplexTypes and EntityTypes are supported.");
            }
        }

        private static EdmEntitySet AddEntitySet(this EdmEntityContainer container, IEntitySetConfiguration entitySet, IDictionary<string, IEdmStructuredType> edmTypeMap)
        {
            return container.AddEntitySet(entitySet.Name, (IEdmEntityType)edmTypeMap[entitySet.EntityType.FullName]);
        }

        private static IEnumerable<Tuple<EdmEntitySet, IEntitySetConfiguration>> AddEntitySets(IEnumerable<IEntitySetConfiguration> entitySets, EdmEntityContainer container, Dictionary<string, IEdmStructuredType> edmTypeMap)
        {
            return entitySets.Select(es => Tuple.Create(container.AddEntitySet(es, edmTypeMap), es));
        }

        private static void AddClrTypeAnnotations(this EdmModel model, IDictionary<IEdmStructuredType, Type> annotationMap)
        {
            foreach (KeyValuePair<IEdmStructuredType, Type> map in annotationMap)
            {
                // pre-populate the model with clr-type annotations so that we dont have to scan 
                // all loaded assemblies to find the clr type for an edm type that we build.
                IEdmStructuredType edmType = map.Key;
                Type clrType = map.Value;
                model.SetAnnotationValue<ClrTypeAnnotation>(edmType, new ClrTypeAnnotation(clrType));
            }
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

        internal static IEnumerable<IEdmFunctionImport> GetAvailableProcedures(this IEdmModel model, IEdmEntityType entityType)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            BindableProcedureFinder annotation = model.GetAnnotationValue<BindableProcedureFinder>(model);
            if (annotation == null)
            {
                return Enumerable.Empty<IEdmFunctionImport>();
            }
            else
            {
                return annotation.FindProcedures(entityType);
            }
        }

        internal static ActionLinkBuilder GetActionLinkBuilder(this IEdmModel model, IEdmFunctionImport action)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            ActionLinkBuilder annotation = model.GetAnnotationValue<ActionLinkBuilder>(action);
            return annotation;
        }
    }
}
