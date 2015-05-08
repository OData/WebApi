// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class PrimitiveTypeTest
    {
        [Fact]
        public void CreateByteArrayPrimitiveProperty()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var file = builder.EntityType<File>();
            var data = file.Property(f => f.Data);

            var model = builder.GetServiceModel();
            var fileType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var dataProperty = fileType.DeclaredProperties.SingleOrDefault(p => p.Name == "Data");

            Assert.Equal(PropertyKind.Primitive, data.Kind);

            Assert.NotNull(dataProperty);
            Assert.Equal("Edm.Binary", dataProperty.Type.FullName());
        }

        [Fact]
        public void CreateStreamPrimitiveProperty()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var file = builder.EntityType<File>();
            var data = file.Property(f => f.StreamData);

            var model = builder.GetServiceModel();
            var fileType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var streamProperty = fileType.DeclaredProperties.SingleOrDefault(p => p.Name == "StreamData");

            Assert.Equal(PropertyKind.Primitive, data.Kind);

            Assert.NotNull(streamProperty);
            Assert.Equal("Edm.Stream", streamProperty.Type.FullName());
        }

        [Fact]
        public void CreateDatePrimitiveProperty()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<File> file = builder.EntityType<File>();
            PrimitivePropertyConfiguration date = file.Property(f => f.DateProperty);

            // Act
            IEdmModel model = builder.GetServiceModel();

            // Assert
            Assert.Equal(PropertyKind.Primitive, date.Kind);

            IEdmEntityType fileType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());

            IEdmProperty dateProperty = Assert.Single(fileType.DeclaredProperties.Where(p => p.Name == "DateProperty"));
            Assert.NotNull(dateProperty);
            Assert.Equal("Edm.Date", dateProperty.Type.FullName());
        }

        [Fact]
        public void CreateTimeOfDayPrimitiveProperty()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<File> file = builder.EntityType<File>();
            PrimitivePropertyConfiguration timeOfDay = file.Property(f => f.TimeOfDayProperty);

            // Act
            IEdmModel model = builder.GetServiceModel();

            // Assert
            Assert.Equal(PropertyKind.Primitive, timeOfDay.Kind);

            IEdmEntityType fileType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());

            IEdmProperty property = Assert.Single(fileType.DeclaredProperties.Where(p => p.Name == "TimeOfDayProperty"));
            Assert.NotNull(property);
            Assert.Equal("Edm.TimeOfDay", property.Type.FullName());
        }
    }

    public class File
    {
        public byte[] Data { get; set; }

        public Stream StreamData { get; set; }

        public Date DateProperty { get; set; }

        public TimeOfDay TimeOfDayProperty { get; set; }
    }
}
