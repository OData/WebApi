// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing.Constraints;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class HttpRouteConstraintBuilderTests
    {
        [Fact]
        public void BuildInlineRouteConstraint_AlphaConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("alpha");

            Assert.IsType<AlphaHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_BoolConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("bool");

            Assert.IsType<BoolHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_CompoundConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => BuildInlineConstraint("compound"));
        }

        [Fact]
        public void BuildInlineRouteConstraint_DateTimeConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("datetime");

            Assert.IsType<DateTimeHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_DecimalConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("decimal");

            Assert.IsType<DecimalHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_DoubleConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("double");

            Assert.IsType<DoubleHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_EnumNameConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => BuildInlineConstraint("enumname"));
        }

        [Fact]
        public void BuildInlineRouteConstraint_EnumValueConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => BuildInlineConstraint("enumvalue"));
        }

        [Fact]
        public void BuildInlineRouteConstraint_FloatConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("float");

            Assert.IsType<FloatHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_GuidConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("guid");

            Assert.IsType<GuidHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_IntConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("int");

            Assert.IsType<IntHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_LengthConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("length", "5");

            Assert.IsType<LengthHttpRouteConstraint>(constraint);
            Assert.Equal(5, ((LengthHttpRouteConstraint)constraint).Length);
        }

        [Fact]
        public void BuildInlineRouteConstraint_LengthRangeConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("length", "5", "10");

            Assert.IsType<LengthHttpRouteConstraint>(constraint);
            LengthHttpRouteConstraint lengthConstraint = (LengthHttpRouteConstraint)constraint;
            Assert.Equal(5, lengthConstraint.MinLength);
            Assert.Equal(10, lengthConstraint.MaxLength);
        }

        [Fact]
        public void BuildInlineRouteConstraint_LongRangeConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("long");

            Assert.IsType<LongHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void BuildInlineRouteConstraint_MaxConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("max", "10");

            Assert.IsType<MaxHttpRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxHttpRouteConstraint)constraint).Max);
        }

        [Fact]
        public void BuildInlineRouteConstraint_MaxLengthConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("maxlength", "10");

            Assert.IsType<MaxLengthHttpRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxLengthHttpRouteConstraint)constraint).MaxLength);
        }

        [Fact]
        public void BuildInlineRouteConstraint_MinConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("min", "3");

            Assert.IsType<MinHttpRouteConstraint>(constraint);
            Assert.Equal(3, ((MinHttpRouteConstraint)constraint).Min);
        }

        [Fact]
        public void BuildInlineRouteConstraint_MinLengthConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("minlength", "3");

            Assert.IsType<MinLengthHttpRouteConstraint>(constraint);
            Assert.Equal(3, ((MinLengthHttpRouteConstraint)constraint).MinLength);
        }

        [Fact]
        public void BuildInlineRouteConstraint_OptionalConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => BuildInlineConstraint("optional"));
        }

        [Fact]
        public void BuildInlineRouteConstraint_RangeConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("range", "5", "10");

            Assert.IsType<RangeHttpRouteConstraint>(constraint);
            RangeHttpRouteConstraint rangeConstraint = (RangeHttpRouteConstraint)constraint;
            Assert.Equal(5, rangeConstraint.Min);
            Assert.Equal(10, rangeConstraint.Max);
        }

        [Fact]
        public void BuildInlineRouteConstraint_RegexConstraint()
        {
            IHttpRouteConstraint constraint = BuildInlineConstraint("regex", "abcdefg");

            Assert.IsType<RegexHttpRouteConstraint>(constraint);
            RegexHttpRouteConstraint regexConstraint = (RegexHttpRouteConstraint)constraint;
            Assert.Equal("abcdefg", regexConstraint.Pattern);
        }

        [Fact]
        public void BuildInlineRouteConstraint_TypeOfConstraintIsNotRegistered()
        {
            Assert.Throws<KeyNotFoundException>(() => BuildInlineConstraint("typeof"));
        }

        private IHttpRouteConstraint BuildInlineConstraint(string constraintKey, params string[] args)
        {
            IHttpRouteConstraint constraint = new DefaultInlineRouteConstraintResolver().ResolveConstraint(constraintKey, args);

            Assert.NotNull(constraint);

            return constraint;
        }
    }
}