﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class FastPropertyAccessorTest
    {
        public class MyProps
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }

        [Fact]
        public void GetterWorksForValueType()
        {
            // Arrange
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("IntProp"));
            mine.IntProp = 4;

            // Assert
            Assert.Equal(4, accessor.GetValue(mine));
        }

        [Fact]
        public void SetterWorksForValueType()
        {
            // Arrange
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("IntProp"));
            mine.IntProp = 4;

            // Act
            accessor.SetValue(mine, 3);

            // Assert
            Assert.Equal(3, accessor.GetValue(mine));
            Assert.Equal(3, mine.IntProp);
        }

        [Fact]
        public void GetterWorksForReferenceType()
        {
            // Arrange
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("StringProp"));
            mine.StringProp = "*4";

            // Assert
            Assert.Equal("*4", accessor.GetValue(mine));
        }

        [Fact]
        public void SetterWorksForReferenceType()
        {
            // Arrange
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("StringProp"));
            mine.StringProp = "*4";

            // Act
            accessor.SetValue(mine, "#3");

            // Assert
            Assert.Equal("#3", accessor.GetValue(mine));
            Assert.Equal("#3", mine.StringProp);
        }
    }
}
