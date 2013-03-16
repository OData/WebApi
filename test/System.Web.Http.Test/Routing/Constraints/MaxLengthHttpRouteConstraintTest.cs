// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class MaxLengthHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueLengthIsGreaterThanMaxLength()
        {
            MaxLengthHttpRouteConstraint constraint = new MaxLengthHttpRouteConstraint(3);
            bool match = TestValue(constraint, "1234");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueLengthIsLessThanMaxLength()
        {
            MaxLengthHttpRouteConstraint constraint = new MaxLengthHttpRouteConstraint(3);
            bool match = TestValue(constraint, "1");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueLengthIsEqualToMaxLength()
        {
            MaxLengthHttpRouteConstraint constraint = new MaxLengthHttpRouteConstraint(3);
            bool match = TestValue(constraint, "123");
            Assert.True(match);
        }
    }
}