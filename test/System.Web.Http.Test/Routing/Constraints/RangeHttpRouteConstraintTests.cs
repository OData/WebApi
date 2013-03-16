// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class RangeHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsGreaterThanMaximum()
        {
            RangeHttpRouteConstraint constraint = new RangeHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, 6);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsLessThanMinimum()
        {
            RangeHttpRouteConstraint constraint = new RangeHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, 2);
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsBetweenMinimumAndMaximum()
        {
            RangeHttpRouteConstraint constraint = new RangeHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, 4);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsEqualToMaximum()
        {
            RangeHttpRouteConstraint constraint = new RangeHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, 5);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsEqualToMinimum()
        {
            RangeHttpRouteConstraint constraint = new RangeHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, 3);
            Assert.True(match);
        }
    }
}