//-----------------------------------------------------------------------------
// <copyright file="ODataQuerySettingsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class ODataQuerySettingsTest
    {
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
