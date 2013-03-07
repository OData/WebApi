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
        private ODataSerializerContext _serializerContext = new ODataSerializerContext();
        private IEdmEntityType _entityType = new Mock<IEdmEntityType>().Object;
        private object _entityInstance = new object();
        private EntityInstanceContext _context = new EntityInstanceContext();

        [Fact]
        public void EmptyCtor_InitializesProperty_SerializerContext()
        {
            var context = new EntityInstanceContext();
            Assert.NotNull(context.SerializerContext);
        }

        [Fact]
        public void Ctor_SetsProperty_SerializerContext()
        {
            var context = new EntityInstanceContext(_serializerContext, _entityType, _entityInstance);
            Assert.Equal(_serializerContext, context.SerializerContext);
        }

        [Fact]
        public void Ctor_SetsProperty_EntityType()
        {
            var context = new EntityInstanceContext(_serializerContext, _entityType, _entityInstance);
            Assert.Equal(_entityType, context.EntityType);
        }

        [Fact]
        public void Ctor_SetsProperty_EntityInstance()
        {
            var context = new EntityInstanceContext(_serializerContext, _entityType, _entityInstance);
            Assert.Equal(_entityInstance, context.EntityInstance);
        }

        [Fact]
        public void Property_EdmModel_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.EdmModel, null, allowNull: true, roundTripTestValue: EdmCoreModel.Instance);
        }

        [Fact]
        public void Property_EntityInstance_RoundTrips()
        {
            Assert.Reflection.Property(_context, (c) => c.EntityInstance, null, allowNull: true, roundTripTestValue: _entityInstance);
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

    }
}
