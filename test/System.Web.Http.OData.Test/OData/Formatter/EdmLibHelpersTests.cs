// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class EdmLibHelpersTests
    {
        [Theory]
        [InlineData(typeof(Customer), "Customer")]
        [InlineData(typeof(int), "Int32")]
        [InlineData(typeof(IEnumerable<int>), "IEnumerable_1OfInt32")]
        [InlineData(typeof(IEnumerable<Func<int, string>>), "IEnumerable_1OfFunc_2OfInt32_String")]
        [InlineData(typeof(List<Func<int, string>>), "List_1OfFunc_2OfInt32_String")]
        public void EdmFullName(Type clrType, string expectedName)
        {
            Assert.Equal(expectedName, clrType.EdmName());
        }
    }
}
