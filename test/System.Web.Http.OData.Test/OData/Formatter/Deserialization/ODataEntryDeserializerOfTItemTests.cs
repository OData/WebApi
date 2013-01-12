// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
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
            MethodInfo method = typeof(ODataEntryDeserializerOfTItemTests).GetMethod("CreateDeserializer",
                BindingFlags.Static | BindingFlags.NonPublic);
            ODataEntryDeserializer deserializer = method.MakeGenericMethod(deserializerType).Invoke(null, null) as ODataEntryDeserializer;

            ArgumentException ex = Assert.ThrowsArgument(
                () => deserializer.ReadInline("type mismatch item", new ODataDeserializerContext()),
                "item");
            Assert.True(ex.Message.StartsWith(String.Format("The argument must be of type '{0}'.", deserializerType.Name)));
        }

        private static ODataEntryDeserializer CreateDeserializer<TItem>()
            where TItem : class
        {
            return new TestODataEntryDeserializer<TItem>(new Mock<IEdmTypeReference>().Object,
                ODataPayloadKind.Unsupported);
        }

        private class TestODataEntryDeserializer<TItem> : ODataEntryDeserializer<TItem> where TItem :class
        {
            public TestODataEntryDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind)
                : base(edmType, payloadKind)
            {
            }
        }
    }
}
