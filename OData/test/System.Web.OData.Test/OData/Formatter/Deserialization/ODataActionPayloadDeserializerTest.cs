// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataActionPayloadDeserializerTest
    {
        private static IEdmModel _model;
        private static IEdmEntityContainer _container;
        private static ODataActionPayloadDeserializer _deserializer;
        private const string _serviceRoot = "http://any/";

        static ODataActionPayloadDeserializerTest()
        {
            _model = GetModel();
            _container = _model.EntityContainer;
            _deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
        }

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
            // Arrange
            ODataDeserializerProvider deserializerProvider = new Mock<ODataDeserializerProvider>().Object;

            // Act
            var deserializer = new ODataActionPayloadDeserializer(deserializerProvider);

            // Assert
            Assert.Same(deserializerProvider, deserializer.DeserializerProvider);
        }

        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = new Mock<ODataDeserializerProvider>().Object;

            // Act
            var deserializer = new ODataActionPayloadDeserializer(deserializerProvider);

            // Assert
            Assert.Equal(ODataPayloadKind.Parameter, deserializer.ODataPayloadKind);
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            // Arrange
            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: typeof(ODataActionParameters), readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_ReadContext()
        {
            // Arrange
            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader, typeof(ODataActionParameters), readContext: null),
                "readContext");
        }

        [Fact]
        public void Read_Throws_SerializationException_ODataPathMissing()
        {
            // Arrange
            ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(new DefaultODataDeserializerProvider());
            ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();

            // Act & Assert
            Assert.Throws<SerializationException>(
                () => deserializer.Read(messageReader, typeof(ODataActionParameters), readContext: new ODataDeserializerContext()),
                "The operation cannot be completed because no ODataPath is available for the request.");
        }

        public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithPrimitiveParametersTest
        {
            get
            {
                return new TheoryDataSet<string, IEdmAction, ODataPath>
                {
                    {"Primitive", GetBoundAction("Primitive"), CreateBoundPath("Primitive") },
                    {"UnboundPrimitive", GetUnboundAction("UnboundPrimitive"), CreateUnboundPath("UnboundPrimitive")}
                };
            }
        }

        public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithComplexParametersTest
        {
            get
            {
                return new TheoryDataSet<string, IEdmAction, ODataPath>
                {
                    {"Complex", GetBoundAction("Complex"), CreateBoundPath("Complex") },
                    {"UnboundComplex", GetUnboundAction("UnboundComplex"), CreateUnboundPath("UnboundComplex")}
                };
            }
        }

        public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithPrimitiveCollectionsTest
        {
            get
            {
                return new TheoryDataSet<string, IEdmAction, ODataPath>
                {
                    {"PrimitiveCollection", GetBoundAction("PrimitiveCollection"), CreateBoundPath("PrimitiveCollection") },
                    {"UnboundPrimitiveCollection", GetUnboundAction("UnboundPrimitiveCollection"), CreateUnboundPath("UnboundPrimitiveCollection")}
                };
            }
        }

        public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithComplexCollectionsTest
        {
            get
            {
                return new TheoryDataSet<string, IEdmAction, ODataPath>
                {
                    {"ComplexCollection", GetBoundAction("ComplexCollection"), CreateBoundPath("ComplexCollection") },
                    {"UnboundComplexCollection", GetUnboundAction("UnboundComplexCollection"), CreateUnboundPath("UnboundComplexCollection")}
                };
            }
        }

        [Theory]
        [PropertyData("DeserializeWithPrimitiveParametersTest")]
        public void Can_DeserializePayload_WithPrimitiveParameters(string actionName, IEdmAction expectedAction, ODataPath path)
        {
            // Arrange
            const int Quantity = 1;
            const string ProductCode = "PCode";
            string body = "{" + string.Format(@" ""Quantity"": {0} , ""ProductCode"": ""{1}"" ", Quantity, ProductCode) + "}";

            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(body));
            message.SetHeader("Content-Type", "application/json");
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
            ODataDeserializerContext context = new ODataDeserializerContext() { Path = path, Model = _model };

            // Act
            ODataActionParameters payload = _deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            // Assert
            Assert.Same(expectedAction, action);
            Assert.NotNull(payload);
            Assert.True(payload.ContainsKey("Quantity"));
            Assert.Equal(Quantity, payload["Quantity"]);
            Assert.True(payload.ContainsKey("ProductCode"));
            Assert.Equal(ProductCode, payload["ProductCode"]);
        }

        [Theory]
        [PropertyData("DeserializeWithComplexParametersTest")]
        public void Can_DeserializePayload_WithComplexParameters(string actionName, IEdmAction expectedAction, ODataPath path)
        {
            // Arrange
            const string Body = @"{ ""Quantity"": 1 , ""Address"": { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } }";
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(Body));
            message.SetHeader("Content-Type", "application/json");
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
            ODataDeserializerContext context = new ODataDeserializerContext() { Path = path, Model = _model };

            // Act
            ODataActionParameters payload = _deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            // Assert
            Assert.Same(expectedAction, action);
            Assert.NotNull(payload);
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

        [Theory]
        [PropertyData("DeserializeWithPrimitiveCollectionsTest")]
        public void Can_DeserializePayload_WithPrimitiveCollections_InUntypedMode(string actionName, IEdmAction expectedAction, ODataPath path)
        {
            // Arrange
            const string Body = @"{ ""Name"": ""Avatar"", ""Ratings"": [ 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 ] }";
            int[] expectedRatings = new int[] { 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 };
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(Body));
            message.SetHeader("Content-Type", "application/json");

            ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, settings, _model);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

            // Act
            ODataUntypedActionParameters payload = _deserializer.Read(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            //Assert
            Assert.Same(expectedAction, action);
            Assert.NotNull(payload);
            Assert.True(payload.ContainsKey("Name"));
            Assert.Equal("Avatar", payload["Name"]);
            Assert.True(payload.ContainsKey("Ratings"));
            IEnumerable<int> ratings = payload["Ratings"] as IEnumerable<int>;
            Assert.Equal(10, ratings.Count());
            Assert.True(expectedRatings.Zip(ratings, (expected, actual) => expected - actual).All(diff => diff == 0));
        }

        [Theory]
        [PropertyData("DeserializeWithComplexCollectionsTest")]
        public void Can_DeserializePayload_WithComplexCollections_InUntypedMode(string actionName, IEdmAction expectedAction, ODataPath path)
        {
            // Arrange
            const string Body = @"{ ""Name"": ""Avatar"" , ""Addresses"": [{ ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 }] }";
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(Body));
            message.SetHeader("Content-Type", "application/json");

            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

            // Act
            ODataUntypedActionParameters payload = _deserializer.Read(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

            // Assert
            Assert.Same(expectedAction, payload.Action);
            Assert.NotNull(payload);
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

        [Theory]
        [PropertyData("DeserializeWithComplexParametersTest")]
        public void Can_DeserializePayload_WithComplexParameters_InUntypedMode(string actionName, IEdmAction expectedAction, ODataPath path)
        {
            // Arrange
            const string Body = @"{ ""Quantity"": 1 , ""Address"": { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } }";
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(Body));
            message.SetHeader("Content-Type", "application/json");

            ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, settings, _model);

            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

            // Act
            ODataUntypedActionParameters payload = _deserializer.Read(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

            // Assert
            Assert.NotNull(payload);
            Assert.Same(expectedAction, payload.Action);
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

        [Theory]
        [PropertyData("DeserializeWithPrimitiveCollectionsTest")]
        public void Can_DeserializePayload_WithPrimitiveCollectionParameters(string actionName, IEdmAction expectedAction, ODataPath path)
        {
            // Arrange
            const string Body = @"{ ""Name"": ""Avatar"", ""Ratings"": [ 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 ] }";
            int[] expectedRatings = new int[] { 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 };

            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(Body));
            message.SetHeader("Content-Type", "application/json");
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);

            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model };

            // Act
            ODataActionParameters payload = _deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
            IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

            // Assert
            Assert.NotNull(payload);
            Assert.Same(expectedAction, action);
            Assert.True(payload.ContainsKey("Name"));
            Assert.Equal("Avatar", payload["Name"]);
            Assert.True(payload.ContainsKey("Ratings"));
            IEnumerable<int> ratings = payload["Ratings"] as IEnumerable<int>;
            Assert.Equal(10, ratings.Count());
            Assert.True(expectedRatings.Zip(ratings, (expected, actual) => expected - actual).All(diff => diff == 0));
        }

        [Theory]
        [PropertyData("DeserializeWithComplexCollectionsTest")]
        public void Can_DeserializePayload_WithComplexCollectionParameters(string actionName, IEdmAction expectedAction, ODataPath path)
        {
            // Arrange
            const string Body = @"{ ""Name"": ""Microsoft"", ""Addresses"": [ { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } ] }";
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(Body));
            message.SetHeader("Content-Type", "application/json");
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model };

            // Act
            ODataActionParameters payload = _deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;

            // Assert
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

        [Theory]
        [PropertyData("DeserializeWithPrimitiveParametersTest")]
        public void Throws_ODataException_When_Parameter_Notfound(string actionName, IEdmAction expectedAction, ODataPath path)
        {
            // Arrange
            const string Body = @"{ ""Quantity"": 1 , ""ProductCode"": ""PCode"", ""MissingParameter"": 1 }";
            ODataMessageWrapper message = new ODataMessageWrapper(GetStringAsStream(Body));
            message.SetHeader("Content-Type", "application/json");
            ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
            ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model };

            // Act & Assert
            Assert.Throws<ODataException>(() =>
            {
                ODataActionParameters payload = _deserializer.Read(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
            }, "The parameter 'MissingParameter' in the request payload is not a valid parameter for the operation '" + actionName + "'.");
        }

        private static ODataPath CreateBoundPath(string actionName)
        {
            string path = String.Format("Customers(1)/A.B.{0}", actionName);
            ODataPath odataPath = new DefaultODataPathHandler().Parse(_model, _serviceRoot, path);
            Assert.NotNull(odataPath); // Guard
            return odataPath;
        }

        private static ODataPath CreateUnboundPath(string actionName)
        {
            string path = String.Format("{0}", actionName);
            ODataPath odataPath = new DefaultODataPathHandler().Parse(_model, _serviceRoot, path);
            Assert.NotNull(odataPath); // Guard
            return odataPath;
        }

        public static IEdmAction GetBoundAction(string actionName)
        {
            IEdmAction action = _model.SchemaElements.OfType<IEdmAction>().SingleOrDefault(a => a.Name == actionName);
            Assert.NotNull(action); // Guard
            return action;
        }

        public static IEdmAction GetUnboundAction(string actionName)
        {
            IEdmActionImport actionImport = _container.OperationImports().SingleOrDefault(a => a.Name == actionName) as IEdmActionImport;
            Assert.NotNull(actionImport); // Guard
            return actionImport.Action;
        }

        private static IEdmModel GetModel()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(Customer)));
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            builder.ContainerName = "C";
            builder.Namespace = "A.B";
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;

            // bound actions
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

            // unbound actions
            ActionConfiguration unboundPrimitive = builder.Action("UnboundPrimitive");
            unboundPrimitive.Parameter<int>("Quantity");
            unboundPrimitive.Parameter<string>("ProductCode");

            ActionConfiguration unboundComplex = builder.Action("UnboundComplex");
            unboundComplex.Parameter<int>("Quantity");
            unboundComplex.Parameter<MyAddress>("Address");

            ActionConfiguration unboundPrimitiveCollection = builder.Action("UnboundPrimitiveCollection");
            unboundPrimitiveCollection.Parameter<string>("Name");
            unboundPrimitiveCollection.CollectionParameter<int>("Ratings");

            ActionConfiguration unboundComplexCollection = builder.Action("UnboundComplexCollection");
            unboundComplexCollection.Parameter<string>("Name");
            unboundComplexCollection.CollectionParameter<MyAddress>("Addresses");

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
