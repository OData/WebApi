// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc.Test
{
    public class CachedDataAnnotationsModelMetadataProviderTest : DataAnnotationsModelMetadataProviderTestBase
    {
        protected override AssociatedMetadataProvider MakeProvider()
        {
            return new CachedDataAnnotationsModelMetadataProvider();
        }
    }
}
