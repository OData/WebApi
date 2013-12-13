// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
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
            Assert.Reflection.BooleanProperty<ODataQuerySettings>(
                new ODataQuerySettings(),
                o => o.EnsureStableOrdering,
                true);
        }

        [Fact]
        public void HandleNullPropagation_Property_RoundTrips()
        {
            Assert.Reflection.EnumProperty<ODataQuerySettings, HandleNullPropagationOption>(
                new ODataQuerySettings(),
                o => o.HandleNullPropagation,
                HandleNullPropagationOption.Default,
                HandleNullPropagationOption.Default - 1,
                HandleNullPropagationOption.True);
        }

        [Fact]
        public void PageSize_Property_RoundTrips()
        {
            Assert.Reflection.NullableIntegerProperty<ODataQuerySettings, int>(
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
            Assert.Reflection.BooleanProperty<ODataQuerySettings>(
                new ODataQuerySettings(),
                o => o.EnableConstantParameterization,
                expectedDefaultValue: true);
        }
    }
}
