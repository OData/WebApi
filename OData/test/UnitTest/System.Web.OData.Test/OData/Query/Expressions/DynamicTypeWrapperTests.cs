// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.OData.Query.Expressions;
using Microsoft.TestCommon;

namespace System.Web.OData.Test.OData.Query.Expressions
{
    public class DynamicTypeWrapperTests
    {
        [Fact]
        public void CanSetProperty()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new DynamicTypeWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            var actual = wrapper.Values[propName];

            Assert.Equal(expectedValue, actual);
        }

        [Fact]
        public void CanTryGetProperty()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new DynamicTypeWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            object actual;
            Assert.True(wrapper.TryGetPropertyValue(propName, out actual));

            Assert.Equal(expectedValue, actual);
        }

        [Fact]
        public void CanEqualWrappers()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new DynamicTypeWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            var wrapper2 = new DynamicTypeWrapper();
            wrapper2.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            Assert.Equal(wrapper, wrapper2);
        }

        [Fact]
        public void GetHashCodeEqualForEqualWrappers()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new DynamicTypeWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            var wrapper2 = new DynamicTypeWrapper();
            wrapper2.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            Assert.Equal(wrapper.GetHashCode(), wrapper2.GetHashCode());
        }
    }
}
