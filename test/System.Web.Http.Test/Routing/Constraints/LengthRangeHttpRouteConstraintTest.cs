// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class LengthRangeHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueLengthIsGreaterThanMaximumLength()
        {
            LengthHttpRouteConstraint constraint = new LengthHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, "123456");
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueLengthIsLessThanMinimumLength()
        {
            LengthHttpRouteConstraint constraint = new LengthHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, "12");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueLengthIsBetweenMinimumAndMaximumLength()
        {
            LengthHttpRouteConstraint constraint = new LengthHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, "1234");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueLengthIsEqualToMaximumLength()
        {
            LengthHttpRouteConstraint constraint = new LengthHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, "12345");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueLengthIsEqualToMinimumLength()
        {
            LengthHttpRouteConstraint constraint = new LengthHttpRouteConstraint(3, 5);
            bool match = TestValue(constraint, "123");
            Assert.True(match);
        }
    }
}