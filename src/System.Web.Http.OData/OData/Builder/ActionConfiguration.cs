// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// ActionConfiguration represents an OData action that you wish to expose via your service
    /// <remarks>
    /// ActionConfigurations are exposed via $metadata as a <FunctionImport/> element.
    /// </remarks> 
    /// </summary>
    public class ActionConfiguration : ProcedureConfiguration
    {
        private List<ParameterConfiguration> _parameters = new List<ParameterConfiguration>();
        private BindingParameterConfiguration _bindingParameter;
        private Func<EntityInstanceContext, Uri> _actionLinkFactory;
        private bool _followsConventions;

        /// <summary>
        /// Create a new ActionConfiguration
        /// </summary>
        /// <param name="builder">The ODataModelBuilder to which this ActionConfiguration should be added.</param>
        /// <param name="name">The name of this ActionConfiguration.</param>
        internal ActionConfiguration(ODataModelBuilder builder, string name)
        {
            Name = name;
            ModelBuilder = builder;
        }

        /// <summary>
        /// Get the bindingParameter. 
        /// <remarks>Null means the Action has no bindingParameter.</remarks>
        /// </summary>
        public BindingParameterConfiguration BindingParameter
        {
            get { return _bindingParameter; }
        }

        /// <inheritdoc />
        public override IEnumerable<ParameterConfiguration> Parameters
        {
            get
            {
                if (_bindingParameter != null)
                {
                    yield return _bindingParameter;
                }
                foreach (ParameterConfiguration parameter in _parameters)
                {
                    yield return parameter;
                }
            }
        }

        /// <inheritdoc />
        public override ProcedureKind Kind
        {
            get { return ProcedureKind.Action; }
        }

        /// <inheritdoc />
        public override bool IsBindable
        {
            get
            {
                return _bindingParameter != null;
            }
        }

        /// <summary>
        /// Whether this action can always be bound.
        /// <example>
        /// For example imagine an Watch action that can be bound to a Movie, it might not always be possible to Watch a movie,
        /// in which case IsAlwaysBindable would return false.
        /// </example>
        /// </summary>
        public override bool IsAlwaysBindable
        {
            get
            {
                if (IsBindable)
                {
                    return _bindingParameter.AlwaysBindable;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether links provided by <see cref="GetActionLink"/> follow OData conventions.
        /// </summary>
        public bool FollowsConventions
        {
            get
            {
                return _followsConventions;
            }
        }

        /// <summary>
        /// Sets the return type to a single EntityType instance.
        /// </summary>
        /// <typeparam name="TEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetName">The name of the entity set which contains the returned entity.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ActionConfiguration ReturnsFromEntitySet<TEntityType>(string entitySetName) where TEntityType : class
        {
            ModelBuilder.EntitySet<TEntityType>(entitySetName);
            EntitySet = ModelBuilder.EntitySets.Single(s => s.Name == entitySetName);
            ReturnType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TEntityType));

            return this;
        }

        /// <summary>
        /// Sets the return type to a single EntityType instance.
        /// </summary>
        /// <typeparam name="TEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetConfiguration">The entity set which contains the returned entity.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ActionConfiguration ReturnsFromEntitySet<TEntityType>(EntitySetConfiguration<TEntityType> entitySetConfiguration) where TEntityType : class
        {
            if (entitySetConfiguration == null)
            {
                throw Error.ArgumentNull("entitySetConfiguration");
            }

            EntitySet = entitySetConfiguration.EntitySet;
            ReturnType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TEntityType));
            return this;
        }

        /// <summary>
        /// Sets the return type to a collection of entities.
        /// </summary>
        /// <typeparam name="TElementEntityType">The entity type.</typeparam>
        /// <param name="entitySetName">The name of the entity set which contains the returned entities.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ActionConfiguration ReturnsCollectionFromEntitySet<TElementEntityType>(string entitySetName) where TElementEntityType : class
        {
            Type clrCollectionType = typeof(IEnumerable<TElementEntityType>);
            ModelBuilder.EntitySet<TElementEntityType>(entitySetName);
            EntitySet = ModelBuilder.EntitySets.Single(s => s.Name == entitySetName);
            IEdmTypeConfiguration elementType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
            return this;
        }

        /// <summary>
        /// Sets the return type to a collection of entities.
        /// </summary>
        /// <typeparam name="TElementEntityType">The entity type.</typeparam>
        /// <param name="entitySetConfiguration">The entity set which contains the returned entities.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ActionConfiguration ReturnsCollectionFromEntitySet<TElementEntityType>(
            EntitySetConfiguration<TElementEntityType> entitySetConfiguration) where TElementEntityType : class
        {
            if (entitySetConfiguration == null)
            {
                throw Error.ArgumentNull("entitySetConfiguration");
            }

            Type clrCollectionType = typeof(IEnumerable<TElementEntityType>);
            EntitySet = entitySetConfiguration.EntitySet;
            IEdmTypeConfiguration elementType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
            return this;
        }

        /// <summary>
        /// Established the return type of the Action.
        /// <remarks>Used when the return type is a single Primitive or ComplexType.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ActionConfiguration Returns<TReturnType>()
        {
            Type returnType = typeof(TReturnType);
            IEdmTypeConfiguration configuration = ModelBuilder.GetTypeConfigurationOrNull(returnType);

            if (configuration is EntityTypeConfiguration)
            {
                throw Error.InvalidOperation(SRResources.ReturnEntityWithoutEntitySet, configuration.FullName);
            }

            if (configuration == null)
            {
                ModelBuilder.AddComplexType(returnType);
                configuration = ModelBuilder.GetTypeConfigurationOrNull(typeof(TReturnType));
            }
            ReturnType = configuration;
            return this;
        }

        /// <summary>
        /// Establishes the return type of the Action
        /// <remarks>Used when the return type is a collection of either Primitive or ComplexTypes.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ActionConfiguration ReturnsCollection<TReturnElementType>()
        {
            // TODO: I don't like this temporary solution that says the CLR type of the collection is IEnumerable<T>.
            // It basically has no meaning. That said the CLR type is meaningful for IEdmTypeConfiguration
            // because I still think it is useful for IEdmPrimitiveTypes too.
            // You can imagine the override of this that takes a delegate using the correct CLR type for the return type.
            Type clrCollectionType = typeof(IEnumerable<TReturnElementType>);
            Type clrElementType = typeof(TReturnElementType);
            IEdmTypeConfiguration edmElementType = ModelBuilder.GetTypeConfigurationOrNull(clrElementType);

            if (edmElementType is EntityTypeConfiguration)
            {
                throw Error.InvalidOperation(SRResources.ReturnEntityCollectionWithoutEntitySet, edmElementType.FullName);
            }

            if (edmElementType == null)
            {
                ModelBuilder.AddComplexType(clrElementType);
                edmElementType = ModelBuilder.GetTypeConfigurationOrNull(clrElementType);
            }
            ReturnType = new CollectionTypeConfiguration(edmElementType, clrCollectionType);
            return this;
        }

        /// <summary>
        /// Specifies the bindingParameter name, type and whether it is alwaysBindable, use only if the Action "isBindable".
        /// </summary>
        public ActionConfiguration SetBindingParameter(string name, IEdmTypeConfiguration bindingParameterType, bool alwaysBindable)
        {
            _bindingParameter = new BindingParameterConfiguration(name, bindingParameterType, alwaysBindable);
            return this;
        }

        /// <summary>
        /// Adds a new non-binding parameter.
        /// </summary>
        public ParameterConfiguration AddParameter(string name, IEdmTypeConfiguration parameterType)
        {
            ParameterConfiguration parameter = new NonbindingParameterConfiguration(name, parameterType);
            _parameters.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// Adds a new non-binding parameter
        /// </summary>  
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ParameterConfiguration Parameter<TParameter>(string name)
        {
            Type type = typeof(TParameter);
            IEdmTypeConfiguration parameterType = ModelBuilder.GetTypeConfigurationOrNull(type);
            if (parameterType == null)
            {
                ModelBuilder.AddComplexType(type);
                parameterType = ModelBuilder.GetTypeConfigurationOrNull(type);
            }
            return AddParameter(name, parameterType);
        }

        /// <summary>
        /// Adds a new non-binding collection parameter
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ParameterConfiguration CollectionParameter<TElementType>(string name)
        {
            Type elementType = typeof(TElementType);
            IEdmTypeConfiguration elementTypeConfiguration = ModelBuilder.GetTypeConfigurationOrNull(elementType);
            if (elementTypeConfiguration == null)
            {
                ModelBuilder.AddComplexType(elementType);
                elementTypeConfiguration = ModelBuilder.GetTypeConfigurationOrNull(elementType);
            }
            CollectionTypeConfiguration parameterType = new CollectionTypeConfiguration(elementTypeConfiguration, typeof(IEnumerable<>).MakeGenericType(elementType));
            return AddParameter(name, parameterType);
        }

        /// <summary>
        /// Register a factory that creates actions links.
        /// </summary>
        public ActionConfiguration HasActionLink(Func<EntityInstanceContext, Uri> actionLinkFactory, bool followsConventions)
        {
            if (!IsBindable || BindingParameter.TypeConfiguration.Kind != EdmTypeKind.Entity)
            {
                throw Error.InvalidOperation(SRResources.HasActionLinkRequiresBindToEntity, Name);
            }
            _actionLinkFactory = actionLinkFactory;
            _followsConventions = followsConventions;
            return this;
        }

        /// <summary>
        /// Retrieves the currently registered action link factory.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public Func<EntityInstanceContext, Uri> GetActionLink()
        {
            return _actionLinkFactory;
        }
    }
}
