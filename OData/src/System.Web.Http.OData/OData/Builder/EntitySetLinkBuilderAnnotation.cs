// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// <see cref="EntitySetLinkBuilderAnnotation" /> is a class used to annotate an <see cref="IEdmEntitySet" /> inside an <see cref="IEdmModel" />
    /// with information about how to build links related to that entity set.
    /// </summary>
    public class EntitySetLinkBuilderAnnotation
    {
        private readonly Func<FeedContext, Uri> _feedSelfLinkBuilder;

        private readonly SelfLinkBuilder<string> _idLinkBuilder;
        private readonly SelfLinkBuilder<Uri> _editLinkBuilder;
        private readonly SelfLinkBuilder<Uri> _readLinkBuilder;

        private readonly Dictionary<IEdmNavigationProperty, NavigationLinkBuilder> _navigationPropertyLinkBuilderLookup = new Dictionary<IEdmNavigationProperty, NavigationLinkBuilder>();
        private readonly string _entitySetName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetLinkBuilderAnnotation" /> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public EntitySetLinkBuilderAnnotation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetLinkBuilderAnnotation"/> class.
        /// </summary>
        /// <param name="entitySet">The entity set for which the link builder is being constructed.</param>
        /// <param name="model">The EDM model that this entity set belongs to.</param>
        /// <remarks>This constructor creates a link builder that generates URL's that follow OData conventions for the given entity set.</remarks>
        public EntitySetLinkBuilderAnnotation(IEdmEntitySet entitySet, IEdmModel model)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            IEdmEntityType elementType = entitySet.ElementType;
            IEnumerable<IEdmEntityType> derivedTypes = model.FindAllDerivedTypes(elementType).Cast<IEdmEntityType>();
            bool derivedTypesDefineNavigationProperty = false;

            // Add navigation link builders for all navigation properties of entity.
            foreach (IEdmNavigationProperty navigationProperty in elementType.NavigationProperties())
            {
                Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory =
                    (entityInstanceContext, navProperty) => entityInstanceContext.GenerateNavigationPropertyLink(navProperty, includeCast: false);
                AddNavigationPropertyLinkBuilder(navigationProperty, new NavigationLinkBuilder(navigationLinkFactory, followsConventions: true));
            }

            // Add navigation link builders for all navigation properties in derived types.
            foreach (IEdmEntityType derivedEntityType in derivedTypes)
            {
                foreach (IEdmNavigationProperty navigationProperty in derivedEntityType.DeclaredNavigationProperties())
                {
                    derivedTypesDefineNavigationProperty = true;
                    Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory =
                    (entityInstanceContext, navProperty) => entityInstanceContext.GenerateNavigationPropertyLink(navProperty, includeCast: true);
                    AddNavigationPropertyLinkBuilder(navigationProperty, new NavigationLinkBuilder(navigationLinkFactory, followsConventions: true));
                }
            }

            _entitySetName = entitySet.Name;
            _feedSelfLinkBuilder = (feedContext) => feedContext.GenerateFeedSelfLink();

            Func<EntityInstanceContext, string> selfLinkFactory =
                (entityInstanceContext) => entityInstanceContext.GenerateSelfLink(includeCast: derivedTypesDefineNavigationProperty);
            _idLinkBuilder = new SelfLinkBuilder<string>(selfLinkFactory, followsConventions: true);
        }

        /// <summary>
        /// Constructs an instance of an <see cref="EntitySetLinkBuilderAnnotation" />.
        /// <remarks>Every parameter must be non-null.</remarks>
        /// </summary>
        public EntitySetLinkBuilderAnnotation(
            IEdmEntitySet entitySet,
            Func<FeedContext, Uri> feedSelfLinkBuilder,
            SelfLinkBuilder<string> idLinkBuilder,
            SelfLinkBuilder<Uri> editLinkBuilder,
            SelfLinkBuilder<Uri> readLinkBuilder)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            _entitySetName = entitySet.Name;
            _feedSelfLinkBuilder = feedSelfLinkBuilder;
            _idLinkBuilder = idLinkBuilder;
            _editLinkBuilder = editLinkBuilder;
            _readLinkBuilder = readLinkBuilder;
        }

        /// <summary>
        /// Constructs an instance of an <see cref="EntitySetLinkBuilderAnnotation" /> from an <see cref="EntitySetConfiguration" />.
        /// </summary>
        public EntitySetLinkBuilderAnnotation(EntitySetConfiguration entitySet)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            _entitySetName = entitySet.Name;
            _feedSelfLinkBuilder = entitySet.GetFeedSelfLink();
            _idLinkBuilder = entitySet.GetIdLink();
            _editLinkBuilder = entitySet.GetEditLink();
            _readLinkBuilder = entitySet.GetReadLink();
        }

        /// <summary>
        /// Register a link builder for a <see cref="IEdmNavigationProperty" /> that navigates from Entities in this EntitySet. 
        /// </summary>
        public void AddNavigationPropertyLinkBuilder(IEdmNavigationProperty navigationProperty, NavigationLinkBuilder linkBuilder)
        {
            _navigationPropertyLinkBuilderLookup[navigationProperty] = linkBuilder;
        }

        /// <summary>
        /// Build a self-link URI given a <see cref="FeedContext" />.
        /// </summary>
        public virtual Uri BuildFeedSelfLink(FeedContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (_feedSelfLinkBuilder == null)
            {
                return null;
            }

            return _feedSelfLinkBuilder(context);
        }

        /// <summary>
        /// Constructs the <see cref="EntitySelfLinks" /> for a particular <see cref="EntityInstanceContext" /> and <see cref="ODataMetadataLevel" />.
        /// </summary>
        public virtual EntitySelfLinks BuildEntitySelfLinks(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
        {
            EntitySelfLinks selfLinks = new EntitySelfLinks();
            selfLinks.IdLink = BuildIdLink(instanceContext, metadataLevel);
            selfLinks.EditLink = BuildEditLink(instanceContext, metadataLevel, selfLinks.IdLink);
            selfLinks.ReadLink = BuildReadLink(instanceContext, metadataLevel, selfLinks.EditLink);
            return selfLinks;
        }

        /// <summary>
        /// Constructs the IdLink for a particular <see cref="EntityInstanceContext" /> and <see cref="ODataMetadataLevel" />.
        /// </summary>
        public virtual string BuildIdLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
        {
            if (instanceContext == null)
            {
                throw Error.ArgumentNull("instanceContext");
            }

            if (_idLinkBuilder == null)
            {
                throw Error.InvalidOperation(SRResources.NoIdLinkFactoryFound, _entitySetName);
            }

            if (IsDefaultOrFull(metadataLevel) || (IsMinimal(metadataLevel) && !_idLinkBuilder.FollowsConventions))
            {
                return _idLinkBuilder.Factory(instanceContext);
            }
            else
            {
                // client can infer it and didn't ask for it.
                return null;
            }
        }

        /// <summary>
        /// Constructs the EditLink URL for a particular <see cref="EntityInstanceContext" /> and <see cref="ODataMetadataLevel" />.
        /// </summary>
        public virtual Uri BuildEditLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel, string idLink)
        {
            if (instanceContext == null)
            {
                throw Error.ArgumentNull("instanceContext");
            }

            if (_editLinkBuilder == null)
            {
                // edit link is the same as id link. emit only in default metadata mode.
                if (metadataLevel == ODataMetadataLevel.Default)
                {
                    return new Uri(idLink);
                }
            }
            else if (IsDefaultOrFull(metadataLevel) ||
                (IsMinimal(metadataLevel) && !_editLinkBuilder.FollowsConventions))
            {
                // edit link is the not the same as id link. Generate if the client asked for it (full metadata modes) or
                // if the client cannot infer it (not follow conventions).
                return _editLinkBuilder.Factory(instanceContext);
            }

            // client can infer it and didn't ask for it.
            return null;
        }

        /// <summary>
        /// Constructs a ReadLink URL for a particular <see cref="EntityInstanceContext" /> and <see cref="ODataMetadataLevel" />.
        /// </summary>
        public virtual Uri BuildReadLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel, Uri editLink)
        {
            if (instanceContext == null)
            {
                throw Error.ArgumentNull("instanceContext");
            }

            if (_readLinkBuilder == null)
            {
                // read link is the same as edit link. emit only in default metadata mode.
                if (metadataLevel == ODataMetadataLevel.Default)
                {
                    return editLink;
                }
            }
            else if (IsDefaultOrFull(metadataLevel) ||
                (IsMinimal(metadataLevel) && !_readLinkBuilder.FollowsConventions))
            {
                // read link is not the same as edit link. Generate if the client asked for it (full metadata modes) or
                // if the client cannot infer it (not follow conventions).
                return _readLinkBuilder.Factory(instanceContext);
            }

            // client can infer it and didn't ask for it.
            return null;
        }

        /// <summary>
        /// Constructs a NavigationLink for a particular <see cref="EntityInstanceContext" />, <see cref="IEdmNavigationProperty" /> and <see cref="ODataMetadataLevel" />.
        /// </summary>
        public virtual Uri BuildNavigationLink(EntityInstanceContext instanceContext, IEdmNavigationProperty navigationProperty, ODataMetadataLevel metadataLevel)
        {
            if (instanceContext == null)
            {
                throw Error.ArgumentNull("instanceContext");
            }

            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            NavigationLinkBuilder navigationLinkBuilder;
            if (!_navigationPropertyLinkBuilderLookup.TryGetValue(navigationProperty, out navigationLinkBuilder))
            {
                throw Error.Argument("navigationProperty", SRResources.NoNavigationLinkFactoryFound, navigationProperty.Name, navigationProperty.DeclaringEntityType(), _entitySetName);
            }
            Contract.Assert(navigationLinkBuilder != null);

            if (IsDefaultOrFull(metadataLevel) ||
                (IsMinimal(metadataLevel) && !navigationLinkBuilder.FollowsConventions))
            {
                return navigationLinkBuilder.Factory(instanceContext, navigationProperty);
            }
            else
            {
                // client can infer it and didn't ask for it.
                return null;
            }
        }

        private static bool IsDefaultOrFull(ODataMetadataLevel level)
        {
            return level == ODataMetadataLevel.Default || level == ODataMetadataLevel.FullMetadata;
        }

        private static bool IsMinimal(ODataMetadataLevel level)
        {
            return level == ODataMetadataLevel.MinimalMetadata;
        }
    }
}
