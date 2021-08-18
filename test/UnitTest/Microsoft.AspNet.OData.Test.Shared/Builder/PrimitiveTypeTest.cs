//-----------------------------------------------------------------------------
// <copyright file="PrimitiveTypeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
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
        public void CreateDatePrimitiveProperty_FromDateTime()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<File> file = builder.EntityType<File>();
            file.Property(f => f.Birthday).AsDate();
            file.Property(f => f.PublishDay).AsDate();

            // Act
            IEdmModel model = builder.GetServiceModel();

            // Assert
            IEdmEntityType fileType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());

            IEdmProperty birthdayProperty = Assert.Single(fileType.DeclaredProperties.Where(p => p.Name == "Birthday"));
            Assert.NotNull(birthdayProperty);
            Assert.False(birthdayProperty.Type.IsNullable);
            Assert.Equal("Edm.Date", birthdayProperty.Type.FullName());

            IEdmProperty publishDayProperty = Assert.Single(fileType.DeclaredProperties.Where(p => p.Name == "PublishDay"));
            Assert.NotNull(publishDayProperty);
            Assert.True(publishDayProperty.Type.IsNullable);
            Assert.Equal("Edm.Date", publishDayProperty.Type.FullName());
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

        [Fact]
        public void CreateTimeOfDayPrimitiveProperty_FromTimeSpan()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<File> file = builder.EntityType<File>();
            file.Property(f => f.CreatedTime).AsTimeOfDay();
            file.Property(f => f.EndTime).AsTimeOfDay();

            // Act
            IEdmModel model = builder.GetServiceModel();

            // Assert
            IEdmEntityType fileType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());

            IEdmProperty createProperty = Assert.Single(fileType.DeclaredProperties.Where(p => p.Name == "CreatedTime"));
            Assert.NotNull(createProperty);
            Assert.False(createProperty.Type.IsNullable);
            Assert.Equal("Edm.TimeOfDay", createProperty.Type.FullName());

            IEdmProperty endProperty = Assert.Single(fileType.DeclaredProperties.Where(p => p.Name == "EndTime"));
            Assert.NotNull(endProperty);
            Assert.True(endProperty.Type.IsNullable);
            Assert.Equal("Edm.TimeOfDay", endProperty.Type.FullName());
        }
    }

    public class File
    {
        public byte[] Data { get; set; }

        public Stream StreamData { get; set; }

        public Date DateProperty { get; set; }

        public TimeOfDay TimeOfDayProperty { get; set; }

        public DateTime Birthday { get; set; }
        public DateTime? PublishDay { get; set; }

        public TimeSpan CreatedTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
