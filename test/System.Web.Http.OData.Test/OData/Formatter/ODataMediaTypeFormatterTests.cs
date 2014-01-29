// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.Http.OData.Routing.ODataPath;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypeFormatterTests : MediaTypeFormatterTestBase<ODataMediaTypeFormatter>
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_PayloadKinds()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataMediaTypeFormatter(payloadKinds: null),
                "payloadKinds");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            ODataPayloadKind[] payloadKinds = new ODataPayloadKind[0];

            Assert.ThrowsArgumentNull(
                () => new ODataMediaTypeFormatter(deserializerProvider: null, serializerProvider: serializerProvider, payloadKinds: payloadKinds),
                "deserializerProvider");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();
            ODataPayloadKind[] payloadKinds = new ODataPayloadKind[0];

            Assert.ThrowsArgumentNull(
                () => new ODataMediaTypeFormatter(deserializerProvider, serializerProvider: null, payloadKinds: payloadKinds),
                "serializerProvider");
        }

        [Fact]
        public void CopyCtor_ThrowsArgumentNull_Request()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[0]);
            Assert.ThrowsArgumentNull(
                () => new ODataMediaTypeFormatter(formatter, version: ODataVersion.V2, request: null),
                "request");
        }

        [Fact]
        public void CopyCtor_ThrowsArgumentNull_Formatter()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataMediaTypeFormatter(formatter: null, version: ODataVersion.V2, request: new HttpRequestMessage()),
                "formatter");
        }

        [Fact]
        public void WriteToStreamAsyncReturnsODataRepresentationForJsonLight()
        {
            WriteToStreamAsyncReturnsODataRepresentation(Resources.WorkItemEntryInJsonLight, true);
        }

        [Fact]
        public void WriteToStreamAsyncReturnsODataRepresentationForAtom()
        {
            WriteToStreamAsyncReturnsODataRepresentation(Resources.WorkItemEntryInAtom, false);
        }

        private static void WriteToStreamAsyncReturnsODataRepresentation(string expectedContent, bool json)
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)");
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, null, model);
            request.SetConfiguration(configuration);
            request.ODataProperties().Model = model;
            IEdmEntitySet entitySet = model.EntityContainers().Single().EntitySets().Single();
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment(entitySet), new KeyValuePathSegment("10"));
            request.ODataProperties().RouteName = routeName;

            ODataMediaTypeFormatter formatter;

            if (json)
            {
                formatter = CreateFormatterWithJson(model, request, ODataPayloadKind.Entry);
            }
            else
            {
                formatter = CreateFormatter(model, request, ODataPayloadKind.Entry);
            }

            ObjectContent<WorkItem> content = new ObjectContent<WorkItem>(
                (WorkItem)TypeInitializer.GetInstance(SupportedTypes.WorkItem), formatter);

            string actualContent = content.ReadAsStringAsync().Result;

            if (json)
            {
                JsonAssert.Equal(expectedContent, actualContent);
            }
            else
            {
                RegexReplacement replaceUpdateTime = new RegexReplacement(
                    "<updated>*.*</updated>", "<updated>UpdatedTime</updated>");
                Assert.Xml.Equal(expectedContent, actualContent, replaceUpdateTime);
            }
        }

        [Theory]
        [InlineData("prefix", "http://localhost/prefix")]
        [InlineData("{a}", "http://localhost/prefix")]
        [InlineData("{a}/{b}", "http://localhost/prefix/prefix2")]
        public void WriteToStreamAsync_ReturnsCorrectBaseUri(string routePrefix, string baseUri)
        {
            IEdmModel model = new ODataConventionModelBuilder().GetEdmModel();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, baseUri);
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.Routes.MapODataServiceRoute(routeName, routePrefix, model);
            request.SetConfiguration(configuration);
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = new ODataPath();
            request.ODataProperties().RouteName = routeName;
            HttpRouteData routeData = new HttpRouteData(new HttpRoute());
            routeData.Values.Add("a", "prefix");
            routeData.Values.Add("b", "prefix2");
            request.SetRouteData(routeData);

            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, ODataPayloadKind.ServiceDocument);
            var content = new ObjectContent<ODataWorkspace>(new ODataWorkspace(), formatter);

            string actualContent = content.ReadAsStringAsync().Result;
            Assert.Contains("xml:base=\"" + baseUri + "\"", actualContent);
        }

        [Fact]
        public void WriteToStreamAsync_Throws_WhenBaseUriCannotBeGenerated()
        {
            IEdmModel model = new ODataConventionModelBuilder().GetEdmModel();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapHttpRoute("OData", "{param}");
            request.SetConfiguration(configuration);
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = new ODataPath();
            request.ODataProperties().RouteName = "OData";

            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, ODataPayloadKind.ServiceDocument);
            var content = new ObjectContent<ODataWorkspace>(new ODataWorkspace(), formatter);

            Assert.Throws<SerializationException>(
                () => content.ReadAsStringAsync().Result,
                "The ODataMediaTypeFormatter was unable to determine the base URI for the request. The request must be processed by an OData route for the OData formatter to serialize the response.");
        }

        [Theory]
        [InlineData(null, null, "3.0")]
        [InlineData("1.0", null, "1.0")]
        [InlineData("2.0", null, "2.0")]
        [InlineData("3.0", null, "3.0")]
        [InlineData(null, "1.0", "1.0")]
        [InlineData(null, "2.0", "2.0")]
        [InlineData(null, "3.0", "3.0")]
        [InlineData("1.0", "1.0", "1.0")]
        [InlineData("1.0", "2.0", "2.0")]
        [InlineData("1.0", "3.0", "3.0")]
        public void SetDefaultContentHeaders_SetsRightODataServiceVersion(string requestDataServiceVersion, string requestMaxDataServiceVersion, string expectedDataServiceVersion)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            if (requestDataServiceVersion != null)
            {
                request.Headers.TryAddWithoutValidation("DataServiceVersion", requestDataServiceVersion);
            }
            if (requestMaxDataServiceVersion != null)
            {
                request.Headers.TryAddWithoutValidation("MaxDataServiceVersion", requestMaxDataServiceVersion);
            }

            HttpContentHeaders contentHeaders = new StringContent("").Headers;

            CreateFormatterWithoutRequest()
            .GetPerRequestFormatterInstance(typeof(int), request, MediaTypeHeaderValue.Parse("application/xml"))
            .SetDefaultContentHeaders(typeof(int), contentHeaders, MediaTypeHeaderValue.Parse("application/xml"));

            IEnumerable<string> headervalues;
            Assert.True(contentHeaders.TryGetValues("DataServiceVersion", out headervalues));
            Assert.Equal(new string[] { expectedDataServiceVersion }, headervalues);
        }

        [Fact]
        public void TryGetInnerTypeForDelta_ChangesRefToGenericParameter_ForDeltas()
        {
            Type type = typeof(Delta<Customer>);

            bool success = ODataMediaTypeFormatter.TryGetInnerTypeForDelta(ref type);

            Assert.Same(typeof(Customer), type);
            Assert.True(success);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(List<string>))]
        public void TryGetInnerTypeForDelta_ReturnsFalse_ForNonDeltas(Type originalType)
        {
            Type type = originalType;

            bool success = ODataMediaTypeFormatter.TryGetInnerTypeForDelta(ref type);

            Assert.Same(originalType, type);
            Assert.False(success);
        }

        [Fact]
        public override Task WriteToStreamAsync_WhenObjectIsNull_WritesDataButDoesNotCloseStream()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatterWithRequest();
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.CanWrite).Returns(true);
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/atom+xml");

            // Act 
            return formatter.WriteToStreamAsync(typeof(SampleType), null, mockStream.Object, content, null).ContinueWith(
                writeTask =>
                {
                    // Assert (OData formatter doesn't support writing nulls)
                    Assert.Equal(TaskStatus.Faulted, writeTask.Status);
                    Assert.Throws<SerializationException>(() => writeTask.ThrowIfFaulted(), "Cannot serialize a null 'entry'.");
                    mockStream.Verify(s => s.Close(), Times.Never());
                    mockStream.Verify(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
                });
        }

        [Theory]
        [InlineData("Test content", "utf-8", true)]
        [InlineData("Test content", "utf-16", true)]
        public override Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            MediaTypeFormatter formatter = CreateFormatterWithRequest();
            formatter.SupportedEncodings.Add(CreateEncoding(encoding));
            string formattedContent = CreateFormattedContent(content);
            string mediaType = string.Format("application/json; odata=minimalmetadata; charset={0}", encoding);

            // Act & assert
            return ReadContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Theory]
        [InlineData("Test content", "utf-8", true)]
        [InlineData("Test content", "utf-16", true)]
        public override Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            MediaTypeFormatter formatter = CreateFormatterWithRequest();
            formatter.SupportedEncodings.Add(CreateEncoding(encoding));
            string formattedContent = CreateFormattedContent(content);
            string mediaType = string.Format("application/json; odata=minimalmetadata; charset={0}", encoding);

            // Act & assert
            return WriteContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Fact]
        public void ReadFromStreamAsync_ThrowsInvalidOperation_WithoutRequest()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var formatter = CreateFormatter(builder.GetEdmModel());

            Assert.Throws<InvalidOperationException>(
                () => formatter.ReadFromStreamAsync(typeof(Customer), new MemoryStream(), content: null, formatterLogger: null),
                "The OData formatter requires an attached request in order to deserialize. Controller classes must derive from ODataController or be marked with ODataFormattingAttribute. Custom parameter bindings must call GetPerRequestFormatterInstance on each formatter and use these per-request instances.");
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsInvalidOperation_WithoutRequest()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var formatter = CreateFormatter(builder.GetEdmModel());

            Assert.Throws<InvalidOperationException>(
                () => formatter.WriteToStreamAsync(typeof(Customer), new Customer(), new MemoryStream(), content: null, transportContext: null),
                "The OData formatter does not support writing client requests. This formatter instance must have an associated request.");
        }

        [Fact]
        public void WriteToStreamAsync_Passes_MetadataLevelToSerializerContext()
        {
            // Arrange
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            Mock<ODataSerializer> serializer = new Mock<ODataSerializer>(ODataPayloadKind.Property);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();

            serializerProvider.Setup(p => p.GetODataPayloadSerializer(model, typeof(int), request)).Returns(serializer.Object);
            serializer
                .Setup(s => s.WriteObject(42, typeof(int), It.IsAny<ODataMessageWriter>(),
                    It.Is<ODataSerializerContext>(c => c.MetadataLevel == ODataMetadataLevel.FullMetadata)))
                .Verifiable();


            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            var formatter = new ODataMediaTypeFormatter(deserializerProvider, serializerProvider.Object, Enumerable.Empty<ODataPayloadKind>());
            formatter.Request = request;
            HttpContent content = new StringContent("42");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata");

            // Act
            formatter.WriteToStreamAsync(typeof(int), 42, new MemoryStream(), content, transportContext: null);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteToStreamAsync_PassesSelectExpandClause_ThroughSerializerContext()
        {
            // Arrange
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            SelectExpandClause selectExpandClause =
                new SelectExpandClause(new SelectItem[0], allSelected: true);
            request.ODataProperties().SelectExpandClause = selectExpandClause;

            Mock<ODataSerializer> serializer = new Mock<ODataSerializer>(ODataPayloadKind.Property);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();

            serializerProvider.Setup(p => p.GetODataPayloadSerializer(model, typeof(int), request)).Returns(serializer.Object);
            serializer
                .Setup(s => s.WriteObject(42, typeof(int), It.IsAny<ODataMessageWriter>(),
                    It.Is<ODataSerializerContext>(c => c.SelectExpandClause == selectExpandClause)))
                .Verifiable();

            ODataDeserializerProvider deserializerProvider = new DefaultODataDeserializerProvider();

            var formatter = new ODataMediaTypeFormatter(deserializerProvider, serializerProvider.Object, Enumerable.Empty<ODataPayloadKind>());
            formatter.Request = request;
            HttpContent content = new StringContent("42");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata");

            // Act
            formatter.WriteToStreamAsync(typeof(int), 42, new MemoryStream(), content, transportContext: null);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void MessageReaderSettings_Property()
        {
            var formatter = CreateFormatter();

            Assert.NotNull(formatter.MessageReaderSettings);
            Assert.True(formatter.MessageReaderSettings.DisableMessageStreamDisposal);
        }

        [Fact]
        public void MessageWriterSettings_Property()
        {
            var formatter = CreateFormatter();

            Assert.NotNull(formatter.MessageWriterSettings);
            Assert.True(formatter.MessageWriterSettings.DisableMessageStreamDisposal);
        }

        [Fact]
        public void MessageReaderQuotas_Property_RoundTrip()
        {
            var formatter = CreateFormatter();
            formatter.MessageReaderQuotas.MaxNestingDepth = 42;

            Assert.Equal(42, formatter.MessageReaderQuotas.MaxNestingDepth);
        }

        [Fact]
        public void MessageWriterQuotas_Property_RoundTrip()
        {
            var formatter = CreateFormatter();
            formatter.MessageWriterQuotas.MaxNestingDepth = 42;

            Assert.Equal(42, formatter.MessageWriterQuotas.MaxNestingDepth);
        }

        [Fact]
        public void Default_ReceiveMessageSize_Is_MaxedOut()
        {
            var formatter = CreateFormatter();
            Assert.Equal(Int64.MaxValue, formatter.MessageReaderQuotas.MaxReceivedMessageSize);
        }

        [Fact]
        public void MessageReaderQuotas_Is_Passed_To_ODataLib()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            formatter.MessageReaderSettings.MessageQuotas.MaxReceivedMessageSize = 1;

            HttpContent content = new StringContent("{ 'Number' : '42' }");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            Assert.Throws<ODataException>(
                () => formatter.ReadFromStreamAsync(typeof(int), content.ReadAsStreamAsync().Result, content, formatterLogger: null).Result,
                "The maximum number of bytes allowed to be read from the stream has been exceeded. After the last read operation, a total of 19 bytes has been read from the stream; however a maximum of 1 bytes is allowed.");
        }

        [Fact]
        public void InvalidXmlCharacters_CanBeWrittenByDefault_InAtom()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Stream stream = new MemoryStream();
            HttpContent content = new StreamContent(stream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml");

            Assert.DoesNotThrow(
                () => formatter.WriteToStreamAsync(typeof(string), "Hello\x16", stream, content, null));
        }

        [Fact]
        public void InvalidXmlCharacters_RaiseExceptionsIfCheckCharactersIsTrue_InAtom()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            formatter.MessageWriterSettings.CheckCharacters = true;
            Stream stream = new MemoryStream();
            HttpContent content = new StreamContent(stream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml");

            //formatter.WriteToStreamAsync(typeof(string), "Hello\x16", stream, content, null).Wait();
            Assert.Throws<ArgumentException>(
                () => formatter.WriteToStreamAsync(typeof(string), "Hello\x16", stream, content, null).Wait(),
                 "'\x16', hexadecimal value 0x16, is an invalid character.");
        }

        [Fact]
        public void Request_IsPassedThroughDeserializerContext()
        {
            // Arrange
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            Mock<ODataEdmTypeDeserializer> deserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Property);
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns(deserializer.Object);
            deserializer
                .Setup(d => d.Read(It.IsAny<ODataMessageReader>(), typeof(int), It.Is<ODataDeserializerContext>(c => c.Request == request)))
                .Verifiable();
            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();

            var formatter = new ODataMediaTypeFormatter(deserializerProvider.Object, serializerProvider, Enumerable.Empty<ODataPayloadKind>());
            formatter.Request = request;
            HttpContent content = new StringContent("42");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata");

            // Act
            formatter.ReadFromStreamAsync(typeof(int), new MemoryStream(), content, formatterLogger: null);

            // Assert
            deserializer.Verify();
        }

        public static TheoryDataSet<ODataPath, ODataPayloadKind> CanReadTypeTypesTestData
        {
            get
            {
                CustomersModelWithInheritance model = new CustomersModelWithInheritance();
                EntitySetPathSegment entitySetSegment = new EntitySetPathSegment(model.Customers);
                KeyValuePathSegment keyValueSegment = new KeyValuePathSegment("42");
                NavigationPathSegment navSegment = new NavigationPathSegment(model.Customer.FindProperty("Orders") as IEdmNavigationProperty);
                PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment(model.Customer.FindProperty("Address") as IEdmStructuralProperty);

                return new TheoryDataSet<ODataPath, ODataPayloadKind>
                {
                    { new ODataPath(entitySetSegment), ODataPayloadKind.Entry }, // POST ~/entityset
                    { new ODataPath(entitySetSegment, keyValueSegment), ODataPayloadKind.Entry }, // PUT ~/entityset(key)
                    { new ODataPath(entitySetSegment, keyValueSegment, navSegment), ODataPayloadKind.Entry }, // PUT ~/entityset(key)/nav
                    { new ODataPath(entitySetSegment, keyValueSegment, propertySegment), ODataPayloadKind.Property }
                };
            }
        }

        [Theory]
        [PropertyData("CanReadTypeTypesTestData")]
        public void CanReadType_ForTypeless_ReturnsExpectedResult_DependingOnODataPathAndPayloadKind(ODataPath path, ODataPayloadKind payloadKind)
        {
            // Arrange
            IEnumerable<ODataPayloadKind> allPayloadKinds = Enum.GetValues(typeof(ODataPayloadKind)).Cast<ODataPayloadKind>();
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            request.ODataProperties().Path = path;

            var formatterWithGivenPayload = new ODataMediaTypeFormatter(new[] { payloadKind }) { Request = request };
            var formatterWithoutGivenPayload = new ODataMediaTypeFormatter(allPayloadKinds.Except(new[] { payloadKind })) { Request = request };

            // Act & Assert
            Assert.True(formatterWithGivenPayload.CanReadType(typeof(IEdmObject)));
            Assert.False(formatterWithoutGivenPayload.CanReadType(typeof(IEdmObject)));
        }

        public static TheoryDataSet<ODataPayloadKind, Type> CanWriteType_ReturnsExpectedResult_ForEdmObjects_TestData
        {
            get
            {
                Type entityCollectionEdmObjectType = new Mock<IEdmObject>().As<IEnumerable<IEdmEntityObject>>().Object.GetType();
                Type complexCollectionEdmObjectType = new Mock<IEdmObject>().As<IEnumerable<IEdmComplexObject>>().Object.GetType();

                return new TheoryDataSet<ODataPayloadKind, Type>
                {
                    { ODataPayloadKind.Entry , typeof(IEdmEntityObject) },
                    { ODataPayloadKind.Entry , typeof(TypedEdmEntityObject) },
                    { ODataPayloadKind.Feed , entityCollectionEdmObjectType },
                    { ODataPayloadKind.Feed , typeof(IEnumerable<IEdmEntityObject>) },
                    { ODataPayloadKind.Property , typeof(IEdmComplexObject) },
                    { ODataPayloadKind.Property , typeof(TypedEdmComplexObject) },
                    { ODataPayloadKind.Collection , complexCollectionEdmObjectType },
                    { ODataPayloadKind.Collection , typeof(IEnumerable<IEdmComplexObject>) },
                    { ODataPayloadKind.Property, typeof(NullEdmComplexObject) }
                };
            }
        }

        [Theory]
        [PropertyData("CanWriteType_ReturnsExpectedResult_ForEdmObjects_TestData")]
        public void CanWriteType_ReturnsTrueForEdmObjects_WithRightPayload(ODataPayloadKind payloadKind, Type type)
        {
            // Arrange
            IEnumerable<ODataPayloadKind> allPayloadKinds = Enum.GetValues(typeof(ODataPayloadKind)).Cast<ODataPayloadKind>();
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);

            var formatterWithGivenPayload = new ODataMediaTypeFormatter(new[] { payloadKind }) { Request = request };
            var formatterWithoutGivenPayload = new ODataMediaTypeFormatter(allPayloadKinds.Except(new[] { payloadKind })) { Request = request };

            // Act & Assert
            Assert.True(formatterWithGivenPayload.CanWriteType(type));
            Assert.False(formatterWithoutGivenPayload.CanWriteType(type));
        }

        public static TheoryDataSet<Type> InvalidIEdmObjectImplementationTypes
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    typeof(IEdmObject),
                    typeof(TypedEdmStructuredObject),
                    new Mock<IEdmObject>().Object.GetType(),
                    new Mock<IEdmObject>().As<IEnumerable<IEdmObject>>().Object.GetType()
                };
            }
        }

        [Theory]
        [PropertyData("InvalidIEdmObjectImplementationTypes")]
        public void CanWriteType_ReturnsFalse_ForInvalidIEdmObjectImplementations_NoMatterThePayload(Type type)
        {
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            IEnumerable<ODataPayloadKind> allPayloadKinds = Enum.GetValues(typeof(ODataPayloadKind)).Cast<ODataPayloadKind>();
            var formatter = new ODataMediaTypeFormatter(allPayloadKinds);
            formatter.Request = request;

            var result = formatter.CanWriteType(type);

            Assert.False(result);
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsSerializationException_IfEdmTypeIsNull()
        {
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            var formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[0]);
            formatter.Request = request;

            Mock<IEdmObject> edmObject = new Mock<IEdmObject>();

            Assert.Throws<SerializationException>(
                () => formatter
                    .WriteToStreamAsync(typeof(int), edmObject.Object, new MemoryStream(), new Mock<HttpContent>().Object, transportContext: null)
                    .Wait(),
                "The EDM type of the object of type 'Castle.Proxies.IEdmObjectProxy' is null. The EDM type of an IEdmObject cannot be null.");
        }

        [Fact]
        public void WriteToStreamAsync_UsesTheRightEdmSerializer_ForEdmObjects()
        {
            // Arrange
            IEdmEntityTypeReference edmType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);

            Mock<IEdmObject> instance = new Mock<IEdmObject>();
            instance.Setup(e => e.GetEdmType()).Returns(edmType);

            Mock<ODataEdmTypeSerializer> serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Entry);
            serializer
                .Setup(s => s.WriteObject(instance.Object, instance.GetType(), It.IsAny<ODataMessageWriter>(), It.IsAny<ODataSerializerContext>()))
                .Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(serializer.Object);

            var formatter = new ODataMediaTypeFormatter(new DefaultODataDeserializerProvider(), serializerProvider.Object, new ODataPayloadKind[0]);
            formatter.Request = request;

            // Act
            formatter
                .WriteToStreamAsync(instance.GetType(), instance.Object, new MemoryStream(), new StreamContent(new MemoryStream()), transportContext: null)
                .Wait();

            // Assert
            serializer.Verify();
        }

        [Theory]
        [InlineData(typeof(SingleResult), false)]
        [InlineData(typeof(SingleResult<SampleType>), true)]
        [InlineData(typeof(SingleResult<TypeNotInModel>), false)]
        public void CanWriteType_ReturnsExpectedResult_ForSingleResult(Type type, bool expectedCanWriteTypeResult)
        {
            IEdmModel model = CreateModel();
            HttpRequestMessage request = CreateFakeODataRequest(model);
            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, ODataPayloadKind.Entry);

            Assert.Equal(expectedCanWriteTypeResult, formatter.CanWriteType(type));
        }

        [Fact]
        public void WriteToStreamAsync_SetsMetadataUriWithSelectClause_OnODataWriterSettings()
        {
            // Arrange
            MemoryStream stream = new MemoryStream();
            StreamContent content = new StreamContent(stream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            IEdmModel model = CreateModel();
            HttpRequestMessage request = CreateFakeODataRequest(model);
            request.RequestUri = new Uri("http://localhost/Customers?$select=something");
            request.ODataProperties().SelectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);

            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, ODataPayloadKind.Entry);

            // Act
            formatter.WriteToStreamAsync(typeof(SampleType[]), new SampleType[0], stream, content, transportContext: null);

            // Assert
            // This is ugly, but ODataWriter doesn't expose the writer settings that it uses. So, validate that
            // the $select clause shows up in the response payload.
            stream.Seek(0, SeekOrigin.Begin);
            string result = content.ReadAsStringAsync().Result;
            Assert.Contains("$select=something", result);
        }

        [Fact]
        public void ReadFromStreamAsync_UsesRightDeserializerFrom_ODataDeserializerProvider()
        {
            // Arrange
            MemoryStream stream = new MemoryStream();
            StringContent content = new StringContent("42");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            IEdmModel model = CreateModel();
            HttpRequestMessage request = CreateFakeODataRequest(model);
            Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Property);
            deserializer.Setup(d => d.Read(It.IsAny<ODataMessageReader>(), typeof(int), It.IsAny<ODataDeserializerContext>()))
                .Verifiable();

            Mock<ODataDeserializerProvider> provider = new Mock<ODataDeserializerProvider>();
            provider.Setup(p => p.GetODataDeserializer(model, typeof(int), request)).Returns(deserializer.Object);

            // Act
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(provider.Object,
                new DefaultODataSerializerProvider(), Enumerable.Empty<ODataPayloadKind>());
            formatter.Request = request;

            formatter.ReadFromStreamAsync(typeof(int), stream, content, null);

            // Assert
            deserializer.Verify();
        }

        private static Encoding CreateEncoding(string name)
        {
            if (name == "utf-8")
            {
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            }
            else if (name == "utf-16")
            {
                return new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);
            }
            else
            {
                throw new ArgumentException("name");
            }
        }

        private static string CreateFormattedContent(string value)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{{\r\n  \"odata.metadata\":\"http://dummy/#Edm.String\",\"value\":\"{0}\"\r\n}}", value);
        }

        protected override ODataMediaTypeFormatter CreateFormatter()
        {
            return CreateFormatterWithRequest();
        }

        protected override Mock<ODataMediaTypeFormatter> CreateMockFormatter()
        {
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            ODataPayloadKind[] payloadKinds = new ODataPayloadKind[] { ODataPayloadKind.Property };
            var formatter = new Mock<ODataMediaTypeFormatter>(payloadKinds) { CallBase = true };
            formatter.Object.Request = request;

            return formatter;
        }

        protected override MediaTypeHeaderValue CreateSupportedMediaType()
        {
            return new MediaTypeHeaderValue("application/atom+xml");
        }

        private static ODataMediaTypeFormatter CreateFormatter(IEdmModel model)
        {
            return new ODataMediaTypeFormatter(new ODataPayloadKind[0]);
        }

        private static ODataMediaTypeFormatter CreateFormatter(IEdmModel model, HttpRequestMessage request,
            params ODataPayloadKind[] payloadKinds)
        {
            return new ODataMediaTypeFormatter(payloadKinds) { Request = request };
        }

        private static ODataMediaTypeFormatter CreateFormatterWithoutRequest()
        {
            return CreateFormatter(CreateModel());
        }

        private static ODataMediaTypeFormatter CreateFormatterWithJson(IEdmModel model, HttpRequestMessage request,
            params ODataPayloadKind[] payloadKinds)
        {
            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, payloadKinds);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateFormatterWithRequest()
        {
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            return CreateFormatter(model, request);
        }

        private static HttpRequestMessage CreateFakeODataRequest(IEdmModel model)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://dummy/");
            request.ODataProperties().Model = model;
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapFakeODataRoute();
            request.SetConfiguration(configuration);
            request.ODataProperties().Path =
                new ODataPath(new EntitySetPathSegment(model.EntityContainers().Single().EntitySets().Single()));
            request.SetFakeODataRouteName();
            return request;
        }

        private static IEdmModel CreateModel()
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.Entity<SampleType>();
            model.EntitySet<SampleType>("sampleTypes");
            return model.GetEdmModel();
        }

        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get
            {
                return new MediaTypeHeaderValue[0];
            }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get
            {
                return new Encoding[0];
            }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get
            {
                return Encoding.UTF8.GetBytes(
                  @"<entry xml:base=""http://localhost/"" xmlns=""http://www.w3.org/2005/Atom"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:georss=""http://www.georss.org/georss"" xmlns:gml=""http://www.opengis.net/gml"">
                      <category term=""System.Net.Http.Formatting.SampleType"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"" />
                      <id />
                      <title />
                      <updated>2012-08-17T00:16:14Z</updated>
                      <author>
                        <name />
                      </author>
                      <content type=""application/xml"">
                        <m:properties>
                          <d:Number m:type=""Edm.Int32"">42</d:Number>
                        </m:properties>
                      </content>
                    </entry>"
                );
            }
        }

        private class TypeNotInModel
        {
        }
    }
}
