// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// <see cref="NavigationSourceLinkBuilderAnnotation" /> is a class used to annotate an <see cref="IEdmNavigationSource" /> inside an <see cref="IEdmModel" />
    /// with information about how to build links related to that navigation source.
    /// </summary>
    public class NavigationSourceLinkBuilderAnnotation
    {
        private readonly SelfLinkBuilder<Uri> _idLinkBuilder;
        private readonly SelfLinkBuilder<Uri> _editLinkBuilder;
        private readonly SelfLinkBuilder<Uri> _readLinkBuilder;

        private readonly Dictionary<IEdmNavigationProperty, NavigationLinkBuilder> _navigationPropertyLinkBuilderLookup = new Dictionary<IEdmNavigationProperty, NavigationLinkBuilder>();
        private readonly string _navigationSourceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSourceLinkBuilderAnnotation" /> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public NavigationSourceLinkBuilderAnnotation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSourceLinkBuilderAnnotation"/> class.
        /// </summary>
        /// <param name="navigationSource">The navigation source for which the link builder is being constructed.</param>
        /// <param name="model">The EDM model that this navigation source belongs to.</param>
        /// <remarks>This constructor creates a link builder that generates URL's that follow OData conventions for the given navigation source.</remarks>
        public NavigationSourceLinkBuilderAnnotation(IEdmNavigationSource navigationSource, IEdmModel model)
        {
            if (navigationSource == null)
            {
                throw Error.ArgumentNull("navigationSource");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            IEdmEntityType elementType = navigationSource.EntityType();
            IEnumerable<IEdmEntityType> derivedTypes = model.FindAllDerivedTypes(elementType).Cast<IEdmEntityType>();

            // Add navigation link builders for all navigation properties of entity.
            foreach (IEdmNavigationProperty navigationProperty in elementType.NavigationProperties())
            {
                Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkFactory =
                    (entityInstanceContext, navProperty) => entityInstanceContext.GenerateNavigationPropertyLink(navProperty, includeCast: false);
                AddNavigationPropertyLinkBuilder(navigationProperty, new NavigationLinkBuilder(navigationLinkFactory, followsConventions: true));
            }

            // Add navigation link builders for all navigation properties in derived types.
            bool derivedTypesDefineNavigationProperty = false;
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

            _navigationSourceName = navigationSource.Name;

            Func<EntityInstanceContext, Uri> selfLinkFactory =
                (entityInstanceContext) => entityInstanceContext.GenerateSelfLink(includeCast: derivedTypesDefineNavigationProperty);
            _idLinkBuilder = new SelfLinkBuilder<Uri>(selfLinkFactory, followsConventions: true);
        }

        /// <summary>
        /// Constructs an instance of an <see cref="NavigationSourceLinkBuilderAnnotation" /> class.
        /// </summary>
        /// <param name="navigationSource">The navigation source for which the link builder is being constructed.</param>
        /// <param name="idLinkBuilder">The ID link builder which is used to build the ID link.</param>
        /// <param name="editLinkBuilder">The Edit link builder which is used to build the Edit link.</param>
        /// <param name="readLinkBuilder">The Read link builder which is used to build the Read link.</param>
        public NavigationSourceLinkBuilderAnnotation(
            IEdmNavigationSource navigationSource,
            SelfLinkBuilder<Uri> idLinkBuilder,
            SelfLinkBuilder<Uri> editLinkBuilder,
            SelfLinkBuilder<Uri> readLinkBuilder)
        {
            if (navigationSource == null)
            {
                throw Error.ArgumentNull("navigationSource");
            }

            _navigationSourceName = navigationSource.Name;
            _idLinkBuilder = idLinkBuilder;
            _editLinkBuilder = editLinkBuilder;
            _readLinkBuilder = readLinkBuilder;
        }

        /// <summary>
        /// Constructs an instance of an <see cref="NavigationSourceLinkBuilderAnnotation" /> from an <see cref="NavigationSourceConfiguration" />.
        /// </summary>
        public NavigationSourceLinkBuilderAnnotation(NavigationSourceConfiguration navigationSource)
        {
            if (navigationSource == null)
            {
                throw Error.ArgumentNull("navigationSource");
            }

            _navigationSourceName = navigationSource.Name;
            _idLinkBuilder = navigationSource.GetIdLink();
            _editLinkBuilder = navigationSource.GetEditLink();
            _readLinkBuilder = navigationSource.GetReadLink();
        }

        /// <summary>
        /// Register a link builder for a <see cref="IEdmNavigationProperty" /> that navigates from Entities in this navigation source. 
        /// </summary>
        public void AddNavigationPropertyLinkBuilder(IEdmNavigationProperty navigationProperty, NavigationLinkBuilder linkBuilder)
        {
            _navigationPropertyLinkBuilderLookup[navigationProperty] = linkBuilder;
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
        public virtual Uri BuildIdLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
        {
            if (instanceContext == null)
            {
                throw Error.ArgumentNull("instanceContext");
            }

            if (_idLinkBuilder == null)
            {
                if (metadataLevel == ODataMetadataLevel.Default)
                {
                    throw Error.InvalidOperation(SRResources.NoIdLinkFactoryFound, _navigationSourceName);
                }

                return null;
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
        public virtual Uri BuildEditLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel, Uri idLink)
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
                    return idLink;
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
                if (metadataLevel == ODataMetadataLevel.Default)
                {
                    throw Error.Argument("navigationProperty", SRResources.NoNavigationLinkFactoryFound, navigationProperty.Name, navigationProperty.DeclaringEntityType(), _navigationSourceName);
                }

                return null;
            }

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
