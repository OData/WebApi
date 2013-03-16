// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class LengthHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotCorrectLength()
        {
            LengthHttpRouteConstraint constraint = new LengthHttpRouteConstraint("3");
            bool match = TestValue(constraint, "1234");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsCorrectLength()
        {
            LengthHttpRouteConstraint constraint = new LengthHttpRouteConstraint("3");
            bool match = TestValue(constraint, "123");
            Assert.True(match);
        }
    }
}