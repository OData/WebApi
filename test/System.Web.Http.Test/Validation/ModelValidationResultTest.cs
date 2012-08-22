// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Http.Validation
{
    public class ModelValidationResultTest
    {
        [Fact]
        public void MemberNameProperty()
        {
            // Arrange
            ModelValidationResult result = new ModelValidationResult();

            // Act & assert
            MemberHelper.TestStringProperty(result, "MemberName", String.Empty);
        }

        [Fact]
        public void MessageProperty()
        {
            // Arrange
            ModelValidationResult result = new ModelValidationResult();

            // Act & assert
            MemberHelper.TestStringProperty(result, "Message", String.Empty);
        }
    }
}
