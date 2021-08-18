//-----------------------------------------------------------------------------
// <copyright file="AllowedArithmeticOperatorsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Query;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class AllowedArithmeticOperatorsTest
    {
        [Fact]
        public void None_MatchesNone()
        {
            Assert.Equal(AllowedArithmeticOperators.None, AllowedArithmeticOperators.All & AllowedArithmeticOperators.None);
        }

        [Fact]
        public void All_Contains_AllArithmeticOperators()
        {
            AllowedArithmeticOperators allArithmeticOperators = 0;
            foreach (AllowedArithmeticOperators allowedArithmeticOperator in Enum.GetValues(typeof(AllowedArithmeticOperators)))
            {
                if (allowedArithmeticOperator != AllowedArithmeticOperators.All)
                {
                    allArithmeticOperators |= allowedArithmeticOperator;
                }
            }

            Assert.Equal(AllowedArithmeticOperators.All, allArithmeticOperators);
        }
    }
}
