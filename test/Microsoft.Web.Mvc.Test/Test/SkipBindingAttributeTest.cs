// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.Test
{
    public class SkipBindingAttributeTest
    {
        [Fact]
        public void GetBinderReturnsModelBinderWhichReturnsNull()
        {
            // Arrange
            CustomModelBinderAttribute attr = new SkipBindingAttribute();
            IModelBinder binder = attr.GetBinder();

            // Act
            object result = binder.BindModel(null, null);

            // Assert
            Assert.Null(result);
        }
    }
}
