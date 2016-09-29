// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Represents a Operation that is exposed in the model
    /// </summary>
    public abstract class OperationConfiguration
    {
        private List<ParameterConfiguration> _parameters = new List<ParameterConfiguration>();
        private BindingParameterConfiguration _bindingParameter;
        private string _namespace;

        /// <summary>
        /// Initializes a new instance of <see cref="OperationConfiguration" /> class.
        /// </summary>
        /// <param name="builder">The ODataModelBuilder to which this OperationConfiguration should be added.</param>
        /// <param name="name">The name of this OperationConfiguration.</param>
        internal OperationConfiguration(ODataModelBuilder builder, string name)
        {
            Name = name;
            ModelBuilder = builder;
        }

        /// <summary>
        /// Gets or sets the currently registered operation link builder.
        /// </summary>
        protected OperationLinkBuilder OperationLinkBuilder { get; set; }

        /// <summary>
        /// Gets a value indicating whether operation links follow OData conventions.
        /// </summary>
        public bool FollowsConventions { get; protected set; }

        /// <summary>
        /// The Name of the operation
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The Title of the operation. When customized, the title of the operation
        /// will be sent back when the OData client asks for an entity or a feed in
        /// JSON full metadata.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The Kind of operation, which can be either Action or Function
        /// </summary>
        public abstract OperationKind Kind { get; }

        /// <summary>
        /// Can the operation be composed upon.
        /// 
        /// For example can a URL that invokes the operation be used as the base URL for 
        /// a request that invokes the operation and does something else with the results
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Copies existing spelling used in EdmLib.")]
        public virtual bool IsComposable { get; internal set; }

        /// <summary>
        /// Does the operation have side-effects.
        /// </summary>
        public abstract bool IsSideEffecting { get; }

        /// <summary>
        /// The FullyQualifiedName is the Name further qualified using the Namespace.
        /// </summary>
        public string FullyQualifiedName
        {
            get { return Namespace + "." + Name; }
        }

        /// <summary>
        /// The Namespace by default is the ModelBuilder's Namespace.
        /// </summary>
        public string Namespace
        {
            get { return _namespace ?? ModelBuilder.Namespace; } 
            set { _namespace = value;  }
        }

        /// <summary>
        /// The type returned when the operation is invoked.
        /// </summary>
        public IEdmTypeConfiguration ReturnType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the return is optional or not.
        /// </summary>
        public bool OptionalReturn { get; set; }

        /// <summary>
        /// The Navigation Source that are returned from.
        /// </summary>
        public NavigationSourceConfiguration NavigationSource { get; set; }

        /// <summary>
        /// The EntitySetPathExpression that entities are returned from.
        /// </summary>
        public IEnumerable<string> EntitySetPath { get; internal set; }

        /// <summary>
        /// Get the bindingParameter. 
        /// <remarks>Null means the operation has no bindingParameter.</remarks>
        /// </summary>
        public virtual BindingParameterConfiguration BindingParameter
        {
            get { return _bindingParameter; }
        }

        /// <summary>
        /// The parameters the operation takes
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
        /// Can the operation be bound to a URL representing the BindingParameter.
        /// </summary>
        public virtual bool IsBindable
        {
            get
            {
                return _bindingParameter != null;
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
            NavigationSource = ModelBuilder.EntitySets.Single(s => s.Name == entitySetName);
            ReturnType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TEntityType));
            OptionalReturn = true;
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
            NavigationSource = ModelBuilder.EntitySets.Single(s => s.Name == entitySetName);
            IEdmTypeConfiguration elementType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
            OptionalReturn = true;
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
            OptionalReturn = true;
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
            OptionalReturn = true;
        }

        /// <summary>
        /// Established the return type of the operation.
        /// <remarks>Used when the return type is a single Primitive or ComplexType.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        internal void ReturnsImplementation(Type clrReturnType)
        {
            IEdmTypeConfiguration configuration = GetOperationTypeConfiguration(clrReturnType);
            ReturnType = configuration;
            OptionalReturn = EdmLibHelpers.IsNullable(clrReturnType);
        }

        /// <summary>
        /// Establishes the return type of the operation
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
            IEdmTypeConfiguration edmElementType = GetOperationTypeConfiguration(clrElementType);
            ReturnType = new CollectionTypeConfiguration(edmElementType, clrCollectionType);
            OptionalReturn = EdmLibHelpers.IsNullable(clrElementType);
        }

        /// <summary>
        /// Specifies the bindingParameter name, type and whether it is alwaysBindable, use only if the operation "isBindable".
        /// </summary>
        internal void SetBindingParameterImplementation(string name, IEdmTypeConfiguration bindingParameterType)
        {
            _bindingParameter = new BindingParameterConfiguration(name, bindingParameterType);
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
        public ParameterConfiguration Parameter(Type clrParameterType, string name)
        {
            if (clrParameterType == null)
            {
                throw Error.ArgumentNull("clrParameterType");
            }

            IEdmTypeConfiguration parameterType = GetOperationTypeConfiguration(clrParameterType);
            return AddParameter(name, parameterType);
        }

        /// <summary>
        /// Adds a new non-binding parameter
        /// </summary>  
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ParameterConfiguration Parameter<TParameter>(string name)
        {
            return this.Parameter(typeof(TParameter), name);
        }

        /// <summary>
        /// Adds a new non-binding collection parameter
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ParameterConfiguration CollectionParameter<TElementType>(string name)
        {
            Type elementType = typeof(TElementType);
            IEdmTypeConfiguration elementTypeConfiguration = GetOperationTypeConfiguration(typeof(TElementType));
            CollectionTypeConfiguration parameterType = new CollectionTypeConfiguration(elementTypeConfiguration, typeof(IEnumerable<>).MakeGenericType(elementType));
            return AddParameter(name, parameterType);
        }

        /// <summary>
        /// Adds a new non-binding entity type parameter.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "In keeping with rest of API")]
        public ParameterConfiguration EntityParameter<TEntityType>(string name) where TEntityType : class
        {
            Type entityType = typeof(TEntityType);
            IEdmTypeConfiguration parameterType =
                ModelBuilder.StructuralTypes.FirstOrDefault(t => t.ClrType == entityType) ??
                ModelBuilder.AddEntityType(entityType);

            return AddParameter(name, parameterType);
        }

        /// <summary>
        /// Adds a new non-binding collection of entity type parameter.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "In keeping with rest of API")]
        public ParameterConfiguration CollectionEntityParameter<TElementEntityType>(string name) where TElementEntityType : class
        {
            Type elementType = typeof(TElementEntityType);
            IEdmTypeConfiguration elementTypeConfiguration =
                ModelBuilder.StructuralTypes.FirstOrDefault(t => t.ClrType == elementType) ??
                ModelBuilder.AddEntityType(elementType);

            CollectionTypeConfiguration parameterType = new CollectionTypeConfiguration(elementTypeConfiguration,
                typeof(IEnumerable<>).MakeGenericType(elementType));

            return AddParameter(name, parameterType);
        }

        /// <summary>
        /// Gets or sets the <see cref="ODataModelBuilder"/> used to create this configuration.
        /// </summary>
        protected ODataModelBuilder ModelBuilder { get; set; }

        private IEdmTypeConfiguration GetOperationTypeConfiguration(Type clrType)
        {
            Type type = TypeHelper.GetUnderlyingTypeOrSelf(clrType);
            IEdmTypeConfiguration edmTypeConfiguration;

            if (type.GetTypeInfo().IsEnum)
            {
                edmTypeConfiguration = ModelBuilder.GetTypeConfigurationOrNull(type);

                if (edmTypeConfiguration != null && EdmLibHelpers.IsNullable(clrType))
                {
                    edmTypeConfiguration = ((EnumTypeConfiguration)edmTypeConfiguration).GetNullableEnumTypeConfiguration();
                }
            }
            else
            {
                edmTypeConfiguration = ModelBuilder.GetTypeConfigurationOrNull(clrType);
            }

            if (edmTypeConfiguration == null)
            {
                if (type.GetTypeInfo().IsEnum)
                {
                    EnumTypeConfiguration enumTypeConfiguration = ModelBuilder.AddEnumType(type);

                    if (EdmLibHelpers.IsNullable(clrType))
                    {
                        edmTypeConfiguration = enumTypeConfiguration.GetNullableEnumTypeConfiguration();
                    }
                    else
                    {
                        edmTypeConfiguration = enumTypeConfiguration;
                    }
                }
                else
                {
                    edmTypeConfiguration = ModelBuilder.AddComplexType(clrType);
                }
            }

            return edmTypeConfiguration;
        }
    }
}
