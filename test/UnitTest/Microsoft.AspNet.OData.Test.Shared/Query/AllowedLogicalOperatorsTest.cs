//-----------------------------------------------------------------------------
// <copyright file="AllowedLogicalOperatorsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Query;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
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

            Assert.Equal(AllowedLogicalOperators.All, allLogicalOperators);
        }
    }
}
