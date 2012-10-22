// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.TestModels;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
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
            Assert.Equal(1, querySettings.LambdaNestingLimit);
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
        public void LambdaNestingLimit_Property_RoundTrips()
        {
            Assert.Reflection.IntegerProperty<ODataQuerySettings, int>(
                new ODataQuerySettings(),
                o => o.LambdaNestingLimit,
                expectedDefaultValue: 1,
                minLegalValue: 1,
                maxLegalValue: int.MaxValue,
                illegalLowerValue: 0,
                illegalUpperValue: null,
                roundTripTestValue: 2);
        }

        [Fact]
        public void ResultLimit_Property_RoundTrips()
        {
            Assert.Reflection.NullableIntegerProperty<ODataQuerySettings, int>(
                new ODataQuerySettings(),
                o => o.ResultLimit,
                expectedDefaultValue: null,
                minLegalValue: 1,
                maxLegalValue: int.MaxValue,
                illegalLowerValue: 0,
                illegalUpperValue: null,
                roundTripTestValue: 2);
        }
    }
}
