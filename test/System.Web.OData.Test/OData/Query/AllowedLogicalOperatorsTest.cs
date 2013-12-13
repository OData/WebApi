// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class AllowedLogicalOperatorsTest
    {
        [Fact]
        public void None_MatchesNone()
        {
            Assert.Equal(AllowedLogicalOperators.None, AllowedLogicalOperators.All & AllowedLogicalOperators.None);
        }

        [Fact]
        public void All_Contains_AllLogicalOperators()
        {
            AllowedLogicalOperators allLogicalOperators = 0;
            foreach (AllowedLogicalOperators allowedLogicalOperator in Enum.GetValues(typeof(AllowedLogicalOperators)))
            {
                if (allowedLogicalOperator != AllowedLogicalOperators.All)
                {
                    allLogicalOperators = allLogicalOperators | allowedLogicalOperator;
                }
            }

            Assert.Equal(allLogicalOperators, AllowedLogicalOperators.All);
        }
    }
}
