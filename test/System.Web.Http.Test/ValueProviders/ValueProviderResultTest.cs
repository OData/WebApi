// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.ValueProviders
{
    public class ValueProviderResultTest
    {
        [Fact]
        public void ConvertTo_ReturnsNullForReferenceTypes_WhenValueIsNull()
        {
            var valueProviderResult = new ValueProviderResult(null, null, CultureInfo.InvariantCulture);

            var convertedValue = valueProviderResult.ConvertTo(typeof(string));

            Assert.Equal(null, convertedValue);
        }

        [Fact]
        public void ConvertTo_ReturnsDefaultForValueTypes_WhenValueIsNull()
        {
            var valueProviderResult = new ValueProviderResult(null, null, CultureInfo.InvariantCulture);

            var convertedValue = valueProviderResult.ConvertTo(typeof(int));

            Assert.Equal(0, convertedValue);
        }
    }
}