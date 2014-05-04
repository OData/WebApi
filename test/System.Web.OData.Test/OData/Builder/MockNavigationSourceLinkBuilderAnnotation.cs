// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Formatter;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    internal class MockNavigationSourceLinkBuilderAnnotation : NavigationSourceLinkBuilderAnnotation
    {
        public SelfLinkBuilder<Uri> IdLinkBuilder { get; set; }

        public SelfLinkBuilder<Uri> EditLinkBuilder { get; set; }

        public SelfLinkBuilder<Uri> ReadLinkBuilder { get; set; }

        public Func<EntityInstanceContext, IEdmNavigationProperty, ODataMetadataLevel, Uri> NavigationLinkBuilder { get; set; }

        public override Uri BuildEditLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel, Uri idLink)
        {
            if (EditLinkBuilder != null)
            {
                return EditLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildIdLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
        {
            if (IdLinkBuilder != null)
            {
                return IdLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildReadLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel, Uri editLink)
        {
            if (ReadLinkBuilder != null)
            {
                return ReadLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildNavigationLink(EntityInstanceContext context, IEdmNavigationProperty navigationProperty, ODataMetadataLevel metadataLevel)
        {
            if (NavigationLinkBuilder != null)
            {
                return NavigationLinkBuilder(context, navigationProperty, metadataLevel);
            }

            return null;
        }
    }
}
