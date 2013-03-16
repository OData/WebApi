// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class IntHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotInt()
        {
            IntHttpRouteConstraint constraint = new IntHttpRouteConstraint();
            bool match = TestValue(constraint, 3.14M);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsNotParsableAsInt()
        {
            IntHttpRouteConstraint constraint = new IntHttpRouteConstraint();
            bool match = TestValue(constraint, "43.567");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsInt()
        {
            IntHttpRouteConstraint constraint = new IntHttpRouteConstraint();
            bool match = TestValue(constraint, 42);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsIntString()
        {
            IntHttpRouteConstraint constraint = new IntHttpRouteConstraint();
            bool match = TestValue(constraint, "42");
            Assert.True(match);
        }
    }
}