//-----------------------------------------------------------------------------
// <copyright file="Linq2ObjectsComparisonMethodsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Expressions
{
    public class Linq2ObjectsComparisonMethodsTest
    {
        public static TheoryDataSet<byte[], byte[], bool> AreByteArraysEqualDataset
        {
            get
            {
                return new TheoryDataSet<byte[], byte[], bool>
                {
                    { new byte[] { 1, 2, 3}, new byte[] { 1, 2, 3} , true},
                    { new byte[] { 1,2,3}, new byte[] { 1, 2} , false},
                    { new byte[] { 1,2}, new byte[] { 1, 2, 3} , false},
                    { null, new byte[] { 1, 2, 3} , false},
                    { new byte[] { 1, 2, 3}, null , false},
                    { null, null , true},
                };
            }
        }

        [Theory]
        [MemberData(nameof(AreByteArraysEqualDataset))]
        public void AreByteArraysEqual(byte[] left, byte[] right, bool result)
        {
            Assert.Equal(
                result,
                Linq2ObjectsComparisonMethods.AreByteArraysEqual(left, right));
        }

        [Theory]
        [MemberData(nameof(AreByteArraysEqualDataset))]
        public void AreByteArraysNotEqual(byte[] left, byte[] right, bool result)
        {
            Assert.Equal(
                !result,
                Linq2ObjectsComparisonMethods.AreByteArraysNotEqual(left, right));
        }
    }
}
