// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents a Procedure that is exposed in the model
    /// </summary>
    public abstract class ProcedureConfiguration
    {
        private List<ParameterConfiguration> _parameters = new List<ParameterConfiguration>();
        private BindingParameterConfiguration _bindingParameter;

        /// <summary>
        /// Initializes a new instance of <see cref="ProcedureConfiguration" /> class.
        /// </summary>
        /// <param name="builder">The ODataModelBuilder to which this ProcedureConfiguration should be added.</param>
        /// <param name="name">The name of this ProcedureConfiguration.</param>
        internal ProcedureConfiguration(ODataModelBuilder builder, string name)
        {
            Name = name;
            ModelBuilder = builder;
        }

        /// <summary>
        /// Gets or sets the currently registered procedure link factory.
        /// </summary>
        protected Func<EntityInstanceContext, Uri> LinkFactory { get; set; }

        /// <summary>
        /// Gets a value indicating whether procedure links follow OData conventions.
        /// </summary>
        public bool FollowsConventions { get; protected set; }

        /// <summary>
        /// The Name of the procedure
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The Kind of procedure, which can be either Action or Function
        /// </summary>
        public abstract ProcedureKind Kind { get; }

        /// <summary>
        /// Can the procedure be composed upon.
        /// 
        /// For example can a URL that invokes the procedure be used as the base url for 
        /// a request that invokes the procedure and does something else with the results
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Copies existing spelling used in EdmLib.")]
        public virtual bool IsComposable { get; internal set; }

        /// <summary>
        /// Does the procedure have side-effects.
        /// </summary>
        public abstract bool IsSideEffecting { get; }

        /// <summary>
        /// The qualified name of the procedure when used in OData urls.
        /// Qualification is required to distinguish the procedure from other possible single part identifiers.
        /// </summary>
        public string ContainerQualifiedName
        {
            get { return ModelBuilder.ContainerName + "." + Name; }
        }

        /// <summary>
        /// The FullyQualifiedName is the ContainerQualifiedName further qualified using the Namespace.
        /// Typically this is not required, because most services have at most one container with the same name.
        /// </summary>
        public string FullyQualifiedName
        {
            get { return ModelBuilder.Namespace + "." + ContainerQualifiedName; }
        }

        /// <summary>
        /// The FullName is the ContainerQualifiedName.
        /// </summary>
        public string FullName
        {
            get { return ContainerQualifiedName; }
        }

        /// <summary>
        /// The type returned when the procedure is invoked.
        /// </summary>
        public IEdmTypeConfiguration ReturnType { get; set; }

        /// <summary>
        /// The EntitySet that entities are returned from.
        /// </summary>
        public EntitySetConfiguration EntitySet { get; set; }

        /// <summary>
        /// The EntitySetPathExpression that entities are returned from.
        /// </summary>
        public IEnumerable<string> EntitySetPath { get; internal set; }

        /// <summary>
        /// Get the bindingParameter. 
        /// <remarks>Null means the procedure has no bindingParameter.</remarks>
        /// </summary>
        public virtual BindingParameterConfiguration BindingParameter
        {
            get { return _bindingParameter; }
        }

        /// <summary>
        /// The parameters the procedure takes
        /// </summary>
        public virtual IEnumerable<ParameterConfiguration> Parameters
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

        /// <summary>
        /// Can the procedure be bound to a url representing the BindingParameter.
        /// </summary>
        public virtual bool IsBindable
        {
            get
            {
                return _bindingParameter != null;
            }
        }

        /// <summary>
        /// Whether this procedure can always be bound.
        /// <example>
        /// For example imagine an Watch action that can be bound to a Movie, it might not always be possible to Watch a movie,
        /// in which case IsAlwaysBindable would return false.
        /// </example>
        /// </summary>
        public virtual bool IsAlwaysBindable
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
        /// Sets the return type to a single EntityType instance.
        /// </summary>
        /// <typeparam name="TEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetName">The entitySetName which contains the return EntityType instance</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        internal void ReturnsFromEntitySetImplementation<TEntityType>(string entitySetName) where TEntityType : class
        {
            ModelBuilder.EntitySet<TEntityType>(entitySetName);
            EntitySet = ModelBuilder.EntitySets.Single(s => s.Name == entitySetName);
            ReturnType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TEntityType));
        }

        /// <summary>
        /// Sets the return type to a collection of EntityType instances.
        /// </summary>
        /// <typeparam name="TElementEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetName">The entitySetName which contains the returned EntityType instances</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        internal void ReturnsCollectionFromEntitySetImplementation<TElementEntityType>(string entitySetName) where TElementEntityType : class
        {
            Type clrCollectionType = typeof(IEnumerable<TElementEntityType>);
            ModelBuilder.EntitySet<TElementEntityType>(entitySetName);
            EntitySet = ModelBuilder.EntitySets.Single(s => s.Name == entitySetName);
            IEdmTypeConfiguration elementType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
        }

        /// <summary>
        /// Sets the return type to a single EntityType instance.
        /// </summary>
        /// <typeparam name="TEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetPath">The entitySetPath which contains the return EntityType instance</param>
        internal void ReturnsEntityViaEntitySetPathImplementation<TEntityType>(IEnumerable<string> entitySetPath) where TEntityType : class
        {
            ReturnType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TEntityType));
            EntitySetPath = entitySetPath;
        }

        /// <summary>
        /// Sets the return type to a collection of EntityType instances.
        /// </summary>
        /// <typeparam name="TElementEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetPath">The entitySetPath which contains the returned EntityType instances</param>
        internal void ReturnsCollectionViaEntitySetPathImplementation<TElementEntityType>(IEnumerable<string> entitySetPath) where TElementEntityType : class
        {
            Type clrCollectionType = typeof(IEnumerable<TElementEntityType>);
            IEdmTypeConfiguration elementType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
            EntitySetPath = entitySetPath;
        }

        /// <summary>
        /// Established the return type of the procedure.
        /// <remarks>Used when the return type is a single Primitive or ComplexType.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        internal void ReturnsImplementation<TReturnType>()
        {
            Type returnType = typeof(TReturnType);
            IEdmTypeConfiguration configuration = ModelBuilder.GetTypeConfigurationOrNull(returnType);
            if (configuration == null)
            {
                ModelBuilder.AddComplexType(returnType);
                configuration = ModelBuilder.GetTypeConfigurationOrNull(typeof(TReturnType));
            }
            ReturnType = configuration;
        }

        /// <summary>
        /// Establishes the return type of the procedure
        /// <remarks>Used when the return type is a collection of either Primitive or ComplexTypes.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        internal void ReturnsCollectionImplementation<TReturnElementType>()
        {
            // TODO: I don't like this temporary solution that says the CLR type of the collection is IEnumerable<T>.
            // It basically has no meaning. That said the CLR type is meaningful for IEdmTypeConfiguration
            // because I still think it is useful for IEdmPrimitiveTypes too.
            // You can imagine the override of this that takes a delegate using the correct CLR type for the return type.
            Type clrCollectionType = typeof(IEnumerable<TReturnElementType>);
            Type clrElementType = typeof(TReturnElementType);
            IEdmTypeConfiguration edmElementType = ModelBuilder.GetTypeConfigurationOrNull(clrElementType);
            if (edmElementType == null)
            {
                ModelBuilder.AddComplexType(clrElementType);
                edmElementType = ModelBuilder.GetTypeConfigurationOrNull(clrElementType);
            }
            ReturnType = new CollectionTypeConfiguration(edmElementType, clrCollectionType);
        }

        /// <summary>
        /// Specifies the bindingParameter name, type and whether it is alwaysBindable, use only if the procedure "isBindable".
        /// </summary>
        internal void SetBindingParameterImplementation(string name, IEdmTypeConfiguration bindingParameterType, bool alwaysBindable)
        {
            _bindingParameter = new BindingParameterConfiguration(name, bindingParameterType, alwaysBindable);
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
        /// Gets or sets the <see cref="ODataModelBuilder"/> used to create this configuration.
        /// </summary>
        protected ODataModelBuilder ModelBuilder { get; set; }
    }
}
