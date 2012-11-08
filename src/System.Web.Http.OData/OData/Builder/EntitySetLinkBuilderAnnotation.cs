// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    internal class EntitySetLinkBuilderAnnotation : IEntitySetLinkBuilder
    {
        private readonly Func<FeedContext, Uri> _feedSelfLinkBuilder;
        private readonly Func<EntityInstanceContext, string> _idLinkBuilder;
        private readonly Func<EntityInstanceContext, Uri> _editLinkBuilder;
        private readonly Func<EntityInstanceContext, Uri> _readLinkBuilder;
        private readonly IDictionary<IEdmNavigationProperty, Func<EntityInstanceContext, IEdmNavigationProperty, Uri>> _navigationPropertyLinkBuilderLookup;
        private readonly IEntitySetConfiguration _entitySet;

        /// <summary>
        /// This constructor is used for unit testing purposes only
        /// </summary>
        public EntitySetLinkBuilderAnnotation()
        {
        }

        public EntitySetLinkBuilderAnnotation(IEntitySetConfiguration entitySet)
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
            _navigationPropertyLinkBuilderLookup = new Dictionary<IEdmNavigationProperty, Func<EntityInstanceContext, IEdmNavigationProperty, Uri>>();
        }

        public void AddNavigationPropertyLinkBuilder(IEdmNavigationProperty navigationProperty, Func<EntityInstanceContext, IEdmNavigationProperty, Uri> linkBuilder)
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

        public virtual string BuildIdLink(EntityInstanceContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (_idLinkBuilder == null)
            {
                return BuildEditLink(context).ToString();
            }

            return _idLinkBuilder(context);
        }

        public virtual Uri BuildEditLink(EntityInstanceContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (_editLinkBuilder == null)
            {
                throw Error.InvalidOperation(SRResources.NoEditLinkFactoryFound, _entitySet.Name);
            }

            return _editLinkBuilder(context);
        }

        public virtual Uri BuildReadLink(EntityInstanceContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (_readLinkBuilder == null)
            {
                return BuildEditLink(context);
            }

            return _readLinkBuilder(context);
        }

        public virtual Uri BuildNavigationLink(EntityInstanceContext context, IEdmNavigationProperty navigationProperty)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            Func<EntityInstanceContext, IEdmNavigationProperty, Uri> navigationLinkBuilderFunc;
            if (!_navigationPropertyLinkBuilderLookup.TryGetValue(navigationProperty, out navigationLinkBuilderFunc))
            {
                throw Error.InvalidOperation(SRResources.NoNavigationLinkFactoryFound, navigationProperty.Name, navigationProperty.DeclaringEntityType(), _entitySet.Name);
            }

            Contract.Assert(navigationLinkBuilderFunc != null);
            return navigationLinkBuilderFunc(context, navigationProperty);
        }
    }
}
