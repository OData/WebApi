// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelClientValidationRuleTest
    {
        [Fact]
        public void ValidationParametersProperty()
        {
            // Arrange
            ModelClientValidationRule rule = new ModelClientValidationRule();

            // Act
            IDictionary<string, object> parameters = rule.ValidationParameters;

            // Assert
            Assert.NotNull(parameters);
            Assert.Empty(parameters);
        }

        [Fact]
        public void ValidationTypeProperty()
        {
            // Arrange
            ModelClientValidationRule rule = new ModelClientValidationRule();

            // Act & assert
            MemberHelper.TestStringProperty(rule, "ValidationType", String.Empty);
        }
    }
}
