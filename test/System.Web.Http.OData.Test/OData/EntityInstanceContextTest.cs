// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class EntityInstanceContextTest
    {
        private ODataSerializerContext _serializerContext = new ODataSerializerContext { Model = EdmCoreModel.Instance };
        private IEdmEntityType _entityType = new EdmEntityType("NS", "Name");
        private object _entityInstance = new object();
        private EntityInstanceContext _context = new EntityInstanceContext();

        [Fact]
        public void EmptyCtor_InitializesProperty_SerializerContext()
        {
            var context = new EntityInstanceContext();
            Assert.NotNull(context.SerializerContext);
        }

        [Fact]
        public void Property_EntityInstance_RoundTrips()
        {
#pragma warning disable 618
            Assert.Reflection.Property(_context, (c) => c.EntityInstance, null, allowNull: true, roundTripTestValue: _entityInstance);
#pragma warning restore 618
        }

        [Fact]
        public void Property_EdmModel_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.EdmModel, null, allowNull: true, roundTripTestValue: EdmCoreModel.Instance);
        }

        [Fact]
        public void Property_EntitySet_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.EntitySet, null, allowNull: true, roundTripTestValue: new Mock<IEdmEntitySet>().Object);
        }

        [Fact]
        public void Property_EntityType_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.EntityType, null, allowNull: true, roundTripTestValue: _entityType);
        }

        [Fact]
        public void Property_Request_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.Request, null, allowNull: true, roundTripTestValue: new HttpRequestMessage());
        }

        [Fact]
        public void Property_SerializerContext_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.SerializerContext, _context.SerializerContext, allowNull: true, roundTripTestValue: new ODataSerializerContext());
        }

        [Fact]
        public void Property__RoundTrips()
        {
            Assert.Reflection.BooleanProperty(_context, (c) => c.SkipExpensiveAvailabilityChecks, false);
        }

        [Fact]
        public void Property_Url_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.Url, null, allowNull: true, roundTripTestValue: new UrlHelper(new HttpRequestMessage()));
        }

        [Fact]
        public void GetPropertyValue_ThrowsInvalidOperation_IfPropertyIsNotFound()
        {
            IEdmEntityTypeReference entityType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            Mock<IEdmStructuredObject> edmObject = new Mock<IEdmStructuredObject>();
            edmObject.Setup(o => o.GetEdmType()).Returns(entityType);
            EntityInstanceContext instanceContext = new EntityInstanceContext(_serializerContext, entityType, edmObject.Object);

            Assert.Throws<InvalidOperationException>(
                () => instanceContext.GetPropertyValue("NotPresentProperty"),
                "The EDM instance of type '[NS.Name Nullable=False]' is missing the property 'NotPresentProperty'.");
        }

        [Fact]
        public void GetPropertyValue_ThrowsInvalidOperation_IfEdmObjectIsNull()
        {
            EntityInstanceContext instanceContext = new EntityInstanceContext();
            Assert.Throws<InvalidOperationException>(
                () => instanceContext.GetPropertyValue("SomeProperty"),
                "The property 'EdmObject' of EntityInstanceContext cannot be null.");
        }

        private class TestEntity
        {
            public int Property { get; set; }
        }
    }
}
