// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Query.Expressions
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
