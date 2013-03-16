// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class MinHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsLessThanMin()
        {
            MinHttpRouteConstraint constraint = new MinHttpRouteConstraint(3);
            bool match = TestValue(constraint, 2);
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsEqualToMin()
        {
            MinHttpRouteConstraint constraint = new MinHttpRouteConstraint(3);
            bool match = TestValue(constraint, 3);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsGreaterThanMin()
        {
            MinHttpRouteConstraint constraint = new MinHttpRouteConstraint(3);
            bool match = TestValue(constraint, 5);
            Assert.True(match);
        }
    }
}