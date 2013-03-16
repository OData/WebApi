// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class EnumValueHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenTheValueIsNotOneOfTheEnumValues()
        {
            EnumValueHttpRouteConstraint<FakeEnum> constraint = new EnumValueHttpRouteConstraint<FakeEnum>();
            bool match = TestValue(constraint, "-1");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenTheValueIsOneOfTheEnumValues()
        {
            EnumValueHttpRouteConstraint<FakeEnum> constraint = new EnumValueHttpRouteConstraint<FakeEnum>();
            bool match = TestValue(constraint, "2");
            Assert.True(match);
        }
    }
}