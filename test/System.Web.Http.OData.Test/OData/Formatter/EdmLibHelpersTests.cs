// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Web.Http.OData.Formatter.Serialization.Models;
using System.Xml.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class EdmLibHelpersTests
    {
        [Theory]
        [InlineData(typeof(Customer), "Customer")]
        [InlineData(typeof(int), "Int32")]
        [InlineData(typeof(IEnumerable<int>), "IEnumerable_1OfInt32")]
        [InlineData(typeof(IEnumerable<Func<int, string>>), "IEnumerable_1OfFunc_2OfInt32_String")]
        [InlineData(typeof(List<Func<int, string>>), "List_1OfFunc_2OfInt32_String")]
        public void EdmFullName(Type clrType, string expectedName)
        {
            Assert.Equal(expectedName, clrType.EdmName());
        }

        [Theory]
        [InlineData(typeof(char), typeof(string))]
        [InlineData(typeof(char?), typeof(string))]
        [InlineData(typeof(ushort), typeof(int))]
        [InlineData(typeof(uint), typeof(long))]
        [InlineData(typeof(ulong), typeof(long))]
        [InlineData(typeof(ushort?), typeof(int?))]
        [InlineData(typeof(uint?), typeof(long?))]
        [InlineData(typeof(ulong?), typeof(long?))]
        [InlineData(typeof(char[]), typeof(string))]
        [InlineData(typeof(Binary), typeof(byte[]))]
        [InlineData(typeof(XElement), typeof(string))]
        public void IsNonstandardEdmPrimitive_Returns_True(Type primitiveType, Type mappedType)
        {
            bool isNonstandardEdmPrimtive;
            Type resultMappedType = EdmLibHelpers.IsNonstandardEdmPrimitive(primitiveType, out isNonstandardEdmPrimtive);

            Assert.True(isNonstandardEdmPrimtive);
            Assert.Equal(mappedType, resultMappedType);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(TimeSpan))]
        public void IsNonstandardEdmPrimitive_Returns_False(Type primitiveType)
        {
            bool isNonstandardEdmPrimtive;
            Type resultMappedType = EdmLibHelpers.IsNonstandardEdmPrimitive(primitiveType, out isNonstandardEdmPrimtive);

            Assert.False(isNonstandardEdmPrimtive);
            Assert.Equal(primitiveType, resultMappedType);
        }
    }
}
