// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Builder
{
    public class NavigationPropertyBinding
    {
        private Func<EntityInstanceContext, string> _linkFactory;

        public NavigationPropertyBinding(NavigationPropertyConfiguration navigationProperty, IEntitySetConfiguration entitySet) 
        {
            NavigationProperty = navigationProperty;
            EntitySet = entitySet;
            _linkFactory = null;
        }

        public NavigationPropertyConfiguration NavigationProperty { get; private set; } 
        
        public IEntitySetConfiguration EntitySet { get; private set; }
       
        public void HasLinkFactory(Func<EntityInstanceContext, string> linkFactoryParameter)
        {
            _linkFactory = linkFactoryParameter;
        }

        public string GetLink(EntityInstanceContext entityContext)
        {
            if (_linkFactory == null)
            {
                throw Error.NotSupported(SRResources.CreatingLinksByConventionNotSupported);
            }

            return _linkFactory(entityContext);
        }
    }
}
