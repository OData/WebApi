// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Builder
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
            IEnumerable<IEdmTypeConfiguration> configTypes = builder.StructuralTypes.Concat<IEdmTypeConfiguration>(builder.EnumTypes);
            EdmTypeMap edmMap = EdmTypeBuilder.GetTypesAndProperties(configTypes);
            Dictionary<Type, IEdmType> edmTypeMap = model.AddTypes(edmMap);

            // Add EntitySets and build the mapping between the EdmEntitySet and the NavigationSourceConfiguration
            NavigationSourceAndAnnotations[] entitySets = container.AddEntitySetAndAnnotations(builder, edmTypeMap);

            // Add Singletons and build the mapping between the EdmSingleton and the NavigationSourceConfiguration
            NavigationSourceAndAnnotations[] singletons = container.AddSingletonAndAnnotations(builder, edmTypeMap);

            // Merge EntitySets and Singletons together
            IEnumerable<NavigationSourceAndAnnotations> navigationSources = entitySets.Concat(singletons);

            // Build the navigation source map
            IDictionary<string, EdmNavigationSource> navigationSourceMap = model.GetNavigationSourceMap(edmMap, navigationSources);

            // Add the core vocabulary annotations
            model.AddCoreVocabularyAnnotations(entitySets, edmMap);

            // Add the capabilities vocabulary annotations
            model.AddCapabilitiesVocabularyAnnotations(entitySets, edmMap);

            // add operations
            model.AddOperations(builder.Operations, container, edmTypeMap, navigationSourceMap);

            // finish up
            model.AddElement(container);

            // build the map from IEdmEntityType to IEdmFunctionImport
            model.SetAnnotationValue<BindableOperationFinder>(model, new BindableOperationFinder(model));

            return model;
        }

        private static void AddTypes(this EdmModel model, Dictionary<Type, IEdmType> types)
        {
            Contract.Assert(model != null);
            Contract.Assert(types != null);

            foreach (IEdmType type in types.Values)
            {
                model.AddType(type);
            }
        }

        private static NavigationSourceAndAnnotations[] AddEntitySetAndAnnotations(this EdmEntityContainer container,
            ODataModelBuilder builder, Dictionary<Type, IEdmType> edmTypeMap)
        {
            IEnumerable<EntitySetConfiguration> configurations = builder.EntitySets;

            // build the entitysets
            IEnumerable<Tuple<EdmEntitySet, EntitySetConfiguration>> entitySets = AddEntitySets(configurations, container, edmTypeMap);

            // return the annotation array
            return entitySets.Select(e => new NavigationSourceAndAnnotations()
            {
                NavigationSource = e.Item1,
                Configuration = e.Item2,
                LinkBuilder = new NavigationSourceLinkBuilderAnnotation(e.Item2),
                Url = new NavigationSourceUrlAnnotation { Url = e.Item2.GetUrl() }
            }).ToArray();
        }

        private static NavigationSourceAndAnnotations[] AddSingletonAndAnnotations(this EdmEntityContainer container,
            ODataModelBuilder builder, Dictionary<Type, IEdmType> edmTypeMap)
        {
            IEnumerable<SingletonConfiguration> configurations = builder.Singletons;

            // build the singletons
            IEnumerable<Tuple<EdmSingleton, SingletonConfiguration>> singletons = AddSingletons(configurations, container, edmTypeMap);

            // return the annotation array
            return singletons.Select(e => new NavigationSourceAndAnnotations()
            {
                NavigationSource = e.Item1,
                Configuration = e.Item2,
                LinkBuilder = new NavigationSourceLinkBuilderAnnotation(e.Item2),
                Url = new NavigationSourceUrlAnnotation { Url = e.Item2.GetUrl() }
            }).ToArray();
        }

        private static IDictionary<string, EdmNavigationSource> GetNavigationSourceMap(this EdmModel model, EdmTypeMap edmMap,
            IEnumerable<NavigationSourceAndAnnotations> navigationSourceAndAnnotations)
        {
            // index the navigation source by name
            Dictionary<string, EdmNavigationSource> edmNavigationSourceMap = navigationSourceAndAnnotations.ToDictionary(e => e.NavigationSource.Name, e => e.NavigationSource);

            // apply the annotations
            foreach (NavigationSourceAndAnnotations navigationSourceAndAnnotation in navigationSourceAndAnnotations)
            {
                EdmNavigationSource navigationSource = navigationSourceAndAnnotation.NavigationSource;
                model.SetAnnotationValue(navigationSource, navigationSourceAndAnnotation.Url);
                model.SetNavigationSourceLinkBuilder(navigationSource, navigationSourceAndAnnotation.LinkBuilder);

                AddNavigationBindings(edmMap, navigationSourceAndAnnotation.Configuration, navigationSource, navigationSourceAndAnnotation.LinkBuilder,
                    edmNavigationSourceMap);
            }

            return edmNavigationSourceMap;
        }

        private static void AddNavigationBindings(EdmTypeMap edmMap,
            NavigationSourceConfiguration navigationSourceConfiguration,
            EdmNavigationSource navigationSource,
            NavigationSourceLinkBuilderAnnotation linkBuilder,
            Dictionary<string, EdmNavigationSource> edmNavigationSourceMap)
        {
            foreach (var binding in navigationSourceConfiguration.Bindings)
            {
                NavigationPropertyConfiguration navigationProperty = binding.NavigationProperty;
                bool isContained = navigationProperty.ContainsTarget;

                IEdmType edmType = edmMap.EdmTypes[navigationProperty.DeclaringType.ClrType];
                IEdmStructuredType structuraType = edmType as IEdmStructuredType;
                IEdmNavigationProperty edmNavigationProperty = structuraType.NavigationProperties()
                    .Single(np => np.Name == navigationProperty.Name);

                string bindingPath = ConvertBindingPath(edmMap, binding);
                if (!isContained)
                {
                    // calculate the binding path
                    navigationSource.AddNavigationTarget(
                        edmNavigationProperty,
                        edmNavigationSourceMap[binding.TargetNavigationSource.Name],
                        new EdmPathExpression(bindingPath));
                }

                NavigationLinkBuilder linkBuilderFunc = navigationSourceConfiguration.GetNavigationPropertyLink(navigationProperty);
                if (linkBuilderFunc != null)
                {
                    linkBuilder.AddNavigationPropertyLinkBuilder(edmNavigationProperty, linkBuilderFunc);
                }
            }
        }

        private static string ConvertBindingPath(EdmTypeMap edmMap, NavigationPropertyBindingConfiguration binding)
        {
            IList<string> bindings = new List<string>();
            foreach (var bindingInfo in binding.Path)
            {
                Type typeCast = bindingInfo as Type;
                PropertyInfo propertyInfo = bindingInfo as PropertyInfo;

                if (typeCast != null)
                {
                    IEdmType edmType = edmMap.EdmTypes[typeCast];
                    bindings.Add(edmType.FullTypeName());
                }
                else if (propertyInfo != null)
                {
                    bindings.Add(edmMap.EdmProperties[propertyInfo].Name);
                }
            }

            return String.Join("/", bindings);
        }

        private static void AddOperationParameters(EdmOperation operation, OperationConfiguration operationConfiguration, Dictionary<Type, IEdmType> edmTypeMap)
        {
            foreach (ParameterConfiguration parameter in operationConfiguration.Parameters)
            {
                bool isParameterOptional = parameter.OptionalParameter;
                IEdmTypeReference parameterTypeReference = GetEdmTypeReference(edmTypeMap, parameter.TypeConfiguration, nullable: isParameterOptional);
                IEdmOperationParameter operationParameter = new EdmOperationParameter(operation, parameter.Name, parameterTypeReference);
                operation.AddParameter(operationParameter);
            }
        }

        private static void AddOperationLinkBuilder(IEdmModel model, IEdmOperation operation, OperationConfiguration operationConfiguration)
        {
            ActionConfiguration actionConfiguration = operationConfiguration as ActionConfiguration;
            IEdmAction action = operation as IEdmAction;
            FunctionConfiguration functionConfiguration = operationConfiguration as FunctionConfiguration;
            IEdmFunction function = operation as IEdmFunction;
            if (operationConfiguration.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Entity)
            {
                if (actionConfiguration != null && actionConfiguration.GetActionLink() != null && action != null)
                {
                    model.SetOperationLinkBuilder(
                        action,
                        new OperationLinkBuilder(actionConfiguration.GetActionLink(), actionConfiguration.FollowsConventions));
                }
                else if (functionConfiguration != null && functionConfiguration.GetFunctionLink() != null && function != null)
                {
                    model.SetOperationLinkBuilder(
                        function,
                        new OperationLinkBuilder(functionConfiguration.GetFunctionLink(), functionConfiguration.FollowsConventions));
                }
            }
            else if (operationConfiguration.BindingParameter.TypeConfiguration.Kind == EdmTypeKind.Collection)
            {
                CollectionTypeConfiguration collectionTypeConfiguration =
                    (CollectionTypeConfiguration)operationConfiguration.BindingParameter.TypeConfiguration;

                if (collectionTypeConfiguration.ElementType.Kind == EdmTypeKind.Entity)
                {
                    if (actionConfiguration != null && actionConfiguration.GetFeedActionLink() != null && action != null)
                    {
                        model.SetOperationLinkBuilder(
                            action,
                            new OperationLinkBuilder(actionConfiguration.GetFeedActionLink(), actionConfiguration.FollowsConventions));
                    }
                    else if (functionConfiguration != null && functionConfiguration.GetFeedFunctionLink() != null && function != null)
                    {
                        model.SetOperationLinkBuilder(
                            function,
                            new OperationLinkBuilder(functionConfiguration.GetFeedFunctionLink(), functionConfiguration.FollowsConventions));
                    }
                }
            }
        }

        private static void ValidateOperationEntitySetPath(IEdmModel model, IEdmOperationImport operationImport, OperationConfiguration operationConfiguration)
        {
            IEdmOperationParameter operationParameter;
            Dictionary<IEdmNavigationProperty, IEdmPathExpression> relativeNavigations;
            IEnumerable<EdmError> edmErrors;
            if (operationConfiguration.EntitySetPath != null && !operationImport.TryGetRelativeEntitySetPath(model, out operationParameter, out relativeNavigations, out edmErrors))
            {
                //throw Error.InvalidOperation(SRResources.OperationHasInvalidEntitySetPath, String.Join("/", operationConfiguration.EntitySetPath), operationConfiguration.FullyQualifiedName);
                throw Error.InvalidOperation("TODO: ");
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", 
            Justification = "The majority of types referenced by this method are EdmLib types this method needs to know about to operate correctly")]
        private static void AddOperations(this EdmModel model, IEnumerable<OperationConfiguration> configurations, EdmEntityContainer container,
            Dictionary<Type, IEdmType> edmTypeMap, IDictionary<string, EdmNavigationSource> edmNavigationSourceMap)
        {
            Contract.Assert(model != null, "Model can't be null");

            ValidateActionOverload(configurations.OfType<ActionConfiguration>());

            foreach (OperationConfiguration operationConfiguration in configurations)
            {
                IEdmTypeReference returnReference = GetEdmTypeReference(edmTypeMap,
                    operationConfiguration.ReturnType,
                    operationConfiguration.ReturnType != null && operationConfiguration.OptionalReturn);
                IEdmExpression expression = GetEdmEntitySetExpression(edmNavigationSourceMap, operationConfiguration);
                IEdmPathExpression pathExpression = operationConfiguration.EntitySetPath != null
                    ? new EdmPathExpression(operationConfiguration.EntitySetPath)
                    : null;

                EdmOperationImport operationImport;

                switch (operationConfiguration.Kind)
                {
                    case OperationKind.Action:
                        operationImport = CreateActionImport(operationConfiguration, container, returnReference, expression, pathExpression);
                        break;
                    case OperationKind.Function:
                        operationImport = CreateFunctionImport((FunctionConfiguration)operationConfiguration, container, returnReference, expression, pathExpression);
                        break;
                    case OperationKind.ServiceOperation:
                        Contract.Assert(false, "ServiceOperations are not supported.");
                        goto default;
                    default:
                        Contract.Assert(false, "Unsupported OperationKind");
                        return;
                }

                EdmOperation operation = (EdmOperation)operationImport.Operation;
                if (operationConfiguration.IsBindable && operationConfiguration.Title != null && operationConfiguration.Title != operationConfiguration.Name)
                {
                    model.SetOperationTitleAnnotation(operation, new OperationTitleAnnotation(operationConfiguration.Title));
                }

                if (operationConfiguration.IsBindable &&
                    operationConfiguration.NavigationSource != null &&
                    edmNavigationSourceMap.ContainsKey(operationConfiguration.NavigationSource.Name))
                {
                    model.SetAnnotationValue(operation, new ReturnedEntitySetAnnotation(operationConfiguration.NavigationSource.Name));
                }

                AddOperationParameters(operation, operationConfiguration, edmTypeMap);

                if (operationConfiguration.IsBindable)
                {
                    AddOperationLinkBuilder(model, operation, operationConfiguration);
                    ValidateOperationEntitySetPath(model, operationImport, operationConfiguration);
                }
                else
                {
                    container.AddElement(operationImport);
                }

                model.AddElement(operation);
            }
        }

        private static EdmOperationImport CreateActionImport(
            OperationConfiguration operationConfiguration,
            EdmEntityContainer container,
            IEdmTypeReference returnReference,
            IEdmExpression expression,
            IEdmPathExpression pathExpression)
        {
            EdmAction operation = new EdmAction(
                operationConfiguration.Namespace,
                operationConfiguration.Name,
                returnReference,
                operationConfiguration.IsBindable,
                pathExpression);
            return new EdmActionImport(container, operationConfiguration.Name, operation, expression);
        }

        private static EdmOperationImport CreateFunctionImport(
            FunctionConfiguration function,
            EdmEntityContainer container,
            IEdmTypeReference returnReference,
            IEdmExpression expression,
            IEdmPathExpression pathExpression)
        {
            EdmFunction operation = new EdmFunction(
                    function.Namespace,
                    function.Name,
                    returnReference,
                    function.IsBindable,
                    pathExpression,
                    function.IsComposable);
            return new EdmFunctionImport(container, function.Name, operation, expression, includeInServiceDocument: function.IncludeInServiceDocument);
        }

        // 11.5.4.2  Action Overload Resolution
        // The same action name may be used multiple times within a schema provided there is at most one unbound overload,
        // and each bound overload specifies a different binding parameter type. If the action is bound and the binding 
        // parameter type is part of an inheritance hierarchy, the action overload is selected based on the type of the
        // URL segment preceding the action name. A type-cast segment can be used to select an action defined on a
        // particular type in the hierarchy.
        private static void ValidateActionOverload(IEnumerable<ActionConfiguration> configurations)
        {
            // 1. validate at most one unbound overload
            ActionConfiguration[] unboundActions = configurations.Where(a => !a.IsBindable).ToArray();
            if (unboundActions.Length > 0)
            {
                HashSet<string> unboundActionNames = new HashSet<string>();
                foreach (ActionConfiguration action in unboundActions)
                {
                    if (!unboundActionNames.Contains(action.Name))
                    {
                        unboundActionNames.Add(action.Name);
                    }
                    else
                    {
                        throw Error.InvalidOperation(SRResources.MoreThanOneUnboundActionFound, action.Name);
                    }
                }
            }

            // 2. validate each bound overload action specifies a different binding parameter type
            ActionConfiguration[] boundActions = configurations.Where(a => a.IsBindable).ToArray();
            if (boundActions.Length > 0)
            {
                var actionNamesToBindingTypes = new Dictionary<string, IList<IEdmTypeConfiguration>>();
                foreach (ActionConfiguration action in boundActions)
                {
                    IEdmTypeConfiguration newBindingType = action.BindingParameter.TypeConfiguration;
                    if (actionNamesToBindingTypes.ContainsKey(action.Name))
                    {
                        IList<IEdmTypeConfiguration> bindingTypes = actionNamesToBindingTypes[action.Name];
                        foreach (IEdmTypeConfiguration type in bindingTypes)
                        {
                            if (type == newBindingType)
                            {
                                throw Error.InvalidOperation(SRResources.MoreThanOneOverloadActionBoundToSameTypeFound,
                                    action.Name, type.FullName);
                            }
                        }

                        bindingTypes.Add(newBindingType);
                    }
                    else
                    {
                        IList<IEdmTypeConfiguration> bindingTypes = new List<IEdmTypeConfiguration>();
                        bindingTypes.Add(newBindingType);
                        actionNamesToBindingTypes.Add(action.Name, bindingTypes);
                    }
                }
            }
        }

        private static Dictionary<Type, IEdmType> AddTypes(this EdmModel model, EdmTypeMap edmTypeMap)
        {
            // build types
            Dictionary<Type, IEdmType> edmTypes = edmTypeMap.EdmTypes;

            // Add an annotate types
            model.AddTypes(edmTypes);
            model.AddClrTypeAnnotations(edmTypes);

            // add annotation for properties
            Dictionary<PropertyInfo, IEdmProperty> edmProperties = edmTypeMap.EdmProperties;
            model.AddClrPropertyInfoAnnotations(edmProperties);
            model.AddPropertyRestrictionsAnnotations(edmTypeMap.EdmPropertiesRestrictions);

            // add dynamic dictionary property annotation for open types
            model.AddDynamicPropertyDictionaryAnnotations(edmTypeMap.OpenTypes);

            return edmTypes;
        }

        private static void AddType(this EdmModel model, IEdmType type)
        {
            if (type.TypeKind == EdmTypeKind.Complex)
            {
                model.AddElement(type as IEdmComplexType);
            }
            else if (type.TypeKind == EdmTypeKind.Entity)
            {
                model.AddElement(type as IEdmEntityType);
            }
            else if (type.TypeKind == EdmTypeKind.Enum)
            {
                model.AddElement(type as IEdmEnumType);
            }
            else
            {
                Contract.Assert(false, "Only ComplexTypes, EntityTypes and EnumTypes are supported.");
            }
        }

        private static EdmEntitySet AddEntitySet(this EdmEntityContainer container, EntitySetConfiguration entitySet, IDictionary<Type, IEdmType> edmTypeMap)
        {
            return container.AddEntitySet(entitySet.Name, (IEdmEntityType)edmTypeMap[entitySet.EntityType.ClrType]);
        }

        private static IEnumerable<Tuple<EdmEntitySet, EntitySetConfiguration>> AddEntitySets(IEnumerable<EntitySetConfiguration> entitySets, EdmEntityContainer container, Dictionary<Type, IEdmType> edmTypeMap)
        {
            return entitySets.Select(es => Tuple.Create(container.AddEntitySet(es, edmTypeMap), es));
        }

        private static EdmSingleton AddSingleton(this EdmEntityContainer container, SingletonConfiguration singletonType, IDictionary<Type, IEdmType> edmTypeMap)
        {
            return container.AddSingleton(singletonType.Name, (IEdmEntityType)edmTypeMap[singletonType.EntityType.ClrType]);
        }

        private static IEnumerable<Tuple<EdmSingleton, SingletonConfiguration>> AddSingletons(IEnumerable<SingletonConfiguration> singletons, EdmEntityContainer container, Dictionary<Type, IEdmType> edmTypeMap)
        {
            return singletons.Select(sg => Tuple.Create(container.AddSingleton(sg, edmTypeMap), sg));
        }

        private static void AddClrTypeAnnotations(this EdmModel model, Dictionary<Type, IEdmType> edmTypes)
        {
            foreach (KeyValuePair<Type, IEdmType> map in edmTypes)
            {
                // pre-populate the model with clr-type annotations so that we dont have to scan 
                // all loaded assemblies to find the clr type for an edm type that we build.
                IEdmType edmType = map.Value;
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

        private static void AddDynamicPropertyDictionaryAnnotations(this EdmModel model,
            Dictionary<IEdmStructuredType, PropertyInfo> openTypes)
        {
            foreach (KeyValuePair<IEdmStructuredType, PropertyInfo> openType in openTypes)
            {
                IEdmStructuredType edmStructuredType = openType.Key;
                PropertyInfo propertyInfo = openType.Value;
                model.SetAnnotationValue(edmStructuredType, new DynamicPropertyDictionaryAnnotation(propertyInfo));
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

        private static void AddCoreVocabularyAnnotations(this EdmModel model, NavigationSourceAndAnnotations[] entitySets, EdmTypeMap edmTypeMap)
        {
            Contract.Assert(model != null);
            Contract.Assert(edmTypeMap != null);

            if (entitySets == null)
            {
                return;
            }

            foreach (NavigationSourceAndAnnotations source in entitySets)
            {
                IEdmEntitySet entitySet = source.NavigationSource as IEdmEntitySet;
                if (entitySet == null)
                {
                    continue;
                }

                EntitySetConfiguration entitySetConfig = source.Configuration as EntitySetConfiguration;
                if (entitySetConfig == null)
                {
                    continue;
                }

                model.AddOptimisticConcurrencyAnnotation(entitySet, entitySetConfig, edmTypeMap);
            }
        }

        private static void AddOptimisticConcurrencyAnnotation(this EdmModel model, IEdmEntitySet target,
            EntitySetConfiguration entitySetConfiguration, EdmTypeMap edmTypeMap)
        {
            EntityTypeConfiguration entityTypeConfig = entitySetConfiguration.EntityType;

            IEnumerable<StructuralPropertyConfiguration> concurrencyPropertyies =
                entityTypeConfig.Properties.OfType<StructuralPropertyConfiguration>().Where(property => property.ConcurrencyToken);

            IList<IEdmStructuralProperty> edmProperties = new List<IEdmStructuralProperty>();

            foreach (StructuralPropertyConfiguration property in concurrencyPropertyies)
            {
                IEdmProperty value;
                if (edmTypeMap.EdmProperties.TryGetValue(property.PropertyInfo, out value))
                {
                    var item = value as IEdmStructuralProperty;
                    if (item != null)
                    {
                        edmProperties.Add(item);
                    }
                }
            }

            if (edmProperties.Any())
            {
                model.SetOptimisticConcurrencyAnnotation(target, edmProperties);
            }
        }

        private static void AddCapabilitiesVocabularyAnnotations(this EdmModel model, NavigationSourceAndAnnotations[] entitySets, EdmTypeMap edmTypeMap)
        {
            Contract.Assert(model != null);
            Contract.Assert(edmTypeMap != null);

            if (entitySets == null)
            {
                return;
            }

            foreach (NavigationSourceAndAnnotations source in entitySets)
            {
                IEdmEntitySet entitySet = source.NavigationSource as IEdmEntitySet;
                if (entitySet == null)
                {
                    continue;
                }

                EntitySetConfiguration entitySetConfig = source.Configuration as EntitySetConfiguration;
                if (entitySetConfig == null)
                {
                    continue;
                }

                model.AddCountRestrictionsAnnotation(entitySet, entitySetConfig, edmTypeMap);
                model.AddNavigationRestrictionsAnnotation(entitySet, entitySetConfig, edmTypeMap);
                model.AddFilterRestrictionsAnnotation(entitySet, entitySetConfig, edmTypeMap);
                model.AddSortRestrictionsAnnotation(entitySet, entitySetConfig, edmTypeMap);
                model.AddExpandRestrictionsAnnotation(entitySet, entitySetConfig, edmTypeMap);
            }
        }

        private static void AddCountRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target,
            EntitySetConfiguration entitySetConfiguration, EdmTypeMap edmTypeMap)
        {
            EntityTypeConfiguration entityTypeConfig = entitySetConfiguration.EntityType;

            IEnumerable<PropertyConfiguration> notCountableProperties = entityTypeConfig.Properties.Where(property => property.NotCountable);

            IList<IEdmProperty> nonCountableProperties = new List<IEdmProperty>();
            IList<IEdmNavigationProperty> nonCountableNavigationProperties = new List<IEdmNavigationProperty>();
            foreach (PropertyConfiguration property in notCountableProperties)
            {
                IEdmProperty value;
                if (edmTypeMap.EdmProperties.TryGetValue(property.PropertyInfo, out value))
                {
                    if (value != null && value.Type.TypeKind() == EdmTypeKind.Collection)
                    {
                        if (value.PropertyKind == EdmPropertyKind.Navigation)
                        {
                            nonCountableNavigationProperties.Add((IEdmNavigationProperty)value);
                        }
                        else
                        {
                            nonCountableProperties.Add(value);
                        }
                    }
                }
            }

            if (nonCountableProperties.Any() || nonCountableNavigationProperties.Any())
            {
                model.SetCountRestrictionsAnnotation(target, true, nonCountableProperties, nonCountableNavigationProperties);
            }
        }

        private static void AddNavigationRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target,
            EntitySetConfiguration entitySetConfiguration, EdmTypeMap edmTypeMap)
        {
            EntityTypeConfiguration entityTypeConfig = entitySetConfiguration.EntityType;

            IEnumerable<PropertyConfiguration> notNavigableProperties = entityTypeConfig.Properties.Where(property => property.NotNavigable);

            IList<Tuple<IEdmNavigationProperty, CapabilitiesNavigationType>> properties =
                new List<Tuple<IEdmNavigationProperty, CapabilitiesNavigationType>>();
            foreach (PropertyConfiguration property in notNavigableProperties)
            {
                IEdmProperty value;
                if (edmTypeMap.EdmProperties.TryGetValue(property.PropertyInfo, out value))
                {
                    if (value != null && value.PropertyKind == EdmPropertyKind.Navigation)
                    {
                        properties.Add(new Tuple<IEdmNavigationProperty, CapabilitiesNavigationType>(
                            (IEdmNavigationProperty)value, CapabilitiesNavigationType.Recursive));
                    }
                }
            }

            if (properties.Any())
            {
                model.SetNavigationRestrictionsAnnotation(target, CapabilitiesNavigationType.Recursive, properties);
            }
        }

        private static void AddFilterRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target,
            EntitySetConfiguration entitySetConfiguration, EdmTypeMap edmTypeMap)
        {
            EntityTypeConfiguration entityTypeConfig = entitySetConfiguration.EntityType;

            IEnumerable<PropertyConfiguration> notFilterProperties = entityTypeConfig.Properties.Where(property => property.NonFilterable);

            IList<IEdmProperty> properties = new List<IEdmProperty>();
            foreach (PropertyConfiguration property in notFilterProperties)
            {
                IEdmProperty value;
                if (edmTypeMap.EdmProperties.TryGetValue(property.PropertyInfo, out value))
                {
                    if (value != null)
                    {
                        properties.Add(value);
                    }
                }
            }

            if (properties.Any())
            {
                model.SetFilterRestrictionsAnnotation(target, true, true, null, properties);
            }
        }

        private static void AddSortRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target,
            EntitySetConfiguration entitySetConfiguration, EdmTypeMap edmTypeMap)
        {
            EntityTypeConfiguration entityTypeConfig = entitySetConfiguration.EntityType;
            IEnumerable<PropertyConfiguration> nonSortableProperties = entityTypeConfig.Properties.Where(property => property.Unsortable);
            IList<IEdmProperty> properties = new List<IEdmProperty>();
            foreach (PropertyConfiguration property in nonSortableProperties)
            {
                IEdmProperty value;
                if (edmTypeMap.EdmProperties.TryGetValue(property.PropertyInfo, out value))
                {
                    if (value != null)
                    {
                        properties.Add(value);
                    }
                }
            }

            if (properties.Any())
            {
                model.SetSortRestrictionsAnnotation(target, true, null, null, properties);
            }
        }

        private static void AddExpandRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target,
            EntitySetConfiguration entitySetConfiguration, EdmTypeMap edmTypeMap)
        {
            EntityTypeConfiguration entityTypeConfig = entitySetConfiguration.EntityType;
            IEnumerable<PropertyConfiguration> nonExpandableProperties = entityTypeConfig.Properties.Where(property => property.NotExpandable);     
            IList<IEdmNavigationProperty> properties = new List<IEdmNavigationProperty>();
            foreach (PropertyConfiguration property in nonExpandableProperties)
            {
                IEdmProperty value;
                if (edmTypeMap.EdmProperties.TryGetValue(property.PropertyInfo, out value))
                {
                    if (value != null && value.PropertyKind == EdmPropertyKind.Navigation)
                    {
                        properties.Add((IEdmNavigationProperty)value);
                    }
                }
            }

            if (properties.Any())
            {
                model.SetExpandRestrictionsAnnotation(target, true, properties);
            }
        }

        private static IEdmExpression GetEdmEntitySetExpression(IDictionary<string, EdmNavigationSource> navigationSources, OperationConfiguration operationConfiguration)
        {
            if (operationConfiguration.NavigationSource != null)
            {
                EdmNavigationSource navigationSource;
                if (navigationSources.TryGetValue(operationConfiguration.NavigationSource.Name, out navigationSource))
                {
                    EdmEntitySet entitySet = navigationSource as EdmEntitySet;
                    if (entitySet != null)
                    {
                        return new EdmPathExpression(entitySet.Name);
                    }
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.EntitySetNotFoundForName, operationConfiguration.NavigationSource.Name);
                }
            }
            else if (operationConfiguration.EntitySetPath != null)
            {
                return new EdmPathExpression(operationConfiguration.EntitySetPath);
            }

            return null;
        }

        private static IEdmTypeReference GetEdmTypeReference(Dictionary<Type, IEdmType> availableTypes, IEdmTypeConfiguration configuration, bool nullable)
        {
            Contract.Assert(availableTypes != null);

            if (configuration == null)
            {
                return null;
            }

            EdmTypeKind kind = configuration.Kind;
            if (kind == EdmTypeKind.Collection)
            {
                CollectionTypeConfiguration collectionType = (CollectionTypeConfiguration)configuration;
                EdmCollectionType edmCollectionType =
                    new EdmCollectionType(GetEdmTypeReference(availableTypes, collectionType.ElementType, nullable));
                return new EdmCollectionTypeReference(edmCollectionType);
            }
            else
            {
                Type configurationClrType = TypeHelper.GetUnderlyingTypeOrSelf(configuration.ClrType);

                if (!configurationClrType.GetTypeInfo().IsEnum)
                {
                    configurationClrType = configuration.ClrType;
                }

                IEdmType type;

                if (availableTypes.TryGetValue(configurationClrType, out type))
                {
                    if (kind == EdmTypeKind.Complex)
                    {
                        return new EdmComplexTypeReference((IEdmComplexType)type, nullable);
                    }
                    else if (kind == EdmTypeKind.Entity)
                    {
                        return new EdmEntityTypeReference((IEdmEntityType)type, nullable);
                    }
                    else if (kind == EdmTypeKind.Enum)
                    {
                        return new EdmEnumTypeReference((IEdmEnumType)type, nullable);
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

        internal static string GetNavigationSourceUrl(this IEdmModel model, IEdmNavigationSource navigationSource)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (navigationSource == null)
            {
                throw Error.ArgumentNull("navigationSource");
            }

            NavigationSourceUrlAnnotation annotation = model.GetAnnotationValue<NavigationSourceUrlAnnotation>(navigationSource);
            if (annotation == null)
            {
                return navigationSource.Name;
            }
            else
            {
                return annotation.Url;
            }
        }

        internal static IEnumerable<IEdmAction> GetAvailableActions(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, false).OfType<IEdmAction>();
        }

        internal static IEnumerable<IEdmFunction> GetAvailableFunctions(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, false).OfType<IEdmFunction>();
        }

        internal static IEnumerable<IEdmOperation> GetAvailableOperationsBoundToCollection(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, true);
        }

        internal static IEnumerable<IEdmOperation> GetAvailableOperations(this IEdmModel model, IEdmEntityType entityType, bool boundToCollection = false)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            BindableOperationFinder annotation = model.GetAnnotationValue<BindableOperationFinder>(model);
            if (annotation == null)
            {
                annotation = new BindableOperationFinder(model);
                model.SetAnnotationValue(model, annotation);
            }

            if (boundToCollection)
            {
                return annotation.FindOperationsBoundToCollection(entityType);
            }
            else
            {
                return annotation.FindOperations(entityType);
            }
        }
    }
}
