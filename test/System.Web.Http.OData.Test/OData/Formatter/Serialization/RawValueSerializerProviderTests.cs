// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class RawValueSerializerProviderTests
    {
        [Theory]
        [PropertyData("EdmPrimitiveData")]
        public void ReturnsAnODataRawValueSerializerForPrimitiveTypes(Type type)
        {
            ODataSerializerProvider provider = new RawValueSerializerProvider();

            ODataSerializer result = provider.GetODataPayloadSerializer(null, type);

            Assert.NotNull(result);
            Assert.IsType<ODataRawValueSerializer>(result);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(Customer))]
        [InlineData(typeof(IEnumerable<Customer>))]
        public void ReturnsNullForNonPrimitiveTypes(Type type)
        {
            ODataSerializerProvider provider = new RawValueSerializerProvider();

            ODataSerializer result = provider.GetODataPayloadSerializer(null, type);

            Assert.Null(result);
        }

        public static TheoryDataSet<Type> EdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    typeof(byte[]), 
                    typeof(bool),
                    typeof(byte),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(double), 
                    typeof(Guid),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(sbyte),
                    typeof(float),
                    typeof(Stream), 
                    typeof(string), 
                    typeof(TimeSpan),
                    typeof(ODataRawSerializerProviderEnum)
                };
            }
        }

        private enum ODataRawSerializerProviderEnum
        {
            FirstValue,
            SecondValue
        }
    }
}
