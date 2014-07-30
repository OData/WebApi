// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataDeserializerContextTest
    {
        [Theory]
        [InlineData(typeof(Delta), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(Delta<Customer>), true)]
        public void Property_IsDeltaOfT_HasRightValue(Type resourceType, bool expectedResult)
        {
            ODataDeserializerContext context = new ODataDeserializerContext { ResourceType = resourceType };
            Assert.Equal(expectedResult, context.IsDeltaOfT);
        }

        [Theory]
        [InlineData(typeof(Delta), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(Delta<Customer>), false)]
        [InlineData(typeof(IEdmObject), true)]
        [InlineData(typeof(IEdmComplexObject), true)]
        [InlineData(typeof(IEdmEntityObject), true)]
        [InlineData(typeof(EdmComplexObject), true)]
        [InlineData(typeof(EdmEntityObject), true)]
        [InlineData(typeof(ODataUntypedActionParameters), true)]
        public void Property_IsUntyped_HasRightValue(Type resourceType, bool expectedResult)
        {
            ODataDeserializerContext context = new ODataDeserializerContext { ResourceType = resourceType };
            Assert.Equal(expectedResult, context.IsUntyped);
        }
    }
}
