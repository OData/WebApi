// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntryDeserializerOfTItemTests
    {
        [Theory]
        [InlineData(typeof(ODataEntry))]
        [InlineData(typeof(ODataFeed))]
        [InlineData(typeof(ODataCollectionValue))]
        [InlineData(typeof(ODataProperty))]
        public void ReadInline_ThrowsArgument_TypeMismatch(Type deserializerType)
        {
            ODataEntryDeserializer deserializer = GetType().GetMethod("CreateDeserializer").MakeGenericMethod(deserializerType).Invoke(null, null) as ODataEntryDeserializer;

            ArgumentException ex = Assert.ThrowsArgument(
                () => deserializer.ReadInline("type mismatch item", new ODataDeserializerContext()),
                "item");
            Assert.True(ex.Message.StartsWith(String.Format("The argument must be of type '{0}'.", deserializerType.Name)));
        }

        public static ODataEntryDeserializer CreateDeserializer<TItem>()
            where TItem : class
        {
            Mock<ODataEntryDeserializer<TItem>> deserializer = new Mock<ODataEntryDeserializer<TItem>>(new Mock<IEdmTypeReference>().Object, ODataPayloadKind.Unsupported);
            return deserializer.Object;
        }
    }
}
