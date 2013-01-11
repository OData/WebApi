// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataEntityTypeSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer _customer;
        ODataEntityTypeSerializer _serializer;
        ODataSerializerContext _writeContext;

        public ODataEntityTypeSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();

            _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindType("Default.Customer"), new ClrTypeAnnotation(typeof(Customer)));
            _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindType("Default.Order"), new ClrTypeAnnotation(typeof(Order)));

            _customerSet = _model.FindDeclaredEntityContainer("Default.Container").FindEntitySet("Customers");
            _customer = new Customer()
            {
                FirstName = "Foo",
                LastName = "Bar",
                ID = 10,
            };

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            _serializer = new ODataEntityTypeSerializer(
                new EdmEntityTypeReference(_customerSet.ElementType, isNullable: false),
                serializerProvider);
            _writeContext = new ODataSerializerContext() { EntitySet = _customerSet, Model = _model };
        }

        [Fact]
        public void WriteObjectInline_UsesCorrectTypeName()
        {
            // Arrange & Assert
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal("Default.Customer", entry.TypeName);
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectIdLink()
        {
            // Arrange
            bool customIdLinkbuilderCalled = false;
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                IdLinkBuilder = new SelfLinkBuilder<string>((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customIdLinkbuilderCalled = true;
                    return "http://sample_id_link";
                },
                followsConventions: false)
            };
            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.Id, "http://sample_id_link");
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customIdLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectEditLink()
        {
            // Arrange
            bool customEditLinkbuilderCalled = false;
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                EditLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customEditLinkbuilderCalled = true;
                    return new Uri("http://sample_edit_link");
                },
                followsConventions: false)
            };
            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.EditLink, new Uri("http://sample_edit_link"));
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customEditLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectReadLink()
        {
            // Arrange
            bool customReadLinkbuilderCalled = false;
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                ReadLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customReadLinkbuilderCalled = true;
                    return new Uri("http://sample_read_link");
                },
                followsConventions: false)
            };

            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.ReadLink, new Uri("http://sample_read_link"));
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customReadLinkbuilderCalled);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation_InDefaultMetadataMode()
        {
            // Arrange
            ODataEntry entry = new ODataEntry();

            // Act
            ODataEntityTypeSerializer.AddTypeNameAnnotationAsNeeded(entry, null, ODataMetadataLevel.Default);

            // Assert
            Assert.Null(entry.GetAnnotation<SerializationTypeNameAnnotation>());
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataEntry entry = new ODataEntry
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataEntityTypeSerializer.AddTypeNameAnnotationAsNeeded(entry, null, ODataMetadataLevel.MinimalMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = entry.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Equal(expectedTypeName, annotation.TypeName);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Default, false)]
        [InlineData(ODataMetadataLevel.FullMetadata, false)]
        [InlineData(ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(ODataMetadataLevel.NoMetadata, true)]
        public void ShouldAddTypeNameAnnotation(ODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldAddTypeNameAnnotation(metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("MatchingType", "MatchingType", ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData("DoesNotMatch1", "DoesNotMatch2", ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData("IgnoredEntryType", "IgnoredEntitySetType", ODataMetadataLevel.NoMetadata, true)]
        public void ShouldSuppressTypeNameSerialization(string entryType, string entitySetType,
            ODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Arrange
            ODataEntry entry = new ODataEntry
            {
                TypeName = entryType
            };
            IEdmEntitySet entitySet = CreateEntitySetWithElementTypeName(entitySetType);

            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldSuppressTypeNameSerialization(entry, null, metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void CreateODataAction_ForAtom_IncludesEverything()
        {
            // Arrange
            string expectedContainerName = "Container";
            string expectedActionName = "Action";
            string expectedTarget = "aa://Target";
            string expectedMetadataPrefix = "http://Metadata";

            IEdmEntityContainer container = CreateFakeContainer(expectedContainerName);
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, expectedActionName, true);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri(expectedTarget), true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(functionImport);
            annotationsManager.SetDefaultContainer(container);
            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory(expectedMetadataPrefix);

            EntityInstanceContext context = CreateContext(model, url);

            // Act
            ODataAction actualAction = ODataEntityTypeSerializer.CreateODataAction(functionImport, context,
                ODataMetadataLevel.Default);

            // Assert
            string expectedMetadata = expectedMetadataPrefix + "#" + expectedContainerName + "." + expectedActionName;
            ODataAction expectedAction = new ODataAction
            {
                Metadata = new Uri(expectedMetadata),
                Target = new Uri(expectedTarget),
                Title = expectedActionName
            };

            AssertEqual(expectedAction, actualAction);
        }

        [Fact]
        public void CreateODataAction_OmitsAction_WhenActionLinkBuilderReturnsNull()
        {
            // Arrange
            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, "IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => null, false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);

            EntityInstanceContext context = CreateContext(model);

            // Act
            ODataAction actualAction = ODataEntityTypeSerializer.CreateODataAction(functionImport, context,
                ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Null(actualAction);
        }

        [Fact]
        public void CreateODataAction_ForJsonLight_OmitsContainerName_PerCreateMetadataFragment()
        {
            // Arrange
            string expectedMetadataPrefix = "http://Metadata";
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer("ContainerShouldNotAppearInResult");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, expectedActionName);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"), false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);
            annotationsManager.SetDefaultContainer(container);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory(expectedMetadataPrefix);

            EntityInstanceContext context = CreateContext(model, url);

            // Act
            ODataAction actualAction = ODataEntityTypeSerializer.CreateODataAction(functionImport, context,
                ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.NotNull(actualAction);
            string expectedMetadata = expectedMetadataPrefix + "#" + expectedActionName;
            AssertEqual(new Uri(expectedMetadata), actualAction.Metadata);
        }

        [Fact]
        public void CreateODataAction_SkipsAlwaysAvailableAction_PerShouldOmitAction()
        {
            // Arrange
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(true);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"), false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);
            annotationsManager.SetIsAlwaysBindable(functionImport);

            IEdmModel model = CreateFakeModel(annotationsManager);

            EntityInstanceContext context = CreateContext(model);

            // Act
            ODataAction actualAction = ODataEntityTypeSerializer.CreateODataAction(functionImport, context,
                ODataMetadataLevel.MinimalMetadata);

            // Assert
            Assert.Null(actualAction);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Default)]
        [InlineData(ODataMetadataLevel.FullMetadata)]
        public void CreateODataAction_IncludesTitle(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, expectedActionName);

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://IgnoreTarget"), false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);

            // Act
            ODataAction actualAction = ODataEntityTypeSerializer.CreateODataAction(functionImport, context,
                metadataLevel);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Equal(expectedActionName, actualAction.Title);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.MinimalMetadata)]
        [InlineData(ODataMetadataLevel.NoMetadata)]
        public void CreateODataAction_OmitsTitle(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, "IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://Ignore"), false);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);

            // Act
            ODataAction actualAction = ODataEntityTypeSerializer.CreateODataAction(functionImport, context,
                metadataLevel);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Null(actualAction.Title);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Default, false)]
        [InlineData(ODataMetadataLevel.Default, true)]
        [InlineData(ODataMetadataLevel.FullMetadata, false)]
        [InlineData(ODataMetadataLevel.FullMetadata, true)]
        [InlineData(ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData(ODataMetadataLevel.NoMetadata, false)]
        public void CreateODataAction_IncludesTarget(ODataMetadataLevel metadataLevel, bool followsConventions)
        {
            // Arrange
            Uri expectedTarget = new Uri("aa://Target");

            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, "IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => expectedTarget, followsConventions);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);

            // Act
            ODataAction actualAction = ODataEntityTypeSerializer.CreateODataAction(functionImport, context,
                metadataLevel);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Equal(expectedTarget, actualAction.Target);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.MinimalMetadata)]
        [InlineData(ODataMetadataLevel.NoMetadata)]
        public void CreateODataAction_OmitsTarget_WhenFollowingConventions(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            IEdmEntityContainer container = CreateFakeContainer("IgnoreContainer");
            IEdmFunctionImport functionImport = CreateFakeFunctionImport(container, "IgnoreAction");

            ActionLinkBuilder linkBuilder = new ActionLinkBuilder((a) => new Uri("aa://Ignore"), true);
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetActionLinkBuilder(functionImport, linkBuilder);

            IEdmModel model = CreateFakeModel(annotationsManager);
            UrlHelper url = CreateMetadataLinkFactory("http://IgnoreMetadataPath");

            EntityInstanceContext context = CreateContext(model, url);

            // Act
            ODataAction actualAction = ODataEntityTypeSerializer.CreateODataAction(functionImport, context,
                metadataLevel);

            // Assert
            Assert.NotNull(actualAction);
            Assert.Null(actualAction.Target);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Default)]
        [InlineData(ODataMetadataLevel.FullMetadata)]
        [InlineData(ODataMetadataLevel.MinimalMetadata)]
        [InlineData(ODataMetadataLevel.NoMetadata)]
        public void CreateMetadataFragment_IncludesNonDefaultContainerName(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            string expectedContainerName = "Container";
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer(expectedContainerName);
            IEdmFunctionImport action = CreateFakeFunctionImport(container, expectedActionName);

            IEdmModel model = CreateFakeModel();

            // Act
            string actualFragment = ODataEntityTypeSerializer.CreateMetadataFragment(action, model, metadataLevel);

            // Assert
            Assert.Equal(expectedContainerName + "." + expectedActionName, actualFragment);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Default)]
        [InlineData(ODataMetadataLevel.FullMetadata)]
        public void CreateMetadataFragment_IncludesDefaultContainerName(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            string expectedContainerName = "Container";
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer(expectedContainerName);
            IEdmFunctionImport action = CreateFakeFunctionImport(container, expectedActionName);

            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetDefaultContainer(container);
            IEdmModel model = CreateFakeModel(annotationsManager);

            // Act
            string actualFragment = ODataEntityTypeSerializer.CreateMetadataFragment(action, model, metadataLevel);

            // Assert
            Assert.Equal(expectedContainerName + "." + expectedActionName, actualFragment);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.MinimalMetadata)]
        [InlineData(ODataMetadataLevel.NoMetadata)]
        public void CreateMetadataFragment_OmitsDefaultContainerName(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            string expectedActionName = "Action";

            IEdmEntityContainer container = CreateFakeContainer("ContainerShouldNotAppearInResult");
            IEdmFunctionImport action = CreateFakeFunctionImport(container, expectedActionName);

            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            annotationsManager.SetDefaultContainer(container);
            IEdmModel model = CreateFakeModel(annotationsManager);

            // Act
            string actualFragment = ODataEntityTypeSerializer.CreateMetadataFragment(action, model, metadataLevel);

            // Assert
            Assert.Equal(expectedActionName, actualFragment);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Default, false, false)]
        [InlineData(ODataMetadataLevel.Default, true, false)]
        [InlineData(ODataMetadataLevel.FullMetadata, false, false)]
        [InlineData(ODataMetadataLevel.FullMetadata, true, false)]
        [InlineData(ODataMetadataLevel.MinimalMetadata, false, false)]
        [InlineData(ODataMetadataLevel.MinimalMetadata, true, true)]
        [InlineData(ODataMetadataLevel.NoMetadata, false, false)]
        [InlineData(ODataMetadataLevel.NoMetadata, true, true)]
        public void TestShouldOmitAction(ODataMetadataLevel metadataLevel, bool isAlwaysAvailable, bool expectedResult)
        {
            // Arrange
            IEdmFunctionImport action = CreateFakeFunctionImport(true);
            IEdmDirectValueAnnotationsManager annonationsManager = CreateFakeAnnotationsManager();

            if (isAlwaysAvailable)
            {
                annonationsManager.SetIsAlwaysBindable(action);
            }

            IEdmModel model = CreateFakeModel(annonationsManager);

            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldOmitAction(action, model, metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        private static void AssertEqual(ODataAction expected, ODataAction actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            AssertEqual(expected.Metadata, actual.Metadata);
            AssertEqual(expected.Target, actual.Target);
            Assert.Equal(expected.Title, actual.Title);
        }

        private static void AssertEqual(Uri expected, Uri actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(expected.AbsoluteUri, actual.AbsoluteUri);
        }

        private static EntityInstanceContext CreateContext(IEdmModel model)
        {
            return new EntityInstanceContext
            {
                EdmModel = model
            };
        }

        private static EntityInstanceContext CreateContext(IEdmModel model, UrlHelper url)
        {
            return new EntityInstanceContext
            {
                EdmModel = model,
                Url = url,
            };
        }

        private static IEdmEntitySet CreateEntitySetWithElementTypeName(string typeName)
        {
            Mock<IEdmEntityType> entityTypeMock = new Mock<IEdmEntityType>();
            entityTypeMock.Setup(o => o.Name).Returns(typeName);
            IEdmEntityType entityType = entityTypeMock.Object;
            Mock<IEdmEntitySet> entitySetMock = new Mock<IEdmEntitySet>();
            entitySetMock.Setup(o => o.ElementType).Returns(entityType);
            return entitySetMock.Object;
        }

        private static IEdmDirectValueAnnotationsManager CreateFakeAnnotationsManager()
        {
            return new FakeAnnotationsManager();
        }

        private static IEdmEntityContainer CreateFakeContainer(string name)
        {
            Mock<IEdmEntityContainer> mock = new Mock<IEdmEntityContainer>();
            mock.Setup(o => o.Name).Returns(name);
            return mock.Object;
        }

        private static IEdmFunctionImport CreateFakeFunctionImport(IEdmEntityContainer container, string name)
        {
            Mock<IEdmFunctionImport> mock = new Mock<IEdmFunctionImport>();
            mock.Setup(o => o.Container).Returns(container);
            mock.Setup(o => o.Name).Returns(name);
            return mock.Object;
        }

        private static IEdmFunctionImport CreateFakeFunctionImport(IEdmEntityContainer container, string name,
            bool isBindable)
        {
            Mock<IEdmFunctionImport> mock = new Mock<IEdmFunctionImport>();
            mock.Setup(o => o.Container).Returns(container);
            mock.Setup(o => o.Name).Returns(name);
            mock.Setup(o => o.IsBindable).Returns(isBindable);
            return mock.Object;
        }

        private static IEdmFunctionImport CreateFakeFunctionImport(bool isBindable)
        {
            Mock<IEdmFunctionImport> mock = new Mock<IEdmFunctionImport>();
            mock.Setup(o => o.IsBindable).Returns(isBindable);
            return mock.Object;
        }

        private static IEdmModel CreateFakeModel()
        {
            IEdmDirectValueAnnotationsManager annotationsManager = CreateFakeAnnotationsManager();
            return CreateFakeModel(annotationsManager);
        }

        private static IEdmModel CreateFakeModel(IEdmDirectValueAnnotationsManager annotationsManager)
        {
            Mock<IEdmModel> model = new Mock<IEdmModel>();
            model.Setup(m => m.DirectValueAnnotationsManager).Returns(annotationsManager);
            return model.Object;
        }

        private static UrlHelper CreateMetadataLinkFactory(string metadataPath)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, metadataPath);
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapFakeODataRoute();
            request.SetConfiguration(configuration);
            return new UrlHelper(request);
        }

        private class Customer
        {
            public Customer()
            {
                this.Orders = new List<Order>();
            }
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public IList<Order> Orders { get; private set; }
        }

        private class Order
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public Customer Customer { get; set; }
        }

        private class FakeAnnotationsManager : IEdmDirectValueAnnotationsManager
        {
            IDictionary<Tuple<IEdmElement, string, string>, object> annotations =
                new Dictionary<Tuple<IEdmElement, string, string>, object>();

            public object GetAnnotationValue(IEdmElement element, string namespaceName, string localName)
            {
                object value;

                if (!annotations.TryGetValue(CreateKey(element, namespaceName, localName), out value))
                {
                    return null;
                }

                return value;
            }

            public object[] GetAnnotationValues(IEnumerable<IEdmDirectValueAnnotationBinding> annotations)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IEdmDirectValueAnnotation> GetDirectValueAnnotations(IEdmElement element)
            {
                throw new NotImplementedException();
            }

            public void SetAnnotationValue(IEdmElement element, string namespaceName, string localName, object value)
            {
                annotations[CreateKey(element, namespaceName, localName)] = value;
            }

            public void SetAnnotationValues(IEnumerable<IEdmDirectValueAnnotationBinding> annotations)
            {
                throw new NotImplementedException();
            }

            private static Tuple<IEdmElement, string, string> CreateKey(IEdmElement element, string namespaceName,
                string localName)
            {
                return new Tuple<IEdmElement, string, string>(element, namespaceName, localName);
            }
        }

    }
}
