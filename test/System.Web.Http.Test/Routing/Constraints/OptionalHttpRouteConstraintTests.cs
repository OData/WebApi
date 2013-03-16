// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class OptionalHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsAvailableAndInnerConstraintFails()
        {
            IHttpRouteConstraint innerConstraint = new AlphaHttpRouteConstraint();
            OptionalHttpRouteConstraint constraint = new OptionalHttpRouteConstraint(innerConstraint);
            bool match = TestValue(constraint, "123");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsAvailableAndInnerConstraintSucceeds()
        {
            IHttpRouteConstraint innerConstraint = new AlphaHttpRouteConstraint();
            OptionalHttpRouteConstraint constraint = new OptionalHttpRouteConstraint(innerConstraint);
            bool match = TestValue(constraint, "abc");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsNotAvailable()
        {
            IHttpRouteConstraint innerConstraint = new AlphaHttpRouteConstraint();
            OptionalHttpRouteConstraint constraint = new OptionalHttpRouteConstraint(innerConstraint);
            bool match = TestValue(constraint, RouteParameter.Optional);
            Assert.True(match);
        }
    }
}