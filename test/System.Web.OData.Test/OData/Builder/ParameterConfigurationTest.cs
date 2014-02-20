// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Builder.TestModels;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class ParameterConfigurationTest
    {
        [Fact]
        public void BindingParameterConfigurationThrowsWhenParameterTypeIsNotEntity()
        { 
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<Address>();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                BindingParameterConfiguration configuration = new BindingParameterConfiguration("name", builder.GetTypeConfigurationOrNull(typeof(Address)), true);
            });
            Assert.True(exception.Message.Contains(string.Format("'{0}'", typeof(Address).FullName)));
            Assert.Equal("parameterType", exception.ParamName);
        }

        [Fact]
        public void NonbindingParameterConfigurationThrowsWhenParameterTypeIsEntity()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                NonbindingParameterConfiguration configuration = new NonbindingParameterConfiguration("name", builder.GetTypeConfigurationOrNull(typeof(Customer)));
            });
            Assert.True(exception.Message.Contains(string.Format("'{0}'", typeof(Customer).FullName)));
            Assert.Equal("parameterType", exception.ParamName);
        }
    }
}
