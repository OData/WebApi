// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc.Test
{
    public class DataAnnotationsModelMetadataProviderTest : DataAnnotationsModelMetadataProviderTestBase
    {
        protected override AssociatedMetadataProvider MakeProvider()
        {
            return new DataAnnotationsModelMetadataProvider();
        }
    }
}
