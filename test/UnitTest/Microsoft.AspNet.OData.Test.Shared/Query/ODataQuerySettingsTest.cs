//-----------------------------------------------------------------------------
// <copyright file="ODataQuerySettingsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class ODataQuerySettingsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(125)]
        [InlineData(int.MaxValue)]
        public void MatchesPatternTimeout_NormalizesAndCopiesConfiguredValue(int? milliseconds)
        {
            var settings = new ODataQuerySettings();
            Assert.Equal(TimeSpan.FromMilliseconds(250), settings.MatchesPatternTimeout);
            settings.MatchesPatternTimeout = milliseconds.HasValue ? (TimeSpan?)TimeSpan.FromMilliseconds(milliseconds.Value) : null;
            TimeSpan? expected = milliseconds > 0 ? (TimeSpan?)TimeSpan.FromMilliseconds(Math.Min(milliseconds.Value, int.MaxValue - 1)) : null;
            Assert.Equal(expected, settings.MatchesPatternTimeout);

            var copy = new ODataQuerySettings();
            copy.CopyFrom(settings);

            Assert.Equal(expected, copy.MatchesPatternTimeout);
        }

        [Fact]
        public void MatchesPatternTimeout_ClampsTimeSpanMaxValue()
        {
            var settings = new ODataQuerySettings { MatchesPatternTimeout = TimeSpan.MaxValue };

            Assert.Equal(TimeSpan.FromMilliseconds(int.MaxValue - 1), settings.MatchesPatternTimeout);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void MatchesPatternTimeout_PreservesRegexMaximumAndClampsLargerSpans(bool aboveMaximum)
        {
            var settings = new ODataQuerySettings { MatchesPatternTimeout = aboveMaximum ? TimeSpan.FromDays(30) : ODataQuerySettings.MaxMatchesPatternTimeout };

            Assert.Equal(ODataQuerySettings.MaxMatchesPatternTimeout, settings.MatchesPatternTimeout);
        }

        [Fact]
        public void MatchesPatternTimeout_CanBeSetAndCleared()
        {
            var settings = new ODataQuerySettings();
            settings.MatchesPatternTimeout = TimeSpan.FromMilliseconds(100);
            Assert.Equal(TimeSpan.FromMilliseconds(100), settings.MatchesPatternTimeout);

            settings.MatchesPatternTimeout = null;
            Assert.Null(settings.MatchesPatternTimeout);

            var copy = new ODataQuerySettings { MatchesPatternTimeout = TimeSpan.FromMilliseconds(100) };
            copy.CopyFrom(settings);
            Assert.Null(copy.MatchesPatternTimeout);
        }

        [Fact]
        public void Ctor_Initializes_All_Properties()
        {
            // Arrange & Act
            ODataQuerySettings querySettings = new ODataQuerySettings();

            // Assert
            Assert.Equal(HandleNullPropagationOption.Default, querySettings.HandleNullPropagation);
            Assert.True(querySettings.EnsureStableOrdering);
        }

        [Fact]
        public void EnsureStableOrdering_Property_RoundTrips()
        {
            ReflectionAssert.BooleanProperty<ODataQuerySettings>(
                new ODataQuerySettings(),
                o => o.EnsureStableOrdering,
                true);
        }

        [Fact]
        public void HandleNullPropagation_Property_RoundTrips()
        {
            ReflectionAssert.EnumProperty<ODataQuerySettings, HandleNullPropagationOption>(
                new ODataQuerySettings(),
                o => o.HandleNullPropagation,
                HandleNullPropagationOption.Default,
                HandleNullPropagationOption.Default - 1,
                HandleNullPropagationOption.True);
        }

        [Fact]
        public void PageSize_Property_RoundTrips()
        {
            ReflectionAssert.NullableIntegerProperty<ODataQuerySettings, int>(
                new ODataQuerySettings(),
                o => o.PageSize,
                expectedDefaultValue: null,
                minLegalValue: 1,
                maxLegalValue: int.MaxValue,
                illegalLowerValue: 0,
                illegalUpperValue: null,
                roundTripTestValue: 2);
        }

        [Fact]
        public void EnableConstantParameterization_Property_RoundTrips()
        {
            ReflectionAssert.BooleanProperty<ODataQuerySettings>(
                new ODataQuerySettings(),
                o => o.EnableConstantParameterization,
                expectedDefaultValue: true);
        }

        [Fact]
        public void EnableCorrelatedSubqueryBuffering_Property_RoundTrips()
        {
            ReflectionAssert.BooleanProperty<ODataQuerySettings>(
                new ODataQuerySettings(),
                o => o.EnableCorrelatedSubqueryBuffering,
                expectedDefaultValue: false);
        }
    }
}
