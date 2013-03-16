// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class EnumNameHttpRouteConstraintTest : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenTheValueIsNotOneOfTheEnumNames()
        {
            EnumNameHttpRouteConstraint<FakeEnum> constraint = new EnumNameHttpRouteConstraint<FakeEnum>();
            bool match = TestValue(constraint, "Black");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenTheValueIsOneOfTheEnumNames()
        {
            EnumNameHttpRouteConstraint<FakeEnum> constraint = new EnumNameHttpRouteConstraint<FakeEnum>();
            bool match = TestValue(constraint, "Blue");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenTheValueIsOfTheEnumType()
        {
            EnumNameHttpRouteConstraint<FakeEnum> constraint = new EnumNameHttpRouteConstraint<FakeEnum>();
            bool match = TestValue(constraint, FakeEnum.Indigo);
            Assert.True(match);
        }
    }
}