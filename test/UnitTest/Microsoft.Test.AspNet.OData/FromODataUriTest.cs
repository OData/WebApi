// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.Test.AspNet.OData.Formatter;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;

namespace Microsoft.Test.AspNet.OData
{
    public class FromODataUriTest
    {
        [Fact]
        public void GetBinding_ReturnsSameBindingTypeAsODataModelBinderProvider()
        {
            HttpConfiguration config = new HttpConfiguration();
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
            HttpConfiguration config = new HttpConfiguration();
            Type parameterType = typeof(FormatterOrder);
            Mock<ParameterInfo> parameterInfoMock = new Mock<ParameterInfo>();
            parameterInfoMock.Setup(info => info.ParameterType).Returns(parameterType);
            ReflectedHttpParameterDescriptor parameter = new ReflectedHttpParameterDescriptor();
            parameter.Configuration = config;
            parameter.ParameterInfo = parameterInfoMock.Object;

            Assert.DoesNotThrow(() => new FromODataUriAttribute().GetBinding(parameter));
        }
    }
}
