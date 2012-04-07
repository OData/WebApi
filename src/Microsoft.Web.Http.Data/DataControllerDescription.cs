// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Web.Http.Data.Metadata;

namespace Microsoft.Web.Http.Data
{
    public class DataControllerDescription
    {
        private static readonly ConcurrentDictionary<Type, DataControllerDescription> _descriptionMap = new ConcurrentDictionary<Type, DataControllerDescription>();
        private static ConcurrentDictionary<Type, HashSet<Type>> _typeDescriptionProviderMap = new ConcurrentDictionary<Type, HashSet<Type>>();

        private static readonly string[] _deletePrefixes = { "Delete", "Remove" };
        private static readonly string[] _insertPrefixes = { "Insert", "Add", "Create" };
        private static readonly string[] _updatePrefixes = { "Update", "Change", "Modify" };
        private Type _dataControllerType;
        private ReadOnlyCollection<Type> _entityTypes;
        private List<UpdateActionDescriptor> _updateActions;

        internal DataControllerDescription(Type dataControllerType, IEnumerable<Type> entityTypes, List<UpdateActionDescriptor> actions)
        {
            _dataControllerType = dataControllerType;
            _entityTypes = entityTypes.ToList().AsReadOnly();
            _updateActions = actions;
        }

        /// <summary>
        /// Gets the Type of the <see cref="DataController"/>
        /// </summary>
        public Type ControllerType
        {
            get { return _dataControllerType; }
        }

        /// <summary>
        /// Gets the entity types exposed by the <see cref="DataController"/>
        /// </summary>
        public IEnumerable<Type> EntityTypes
        {
            get { return _entityTypes; }
        }

        public static DataControllerDescription GetDescription(HttpControllerDescriptor controllerDescriptor)
        {
            return _descriptionMap.GetOrAdd(controllerDescriptor.ControllerType, type =>
            {
                return CreateDescription(controllerDescriptor);
            });
        }

        /// <summary>
        /// Creates and returns the metadata provider for the specified DataController Type.
        /// </summary>
        /// <param name="dataControllerType">The DataController Type.</param>
        /// <returns>The metadata provider.</returns>
        internal static MetadataProvider CreateMetadataProvider(Type dataControllerType)
        {
            // construct a list of all types in the inheritance hierarchy for the controller
            List<Type> baseTypes = new List<Type>();
            Type currType = dataControllerType;
            while (currType != typeof(DataController))
            {
                baseTypes.Add(currType);
                currType = currType.BaseType;
            }

            // create our base reflection provider
            List<MetadataProvider> providerList = new List<MetadataProvider>();
            ReflectionMetadataProvider reflectionProvider = new ReflectionMetadataProvider();

            // Set the IsEntity function which consults the chain of providers.
            Func<Type, bool> isEntityTypeFunc = (t) => providerList.Any(p => p.LookUpIsEntityType(t));
            reflectionProvider.SetIsEntityTypeFunc(isEntityTypeFunc);

            // Now from most derived to base, create any declared metadata providers,
            // chaining the instances as we progress. Note that ordering from derived to
            // base is important - we want to ensure that any providers the user has placed on
            // their DataController directly come before any DAL providers.
            MetadataProvider currProvider = reflectionProvider;
            providerList.Add(currProvider);
            for (int i = 0; i < baseTypes.Count; i++)
            {
                currType = baseTypes[i];

                // Reflection rather than TD is used here so we only get explicit
                // Type attributes. TD inherits attributes by default, even if the
                // attributes aren't inheritable.
                foreach (MetadataProviderAttribute providerAttribute in
                    currType.GetCustomAttributes(typeof(MetadataProviderAttribute), false))
                {
                    currProvider = providerAttribute.CreateProvider(dataControllerType, currProvider);
                    currProvider.SetIsEntityTypeFunc(isEntityTypeFunc);
                    providerList.Add(currProvider);
                }
            }

            return currProvider;
        }

