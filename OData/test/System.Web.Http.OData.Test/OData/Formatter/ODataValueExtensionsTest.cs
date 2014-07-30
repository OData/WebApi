// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ODataValueExtensionsTest
    {
        public static TheoryDataSet<ODataValue, object> GetInnerValueTestData
        {
            get
            {
                ODataComplexValue complexValue = new ODataComplexValue();
                ODataCollectionValue collectionValue = new ODataCollectionValue();
                ODataStreamReferenceValue streamReferenceValue = new ODataStreamReferenceValue();

                return new TheoryDataSet<ODataValue, object>
                {
                    { new ODataPrimitiveValue(100), 100 },
                    { new ODataNullValue(), null },
                    { complexValue,  complexValue },
                    { collectionValue, collectionValue },
                    { streamReferenceValue, streamReferenceValue },
                    { null, null } 
                };
            }
        }

        [Theory]
        [PropertyData("GetInnerValueTestData")]
        public void GetInnerValue_Returns_CorrectObject(ODataValue value, object expectedResult)
        {
            Assert.Equal(expectedResult, value.GetInnerValue());
        }
    }
}
