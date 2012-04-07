// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.Filters
{
    public class FilterInfoComparerTest
    {
        [Theory]
        [PropertyData("CompareTestData")]
        public void Compare(FilterInfo x, FilterInfo y, int expectedSign)
        {
            int result = FilterInfoComparer.Instance.Compare(x, y);

            Assert.Equal(expectedSign, Math.Sign(result));
        }

        public static IEnumerable<object[]> CompareTestData
        {
            get
            {
                IFilter f = new Mock<IFilter>().Object;
                return new TheoryDataSet<FilterInfo, FilterInfo, int>()
                {
                    { null, null, 0 },
                    { new FilterInfo(f, FilterScope.Action), null, 1 },
                    { null, new FilterInfo(f, FilterScope.Action), -1 },
                    { new FilterInfo(f, FilterScope.Action), new FilterInfo(f, FilterScope.Action), 0 },
                    { new FilterInfo(f, FilterScope.Controller), new FilterInfo(f, FilterScope.Action), -1 },
                    { new FilterInfo(f, FilterScope.Action), new FilterInfo(f, FilterScope.Controller), 1 },
                };
            }
        }
    }
}