        private static DataControllerDescription CreateDescription(HttpControllerDescriptor controllerDescriptor)
        {
            Type dataControllerType = controllerDescriptor.ControllerType;
            MetadataProvider metadataProvider = CreateMetadataProvider(dataControllerType);

            // get all public candidate methods and create the operations
            HashSet<Type> entityTypes = new HashSet<Type>();
            List<UpdateActionDescriptor> actions = new List<UpdateActionDescriptor>();
            IEnumerable<MethodInfo> methodsToInspect =
                dataControllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => (p.DeclaringType != typeof(DataController) && (p.DeclaringType != typeof(object))) && !p.IsSpecialName);

            foreach (MethodInfo method in methodsToInspect)
            {
                if (method.GetCustomAttributes(typeof(NonActionAttribute), false).Length > 0)
                {
                    continue;
                }

                if (method.IsVirtual && method.GetBaseDefinition().DeclaringType == typeof(DataController))
                {
                    // don't want to infer overrides of DataController virtual methods as
                    // operations
                    continue;
                }

                // We need to ensure the buddy metadata provider is registered BEFORE we
                // attempt to do convention, since we rely on IsEntity which relies on
                // KeyAttributes being present (possibly from "buddy" classes)
                RegisterAssociatedMetadataProvider(method);

                ChangeOperation operationType = ClassifyUpdateOperation(method, metadataProvider);
                if (operationType != ChangeOperation.None)
                {
                    Type entityType = method.GetParameters()[0].ParameterType;
                    UpdateActionDescriptor actionDescriptor = new UpdateActionDescriptor(controllerDescriptor, method, entityType, operationType);
                    ValidateAction(actionDescriptor);
                    actions.Add(actionDescriptor);

                    // TODO : currently considering entity types w/o any query methods
                    // exposing them. Should we?
                    if (metadataProvider.IsEntityType(entityType))
                    {
                        AddEntityType(entityType, entityTypes, metadataProvider);
                    }
                }
                else
                {
                    // if the method is a "query" operation returning an entity,
                    // add to entity types
                    if (method.ReturnType != typeof(void))
                    {
                        Type returnType = TypeUtility.UnwrapTaskInnerType(method.ReturnType);
                        Type elementType = TypeUtility.GetElementType(returnType);
                        if (metadataProvider.IsEntityType(elementType))
                        {
                            AddEntityType(elementType, entityTypes, metadataProvider);
                        }
                    }
                }
            }

            return new DataControllerDescription(dataControllerType, entityTypes, actions);
        }

        /// <summary>
        /// Adds the specified entity type and any associated entity types recursively to the specified set.
        /// </summary>
        /// <param name="entityType">The entity Type to add.</param>
        /// <param name="entityTypes">The types set to accumulate in.</param>
        /// <param name="metadataProvider">The metadata provider.</param>
        private static void AddEntityType(Type entityType, HashSet<Type> entityTypes, MetadataProvider metadataProvider)
        {
            if (entityTypes.Contains(entityType))
            {
                // already added this type
                return;
            }

            entityTypes.Add(entityType);
            RegisterDataControllerTypeDescriptionProvider(entityType, metadataProvider);

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(entityType))
            {
                // for any "exposed" association members, recursively add the associated
                // entity type
                if (pd.Attributes[typeof(AssociationAttribute)] != null && TypeUtility.IsDataMember(pd))
                {
                    Type includedEntityType = TypeUtility.GetElementType(pd.PropertyType);
                    if (metadataProvider.IsEntityType(entityType))
                    {
                        AddEntityType(includedEntityType, entityTypes, metadataProvider);
                    }
                }
            }

