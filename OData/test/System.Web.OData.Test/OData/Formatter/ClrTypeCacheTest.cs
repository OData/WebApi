// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter
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
