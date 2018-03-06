// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.Test.AspNet.OData.Common;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.Formatter;
using Moq;
using Xunit;

namespace Microsoft.Test.AspNet.OData
{
    public class FromODataUriTest
    {
        [Fact]
        public void GetBinding_ReturnsSameBindingTypeAsODataModelBinderProvider()
        {
            var config = RoutingConfigurationFactory.Create();
            Type parameterType = typeof(Guid);
            Mock<ParameterInfo> parameterInfoMock = new Mock<ParameterInfo>();
            parameterInfoMock.Setup(info => info.ParameterType).Returns(parameterType);

            ReflectedHttpParameterDescriptor parameter = new ReflectedHttpParameterDescriptor();
            parameter.Configuration = config;
            parameter.ParameterInfo = parameterInfoMock.Object;

            HttpParameterBinding binding = new FromODataUriAttribute().GetBinding(parameter);

            ModelBinderParameterBinding modelBinding = Assert.IsType<ModelBinderParameterBinding>(binding);
            Assert.Equal(new ODataModelBinderProvider().GetBinder(config, parameterType).GetType(), modelBinding.Binder.GetType());
        }

        [Fact]
        public void GetBinding_DoesnotThrowForNonPrimitives()
        {
            var config = RoutingConfigurationFactory.Create();
            Type parameterType = typeof(FormatterOrder);
            Mock<ParameterInfo> parameterInfoMock = new Mock<ParameterInfo>();
            parameterInfoMock.Setup(info => info.ParameterType).Returns(parameterType);
            ReflectedHttpParameterDescriptor parameter = new ReflectedHttpParameterDescriptor();
            parameter.Configuration = config;
            parameter.ParameterInfo = parameterInfoMock.Object;

            ExceptionAssert.DoesNotThrow(() => new FromODataUriAttribute().GetBinding(parameter));
        }
    }
}
#endif
