// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.TestUtil;
using System.Web.UI;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class OutputCacheAttributeTest
    {
        [Fact]
        public void CacheProfileProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "CacheProfile", String.Empty);
        }

        [Fact]
        public void CacheSettingsProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute()
            {
                CacheProfile = "SomeProfile",
                Duration = 50,
                Location = OutputCacheLocation.Downstream,
                NoStore = true,
                SqlDependency = "SomeSqlDependency",
                VaryByContentEncoding = "SomeContentEncoding",
                VaryByCustom = "SomeCustom",
                VaryByHeader = "SomeHeader",
                VaryByParam = "SomeParam",
            };

            // Act
            OutputCacheParameters cacheSettings = attr.CacheSettings;

            // Assert
            Assert.Equal("SomeProfile", cacheSettings.CacheProfile);
            Assert.Equal(50, cacheSettings.Duration);
            Assert.Equal(OutputCacheLocation.Downstream, cacheSettings.Location);
            Assert.Equal(true, cacheSettings.NoStore);
            Assert.Equal("SomeSqlDependency", cacheSettings.SqlDependency);
            Assert.Equal("SomeContentEncoding", cacheSettings.VaryByContentEncoding);
            Assert.Equal("SomeCustom", cacheSettings.VaryByCustom);
            Assert.Equal("SomeHeader", cacheSettings.VaryByHeader);
            Assert.Equal("SomeParam", cacheSettings.VaryByParam);
        }

        [Fact]
        public void DurationProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestInt32Property(attr, "Duration", 10, 20);
        }

        [Fact]
        public void LocationProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestPropertyValue(attr, "Location", OutputCacheLocation.ServerAndClient);
        }

        [Fact]
        public void NoStoreProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestBooleanProperty(attr, "NoStore", false /* initialValue */, false /* testDefaultValue */);
        }

        [Fact]
        public void OnResultExecutingThrowsIfFilterContextIsNull()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnResultExecuting(null); }, "filterContext");
        }

        [Fact]
        public void SqlDependencyProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "SqlDependency", String.Empty);
        }

        [Fact]
        public void VaryByContentEncodingProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByContentEncoding", String.Empty);
        }

        [Fact]
        public void VaryByCustomProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByCustom", String.Empty);
        }

        [Fact]
        public void VaryByHeaderProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByHeader", String.Empty);
        }

        [Fact]
        public void VaryByParamProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByParam", "*");
        }

        [Fact]
        public void OutputCacheDoesNotExecuteIfInChildAction()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();
            Mock<ResultExecutingContext> context = new Mock<ResultExecutingContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act
            attr.OnResultExecuting(context.Object);

            // Assert
            context.Verify();
            context.Verify(c => c.Result, Times.Never());
        }

        // GetChildActionUniqueId

        [Fact]
        public void GetChildActionUniqueId_ReturnsRepeatableValueForIdenticalContext()
        {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context = new MockActionExecutingContext();

            // Act
            string result1 = attr.GetChildActionUniqueId(context.Object);
            string result2 = attr.GetChildActionUniqueId(context.Object);

            // Assert
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionDescriptorsUniqueId()
        {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context1 = new MockActionExecutingContext();
            context1.Setup(c => c.ActionDescriptor.UniqueId).Returns("1");
            var context2 = new MockActionExecutingContext();
            context2.Setup(c => c.ActionDescriptor.UniqueId).Returns("2");

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByCustom()
        {
            // Arrange
            var attr = new OutputCacheAttribute { VaryByCustom = "foo" };
            var context1 = new MockActionExecutingContext();
            context1.Setup(c => c.HttpContext.ApplicationInstance.GetVaryByCustomString(It.IsAny<HttpContext>(), "foo")).Returns("1");
            var context2 = new MockActionExecutingContext();
            context2.Setup(c => c.HttpContext.ApplicationInstance.GetVaryByCustomString(It.IsAny<HttpContext>(), "foo")).Returns("2");

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_AllParametersByDefault()
        {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            var context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_DoesNotVaryByActionParametersWhenVaryByParamIsNone()
        {
            // Arrange
            var attr = new OutputCacheAttribute { VaryByParam = "none" };
            var context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            var context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters()
        {
            // Arrange
            var attr = new OutputCacheAttribute { VaryByParam = "bar" };
            var context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            var context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(result1, result2);
        }

        class MockActionExecutingContext : Mock<ActionExecutingContext>
        {
            public Dictionary<string, object> ActionParameters = new Dictionary<string, object>();

            public MockActionExecutingContext()
            {
                Setup(c => c.ActionDescriptor.UniqueId).Returns("abc123");
                Setup(c => c.ActionParameters).Returns(() => ActionParameters);
            }
        }
    }
}
