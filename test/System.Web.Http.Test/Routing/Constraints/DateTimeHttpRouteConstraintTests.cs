// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing.Constraints
{
    public class DateTimeHttpRouteConstraintTests : HttpRouteConstraintTestBase
    {
        [Fact]
        public void Match_FailsWhenValueIsNotDateTime()
        {
            DateTimeHttpRouteConstraint constraint = new DateTimeHttpRouteConstraint();
            bool match = TestValue(constraint, 1);
            Assert.False(match);
        }

        [Fact]
        public void Match_FailsWhenValueIsNotParsableAsDateTime()
        {
            DateTimeHttpRouteConstraint constraint = new DateTimeHttpRouteConstraint();
            bool match = TestValue(constraint, "Not a date");
            Assert.False(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsDateString()
        {
            DateTimeHttpRouteConstraint constraint = new DateTimeHttpRouteConstraint();
            bool match = TestValue(constraint, "12/25/2012");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsDateTime()
        {
            DateTimeHttpRouteConstraint constraint = new DateTimeHttpRouteConstraint();
            bool match = TestValue(constraint, DateTime.Now);
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsDateTimeString()
        {
            DateTimeHttpRouteConstraint constraint = new DateTimeHttpRouteConstraint();
            bool match = TestValue(constraint, "12/31/2009 11:45:00 PM");
            Assert.True(match);
        }

        [Fact]
        public void Match_SucceedsWhenValueIsTimeString()
        {
            DateTimeHttpRouteConstraint constraint = new DateTimeHttpRouteConstraint();
            bool match = TestValue(constraint, "2:30:00 PM");
            Assert.True(match);
        }
    }
}