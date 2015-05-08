﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
#if ASPNETWEBAPI
using System.Web.Http.Routing.Constraints;
#else
using System.Web.Mvc.Routing.Constraints;
#endif
using Microsoft.TestCommon;

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class DefaultInlineConstraintResolverTest
    {
        [Fact]
        public void ResolveConstraint_AlphaConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("alpha");

            Assert.IsType<AlphaRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_BoolConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("bool");

            Assert.IsType<BoolRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_CompoundConstraintIsNotRegistered()
        {
            Assert.Null(new DefaultInlineConstraintResolver().ResolveConstraint("compound"));
        }

        [Fact]
        public void ResolveConstraint_DateTimeConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("datetime");

            Assert.IsType<DateTimeRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_DecimalConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("decimal");

            Assert.IsType<DecimalRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_DoubleConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("double");

            Assert.IsType<DoubleRouteConstraint>(constraint);
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
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("float");

            Assert.IsType<FloatRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_GuidConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("guid");

            Assert.IsType<GuidRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_IntConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("int");

            Assert.IsType<IntRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_LengthConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("length(5)");

            Assert.IsType<LengthRouteConstraint>(constraint);
            Assert.Equal(5, ((LengthRouteConstraint)constraint).Length);
        }

        [Fact]
        public void ResolveConstraint_LengthRangeConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("length(5, 10)");

            Assert.IsType<LengthRouteConstraint>(constraint);
            LengthRouteConstraint lengthConstraint = (LengthRouteConstraint)constraint;
            Assert.Equal(5, lengthConstraint.MinLength);
            Assert.Equal(10, lengthConstraint.MaxLength);
        }

        [Fact]
        public void ResolveConstraint_LongRangeConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("long");

            Assert.IsType<LongRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_MaxConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("max(10)");

            Assert.IsType<MaxRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxRouteConstraint)constraint).Max);
        }

        [Fact]
        public void ResolveConstraint_MaxLengthConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("maxlength(10)");

            Assert.IsType<MaxLengthRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxLengthRouteConstraint)constraint).MaxLength);
        }

        [Fact]
        public void ResolveConstraint_MinConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("min(3)");

            Assert.IsType<MinRouteConstraint>(constraint);
            Assert.Equal(3, ((MinRouteConstraint)constraint).Min);
        }

        [Fact]
        public void ResolveConstraint_MinLengthConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("minlength(3)");

            Assert.IsType<MinLengthRouteConstraint>(constraint);
            Assert.Equal(3, ((MinLengthRouteConstraint)constraint).MinLength);
        }

        [Fact]
        public void ResolveConstraint_OptionalConstraintIsNotRegistered()
        {
            Assert.Null(new DefaultInlineConstraintResolver().ResolveConstraint("optional"));
        }

        [Fact]
        public void ResolveConstraint_RangeConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("range(5, 10)");

            Assert.IsType<RangeRouteConstraint>(constraint);
            RangeRouteConstraint rangeConstraint = (RangeRouteConstraint)constraint;
            Assert.Equal(5, rangeConstraint.Min);
            Assert.Equal(10, rangeConstraint.Max);
        }

        [Fact]
        public void ResolveConstraint_RegexConstraint()
        {
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("regex(abc,defg)");

            Assert.IsType<RegexRouteConstraint>(constraint);
            RegexRouteConstraint regexConstraint = (RegexRouteConstraint)constraint;
            Assert.Equal("abc,defg", regexConstraint.Pattern);
        }

        [Fact]
        public void ResolveConstraint_IntConstraintWithArgument_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => new DefaultInlineConstraintResolver().ResolveConstraint("int(5)"),
               "Could not find a constructor for constraint type 'IntRouteConstraint' with the following number of parameters: 1.");
        }

        [Fact]
        public void ResolveConstraint_SupportsCustomConstraints()
        {
            var resolver = new DefaultInlineConstraintResolver();
            resolver.ConstraintMap.Add("custom", typeof(IntRouteConstraint));

            var constraint = resolver.ResolveConstraint("custom");

            Assert.IsType<IntRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_CustomConstraintThatDoesNotImplementouteConstraintInterfact_Throws()
        {
            var resolver = new DefaultInlineConstraintResolver();
            resolver.ConstraintMap.Add("custom", typeof(string));

            Assert.Throws<InvalidOperationException>(
                () => resolver.ResolveConstraint("custom"),
#if ASPNETWEBAPI
                "The constraint type 'String' which is mapped to constraint key 'custom' must implement the IHttpRouteConstraint interface.");
#else
                "The constraint type 'String' which is mapped to constraint key 'custom' must implement the IRouteConstraint interface.");
#endif
        }
    }
}