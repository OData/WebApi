// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class ActionLinkBuilderTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FollowsConventions_IsSpecifiedValue(bool value)
        {
            // Arrange
            ActionLinkBuilder builder = new ActionLinkBuilder((a) => { throw new NotImplementedException(); },
                followsConventions: value);

            // Act
            bool followsConventions = builder.FollowsConventions;

            // Assert
            Assert.Equal(value, followsConventions);
        }
    }
}
