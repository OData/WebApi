// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class CompareAttributeAdapterTest
    {
        [Fact]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesDisplayName()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(PropertyDisplayNameModel), "MyProperty");
            var context = new ControllerContext();
            var attribute = new System.ComponentModel.DataAnnotations.CompareAttribute("OtherProperty");
            var adapter = new CompareAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("'MyPropertyDisplayName' and 'OtherPropertyDisplayName' do not match.", rule.ErrorMessage);
        }

        [Fact]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesPropertyName()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(PropertyNameModel), "MyProperty");
            var context = new ControllerContext();
            var attribute = new System.ComponentModel.DataAnnotations.CompareAttribute("OtherProperty");
            var adapter = new CompareAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("'MyProperty' and 'OtherProperty' do not match.", rule.ErrorMessage);
        }

        private class PropertyDisplayNameModel
        {
            [DisplayName("MyPropertyDisplayName")]
            public string MyProperty { get; set; }

            [DisplayName("OtherPropertyDisplayName")]
            public string OtherProperty { get; set; }
        }

        private class PropertyNameModel
        {
            public string MyProperty { get; set; }

            public string OtherProperty { get; set; }
        }
    }
}