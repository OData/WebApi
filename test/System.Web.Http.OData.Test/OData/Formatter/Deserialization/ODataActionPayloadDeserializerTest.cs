// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataActionPayloadDeserializerTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataActionPayloadDeserializer(deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public void Ctor_SetsProperty_DeserializerProvider()
        {
            ODataDeserializerProvider deserializerProvider = new Mock<ODataDeserializerProvider>().Object;
            var deserializer = new ODataActionPayloadDeserializer(deserializerProvider);
            Assert.Equal(deserializerProvider, deserializer.DeserializerProvider);
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: typeof(ODataActionParameters), readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_ReadContext()
        {
            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();
            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader, typeof(ODataActionParameters), readContext: null),
                "readContext");
        }

        [Fact]
        public void Read_Throws_SerializationException_ODataPathMissing()
        {
            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();
            Assert.Throws<SerializationException>(
                () => deserializer.Read(messageReader, typeof(ODataActionParameters), readContext: new ODataDeserializerContext()),
                "The operation cannot be completed because no ODataPath is available for the request.");
        }

        [Fact]
        public void Can_deserialize_payload_with_primitive_parameters()
        {
            string actionName = "Primitive";
            int quantity = 1;
            string productCode = "PCode";
            string body = "{" + string.Format(@" ""Quantity"": {0} , ""ProductCode"": ""{1}"" ", quantity, productCode) + "}";

            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json;odata=verbose");

            IEdmModel model = GetModel();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), model);
            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataPath path = CreatePath(model, actionName);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };
            ODataActionParameters payload = deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;

            Assert.NotNull(payload);
            Assert.Same(
                model.EntityContainers().Single().FunctionImports().SingleOrDefault(f => f.Name == "Primitive"),
                ODataActionPayloadDeserializer.GetFunctionImport(context));
            Assert.True(payload.ContainsKey("Quantity"));
            Assert.Equal(quantity, payload["Quantity"]);
            Assert.True(payload.ContainsKey("ProductCode"));
            Assert.Equal(productCode, payload["ProductCode"]);
        }

        [Fact]
        public void Can_deserialize_payload_with_complex_parameters()
        {
            string actionName = "Complex";
            string body = @"{ ""Quantity"": 1 , ""Address"": { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } }";

            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json;odata=verbose");
            IEdmModel model = GetModel();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), model);

            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataPath path = CreatePath(model, actionName);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };
            ODataActionParameters payload = deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;

            Assert.NotNull(payload);
            Assert.Same(
                model.EntityContainers().Single().FunctionImports().SingleOrDefault(f => f.Name == "Complex"),
                ODataActionPayloadDeserializer.GetFunctionImport(context));
            Assert.True(payload.ContainsKey("Quantity"));
            Assert.Equal(1, payload["Quantity"]);
            Assert.True(payload.ContainsKey("Address"));
            MyAddress address = payload["Address"] as MyAddress;
            Assert.NotNull(address);
            Assert.Equal("1 Microsoft Way", address.StreetAddress);
            Assert.Equal("Redmond", address.City);
            Assert.Equal("WA", address.State);
            Assert.Equal(98052, address.ZipCode);
        }

        [Fact]
        void Can_Deserialize_PrimitiveCollections_InUntypedMode()
        {
            // Arrange
            IEdmModel model = GetModel();
            IEdmFunctionImport action = model.EntityContainers().Single().FunctionImports().SingleOrDefault(f => f.Name == "PrimitiveCollection");
            string body = @"{ ""Name"": ""Avatar"", ""Ratings"": [ 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 ] }";
            int[] expectedRatings = new int[] { 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 };
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json;odata=verbose");
            ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, settings, model);

            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataPath path = CreatePath(model, action.Name);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model, ResourceType = typeof(ODataUntypedActionParameters) };

            // Act
            ODataUntypedActionParameters payload = deserializer.Read(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

            //Assert
            Assert.NotNull(payload);
            Assert.Same(
                model.EntityContainers().Single().FunctionImports().SingleOrDefault(f => f.Name == "PrimitiveCollection"),
                ODataActionPayloadDeserializer.GetFunctionImport(context));
            Assert.True(payload.ContainsKey("Name"));
            Assert.Equal("Avatar", payload["Name"]);
            Assert.True(payload.ContainsKey("Ratings"));
            IEnumerable<int> ratings = payload["Ratings"] as IEnumerable<int>;
            Assert.Equal(10, ratings.Count());
            Assert.True(expectedRatings.Zip(ratings, (expected, actual) => expected - actual).All(diff => diff == 0));
        }

        [Fact]
        void Can_Deserialize_ComplexCollections_InUntypedMode()
        {
            //Arrange
            IEdmModel model = GetModel();
            IEdmFunctionImport action = model.EntityContainers().Single().FunctionImports().SingleOrDefault(f => f.Name == "ComplexCollection");
            string body = @"{ ""Name"": ""Avatar"" , ""Addresses"": [{ ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 }] }";

            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json;odata=verbose");
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), model);

            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataPath path = CreatePath(model, action.Name);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model, ResourceType = typeof(ODataUntypedActionParameters) };

            //Act
            ODataUntypedActionParameters payload = deserializer.Read(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

            //Assert
            Assert.NotNull(payload);
            Assert.Same(
                model.EntityContainers().Single().FunctionImports().SingleOrDefault(f => f.Name == "ComplexCollection"),
                ODataActionPayloadDeserializer.GetFunctionImport(context));
            Assert.True(payload.ContainsKey("Name"));
            Assert.Equal("Avatar", payload["Name"]);
            Assert.True(payload.ContainsKey("Addresses"));
            IEnumerable<IEdmObject> addresses = payload["Addresses"] as EdmComplexObjectCollection;
            dynamic address = addresses.SingleOrDefault();
            Assert.NotNull(address);
            Assert.Equal("1 Microsoft Way", address.StreetAddress);
            Assert.Equal("Redmond", address.City);
            Assert.Equal("WA", address.State);
            Assert.Equal(98052, address.ZipCode);
        }

        [Fact]
        public void Can_DeserializePayload_InUntypedMode()
        {
            // Arrange
            IEdmModel model = GetModel();
            IEdmFunctionImport action = model.EntityContainers().Single().FunctionImports().SingleOrDefault(f => f.Name == "Complex");
            string body = @"{ ""Quantity"": 1 , ""Address"": { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } }";
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json;odata=verbose");
            ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, settings, model);

            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataPath path = CreatePath(model, action.Name);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model, ResourceType = typeof(ODataUntypedActionParameters) };

            // Act
            ODataUntypedActionParameters payload = deserializer.Read(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

            // Assert
            Assert.NotNull(payload);
            Assert.Same(action, payload.Action);
            Assert.True(payload.ContainsKey("Quantity"));
            Assert.Equal(1, payload["Quantity"]);
            Assert.True(payload.ContainsKey("Address"));
            dynamic address = payload["Address"] as EdmComplexObject;
            Assert.IsType<EdmComplexObject>(address);
            Assert.Equal("1 Microsoft Way", address.StreetAddress);
            Assert.Equal("Redmond", address.City);
            Assert.Equal("WA", address.State);
            Assert.Equal(98052, address.ZipCode);
        }

        [Fact]
        public void Can_deserialize_payload_with_primitive_collection_parameters()
        {
            string actionName = "PrimitiveCollection";
            string body = @"{ ""Name"": ""Avatar"", ""Ratings"": [ 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 ] }";
            int[] expectedRatings = new int[] { 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 };
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json;odata=verbose");
            IEdmModel model = GetModel();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), model);

            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataPath path = CreatePath(model, actionName);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };
            ODataActionParameters payload = deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;

            Assert.NotNull(payload);
            Assert.Same(
                model.EntityContainers().Single().FunctionImports().SingleOrDefault(f => f.Name == "PrimitiveCollection"),
                ODataActionPayloadDeserializer.GetFunctionImport(context));
            Assert.True(payload.ContainsKey("Name"));
            Assert.Equal("Avatar", payload["Name"]);
            Assert.True(payload.ContainsKey("Ratings"));
            IEnumerable<int> ratings = payload["Ratings"] as IEnumerable<int>;
            Assert.Equal(10, ratings.Count());
            Assert.True(expectedRatings.Zip(ratings, (expected, actual) => expected - actual).All(diff => diff == 0));
        }

        [Fact]
        public void Can_deserialize_payload_with_complex_collection_parameters()
        {
            string actionName = "ComplexCollection";
            string body = @"{ ""Name"": ""Microsoft"", ""Addresses"": [ { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } ] }";
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json;odata=verbose");
            IEdmModel model = GetModel();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), model);

            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataPath path = CreatePath(model, actionName);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };
            ODataActionParameters payload = deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;

            Assert.NotNull(payload);
            Assert.True(payload.ContainsKey("Name"));
            Assert.Equal("Microsoft", payload["Name"]);
            Assert.True(payload.ContainsKey("Addresses"));
            IList<MyAddress> addresses = (payload["Addresses"] as IEnumerable<MyAddress>).ToList();
            Assert.NotNull(addresses);
            Assert.Equal(1, addresses.Count);
            MyAddress address = addresses[0];
            Assert.NotNull(address);
            Assert.Equal("1 Microsoft Way", address.StreetAddress);
            Assert.Equal("Redmond", address.City);
            Assert.Equal("WA", address.State);
            Assert.Equal(98052, address.ZipCode);
        }

        [Fact]
        public void Throws_ODataException_when_parameter_not_found()
        {
            string body = @"{ ""Quantity"": 1 , ""ProductCode"": ""PCode"", ""MissingParameter"": 1 }";

            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json;odata=verbose");
            IEdmModel model = GetModel();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), model);

            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataPath path = CreatePath(model, "Primitive");
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = model };
            Assert.Throws<ODataException>(() =>
            {
                ODataActionParameters payload = deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
            }, "The parameter 'MissingParameter' in the request payload is not a valid parameter for the function import 'Primitive'.");
        }

        private static ODataPath CreatePath(IEdmModel model, string actionName)
        {
            IEdmFunctionImport functionImport =
                model.EntityContainers().Single().FindFunctionImports(actionName).Single();
            return new ODataPath(new ActionPathSegment(functionImport));
        }

        private IEdmModel GetModel()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(Customer)));
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            builder.ContainerName = "C";
            builder.Namespace = "A.B";
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;

            ActionConfiguration primitive = customer.Action("Primitive");
            primitive.Parameter<int>("Quantity");
            primitive.Parameter<string>("ProductCode");

            ActionConfiguration complex = customer.Action("Complex");
            complex.Parameter<int>("Quantity");
            complex.Parameter<MyAddress>("Address");

            ActionConfiguration primitiveCollection = customer.Action("PrimitiveCollection");
            primitiveCollection.Parameter<string>("Name");
            primitiveCollection.CollectionParameter<int>("Ratings");

            ActionConfiguration complexCollection = customer.Action("ComplexCollection");
            complexCollection.Parameter<string>("Name");
            complexCollection.CollectionParameter<MyAddress>("Addresses");

            return builder.GetEdmModel();
        }

        private static Stream GetStringAsStream(string body)
        {
            Stream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(body);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private class DerivedODataActionParameters : ODataActionParameters
        {
        }
    }

    public class MyAddress
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }
    }
}
