// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class DecimalHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotDecimal()
        {
            DecimalHttpRouteConstraint constraint = new DecimalHttpRouteConstraint();
            bool match = TestValue(constraint, false);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsNotParsableAsDecimal()
        {
            DecimalHttpRouteConstraint constraint = new DecimalHttpRouteConstraint();
            bool match = TestValue(constraint, "what a nice day!");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsDecimal()
        {
            DecimalHttpRouteConstraint constraint = new DecimalHttpRouteConstraint();
            bool match = TestValue(constraint, 3.14M);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsDecimalString()
        {
            DecimalHttpRouteConstraint constraint = new DecimalHttpRouteConstraint();
            bool match = TestValue(constraint, "3.14");
            Assert.True(match);
        }
    }
}