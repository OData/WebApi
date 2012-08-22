// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class PrimitiveTypeTest
    {
        [Fact]
        public void CreateByteArrayPrimitiveProperty()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var file = builder.Entity<File>();
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
            var file = builder.Entity<File>();
            var data = file.Property(f => f.StreamData);

            var model = builder.GetServiceModel();
            var fileType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var streamProperty = fileType.DeclaredProperties.SingleOrDefault(p => p.Name == "StreamData");

            Assert.Equal(PropertyKind.Primitive, data.Kind);

            Assert.NotNull(streamProperty);
            Assert.Equal("Edm.Stream", streamProperty.Type.FullName());
        }
    }

    public class File
    {
        public byte[] Data { get; set; }

        public Stream StreamData { get; set; }
    }
}
