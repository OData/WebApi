// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataPrimitiveSerializerTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentNull_edmPrimitiveType()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                var serializer = new ODataPrimitiveSerializer(edmPrimitiveType: null);
            }, "edmType");
        }

        [Theory]
        [PropertyData("EdmPrimitiveKinds")]
        public void Constructor_SucceedsForValidPrimitiveType(EdmPrimitiveTypeKind primitiveTypeKind)
        {
            IEdmPrimitiveType edmPrimitiveType = EdmCoreModel.Instance.SchemaElements
                                                                .OfType<IEdmPrimitiveType>()
                                                                .Where(primitiveType => primitiveType.PrimitiveKind == primitiveTypeKind)
                                                                .FirstOrDefault();
            IEdmPrimitiveTypeReference edmPrimitiveTypeReference = new EdmPrimitiveTypeReference(edmPrimitiveType, false);

            var serializer = new ODataPrimitiveSerializer(edmPrimitiveTypeReference);

            Assert.Equal(serializer.EdmType, edmPrimitiveTypeReference);
            Assert.Equal(serializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Fact]
        public void CreateProperty()
        {
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(int));
            var serializer = new ODataPrimitiveSerializer(edmPrimitiveType);

            var odataProperty = serializer.CreateProperty(20, "elementName", writeContext: null);
            Assert.NotNull(odataProperty);
            Assert.Equal(odataProperty.Name, "elementName");
            Assert.Equal(odataProperty.Value, 20);
        }

        public static TheoryDataSet<EdmPrimitiveTypeKind> EdmPrimitiveKinds
        {
            get
            {
                TheoryDataSet<EdmPrimitiveTypeKind> dataset = new TheoryDataSet<EdmPrimitiveTypeKind>();
                var primitiveKinds = Enum.GetValues(typeof(EdmPrimitiveTypeKind))
                                        .OfType<EdmPrimitiveTypeKind>()
                                        .Where(primitiveKind => primitiveKind != EdmPrimitiveTypeKind.None);

                foreach (var primitiveKind in primitiveKinds)
                {
                    dataset.Add(primitiveKind);
                }
                return dataset;
            }
        }
    }
}
