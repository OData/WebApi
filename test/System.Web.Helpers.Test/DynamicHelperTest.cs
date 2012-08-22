// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Dynamic;
using Microsoft.Internal.Web.Utils;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    public class DynamicHelperTest
    {
        [Fact]
        public void TryGetMemberValueReturnsValueIfBinderIsNotCSharp()
        {
            // Arrange
            var mockMemberBinder = new MockMemberBinder("Foo");
            var dynamic = new DynamicWrapper(new { Foo = "Bar" });

            // Act
            object value;
            bool result = DynamicHelper.TryGetMemberValue(dynamic, mockMemberBinder, out value);

            // Assert
            Assert.Equal(value, "Bar");
        }

        private class MockMemberBinder : GetMemberBinder
        {
            public MockMemberBinder(string name)
                : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }
    }
}
