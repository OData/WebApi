// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter.Serialization.Models;
using System.Web.Http.OData.Query.Expressions;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class EdmLibHelpersTests
    {
        [Theory]
        [InlineData(typeof(Customer), "Customer")]
        [InlineData(typeof(int), "Int32")]
        [InlineData(typeof(IEnumerable<int>), "IEnumerable_1OfInt32")]
        [InlineData(typeof(IEnumerable<Func<int, string>>), "IEnumerable_1OfFunc_2OfInt32_String")]
        [InlineData(typeof(List<Func<int, string>>), "List_1OfFunc_2OfInt32_String")]
        public void EdmFullName(Type clrType, string expectedName)
        {
            Assert.Equal(expectedName, clrType.EdmName());
        }

        [Theory]
        [InlineData(typeof(char), typeof(string))]
        [InlineData(typeof(char?), typeof(string))]
        [InlineData(typeof(ushort), typeof(int))]
        [InlineData(typeof(uint), typeof(long))]
        [InlineData(typeof(ulong), typeof(long))]
        [InlineData(typeof(ushort?), typeof(int?))]
        [InlineData(typeof(uint?), typeof(long?))]
        [InlineData(typeof(ulong?), typeof(long?))]
        [InlineData(typeof(char[]), typeof(string))]
        [InlineData(typeof(Binary), typeof(byte[]))]
        [InlineData(typeof(XElement), typeof(string))]
        public void IsNonstandardEdmPrimitive_Returns_True(Type primitiveType, Type mappedType)
        {
            bool isNonstandardEdmPrimtive;
            Type resultMappedType = EdmLibHelpers.IsNonstandardEdmPrimitive(primitiveType, out isNonstandardEdmPrimtive);

            Assert.True(isNonstandardEdmPrimtive);
            Assert.Equal(mappedType, resultMappedType);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(TimeSpan))]
        public void IsNonstandardEdmPrimitive_Returns_False(Type primitiveType)
        {
            bool isNonstandardEdmPrimtive;
            Type resultMappedType = EdmLibHelpers.IsNonstandardEdmPrimitive(primitiveType, out isNonstandardEdmPrimtive);

            Assert.False(isNonstandardEdmPrimtive);
            Assert.Equal(primitiveType, resultMappedType);
        }

        [Fact]
        public void GetEdmType_ReturnsBaseType()
        {
            IEdmModel model = GetEdmModel();
            Assert.Equal(model.GetEdmType(typeof(BaseType)), model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.Name == "BaseType").Single());
        }

        [Fact]
        public void GetEdmType_ReturnsDerivedType()
        {
            IEdmModel model = GetEdmModel();
            Assert.Equal(model.GetEdmType(typeof(DerivedTypeA)), model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.Name == "DerivedTypeA").Single());
            Assert.Equal(model.GetEdmType(typeof(DerivedTypeB)), model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.Name == "DerivedTypeB").Single());
        }

        [Fact]
        public void GetEdmType_Returns_NearestDerivedType()
        {
            IEdmModel model = GetEdmModel();
            Assert.Equal(model.GetEdmType(typeof(DerivedTypeAA)), model.SchemaElements.OfType<IEdmEntityType>().Where(t => t.Name == "DerivedTypeA").Single());
        }

        [Fact]
        public void GetEdmType_ReturnsNull_ForUnknownType()
        {
            IEdmModel model = GetEdmModel();
            Assert.Null(model.GetEdmType(typeof(TypeNotInModel)));
        }

        [Fact]
        public void GetEdmType_ReturnsCollection_ForIEnumerableOfT()
        {
            IEdmModel model = GetEdmModel();
            IEdmType edmType = model.GetEdmType(typeof(IEnumerable<BaseType>));

            Assert.Equal(EdmTypeKind.Collection, edmType.TypeKind);
            Assert.Equal("System.Web.Http.OData.Formatter.BaseType", (edmType as IEdmCollectionType).ElementType.FullName());
        }

        [Fact]
        public void GetEdmType_ReturnsCollection_ForIEnumerableOfSelectExpandWrapperOfT()
        {
            IEdmModel model = GetEdmModel();
            IEdmType edmType = model.GetEdmType(typeof(IEnumerable<SelectExpandWrapper<BaseType>>));

            Assert.Equal(EdmTypeKind.Collection, edmType.TypeKind);
            Assert.Equal("System.Web.Http.OData.Formatter.BaseType", (edmType as IEdmCollectionType).ElementType.FullName());
        }

        [Fact]
        public void GetEdmType_ReturnsNull_ForRecursiveCollections()
        {
            IEdmModel model = GetEdmModel();

            Assert.Null(model.GetEdmType(typeof(RecursiveCollection)));
        }

        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(List<int>), true)]
        [InlineData(typeof(int[]), true)]
        [InlineData(typeof(object), true)]
        [InlineData(typeof(Nullable<int>), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(char), false)]
        [InlineData(typeof(IEnumerable<int>), true)]
        [InlineData(typeof(ICollection<int>), true)]
        [InlineData(typeof(DateTime), false)]
        public void IsNullable_RecognizesClassesAndInterfacesAndNullableOfTs(Type type, bool isNullable)
        {
            Assert.Equal(isNullable, EdmLibHelpers.IsNullable(type));
        }


        public static TheoryDataSet<IEdmType, bool, Type> ToEdmTypeReferenceTestData
        {
            get
            {
                IEdmEntityType entity = new EdmEntityType("NS", "Entity");
                IEdmComplexType complex = new EdmComplexType("NS", "Complex");
                IEdmPrimitiveType primitive = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
                IEdmCollectionType collection = new EdmCollectionType(new EdmEntityTypeReference(entity, isNullable: false));

                return new TheoryDataSet<IEdmType, bool, Type>
                {
                    { primitive, true, typeof(IEdmPrimitiveTypeReference) },
                    { primitive, false, typeof(IEdmPrimitiveTypeReference) },
                    { entity, true, typeof(IEdmEntityTypeReference) },
                    { entity, false, typeof(IEdmEntityTypeReference) },
                    { complex, true, typeof(IEdmComplexTypeReference) },
                    { complex, false, typeof(IEdmComplexTypeReference) },
                    { collection, true, typeof(IEdmCollectionTypeReference) },
                    { collection, false, typeof(IEdmCollectionTypeReference) }
                };
            }
        }

        [Theory]
        [PropertyData("ToEdmTypeReferenceTestData")]
        public void ToEdmTypeReference_InstantiatesRightEdmTypeReference(IEdmType edmType, bool isNullable, Type expectedType)
        {
            IEdmTypeReference result = EdmLibHelpers.ToEdmTypeReference(edmType, isNullable);

            Assert.Equal(isNullable, result.IsNullable);
            Assert.Equal(edmType, result.Definition);
            Assert.IsAssignableFrom(expectedType, result);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder modelBuilder = new ODataModelBuilder();
            modelBuilder
                .Entity<DerivedTypeA>()
                .DerivesFrom<BaseType>();

            modelBuilder
                .Entity<DerivedTypeB>()
                .DerivesFrom<BaseType>();

            return modelBuilder.GetEdmModel();
        }

        public class BaseType
        {
        }

        public class DerivedTypeA : BaseType
        {
        }

        public class DerivedTypeB : BaseType
        {
        }

        public class DerivedTypeAA : DerivedTypeA
        {
        }

        public class TypeNotInModel
        {
        }

        public class RecursiveCollection : List<RecursiveCollection>
        {
        }
    }
}
