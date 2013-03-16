// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class FloatHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotFloat()
        {
            FloatHttpRouteConstraint constraint = new FloatHttpRouteConstraint();
            bool match = TestValue(constraint, false);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsNotParsableAsFloat()
        {
            FloatHttpRouteConstraint constraint = new FloatHttpRouteConstraint();
            bool match = TestValue(constraint, "what a nice day!");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsFloat()
        {
            FloatHttpRouteConstraint constraint = new FloatHttpRouteConstraint();
            bool match = TestValue(constraint, 3.14F);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsFloatString()
        {
            FloatHttpRouteConstraint constraint = new FloatHttpRouteConstraint();
            bool match = TestValue(constraint, "3.14");
            Assert.True(match);
        }
    }
}