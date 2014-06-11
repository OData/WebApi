// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// FunctionConfiguration represents an OData function that you wish to expose via your service.
    /// <remarks>
    /// FunctionConfigurations are exposed via $metadata as a <FunctionImport/> element.
    /// </remarks>
    /// </summary>
    public class FunctionConfiguration : ProcedureConfiguration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FunctionConfiguration" /> class.
        /// </summary>
        /// <param name="builder">The ODataModelBuilder to which this FunctionConfiguration should be added.</param>
        /// <param name="name">The name of this FunctionConfiguration.</param>
        internal FunctionConfiguration(ODataModelBuilder builder, string name) : base(builder, name)
        {
            // By default, function import is included in service document
            IncludeInServiceDocument = true;
        }

        /// <inheritdoc />
        public override ProcedureKind Kind
        {
            get { return ProcedureKind.Function; }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Copies existing spelling used in EdmLib.")]
        public new bool IsComposable
        {
            get { return base.IsComposable; }
            set { base.IsComposable = value; }
        }

        /// <inheritdoc />
        public override bool IsSideEffecting
        {
            get { return false; }
        }

        /// <summary>
        /// Gets/Sets a value indicating whether the function is supported in $filter.
        /// </summary>
        public bool SupportedInFilter { get; set; }

        /// <summary>
        /// Gets/Sets a value indicating whether the function is supported in $orderby.
        /// </summary>
        public bool SupportedInOrderBy { get; set; }

        /// <summary>
        /// Gets/Set a value indicating whether the procedure is included in service document or not.
        /// Meaningful only for function imports; ignore for bound functions.
        /// </summary>
        public bool IncludeInServiceDocument { get; set; }

        /// <summary>
        /// Register a factory that creates functions links.
        /// </summary>
        public FunctionConfiguration HasFunctionLink(Func<EntityInstanceContext, Uri> functionLinkFactory, bool followsConventions)
        {
            if (functionLinkFactory == null)
            {
                throw new ArgumentNullException("functionLinkFactory");
            }

            if (!IsBindable || BindingParameter.TypeConfiguration.Kind != EdmTypeKind.Entity)
            {
                throw Error.InvalidOperation(SRResources.HasActionLinkRequiresBindToEntity, Name);
            }
            LinkFactory = functionLinkFactory;
            FollowsConventions = followsConventions;
            return this;
        }

        /// <summary>
        /// Retrieves the currently registered function link factory.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public Func<EntityInstanceContext, Uri> GetFunctionLink()
        {
            return LinkFactory;
        }

        /// <summary>
        /// Sets the return type to a single EntityType instance.
        /// </summary>
        /// <typeparam name="TEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetName">The entitySetName which contains the return EntityType instance</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public FunctionConfiguration ReturnsFromEntitySet<TEntityType>(string entitySetName) where TEntityType : class
        {
            ReturnsFromEntitySetImplementation<TEntityType>(entitySetName);
            return this;
        }

        /// <summary>
        /// Sets the return type to a collection of EntityType instances.
        /// </summary>
        /// <typeparam name="TElementEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetName">The entitySetName which contains the returned EntityType instances</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public FunctionConfiguration ReturnsCollectionFromEntitySet<TElementEntityType>(string entitySetName) where TElementEntityType : class
        {
            ReturnsCollectionFromEntitySetImplementation<TElementEntityType>(entitySetName);
            return this;
        }

        /// <summary>
        /// Established the return type of the Function.
        /// <remarks>Used when the return type is a single Primitive or ComplexType.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public FunctionConfiguration Returns<TReturnType>()
        {
            ReturnsImplementation<TReturnType>();
            return this;
        }

        /// <summary>
        /// Establishes the return type of the Function
        /// <remarks>Used when the return type is a collection of either Primitive or ComplexTypes.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public FunctionConfiguration ReturnsCollection<TReturnElementType>()
        {
            ReturnsCollectionImplementation<TReturnElementType>();
            return this;
        }

        /// <summary>
        /// Specifies the bindingParameter name, type and whether it is alwaysBindable, use only if the Function "isBindable".
        /// </summary>
        public FunctionConfiguration SetBindingParameter(string name, IEdmTypeConfiguration bindingParameterType)
        {
            SetBindingParameterImplementation(name, bindingParameterType);
            return this;
        }

        /// <summary>
        /// Sets the return type to a single EntityType instance.
        /// </summary>
        /// <typeparam name="TEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetPath">The entitySetPath which contains the return EntityType instance</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public FunctionConfiguration ReturnsEntityViaEntitySetPath<TEntityType>(string entitySetPath) where TEntityType : class
        {
            if (String.IsNullOrEmpty(entitySetPath))
            {
                throw new ArgumentNullException("entitySetPath");
                }
            ReturnsEntityViaEntitySetPathImplementation<TEntityType>(entitySetPath.Split('/'));
            return this;
        }

        /// <summary>
        /// Sets the return type to a single EntityType instance.
        /// </summary>
        /// <typeparam name="TEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetPath">The entitySetPath which contains the return EntityType instance</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public FunctionConfiguration ReturnsEntityViaEntitySetPath<TEntityType>(params string[] entitySetPath) where TEntityType : class
        {
            ReturnsEntityViaEntitySetPathImplementation<TEntityType>(entitySetPath);
            return this;
        }

        /// <summary>
        /// Sets the return type to a collection of EntityType instances.
        /// </summary>
        /// <typeparam name="TElementEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetPath">The entitySetPath which contains the returned EntityType instances</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public FunctionConfiguration ReturnsCollectionViaEntitySetPath<TElementEntityType>(string entitySetPath) where TElementEntityType : class
        {
            if (String.IsNullOrEmpty(entitySetPath))
            {
                throw new ArgumentNullException("entitySetPath");
            }
            ReturnsCollectionViaEntitySetPathImplementation<TElementEntityType>(entitySetPath.Split('/'));
            return this;
        }

        /// <summary>
        /// Sets the return type to a collection of EntityType instances.
        /// </summary>
        /// <typeparam name="TElementEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetPath">The entitySetPath which contains the returned EntityType instances</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public FunctionConfiguration ReturnsCollectionViaEntitySetPath<TElementEntityType>(params string[] entitySetPath) where TElementEntityType : class
        {
            ReturnsCollectionViaEntitySetPathImplementation<TElementEntityType>(entitySetPath);
            return this;
        }
    }
}
