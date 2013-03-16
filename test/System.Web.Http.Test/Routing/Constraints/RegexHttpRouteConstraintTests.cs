// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class RegexHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenPatternIsNotMatched()
        {
            RegexHttpRouteConstraint constraint = new RegexHttpRouteConstraint(@"^\d{3}$");
            bool match = TestValue(constraint, "1234");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenPatternIsMatched()
        {
            RegexHttpRouteConstraint constraint = new RegexHttpRouteConstraint(@"^\d{3}-\d{3}-\d{4}$");
            bool match = TestValue(constraint, "406-555-1212");
            Assert.True(match);
        }
    }
}