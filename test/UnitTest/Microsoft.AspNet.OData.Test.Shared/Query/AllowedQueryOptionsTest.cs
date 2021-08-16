//-----------------------------------------------------------------------------
// <copyright file="AllowedQueryOptionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Query;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
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
        [InlineData(AllowedQueryOptions.Count)]
        [InlineData(AllowedQueryOptions.Select)]
        [InlineData(AllowedQueryOptions.Expand)]
        [InlineData(AllowedQueryOptions.Format)]
        [InlineData(AllowedQueryOptions.SkipToken)]
        public void Supported_Contains_SupportedQueryOptions(AllowedQueryOptions queryOption)
        {
            Assert.Equal(queryOption, AllowedQueryOptions.Supported & queryOption);
        }

        [Fact]
        public void Supported_DoesNotContain_UnsupportedQueryOptions()
        {
            Assert.Equal(AllowedQueryOptions.None, AllowedQueryOptions.Supported & (AllowedQueryOptions.DeltaToken));
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

            Assert.Equal(AllowedQueryOptions.All, allQueryOptions);
        }
    }
}
