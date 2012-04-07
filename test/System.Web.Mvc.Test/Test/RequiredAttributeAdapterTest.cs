// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class RequiredAttributeAdapterTest
    {
        [Fact]
        public void ClientRulesWithRequiredAttribute()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(string), "Length");
            var context = new ControllerContext();
            var attribute = new RequiredAttribute();
            var adapter = new RequiredAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("required", rule.ValidationType);
            Assert.Empty(rule.ValidationParameters);
            Assert.Equal(@"The Length field is required.", rule.ErrorMessage);
        }
    }
}
