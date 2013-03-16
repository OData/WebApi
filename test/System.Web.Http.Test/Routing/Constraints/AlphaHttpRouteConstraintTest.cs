// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class AlphaHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueContainsNumber()
        {
            AlphaHttpRouteConstraint constraint = new AlphaHttpRouteConstraint();
            bool match = TestValue(constraint, "abc123");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueContainsNonAlphaNum()
        {
            AlphaHttpRouteConstraint constraint = new AlphaHttpRouteConstraint();
            bool match = TestValue(constraint, "abc_");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsAlpha()
        {
            AlphaHttpRouteConstraint constraint = new AlphaHttpRouteConstraint();
            bool match = TestValue(constraint, "abcABC");
            Assert.True(match);
        }
    }
}
