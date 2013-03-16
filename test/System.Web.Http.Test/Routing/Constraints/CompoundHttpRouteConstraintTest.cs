// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class CompoundHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenAnyInnerConstraintFails()
        {
            IHttpRouteConstraint[] innerConstraints = new IHttpRouteConstraint[]
            {
                new AlphaHttpRouteConstraint(), 
                new LengthHttpRouteConstraint(3)
            };

            CompoundHttpRouteConstraint constraint = new CompoundHttpRouteConstraint(innerConstraints);
            bool match = TestValue(constraint, "abcd");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenInnerConstraintsMatch()
        {
            IHttpRouteConstraint[] innerConstraints = new IHttpRouteConstraint[]
            {
                new AlphaHttpRouteConstraint(), 
                new LengthHttpRouteConstraint(3)
            };

            CompoundHttpRouteConstraint constraint = new CompoundHttpRouteConstraint(innerConstraints);
            bool match = TestValue(constraint, "abc");
            Assert.True(match);
        }
    }
}