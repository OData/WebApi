// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class BoolHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotBool()
        {
            BoolHttpRouteConstraint constraint = new BoolHttpRouteConstraint();
            bool match = TestValue(constraint, 123);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsNotParsableAsBool()
        {
            BoolHttpRouteConstraint constraint = new BoolHttpRouteConstraint();
            bool match = TestValue(constraint, "wakawaka");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsFalseString()
        {
            BoolHttpRouteConstraint constraint = new BoolHttpRouteConstraint();
            bool match = TestValue(constraint, "false");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsTrueString()
        {
            BoolHttpRouteConstraint constraint = new BoolHttpRouteConstraint();
            bool match = TestValue(constraint, "true");
            Assert.True(match);
        }
    }
}