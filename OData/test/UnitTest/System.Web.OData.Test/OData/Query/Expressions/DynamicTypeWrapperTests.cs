using Microsoft.TestCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Query.Expressions;

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
            wrapper.SetPropertyValue(propName, expectedValue);

            var actual = wrapper.GetPropertyValue(propName);

            Assert.Equal(expectedValue, actual);
        }

        [Fact]
        public void CanTryGetProperty()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new DynamicTypeWrapper();
            wrapper.SetPropertyValue(propName, expectedValue);

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
            wrapper.SetPropertyValue(propName, expectedValue);

            var wrapper2 = new DynamicTypeWrapper();
            wrapper2.SetPropertyValue(propName, expectedValue);

            Assert.Equal(wrapper, wrapper2);
        }

        [Fact]
        public void GetHashCodeEqualForEqualWrappers()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new DynamicTypeWrapper();
            wrapper.SetPropertyValue(propName, expectedValue);

            var wrapper2 = new DynamicTypeWrapper();
            wrapper2.SetPropertyValue(propName, expectedValue);

            Assert.Equal(wrapper.GetHashCode(), wrapper2.GetHashCode());
        }
    }
}
