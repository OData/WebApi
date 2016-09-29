// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// ActionConfiguration represents an OData action that you wish to expose via your service.
    /// <remarks>
    /// ActionConfigurations are exposed via $metadata as a <Action/> element for bound action and <ActionImport/> element for unbound action.
    /// </remarks> 
    /// </summary>
    public class ActionConfiguration : OperationConfiguration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ActionConfiguration" /> class.
        /// </summary>
        /// <param name="builder">The ODataModelBuilder to which this ActionConfiguration should be added.</param>
        /// <param name="name">The name of this ActionConfiguration.</param>
        public ActionConfiguration(ODataModelBuilder builder, string name)
            : base(builder, name)
        {
        }

        /// <inheritdoc />
        public override OperationKind Kind
        {
            get { return OperationKind.Action; }
        }

        /// <inheritdoc />
        public override bool IsSideEffecting
        {
            get { return true; }
        }

        /// <summary>
        /// Register a factory that creates actions links.
        /// </summary>
        public ActionConfiguration HasActionLink(Func<ResourceContext, Uri> actionLinkFactory, bool followsConventions)
        {
            if (actionLinkFactory == null)
            {
                throw new ArgumentNullException("actionLinkFactory");
            }

            if (!IsBindable || BindingParameter.TypeConfiguration.Kind != EdmTypeKind.Entity)
            {
                throw Error.InvalidOperation(SRResources.HasActionLinkRequiresBindToEntity, Name);
            }

            OperationLinkBuilder = new OperationLinkBuilder(actionLinkFactory, followsConventions);
            FollowsConventions = followsConventions;
            return this;
        }

        /// <summary>
        /// Retrieves the currently registered action link factory.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public Func<ResourceContext, Uri> GetActionLink()
        {
            if (OperationLinkBuilder == null)
            {
                return null;
            }

            return OperationLinkBuilder.LinkFactory;
        }

        /// <summary>
        /// Register a factory that creates feed actions links.
        /// </summary>
        public ActionConfiguration HasFeedActionLink(Func<ResourceSetContext, Uri> actionLinkFactory, bool followsConventions)
        {
            if (actionLinkFactory == null)
            {
                throw new ArgumentNullException("actionLinkFactory");
            }

            if (!IsBindable ||
                BindingParameter.TypeConfiguration.Kind != EdmTypeKind.Collection ||
                ((CollectionTypeConfiguration)BindingParameter.TypeConfiguration).ElementType.Kind != EdmTypeKind.Entity)
            {
                throw Error.InvalidOperation(/*SRResources.HasActionLinkRequiresBindToCollectionOfEntity, Name*/"TODO: ");
            }

            OperationLinkBuilder = new OperationLinkBuilder(actionLinkFactory, followsConventions);
            FollowsConventions = followsConventions;
            return this;
        }

        /// <summary>
        /// Retrieves the currently registered feed action link factory.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Consistent with EF Has/Get pattern")]
        public Func<ResourceSetContext, Uri> GetFeedActionLink()
        {
            if (OperationLinkBuilder == null)
            {
                return null;
            }

            return OperationLinkBuilder.FeedLinkFactory;
        }

        /// <summary>
        /// Sets the return type to a single EntityType instance.
        /// </summary>
        /// <typeparam name="TEntityType">The type that is an EntityType</typeparam>
        /// <param name="entitySetName">The name of the entity set which contains the returned entity.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ActionConfiguration ReturnsFromEntitySet<TEntityType>(string entitySetName) where TEntityType : class
        {
            ReturnsFromEntitySetImplementation<TEntityType>(entitySetName);
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

            NavigationSource = entitySetConfiguration.EntitySet;
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
            ReturnsCollectionFromEntitySetImplementation<TElementEntityType>(entitySetName);
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
            NavigationSource = entitySetConfiguration.EntitySet;
            IEdmTypeConfiguration elementType = ModelBuilder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
            return this;
        }

        /// <summary>
        /// Established the return type of the Action.
        /// <remarks>Used when the return type is a single Primitive or ComplexType.</remarks>
        /// </summary>
        public ActionConfiguration Returns(Type clrReturnType)
        {
            if (clrReturnType == null)
            {
                throw Error.ArgumentNull("clrReturnType");
            }

            IEdmTypeConfiguration configuration = ModelBuilder.GetTypeConfigurationOrNull(clrReturnType);

            if (configuration is EntityTypeConfiguration)
            {
                throw Error.InvalidOperation(SRResources.ReturnEntityWithoutEntitySet, configuration.FullName);
            }

            ReturnsImplementation(clrReturnType);
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
            return this.Returns(returnType);
        }

        /// <summary>
        /// Establishes the return type of the Action
        /// <remarks>Used when the return type is a collection of either Primitive or ComplexTypes.</remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "In keeping with rest of API")]
        public ActionConfiguration ReturnsCollection<TReturnElementType>()
        {
            Type clrElementType = typeof(TReturnElementType);
            IEdmTypeConfiguration edmElementType = ModelBuilder.GetTypeConfigurationOrNull(clrElementType);

            if (edmElementType is EntityTypeConfiguration)
            {
                throw Error.InvalidOperation(SRResources.ReturnEntityCollectionWithoutEntitySet, edmElementType.FullName);
            }

            ReturnsCollectionImplementation<TReturnElementType>();
            return this;
        }

        /// <summary>
        /// Specifies the bindingParameter name, type and whether it is alwaysBindable, use only if the Action "isBindable".
        /// </summary>
        public ActionConfiguration SetBindingParameter(string name, IEdmTypeConfiguration bindingParameterType)
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
        public ActionConfiguration ReturnsEntityViaEntitySetPath<TEntityType>(string entitySetPath) where TEntityType : class
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
        public ActionConfiguration ReturnsEntityViaEntitySetPath<TEntityType>(params string[] entitySetPath) where TEntityType : class
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
        public ActionConfiguration ReturnsCollectionViaEntitySetPath<TElementEntityType>(string entitySetPath) where TElementEntityType : class
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
        public ActionConfiguration ReturnsCollectionViaEntitySetPath<TElementEntityType>(params string[] entitySetPath) where TElementEntityType : class
        {
            ReturnsCollectionViaEntitySetPathImplementation<TElementEntityType>(entitySetPath);
            return this;
        }
    }
}
