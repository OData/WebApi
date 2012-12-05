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
