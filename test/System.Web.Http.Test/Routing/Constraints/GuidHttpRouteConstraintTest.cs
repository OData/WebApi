// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class GuidHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotGuid()
        {
            GuidHttpRouteConstraint constraint = new GuidHttpRouteConstraint();
            bool match = TestValue(constraint, false);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsNotParsableAsGuid()
        {
            GuidHttpRouteConstraint constraint = new GuidHttpRouteConstraint();
            bool match = TestValue(constraint, "this-is-not-a-guid");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsGuid()
        {
            GuidHttpRouteConstraint constraint = new GuidHttpRouteConstraint();
            bool match = TestValue(constraint, Guid.NewGuid());
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsGuidString()
        {
            GuidHttpRouteConstraint constraint = new GuidHttpRouteConstraint();
            bool match = TestValue(constraint, Guid.NewGuid().ToString());
            Assert.True(match);
        }
    }
}