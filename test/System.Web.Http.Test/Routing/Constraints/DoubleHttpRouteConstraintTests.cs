// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class DoubleHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotDouble()
        {
            DoubleHttpRouteConstraint constraint = new DoubleHttpRouteConstraint();
            bool match = TestValue(constraint, false);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsNotParsableAsDouble()
        {
            DoubleHttpRouteConstraint constraint = new DoubleHttpRouteConstraint();
            bool match = TestValue(constraint, "what a nice day!");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsDouble()
        {
            DoubleHttpRouteConstraint constraint = new DoubleHttpRouteConstraint();
            bool match = TestValue(constraint, 3.14D);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsDoubleString()
        {
            DoubleHttpRouteConstraint constraint = new DoubleHttpRouteConstraint();
            bool match = TestValue(constraint, "3.14");
            Assert.True(match);
        }
    }
}