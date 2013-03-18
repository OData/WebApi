// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Routing.Constraints;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class DefaultInlineConstraintResolverTest
    {
        [Fact]
        public void ResolveConstraint_AlphaConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("alpha");

            Assert.IsType<AlphaHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_BoolConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("bool");

            Assert.IsType<BoolHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_CompoundConstraintIsNotRegistered()
        {
            Assert.Null(new DefaultInlineConstraintResolver().ResolveConstraint("compound"));
        }

        [Fact]
        public void ResolveConstraint_DateTimeConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("datetime");

            Assert.IsType<DateTimeHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_DecimalConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("decimal");

            Assert.IsType<DecimalHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_DoubleConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("double");

            Assert.IsType<DoubleHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_EnumNameConstraintIsNotRegistered()
        {
            Assert.Null(new DefaultInlineConstraintResolver().ResolveConstraint("enumname"));
        }

        [Fact]
        public void ResolveConstraint_EnumValueConstraintIsNotRegistered()
        {
            Assert.Null(new DefaultInlineConstraintResolver().ResolveConstraint("enumvalue"));
        }

        [Fact]
        public void ResolveConstraint_FloatConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("float");

            Assert.IsType<FloatHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_GuidConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("guid");

            Assert.IsType<GuidHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_IntConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("int");

            Assert.IsType<IntHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_LengthConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("length(5)");

            Assert.IsType<LengthHttpRouteConstraint>(constraint);
            Assert.Equal(5, ((LengthHttpRouteConstraint)constraint).Length);
        }

        [Fact]
        public void ResolveConstraint_LengthRangeConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("length(5, 10)");

            Assert.IsType<LengthHttpRouteConstraint>(constraint);
            LengthHttpRouteConstraint lengthConstraint = (LengthHttpRouteConstraint)constraint;
            Assert.Equal(5, lengthConstraint.MinLength);
            Assert.Equal(10, lengthConstraint.MaxLength);
        }

        [Fact]
        public void ResolveConstraint_LongRangeConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("long");

            Assert.IsType<LongHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_MaxConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("max(10)");

            Assert.IsType<MaxHttpRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxHttpRouteConstraint)constraint).Max);
        }

        [Fact]
        public void ResolveConstraint_MaxLengthConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("maxlength(10)");

            Assert.IsType<MaxLengthHttpRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxLengthHttpRouteConstraint)constraint).MaxLength);
        }

        [Fact]
        public void ResolveConstraint_MinConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("min(3)");

            Assert.IsType<MinHttpRouteConstraint>(constraint);
            Assert.Equal(3, ((MinHttpRouteConstraint)constraint).Min);
        }

        [Fact]
        public void ResolveConstraint_MinLengthConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("minlength(3)");

            Assert.IsType<MinLengthHttpRouteConstraint>(constraint);
            Assert.Equal(3, ((MinLengthHttpRouteConstraint)constraint).MinLength);
        }

        [Fact]
        public void ResolveConstraint_OptionalConstraintIsNotRegistered()
        {
            Assert.Null(new DefaultInlineConstraintResolver().ResolveConstraint("optional"));
        }

        [Fact]
        public void ResolveConstraint_RangeConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("range(5, 10)");

            Assert.IsType<RangeHttpRouteConstraint>(constraint);
            RangeHttpRouteConstraint rangeConstraint = (RangeHttpRouteConstraint)constraint;
            Assert.Equal(5, rangeConstraint.Min);
            Assert.Equal(10, rangeConstraint.Max);
        }

        [Fact]
        public void ResolveConstraint_RegexConstraint()
        {
            IHttpRouteConstraint constraint = new DefaultInlineConstraintResolver().ResolveConstraint("regex(abc,defg)");

            Assert.IsType<RegexHttpRouteConstraint>(constraint);
            RegexHttpRouteConstraint regexConstraint = (RegexHttpRouteConstraint)constraint;
            Assert.Equal("abc,defg", regexConstraint.Pattern);
        }

        [Fact]
        public void ResolveConstraint_IntConstraintWithArgument_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => new DefaultInlineConstraintResolver().ResolveConstraint("int(5)"),
               "Could not find a constructor for constraint type 'IntHttpRouteConstraint' with the following number of parameters: 1.");
        }

        [Fact]
        public void ResolveConstraint_SupportsCustomConstraints()
        {
            var resolver = new DefaultInlineConstraintResolver();
            resolver.ConstraintMap.Add("custom", typeof(IntHttpRouteConstraint));

            IHttpRouteConstraint constraint = resolver.ResolveConstraint("custom");

            Assert.IsType<IntHttpRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_CustomConstraintThatDoesNotImplementIHttpRouteConstraint_Throws()
        {
            var resolver = new DefaultInlineConstraintResolver();
            resolver.ConstraintMap.Add("custom", typeof(string));

            Assert.Throws<InvalidOperationException>(
                () => resolver.ResolveConstraint("custom"),
               "The constraint type 'String' which is mapped to constraint key 'custom' must implement the IHttpRouteConstraint interface.");
        }
    }
}