// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class AllowedQueryOptionsTest
    {
        [Fact]
        public void None_MatchesNone()
        {
            Assert.Equal(AllowedQueryOptions.None, AllowedQueryOptions.All & AllowedQueryOptions.None);
        }

        [Theory]
        [InlineData(AllowedQueryOptions.Filter)]
        [InlineData(AllowedQueryOptions.OrderBy)]
        [InlineData(AllowedQueryOptions.Skip)]
        [InlineData(AllowedQueryOptions.Top)]
        [InlineData(AllowedQueryOptions.InlineCount)]
        [InlineData(AllowedQueryOptions.Select)]
        [InlineData(AllowedQueryOptions.Expand)]
        public void Supported_Contains_SupportedQueryOptions(AllowedQueryOptions queryOption)
        {
            Assert.Equal(queryOption, AllowedQueryOptions.Supported & queryOption);
        }

        [Theory]
        [InlineData(AllowedQueryOptions.Format)]
        [InlineData(AllowedQueryOptions.SkipToken)]
        public void Supported_DoesNotContain_UnsupportedQueryOptions(AllowedQueryOptions queryOption)
        {
            Assert.Equal(AllowedQueryOptions.None, AllowedQueryOptions.Supported & queryOption);
        }

        [Fact]
        public void All_Contains_AllQueryOptions()
        {
            AllowedQueryOptions allQueryOptions = 0;
            foreach (AllowedQueryOptions allowedQueryOption in Enum.GetValues(typeof(AllowedQueryOptions)))
            {
                if (allowedQueryOption != AllowedQueryOptions.All)
                {
                    allQueryOptions = allQueryOptions | allowedQueryOption;
                }
            }

            Assert.Equal(allQueryOptions, AllowedQueryOptions.All);
        }
    }
}
