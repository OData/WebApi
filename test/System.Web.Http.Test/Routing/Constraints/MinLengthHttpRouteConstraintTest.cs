// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class MinLengthHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueLengthIsLessThanMinLength()
        {
            MinLengthHttpRouteConstraint constraint = new MinLengthHttpRouteConstraint(3);
            bool match = TestValue(constraint, "12");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueLengthIsEqualToMinLength()
        {
            MinLengthHttpRouteConstraint constraint = new MinLengthHttpRouteConstraint(3);
            bool match = TestValue(constraint, "123");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueLengthIsGreaterThanMinLength()
        {
            MinLengthHttpRouteConstraint constraint = new MinLengthHttpRouteConstraint(3);
            bool match = TestValue(constraint, "1234");
            Assert.True(match);
        }
    }
}