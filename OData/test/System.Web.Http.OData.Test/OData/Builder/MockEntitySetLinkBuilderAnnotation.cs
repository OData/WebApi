// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    internal class MockEntitySetLinkBuilderAnnotation : EntitySetLinkBuilderAnnotation
    {
        public SelfLinkBuilder<string> IdLinkBuilder { get; set; }

        public SelfLinkBuilder<Uri> EditLinkBuilder { get; set; }

        public SelfLinkBuilder<Uri> ReadLinkBuilder { get; set; }

        public Func<FeedContext, Uri> FeedSelfLinkBuilder { get; set; }

        public Func<EntityInstanceContext, IEdmNavigationProperty, ODataMetadataLevel, Uri> NavigationLinkBuilder { get; set; }


        public override Uri BuildFeedSelfLink(FeedContext context)
        {
            if (FeedSelfLinkBuilder != null)
            {
                return FeedSelfLinkBuilder(context);
            }
            return null;
        }

        public override Uri BuildEditLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel, string idLink)
        {
            if (EditLinkBuilder != null)
            {
                return EditLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override string BuildIdLink(EntityInstanceContext instanceContext, ODataMetadataLevel metadataLevel)
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
