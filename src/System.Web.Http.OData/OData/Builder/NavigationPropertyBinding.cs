// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Builder
{
    public class NavigationPropertyBinding
    {
        private Func<EntityInstanceContext, string> linkFactory;

        public NavigationPropertyBinding(NavigationPropertyConfiguration navigationProperty, IEntitySetConfiguration entitySet) 
        {
            this.NavigationProperty = navigationProperty;
            this.EntitySet = entitySet;
            this.linkFactory = null;
        }

        public NavigationPropertyConfiguration NavigationProperty { get; private set; } 
        
        public IEntitySetConfiguration EntitySet { get; private set; }
       
        public void HasLinkFactory(Func<EntityInstanceContext, string> linkFactoryParameter)
        {
            this.linkFactory = linkFactoryParameter;
        }

        public string GetLink(EntityInstanceContext entityContext)
        {
            if (this.linkFactory == null)
            {
                throw Error.NotSupported(SRResources.CreatingLinksByConventionNotSupported);
            }

            return linkFactory(entityContext);
        }
    }
}
