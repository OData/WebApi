// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.OData.Query.Expressions
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
        [PropertyData("AreByteArraysEqualDataset")]
        public void AreByteArraysEqual(byte[] left, byte[] right, bool result)
        {
            Assert.Equal(
                result,
                Linq2ObjectsComparisonMethods.AreByteArraysEqual(left, right));
        }

        [Theory]
        [PropertyData("AreByteArraysEqualDataset")]
        public void AreByteArraysNotEqual(byte[] left, byte[] right, bool result)
        {
            Assert.Equal(
                !result,
                Linq2ObjectsComparisonMethods.AreByteArraysNotEqual(left, right));
        }
    }
}
