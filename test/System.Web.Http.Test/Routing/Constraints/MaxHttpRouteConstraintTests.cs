// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class MaxHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsGreaterThanMax()
        {
            MaxHttpRouteConstraint constraint = new MaxHttpRouteConstraint("3");
            bool match = TestValue(constraint, 5);
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsLessThanMax()
        {
            MaxHttpRouteConstraint constraint = new MaxHttpRouteConstraint("3");
            bool match = TestValue(constraint, 1);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsEqualToMax()
        {
            MaxHttpRouteConstraint constraint = new MaxHttpRouteConstraint("3");
            bool match = TestValue(constraint, 3);
            Assert.True(match);
        }
    }
}