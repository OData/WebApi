// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class LongHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotLong()
        {
            LongHttpRouteConstraint constraint = new LongHttpRouteConstraint();
            bool match = TestValue(constraint, 3.14M);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsNotParsableAsLong()
        {
            LongHttpRouteConstraint constraint = new LongHttpRouteConstraint();
            bool match = TestValue(constraint, "43.567");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsLong()
        {
            LongHttpRouteConstraint constraint = new LongHttpRouteConstraint();
            bool match = TestValue(constraint, 42L);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsLongString()
        {
            LongHttpRouteConstraint constraint = new LongHttpRouteConstraint();
            bool match = TestValue(constraint, "123456790");
            Assert.True(match);
        }
    }
}