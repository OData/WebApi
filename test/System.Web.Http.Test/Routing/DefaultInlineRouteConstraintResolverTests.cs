// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing.Constraints;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class DefaultInlineRouteConstraintResolverTests
    {
        [Fact]
        public void BuildInlineRouteConstraint_AlphaConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("alpha");

            Assert.IsType<AlphaHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_BoolConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("bool");

            Assert.IsType<BoolHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_CompoundConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => ResolveConstraint("compound"));
        }

        [Fact]
        public void BuildInlineRouteConstraint_DateTimeConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("datetime");

            Assert.IsType<DateTimeHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_DecimalConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("decimal");

            Assert.IsType<DecimalHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_DoubleConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("double");

            Assert.IsType<DoubleHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_EnumNameConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => ResolveConstraint("enumname"));
        }

        [Fact]
        public void BuildInlineRouteConstraint_EnumValueConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => ResolveConstraint("enumvalue"));
        }

        [Fact]
        public void BuildInlineRouteConstraint_FloatConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("float");

            Assert.IsType<FloatHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_GuidConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("guid");

            Assert.IsType<GuidHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_IntConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("int");

            Assert.IsType<IntHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_LengthConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("length(5)");

            Assert.IsType<LengthHttpRouteConstraint>(constraint);
            Assert.Equal(5, ((LengthHttpRouteConstraint)constraint).Length);
        }

        [Fact]
        public void BuildInlineRouteConstraint_LengthRangeConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("length(5, 10)");

            Assert.IsType<LengthHttpRouteConstraint>(constraint);
            LengthHttpRouteConstraint lengthConstraint = (LengthHttpRouteConstraint)constraint;
            Assert.Equal(5, lengthConstraint.MinLength);
            Assert.Equal(10, lengthConstraint.MaxLength);
        }

        [Fact]
        public void BuildInlineRouteConstraint_LongRangeConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("long");

            Assert.IsType<LongHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_MaxConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("max(10)");

            Assert.IsType<MaxHttpRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxHttpRouteConstraint)constraint).Max);
        }

        [Fact]
        public void BuildInlineRouteConstraint_MaxLengthConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("maxlength(10)");

            Assert.IsType<MaxLengthHttpRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxLengthHttpRouteConstraint)constraint).MaxLength);
        }

        [Fact]
        public void BuildInlineRouteConstraint_MinConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("min(3)");

            Assert.IsType<MinHttpRouteConstraint>(constraint);
            Assert.Equal(3, ((MinHttpRouteConstraint)constraint).Min);
        }

        [Fact]
        public void BuildInlineRouteConstraint_MinLengthConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("minlength(3)");

            Assert.IsType<MinLengthHttpRouteConstraint>(constraint);
            Assert.Equal(3, ((MinLengthHttpRouteConstraint)constraint).MinLength);
        }

        [Fact]
        public void BuildInlineRouteConstraint_OptionalConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => ResolveConstraint("optional"));
        }

        [Fact]
        public void BuildInlineRouteConstraint_RangeConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("range(5, 10)");

            Assert.IsType<RangeHttpRouteConstraint>(constraint);
            RangeHttpRouteConstraint rangeConstraint = (RangeHttpRouteConstraint)constraint;
            Assert.Equal(5, rangeConstraint.Min);
            Assert.Equal(10, rangeConstraint.Max);
        }

        [Fact]
        public void BuildInlineRouteConstraint_RegexConstraint()
        {
            IHttpRouteConstraint constraint = ResolveConstraint("regex(abc,defg)");

            Assert.IsType<RegexHttpRouteConstraint>(constraint);
            RegexHttpRouteConstraint regexConstraint = (RegexHttpRouteConstraint)constraint;
            Assert.Equal("abc,defg", regexConstraint.Pattern);
        }

        private IHttpRouteConstraint ResolveConstraint(string constraintDefinition)
        {
            IHttpRouteConstraint constraint = new DefaultInlineRouteConstraintResolver().ResolveConstraint(constraintDefinition);

            Assert.NotNull(constraint);

            return constraint;
        }
    }
}