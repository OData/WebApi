// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    internal class EntitySetLinkBuilderAnnotation
    {
        private readonly Func<FeedContext, Uri> _feedSelfLinkBuilder;

        private readonly SelfLinkBuilder<string> _idLinkBuilder;
        private readonly SelfLinkBuilder<Uri> _editLinkBuilder;
        private readonly SelfLinkBuilder<Uri> _readLinkBuilder;

        private readonly Dictionary<IEdmNavigationProperty, NavigationLinkBuilder> _navigationPropertyLinkBuilderLookup;
        private readonly EntitySetConfiguration _entitySet;

        // This constructor is used for unit testing purposes only
        public EntitySetLinkBuilderAnnotation()
        {
            _navigationPropertyLinkBuilderLookup = new Dictionary<IEdmNavigationProperty, NavigationLinkBuilder>();
        }

        public EntitySetLinkBuilderAnnotation(EntitySetConfiguration entitySet)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            _entitySet = entitySet;
            _feedSelfLinkBuilder = entitySet.GetFeedSelfLink();
            _idLinkBuilder = entitySet.GetIdLink();

            _editLinkBuilder = entitySet.GetEditLink();
            _readLinkBuilder = entitySet.GetReadLink();
            _navigationPropertyLinkBuilderLookup = new Dictionary<IEdmNavigationProperty, NavigationLinkBuilder>();
        }

        public void AddNavigationPropertyLinkBuilder(IEdmNavigationProperty navigationProperty, NavigationLinkBuilder linkBuilder)
        {
            _navigationPropertyLinkBuilderLookup[navigationProperty] = linkBuilder;
        }

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

        public virtual EntitySelfLinks BuildEntitySelfLinks(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
        {
            EntitySelfLinks selfLinks = new EntitySelfLinks();
            selfLinks.IdLink = BuildIdLink(instanceContext, metadataLevel);
            selfLinks.EditLink = BuildEditLink(instanceContext, metadataLevel, selfLinks.IdLink);
            selfLinks.ReadLink = BuildReadLink(instanceContext, metadataLevel, selfLinks.EditLink);
            return selfLinks;
        }

        public virtual string BuildIdLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
        {
            if (instanceContext == null)
            {
                throw Error.ArgumentNull("instanceContext");
            }

            if (_idLinkBuilder == null)
            {
                throw Error.InvalidOperation(SRResources.NoIdLinkFactoryFound, _entitySet.Name);
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
            else
            {
                if (IsDefaultOrFull(metadataLevel) ||
                    (IsMinimal(metadataLevel) && !_editLinkBuilder.FollowsConventions))
                {
                    Uri generatedEditLink = _editLinkBuilder.Factory(instanceContext);
                    if (generatedEditLink != null && generatedEditLink.Equals(new Uri(idLink)))
                    {
                        // edit link is the same as id link. emit only in default metadata mode.
                        if (metadataLevel == ODataMetadataLevel.Default)
                        {
                            return new Uri(idLink);
                        }
                    }
                    else
                    {
                        return generatedEditLink;
                    }
                }
            }

            // client can infer it and didn't ask for it.
            return null;
        }

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
            else
            {
                if (IsDefaultOrFull(metadataLevel) ||
                    (IsMinimal(metadataLevel) && !_readLinkBuilder.FollowsConventions))
                {
                    Uri generatedReadLink = _readLinkBuilder.Factory(instanceContext);
                    if (editLink == generatedReadLink)
                    {
                        // read link is the same as edit link. emit only in default metadata mode.
                        if (metadataLevel == ODataMetadataLevel.Default)
                        {
                            return editLink;
                        }
                    }
                    else
                    {
                        return generatedReadLink;
                    }
                }
            }

            // client can infer it and didn't ask for it.
            return null;
        }

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
                throw Error.Argument("navigationProperty", SRResources.NoNavigationLinkFactoryFound, navigationProperty.Name, navigationProperty.DeclaringEntityType(), _entitySet.Name);
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
