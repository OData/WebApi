// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Expressions;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Builder
{
    internal static class EdmModelHelperMethods
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
            Dictionary<Type, IEdmStructuredType> edmTypeMap = model.AddTypes(builder.StructuralTypes);
            Dictionary<string, EdmEntitySet> edmEntitySetMap = model.AddEntitySets(builder, container, edmTypeMap);

            // add procedures
            model.AddProcedures(builder.Procedures, container, edmTypeMap, edmEntitySetMap);

            // finish up
            model.AddElement(container);
            model.SetIsDefaultEntityContainer(container, isDefaultContainer: true);

            // build the map from IEdmEntityType to IEdmFunctionImport
            model.SetAnnotationValue<BindableProcedureFinder>(model, new BindableProcedureFinder(model));

            // set the data service version annotations.
            model.SetDataServiceVersion(builder.DataServiceVersion);
            model.SetMaxDataServiceVersion(builder.MaxDataServiceVersion);

            return model;
        }

        private static void AddTypes(this EdmModel model, Dictionary<Type, IEdmStructuredType> structuredTypes)
        {
            Contract.Assert(model != null);
            Contract.Assert(structuredTypes != null);

            foreach (IEdmStructuredType type in structuredTypes.Values)
            {
                model.AddType(type);
            }
        }

        private static Dictionary<string, EdmEntitySet> AddEntitySets(this EdmModel model, ODataModelBuilder builder, EdmEntityContainer container, Dictionary<Type, IEdmStructuredType> edmTypeMap)
        {
            IEnumerable<EntitySetConfiguration> configurations = builder.EntitySets;

            // build the entitysets and their annotations
            IEnumerable<Tuple<EdmEntitySet, EntitySetConfiguration>> entitySets = AddEntitySets(configurations, container, edmTypeMap);
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
                model.SetEntitySetLinkBuilder(entitySet, iter.Annotations.LinkBuilder);

                AddNavigationBindings(iter.Configuration, iter.EntitySet, iter.Annotations.LinkBuilder, builder, edmTypeMap, edmEntitySetMap);
            }
            return edmEntitySetMap;
        }

        private static void AddNavigationBindings(EntitySetConfiguration configuration, EdmEntitySet entitySet, EntitySetLinkBuilderAnnotation linkBuilder, ODataModelBuilder builder,
            Dictionary<Type, IEdmStructuredType> edmTypeMap, Dictionary<string, EdmEntitySet> edmEntitySetMap)
        {
            foreach (EntityTypeConfiguration entity in builder.ThisAndBaseAndDerivedTypes(configuration.EntityType))
            {
                foreach (NavigationPropertyConfiguration navigation in entity.NavigationProperties)
                {
                    NavigationPropertyBindingConfiguration binding = configuration.FindBinding(navigation);
                    if (binding != null)
                    {
                        EdmEntityType edmEntityType = edmTypeMap[entity.ClrType] as EdmEntityType;
                        IEdmNavigationProperty edmNavigationProperty = edmEntityType.NavigationProperties().Single(np => np.Name == navigation.Name);

                        entitySet.AddNavigationTarget(edmNavigationProperty, edmEntitySetMap[binding.EntitySet.Name]);

                        NavigationLinkBuilder linkBuilderFunc = configuration.GetNavigationPropertyLink(navigation);
                        if (linkBuilderFunc != null)
                        {
                            linkBuilder.AddNavigationPropertyLinkBuilder(edmNavigationProperty, linkBuilderFunc);
                        }
                    }
                }
            }
        }

        private static void AddProcedureParameters(EdmFunctionImport functionImport, ProcedureConfiguration procedure, Dictionary<Type, IEdmStructuredType> edmTypeMap)
        {
            foreach (ParameterConfiguration parameter in procedure.Parameters)
            {
                // TODO: http://aspnetwebstack.codeplex.com/workitem/417
                bool isParameterOptional = EdmLibHelpers.IsNullable(parameter.TypeConfiguration.ClrType);
                IEdmTypeReference parameterTypeReference = GetEdmTypeReference(edmTypeMap, parameter.TypeConfiguration, nullable: isParameterOptional);
                EdmFunctionParameter functionParameter = new EdmFunctionParameter(functionImport, parameter.Name, parameterTypeReference, EdmFunctionParameterMode.In);
                functionImport.AddParameter(functionParameter);
            }
        }

        private static void AddProcedureLinkBuilder(IEdmModel model, EdmFunctionImport functionImport, ProcedureConfiguration procedure)
        {
            if (procedure.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity)
            {
                ActionConfiguration action = procedure as ActionConfiguration;
                FunctionConfiguration function = procedure as FunctionConfiguration;
                if (action != null && action.GetActionLink() != null)
                {
                    model.SetActionLinkBuilder(functionImport, new ActionLinkBuilder(action.GetActionLink(), action.FollowsConventions));
                }
                else if (function != null && function.GetFunctionLink() != null)
                {
                    model.SetFunctionLinkBuilder(functionImport, new FunctionLinkBuilder(function.GetFunctionLink(), function.FollowsConventions));
                }
            }
        }

        private static void ValidateProcedureEntitySetPath(IEdmModel model, EdmFunctionImport functionImport, ProcedureConfiguration procedure)
        {
            IEdmFunctionParameter procedureParameter;
            IEnumerable<IEdmNavigationProperty> navPath;
            if (procedure.EntitySetPath != null && !functionImport.TryGetRelativeEntitySetPath(model, out procedureParameter, out navPath))
            {
                throw Error.InvalidOperation(SRResources.ProcedureHasInvalidEntitySetPath, String.Join("/", procedure.EntitySetPath.ToArray()), procedure.FullName);
            }
        }

        private static void AddProcedures(this IEdmModel model, IEnumerable<ProcedureConfiguration> configurations, EdmEntityContainer container, Dictionary<Type, IEdmStructuredType> edmTypeMap, Dictionary<string, EdmEntitySet> edmEntitySetMap)
        {
            foreach (ProcedureConfiguration procedure in configurations)
            {
                switch (procedure.Kind)
                {
                    case ProcedureKind.Action:
                    case ProcedureKind.Function:
                        IEdmTypeReference returnReference = GetEdmTypeReference(edmTypeMap, procedure.ReturnType, nullable: true);
                        IEdmExpression expression = GetEdmEntitySetExpression(edmEntitySetMap, procedure);
                        EdmFunctionImport functionImport = new EdmFunctionImport(container, procedure.Name, returnReference, expression, procedure.IsSideEffecting, procedure.IsComposable, procedure.IsBindable);

                        AddProcedureParameters(functionImport, procedure, edmTypeMap);
                        if (procedure.IsBindable)
                        {
                            model.SetIsAlwaysBindable(functionImport, procedure.IsAlwaysBindable);
                            AddProcedureLinkBuilder(model, functionImport, procedure);
                            ValidateProcedureEntitySetPath(model, functionImport, procedure);
                        }
                        container.AddElement(functionImport);
                        break;
                    case ProcedureKind.ServiceOperation:
                        Contract.Assert(false, "ServiceOperations are not supported.");
                        break;
                }
            }
        }

        private static Dictionary<Type, IEdmStructuredType> AddTypes(this EdmModel model, IEnumerable<StructuralTypeConfiguration> types)
        {
            StructuralTypeConfiguration[] configTypes = types.ToArray();

            // build types
            EdmTypeMap edmTypeMap = EdmTypeBuilder.GetTypesAndProperties(configTypes);
            Dictionary<Type, IEdmStructuredType> edmTypes = edmTypeMap.EdmTypes;

            // Add an annotate types
            model.AddTypes(edmTypes);
            model.AddClrTypeAnnotations(edmTypes);

            // add annotation for properties
            Dictionary<PropertyInfo, IEdmProperty> edmProperties = edmTypeMap.EdmProperties;
            model.AddClrPropertyInfoAnnotations(edmProperties);
            model.AddPropertyRestrictionsAnnotations(edmTypeMap.EdmPropertiesRestrictions);
            return edmTypes;
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

        private static EdmEntitySet AddEntitySet(this EdmEntityContainer container, EntitySetConfiguration entitySet, IDictionary<Type, IEdmStructuredType> edmTypeMap)
        {
            return container.AddEntitySet(entitySet.Name, (IEdmEntityType)edmTypeMap[entitySet.EntityType.ClrType]);
        }

        private static IEnumerable<Tuple<EdmEntitySet, EntitySetConfiguration>> AddEntitySets(IEnumerable<EntitySetConfiguration> entitySets, EdmEntityContainer container, Dictionary<Type, IEdmStructuredType> edmTypeMap)
        {
            return entitySets.Select(es => Tuple.Create(container.AddEntitySet(es, edmTypeMap), es));
        }

        private static void AddClrTypeAnnotations(this EdmModel model, Dictionary<Type, IEdmStructuredType> edmTypes)
        {
            foreach (KeyValuePair<Type, IEdmStructuredType> map in edmTypes)
            {
                // pre-populate the model with clr-type annotations so that we dont have to scan 
                // all loaded assemblies to find the clr type for an edm type that we build.
                IEdmStructuredType edmType = map.Value;
                Type clrType = map.Key;
                model.SetAnnotationValue<ClrTypeAnnotation>(edmType, new ClrTypeAnnotation(clrType));
            }
        }

        private static void AddClrPropertyInfoAnnotations(this EdmModel model, Dictionary<PropertyInfo, IEdmProperty> edmProperties)
        {
            foreach (KeyValuePair<PropertyInfo, IEdmProperty> edmPropertyMap in edmProperties)
            {
                IEdmProperty edmProperty = edmPropertyMap.Value;
                PropertyInfo clrProperty = edmPropertyMap.Key;
                if (edmProperty.Name != clrProperty.Name)
                {
                    model.SetAnnotationValue(edmProperty, new ClrPropertyInfoAnnotation(clrProperty));
                }
            }
        }

        private static void AddPropertyRestrictionsAnnotations(this EdmModel model, Dictionary<IEdmProperty, QueryableRestrictions> edmPropertiesRestrictions)
        {
            foreach (KeyValuePair<IEdmProperty, QueryableRestrictions> edmPropertyRestriction in edmPropertiesRestrictions)
            {
                IEdmProperty edmProperty = edmPropertyRestriction.Key;
                QueryableRestrictions restrictions = edmPropertyRestriction.Value;
                model.SetAnnotationValue(edmProperty, new QueryableRestrictionsAnnotation(restrictions));
            }
        }

        private static IEdmExpression GetEdmEntitySetExpression(Dictionary<string, EdmEntitySet> entitySets, ProcedureConfiguration procedure)
        {
            if (procedure.EntitySet != null)
            {
                if (entitySets.ContainsKey(procedure.EntitySet.Name))
                {
                    EdmEntitySet entitySet = entitySets[procedure.EntitySet.Name];
                    return new EdmEntitySetReferenceExpression(entitySet);
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.EntitySetNotFoundForName, procedure.EntitySet.Name);
                }
            }
            else if (procedure.EntitySetPath != null)
            {
                return new EdmPathExpression(procedure.EntitySetPath);
            }

            return null;
        }

        private static IEdmTypeReference GetEdmTypeReference(Dictionary<Type, IEdmStructuredType> availableTypes, IEdmTypeConfiguration configuration, bool nullable)
        {
            Contract.Assert(availableTypes != null);

            if (configuration == null)
            {
                return null;
            }

            EdmTypeKind kind = configuration.Kind;
            if (kind == EdmTypeKind.Collection)
            {
                CollectionTypeConfiguration collectionType = configuration as CollectionTypeConfiguration;
                EdmCollectionType edmCollectionType = new EdmCollectionType(GetEdmTypeReference(availableTypes, collectionType.ElementType, false));
                return new EdmCollectionTypeReference(edmCollectionType, nullable);
            }
            else if (availableTypes.ContainsKey(configuration.ClrType))
            {
                IEdmStructuredType structuralType = availableTypes[configuration.ClrType];
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
                annotation = new BindableProcedureFinder(model);
                model.SetAnnotationValue(model, annotation);
            }

            return annotation.FindProcedures(entityType);
        }
    }
}
