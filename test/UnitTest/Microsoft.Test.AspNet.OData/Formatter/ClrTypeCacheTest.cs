// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;

namespace Microsoft.Test.AspNet.OData.Formatter
{
    public class ClrTypeCacheTest
    {
        [Fact]
        public void GetEdmType_Returns_CachedInstance()
        {
            ClrTypeCache cache = new ClrTypeCache();
            IEdmModel model = EdmCoreModel.Instance;

            IEdmTypeReference edmType1 = cache.GetEdmType(typeof(int), model);
            IEdmTypeReference edmType2 = cache.GetEdmType(typeof(int), model);

            Assert.NotNull(edmType1);
            Assert.Same(edmType1, edmType2);
        }
    }
}
