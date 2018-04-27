// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.Test.AspNet.OData.Builder
{
    internal class MockNavigationSourceLinkBuilderAnnotation : NavigationSourceLinkBuilderAnnotation
    {
        public SelfLinkBuilder<Uri> IdLinkBuilder { get; set; }

        public SelfLinkBuilder<Uri> EditLinkBuilder { get; set; }

        public SelfLinkBuilder<Uri> ReadLinkBuilder { get; set; }

        public Func<ResourceContext, IEdmNavigationProperty, ODataMetadataLevel, Uri> NavigationLinkBuilder { get; set; }

        public override Uri BuildEditLink(ResourceContext instanceContext, ODataMetadataLevel metadataLevel, Uri idLink)
        {
            if (EditLinkBuilder != null)
            {
                return EditLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildIdLink(ResourceContext instanceContext, ODataMetadataLevel metadataLevel)
        {
            if (IdLinkBuilder != null)
            {
                return IdLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildReadLink(ResourceContext instanceContext, ODataMetadataLevel metadataLevel, Uri editLink)
        {
            if (ReadLinkBuilder != null)
            {
                return ReadLinkBuilder.Factory(instanceContext);
            }

            return null;
        }

        public override Uri BuildNavigationLink(ResourceContext context, IEdmNavigationProperty navigationProperty, ODataMetadataLevel metadataLevel)
        {
            if (NavigationLinkBuilder != null)
            {
                return NavigationLinkBuilder(context, navigationProperty, metadataLevel);
            }

            return null;
        }
    }
}
