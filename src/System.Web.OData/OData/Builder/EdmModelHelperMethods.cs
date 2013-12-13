// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Expressions;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Expressions;
using Microsoft.OData.Edm.Validation;

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

        private static void AddProcedureParameters(EdmOperation operation, ProcedureConfiguration procedure, Dictionary<Type, IEdmStructuredType> edmTypeMap)
        {
            foreach (ParameterConfiguration parameter in procedure.Parameters)
            {
                // TODO: http://aspnetwebstack.codeplex.com/workitem/417
                bool isParameterOptional = EdmLibHelpers.IsNullable(parameter.TypeConfiguration.ClrType);
                IEdmTypeReference parameterTypeReference = GetEdmTypeReference(edmTypeMap, parameter.TypeConfiguration, nullable: isParameterOptional);
                IEdmOperationParameter operationParameter = new EdmOperationParameter(operation, parameter.Name, parameterTypeReference);
                operation.AddParameter(operationParameter);
            }
        }

        private static void AddProcedureLinkBuilder(IEdmModel model, IEdmOperation operation, ProcedureConfiguration procedure)
        {
            if (procedure.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity)
            {
                ActionConfiguration actionConfiguration = procedure as ActionConfiguration;
                IEdmAction action = operation as IEdmAction;
                FunctionConfiguration functionConfiguration = procedure as FunctionConfiguration;
                IEdmFunction function = operation as IEdmFunction;
                if (actionConfiguration != null && actionConfiguration.GetActionLink() != null && action != null)
                {
                    model.SetActionLinkBuilder(
                        action,
                        new ActionLinkBuilder(actionConfiguration.GetActionLink(), actionConfiguration.FollowsConventions));
                }
                else if (functionConfiguration != null && functionConfiguration.GetFunctionLink() != null && function != null)
                {
                    model.SetFunctionLinkBuilder(
                        function,
                        new FunctionLinkBuilder(functionConfiguration.GetFunctionLink(), functionConfiguration.FollowsConventions));
                }
            }
        }

        private static void ValidateProcedureEntitySetPath(IEdmModel model, IEdmOperationImport operationImport, ProcedureConfiguration procedure)
        {
            IEdmOperationParameter procedureParameter;
            IEnumerable<IEdmNavigationProperty> navPath;
            IEnumerable<EdmError> edmErrors;
            if (procedure.EntitySetPath != null && !operationImport.TryGetRelativeEntitySetPath(model, out procedureParameter, out navPath, out edmErrors))
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
                        AddProcedure(model, procedure, container, edmTypeMap, edmEntitySetMap, isAction: true);
                        break;
                    case ProcedureKind.Function:
                        AddProcedure(model, procedure, container, edmTypeMap, edmEntitySetMap, isAction: false);
                        break;
                    case ProcedureKind.ServiceOperation:
                        Contract.Assert(false, "ServiceOperations are not supported.");
                        break;
                }
            }
        }

        private static void AddProcedure(
            this IEdmModel model,
            ProcedureConfiguration procedure,
            EdmEntityContainer container,
            Dictionary<Type, IEdmStructuredType> edmTypeMap,
            Dictionary<string, EdmEntitySet> edmEntitySetMap,
            bool isAction)
        {
            IEdmTypeReference returnReference = GetEdmTypeReference(edmTypeMap, procedure.ReturnType, nullable: true);
            IEdmExpression expression = GetEdmEntitySetExpression(edmEntitySetMap, procedure);
            IEdmPathExpression pathExpression = procedure.EntitySetPath != null
                ? new EdmPathExpression(procedure.EntitySetPath)
                : null;

            EdmOperation operation;
            EdmOperationImport operationImport;
            if (isAction)
            {
                EdmAction action = new EdmAction(
                    container.Namespace, procedure.Name, returnReference, procedure.IsBindable, pathExpression);
                operation = action;
                operationImport = new EdmActionImport(container, procedure.Name, action, expression);
            }
            else
            {
                EdmFunction function = new EdmFunction(
                    container.Namespace,
                    procedure.Name,
                    returnReference,
                    procedure.IsBindable,
                    pathExpression,
                    procedure.IsComposable);
                operation = function;
                operationImport = new EdmFunctionImport(
                    container, procedure.Name, function, expression, includeInServiceDocument: true);
            }

            AddProcedureParameters(operation, procedure, edmTypeMap);
            if (procedure.IsBindable)
            {
                model.SetIsAlwaysBindable(operation, procedure.IsAlwaysBindable);
                AddProcedureLinkBuilder(model, operation, procedure);
                ValidateProcedureEntitySetPath(model, operationImport, procedure);
            }

            EdmModel edmModel = model as EdmModel;
            if (edmModel != null)
            {
                edmModel.AddElement(operation);
            }
            container.AddElement(operationImport);
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

        internal static IEnumerable<IEdmAction> GetAvailableActions(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableProcedures(entityType).OfType<IEdmAction>();
        }

        internal static IEnumerable<IEdmFunction> GetAvailableFunctions(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableProcedures(entityType).OfType<IEdmFunction>();
        }

        internal static IEnumerable<IEdmOperation> GetAvailableProcedures(this IEdmModel model, IEdmEntityType entityType)
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