            // Recursively add any derived entity types specified by [KnownType]
            // attributes
            IEnumerable<Type> knownTypes = TypeUtility.GetKnownTypes(entityType, true);
            foreach (Type knownType in knownTypes)
            {
                if (entityType.IsAssignableFrom(knownType))
                {
                    AddEntityType(knownType, entityTypes, metadataProvider);
                }
            }
        }

        private static void ValidateAction(UpdateActionDescriptor updateAction)
        {
            // Only authorization filters are supported on CUD actions. This will capture 99% of user errors.
            // There is the chance that someone might attempt to implement an attribute that implements both
            // IAuthorizationFilter AND another filter type, but we don't want to have a black-list of filter
            // types here.
            if (updateAction.GetFilters().Any(p => !typeof(AuthorizationFilterAttribute).IsAssignableFrom(p.GetType())))
            {
                throw Error.NotSupported(Resource.InvalidAction_UnsupportedFilterType, updateAction.ControllerDescriptor.ControllerType.Name, updateAction.ActionName);
            }
        }

        private static ChangeOperation ClassifyUpdateOperation(MethodInfo method, MetadataProvider metadataProvider)
        {
            ChangeOperation operationType;

            AttributeCollection methodAttributes = new AttributeCollection(method.GetCustomAttributes(false).Cast<Attribute>().ToArray());

            // Check if explicit attributes exist.
            if (methodAttributes[typeof(InsertAttribute)] != null)
            {
                operationType = ChangeOperation.Insert;
            }
            else if (methodAttributes[typeof(UpdateAttribute)] != null)
            {
                UpdateAttribute updateAttribute = (UpdateAttribute)methodAttributes[typeof(UpdateAttribute)];
                if (updateAttribute.UsingCustomMethod)
                {
                    operationType = ChangeOperation.Custom;
                }
                else
                {
                    operationType = ChangeOperation.Update;
                }
            }
            else if (methodAttributes[typeof(DeleteAttribute)] != null)
            {
                operationType = ChangeOperation.Delete;
            }
            else
            {
                return TryClassifyUpdateOperationImplicit(method, metadataProvider);
            }

            return operationType;
        }

        private static ChangeOperation TryClassifyUpdateOperationImplicit(MethodInfo method, MetadataProvider metadataProvider)
        {
            ChangeOperation operationType = ChangeOperation.None;
            if (method.ReturnType == typeof(void))
            {
                // Check if this looks like an insert, update or delete method.
                if (_insertPrefixes.Any(p => method.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    operationType = ChangeOperation.Insert;
                }
                else if (_updatePrefixes.Any(p => method.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    operationType = ChangeOperation.Update;
                }
                else if (_deletePrefixes.Any(p => method.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    operationType = ChangeOperation.Delete;
                }
                else if (IsCustomUpdateMethod(method, metadataProvider))
                {
                    operationType = ChangeOperation.Custom;
                }
            }

            return operationType;
        }

        private static bool IsCustomUpdateMethod(MethodInfo method, MetadataProvider metadataProvider)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                return false;
            }
            if (method.ReturnType != typeof(void))
            {
                return false;
            }

            return metadataProvider.IsEntityType(parameters[0].ParameterType);
        }

        /// <summary>
        /// Register the associated metadata provider for Types in the signature
        /// of the specified method as required.
        /// </summary>
        /// <param name="methodInfo">The method to register for.</param>
        private static void RegisterAssociatedMetadataProvider(MethodInfo methodInfo)
        {
            Type type = TypeUtility.GetElementType(methodInfo.ReturnType);
            if (type != typeof(void) && type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).Length != 0)
            {
                RegisterAssociatedMetadataTypeTypeDescriptor(type);
            }
            foreach (ParameterInfo parameter in methodInfo.GetParameters())
            {
                type = parameter.ParameterType;
                if (type != typeof(void) && type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).Length != 0)
                {
                    RegisterAssociatedMetadataTypeTypeDescriptor(type);
                }
            }
        }

        /// <summary>
        /// Verifies that the <see cref="MetadataTypeAttribute"/> reference does not contain a cyclic reference and 
        /// registers the AssociatedMetadataTypeTypeDescriptionProvider in that case.
        /// </summary>
        /// <param name="type">The entity type with the MetadataType attribute.</param>
        private static void RegisterAssociatedMetadataTypeTypeDescriptor(Type type)
        {
            Type currentType = type;
            HashSet<Type> metadataTypeReferences = new HashSet<Type>();
            metadataTypeReferences.Add(currentType);
            while (true)
            {
                MetadataTypeAttribute attribute = (MetadataTypeAttribute)Attribute.GetCustomAttribute(currentType, typeof(MetadataTypeAttribute));
                if (attribute == null)
                {
                    break;
                }
                else
                {
                    currentType = attribute.MetadataClassType;
                    // If we find a cyclic reference, throw an error. 
                    if (metadataTypeReferences.Contains(currentType))
                    {
                        throw Error.InvalidOperation(Resource.CyclicMetadataTypeAttributesFound, type.FullName);
                    }
                    else
                    {
                        metadataTypeReferences.Add(currentType);
                    }
                }
            }

            // If the MetadataType reference chain doesn't contain a cycle, register the use of the AssociatedMetadataTypeTypeDescriptionProvider.
            RegisterCustomTypeDescriptor(new AssociatedMetadataTypeTypeDescriptionProvider(type), type);
        }

        // The JITer enforces CAS. By creating a separate method we can avoid getting SecurityExceptions 
        // when we weren't going to really call TypeDescriptor.AddProvider.
        internal static void RegisterCustomTypeDescriptor(TypeDescriptionProvider tdp, Type type)
        {
            // Check if we already registered provider with the specified type.
            HashSet<Type> existingProviders = _typeDescriptionProviderMap.GetOrAdd(type, t =>
            {
                return new HashSet<Type>();
            });

            if (!existingProviders.Contains(tdp.GetType()))
            {
                TypeDescriptor.AddProviderTransparent(tdp, type);
                existingProviders.Add(tdp.GetType());
            }
        }

        /// <summary>
        /// Register our DataControllerTypeDescriptionProvider for the specified Type. This provider is responsible for surfacing the
        /// custom TDs returned by metadata providers.
        /// </summary>
        /// <param name="type">The Type that we should register for.</param>
        /// <param name="metadataProvider">The metadata provider.</param>
        private static void RegisterDataControllerTypeDescriptionProvider(Type type, MetadataProvider metadataProvider)
        {
            DataControllerTypeDescriptionProvider tdp = new DataControllerTypeDescriptionProvider(type, metadataProvider);
            RegisterCustomTypeDescriptor(tdp, type);
        }

        public UpdateActionDescriptor GetUpdateAction(string name)
        {
            return _updateActions.FirstOrDefault(p => p.ActionName == name);
        }

        public UpdateActionDescriptor GetUpdateAction(Type entityType, ChangeOperation operationType)
        {
            return _updateActions.FirstOrDefault(p => (p.EntityType == entityType) && (p.ChangeOperation == operationType));
        }

        public UpdateActionDescriptor GetCustomMethod(Type entityType, string methodName)
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }
            if (methodName == null)
            {
                throw Error.ArgumentNull("methodName");
            }

            return _updateActions.FirstOrDefault(p => (p.EntityType == entityType) && (p.ChangeOperation == ChangeOperation.Custom) && (p.ActionName == methodName));
        }

        /// <summary>
        /// This is the default provider in the metadata provider chain. It is based solely on
        /// attributes applied directly to types (either via CLR attributes, or via "buddy" metadata class).
        /// </summary>
        private class ReflectionMetadataProvider : MetadataProvider
        {
            public ReflectionMetadataProvider()
                : base(parent: null)
            {
            }

            /// <summary>
            /// Returns true if the Type has at least one member marked with KeyAttribute.
            /// </summary>
            /// <param name="type">The Type to check.</param>
            /// <returns>True if the Type is an entity, false otherwise.</returns>
            public override bool LookUpIsEntityType(Type type)
            {
                return TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>().Any(p => p.Attributes[typeof(KeyAttribute)] != null);
            }
        }
    }
}
