// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    internal class EntitySetLinkBuilderAnnotation : IEntitySetLinkBuilder
    {
        private readonly Func<EntityInstanceContext, string> _idLinkBuilderFunc;
        private readonly Func<EntityInstanceContext, Uri> _editLinkBuilderFunc;
        private readonly Func<EntityInstanceContext, Uri> _readLinkBuilderFunc;
        private readonly IDictionary<IEdmNavigationProperty, Func<EntityInstanceContext, IEdmNavigationProperty, Uri>> _navigationPropertyLinkBuilderFuncLookup;
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

            Func<EntityInstanceContext, string> idLinkBuilderFunc = entitySet.GetIdLink();
            Func<EntityInstanceContext, Uri> editLinkBuilderFunc = entitySet.GetEditLink();
            Func<EntityInstanceContext, Uri> readLinkBuilderFunc = entitySet.GetReadLink();

            _idLinkBuilderFunc = idLinkBuilderFunc;
            _editLinkBuilderFunc = editLinkBuilderFunc;
            _readLinkBuilderFunc = readLinkBuilderFunc;
            _navigationPropertyLinkBuilderFuncLookup = new Dictionary<IEdmNavigationProperty, Func<EntityInstanceContext, IEdmNavigationProperty, Uri>>();
        }

        public void AddNavigationPropertyLinkBuilder(IEdmNavigationProperty navigationProperty, Func<EntityInstanceContext, IEdmNavigationProperty, Uri> linkBuilder)
        {
            _navigationPropertyLinkBuilderFuncLookup[navigationProperty] = linkBuilder;
        }

        public virtual string BuildIdLink(EntityInstanceContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (_idLinkBuilderFunc == null)
            {
                return BuildEditLink(context).ToString();
            }

            return _idLinkBuilderFunc(context);
        }

        public virtual Uri BuildEditLink(EntityInstanceContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (_editLinkBuilderFunc == null)
            {
                throw Error.InvalidOperation(SRResources.NoEditLinkFactoryFound, _entitySet.Name);
            }

            return _editLinkBuilderFunc(context);
        }

        public virtual Uri BuildReadLink(EntityInstanceContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (_readLinkBuilderFunc == null)
            {
                return BuildEditLink(context);
            }

            return _readLinkBuilderFunc(context);
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
            if (!_navigationPropertyLinkBuilderFuncLookup.TryGetValue(navigationProperty, out navigationLinkBuilderFunc))
            {
                throw Error.InvalidOperation(SRResources.NoNavigationLinkFactoryFound, navigationProperty.Name, _entitySet.Name);
            }

            return _navigationPropertyLinkBuilderFuncLookup[navigationProperty](context, navigationProperty);
        }
    }
}
