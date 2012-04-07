// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class HttpParameterDescriptorTest
    {
        [Fact]
        public void Default_Constructor()
        {
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor>().Object;

            Assert.Null(parameterDescriptor.ParameterName);
            Assert.Null(parameterDescriptor.ParameterType);
            Assert.Null(parameterDescriptor.Configuration);
            Assert.Null(parameterDescriptor.Prefix);
            Assert.Null(parameterDescriptor.ModelBinderAttribute);
            Assert.Null(parameterDescriptor.ActionDescriptor);
            Assert.Null(parameterDescriptor.DefaultValue);
            Assert.NotNull(parameterDescriptor.Properties);
        }

        [Fact]
        public void Configuration_Property()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor> { CallBase = true }.Object;

            Assert.Reflection.Property<HttpParameterDescriptor, HttpConfiguration>(
                 instance: parameterDescriptor,
                 propertyGetter: pd => pd.Configuration,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: config);
        }

        [Fact]
        public void ActionDescriptor_Property()
        {
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor> { CallBase = true }.Object;
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;

            Assert.Reflection.Property<HttpParameterDescriptor, HttpActionDescriptor>(
                 instance: parameterDescriptor,
                 propertyGetter: pd => pd.ActionDescriptor,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: actionDescriptor);
        }

        [Fact]
        public void GetCustomAttributes_Returns_EmptyAttributes()
        {
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor> { CallBase = true }.Object;
            IEnumerable<object> attributes = parameterDescriptor.GetCustomAttributes<object>();

            Assert.Equal(0, attributes.Count());
        }

        [Fact]
        public void GetCustomAttributes_AttributeType_Returns_EmptyAttributes()
        {
            HttpParameterDescriptor parameterDescriptor = new Mock<HttpParameterDescriptor> { CallBase = true }.Object;
            IEnumerable<FromBodyAttribute> attributes = parameterDescriptor.GetCustomAttributes<FromBodyAttribute>();

            Assert.Equal(0, attributes.Count());
        }
    }
}
