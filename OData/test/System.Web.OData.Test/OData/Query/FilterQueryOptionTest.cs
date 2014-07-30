// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Query.Expressions;
using System.Web.OData.Query.Validators;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.TreeNodeKinds;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;
using Address = System.Web.OData.Builder.TestModels.Address;

namespace System.Web.OData.Query
{
    public class FilterQueryOptionTest
    {
        // Legal filter queries usable against CustomerFilterTestData.
        // Tuple is: filter, expected list of customer ID's
        public static TheoryDataSet<string, int[]> CustomerTestFilters
        {
            get
            {
                return new TheoryDataSet<string, int[]>
                {
                    // Primitive properties
                    { "Name eq 'Highest'", new int[] { 2 } },
                    { "endswith(Name, 'est')", new int[] { 1, 2 } },

                    // Complex properties
                    { "Address/City eq 'redmond'", new int[] { 1 } },
                    { "contains(Address/City, 'e')", new int[] { 1, 2 } },

                    // Primitive property collections
                    { "Aliases/any(alias: alias eq 'alias34')", new int[] { 3, 4 } },
                    { "Aliases/any(alias: alias eq 'alias4')", new int[] { 4 } },
                    { "Aliases/all(alias: alias eq 'alias2')", new int[] { 2 } },

                    // Navigational properties
                    { "Orders/any(order: order/OrderId eq 12)", new int[] { 1 } },
                };
            }
        }

        // Test data used by CustomerTestFilters TheoryDataSet
        public static List<Customer> CustomerFilterTestData
        {
            get
            {
                List<Customer> customerList = new List<Customer>();

                Customer c = new Customer
                {
                    CustomerId = 1,
                    Name = "Lowest",
                    Address = new Address { City = "redmond" },
                };
                c.Orders = new List<Order>
                {
                    new Order { OrderId = 11, Customer = c },
                    new Order { OrderId = 12, Customer = c },
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 2,
                    Name = "Highest",
                    Address = new Address { City = "seattle" },
                    Aliases = new List<string> { "alias2", "alias2" }
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 3,
                    Name = "Middle",
                    Address = new Address { City = "hobart" },
                    Aliases = new List<string> { "alias2", "alias34", "alias31" }
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 4,
                    Name = "NewLow",
                    Aliases = new List<string> { "alias34", "alias4" }
                };
                customerList.Add(c);

                return customerList;
            }
        }

        // Legal filter queries usable against EnumModelTestData.
        // Tuple is: filter, expected list of customer ID's
        public static TheoryDataSet<string, int[]> EnumModelTestFilters
        {
            get
            {
                return new TheoryDataSet<string, int[]>
                {
                    // Simple Enums
                    { "Simple eq Microsoft.TestCommon.Types.SimpleEnum'First'", new int[] { 1, 3 } },
                    { "Simple eq Microsoft.TestCommon.Types.SimpleEnum'0'", new int[] { 1, 3 } },
                    { "Simple eq Microsoft.TestCommon.Types.SimpleEnum'Fourth'", new int[] { } },
                    { "Simple eq Microsoft.TestCommon.Types.SimpleEnum'3'", new int[] { } },
                    { "Microsoft.TestCommon.Types.SimpleEnum'First' eq Simple", new int[] { 1, 3 } },
                    { "Simple eq cast('0',Microsoft.TestCommon.Types.SimpleEnum)", new int[] { 1, 3} },
                    { "Simple eq cast('First','Microsoft.TestCommon.Types.SimpleEnum')", new int[] { 1, 3} },
                    { "Simple eq null", new int[] { } },
                    { "null eq Simple", new int[] { } },
                    { "Simple eq SimpleNullable", new int[] { 1 } },
                    { "Simple has Microsoft.TestCommon.Types.SimpleEnum'First'", new int[] { 1, 2, 3, 5, 6 } },
                    { "Simple has Microsoft.TestCommon.Types.SimpleEnum'0'", new int[] { 1, 2, 3, 5, 6 } },
                    { "Simple has Microsoft.TestCommon.Types.SimpleEnum'Second'", new int[] { 5 } },
                    { "SimpleNullable eq Microsoft.TestCommon.Types.SimpleEnum'First'", new int[] { 1 } },
                    { "Microsoft.TestCommon.Types.SimpleEnum'First' eq SimpleNullable", new int[] { 1 } },
                    { "SimpleNullable eq null", new int[] { 3, 5 } },
                    { "null eq SimpleNullable", new int[] { 3, 5 } },

                    // Long enums
                    { "Long eq Microsoft.TestCommon.Types.LongEnum'SecondLong'", new int[] { 2 } },
                    { "Long eq Microsoft.TestCommon.Types.LongEnum'FourthLong'", new int[] { } },
                    { "Long eq Microsoft.TestCommon.Types.LongEnum'3'", new int[] { } },

                    // Byte enums
                    { "Byte eq Microsoft.TestCommon.Types.ByteEnum'SecondByte'", new int[] { 2 } },

                    // SByte enums
                    { "SByte eq Microsoft.TestCommon.Types.SByteEnum'SecondSByte'", new int[] { 2 } },

                    // Short enums
                    { "Short eq Microsoft.TestCommon.Types.ShortEnum'SecondShort'", new int[] { 2 } },

                    // UShort enums
                    { "UShort eq Microsoft.TestCommon.Types.UShortEnum'SecondUShort'", new int[] { 2 } },

                    // UInt enums
                    { "UInt eq Microsoft.TestCommon.Types.UIntEnum'SecondUInt'", new int[] { 2 } },

                    // Flag enums
                    { "Flag eq Microsoft.TestCommon.Types.FlagsEnum'One, Four'", new int[] { 1 } },
                    { "Microsoft.TestCommon.Types.FlagsEnum'One, Four' eq Flag", new int[] { 1 } },
                    { "Flag eq Microsoft.TestCommon.Types.FlagsEnum'0'", new int[] { } },
                    { "Flag eq Microsoft.TestCommon.Types.FlagsEnum'1'", new int[] { 5 } },
                    { "Flag eq Microsoft.TestCommon.Types.FlagsEnum'5'", new int[] { 1 } },
                    { "Flag has Microsoft.TestCommon.Types.FlagsEnum'One, Four'", new int[] { 1 } },
                    { "Flag has Microsoft.TestCommon.Types.FlagsEnum'One'", new int[] { 1, 2, 5 } },
                    { "Flag eq null", new int[] { } },
                    { "null eq Flag", new int[] { } },
                    { "Flag eq FlagNullable", new int[] { 1 } },
                    { "FlagNullable eq Microsoft.TestCommon.Types.FlagsEnum'One, Four'", new int[] { 1 } },
                    { "Microsoft.TestCommon.Types.FlagsEnum'One, Four' eq FlagNullable", new int[] { 1 } },
                    { "FlagNullable eq null", new int[] { 3, 5 } },
                    { "null eq FlagNullable", new int[] { 3, 5 } },

                    // Flag enums with different formats
                    { "Flag eq Microsoft.TestCommon.Types.FlagsEnum'One,Four'", new int[] { 1 } },
                    { "Flag eq Microsoft.TestCommon.Types.FlagsEnum'One,    Four'", new int[] { 1 } },
                    { "Flag eq Microsoft.TestCommon.Types.FlagsEnum'Four, One'", new int[] { 1 } },

                    // Other expressions
                    { "Flag ne Microsoft.TestCommon.Types.FlagsEnum'One, Four'", new int[] { 2, 3, 5, 6 } },
                    { "Flag eq FlagNullable and Simple eq SimpleNullable", new int[] { 1 } },
                    { "Simple gt Microsoft.TestCommon.Types.SimpleEnum'First'", new int[] { 2, 5, 6 } },
                    { "Flag ge Microsoft.TestCommon.Types.FlagsEnum'Four,One'", new int[] { 1, 3, 6 } }
                };
            }
        }

        // Test data used by EnumModelTestFilters TheoryDataSet
        public static List<EnumModel> EnumModelTestData
        {
            get
            {
                return new List<EnumModel>()
                {
                    new EnumModel()
                    {
                        Id = 1,
                        Simple = SimpleEnum.First,
                        SimpleNullable = SimpleEnum.First,
                        Long = LongEnum.ThirdLong,
                        Byte = ByteEnum.ThirdByte,
                        SByte = SByteEnum.ThirdSByte,
                        Short = ShortEnum.ThirdShort,
                        UShort = UShortEnum.ThirdUShort,
                        UInt = UIntEnum.ThirdUInt,
                        Flag = FlagsEnum.One | FlagsEnum.Four,
                        FlagNullable = FlagsEnum.One | FlagsEnum.Four
                    },
                    new EnumModel()
                    {
                        Id = 2,
                        Simple = SimpleEnum.Third,
                        SimpleNullable = SimpleEnum.Second,
                        Long = LongEnum.SecondLong,
                        Byte = ByteEnum.SecondByte,
                        SByte = SByteEnum.SecondSByte,
                        Short = ShortEnum.SecondShort,
                        UShort = UShortEnum.SecondUShort,
                        UInt = UIntEnum.SecondUInt,
                        Flag = FlagsEnum.One | FlagsEnum.Two,
                        FlagNullable = FlagsEnum.Two | FlagsEnum.Four
                    },
                    new EnumModel()
                    {
                        Id = 3,
                        Simple = SimpleEnum.First,
                        SimpleNullable = null,
                        Long = LongEnum.FirstLong,
                        Byte = ByteEnum.FirstByte,
                        SByte = SByteEnum.FirstSByte,
                        Short = ShortEnum.FirstShort,
                        UShort = UShortEnum.FirstUShort,
                        UInt = UIntEnum.FirstUInt,
                        Flag = FlagsEnum.Two | FlagsEnum.Four,
                        FlagNullable = null
                    },
                    new EnumModel()
                    {
                        Id = 5,
                        Simple = SimpleEnum.Second,
                        SimpleNullable = null,
                        Long = LongEnum.FirstLong,
                        Byte = ByteEnum.FirstByte,
                        SByte = SByteEnum.FirstSByte,
                        Short = ShortEnum.FirstShort,
                        UShort = UShortEnum.FirstUShort,
                        UInt = UIntEnum.FirstUInt,
                        Flag = FlagsEnum.One,
                        FlagNullable = null
                    },
                    new EnumModel()
                    {
                        Id = 6,
                        Simple = (SimpleEnum)4,
                        SimpleNullable = (SimpleEnum)8,
                        Long = (LongEnum)4,
                        Byte = (ByteEnum)8,
                        SByte = (SByteEnum)8,
                        Short = (ShortEnum)8,
                        UShort = (UShortEnum)8,
                        UInt = (UIntEnum)8,
                        Flag = (FlagsEnum)8,
                        FlagNullable = (FlagsEnum)16
                    }
                };
            }
        }

        // Legal filter queries usable against CastModelTestData.
        // Tuple is: filter, expected list of Id's
        public static TheoryDataSet<string, int[]> CastModelTestFilters
        {
            get
            {
                return new TheoryDataSet<string, int[]>
                {
                    // Cast works.
                    { "cast(IntProp,Edm.Double) lt 2.5", new int[] { 1, 2 }},
                    { "cast(IntProp,Edm.String) eq '3'", new int[] { 3 } },
                    { "cast(NullableIntProp,Edm.Int64) eq 1", new int[] { 1 }},
                    { "cast(NullableIntProp,Edm.String) eq null", new int[] { 3 }},
                    { "cast('Two, Four',Microsoft.TestCommon.Types.FlagsEnum) has FlagsEnumProp", new int[] { 2, 3 } },
                    { "contains(cast(StringProp,Edm.String),'String')", new int[] { 1, 3 } },
                    { "cast(NullableSimpleEnumProp,Edm.String) ne '0'", new int[] { 2, 3 } },
                    { "cast(DateTimeOffsetProp,Edm.DateTimeOffset) eq 2001-01-01T01:01:01.000+00:00", new int[] { 1 } },
                    { "cast(GuidProp,Edm.String) eq '00000000-0000-0000-0000-000000000000'", new int[] { 1, 3 } },
                    { "cast(null,Edm.Int32) ne null", new int[] { } },
                    { "cast(null,Edm.String) eq null", new int[] { 1, 2, 3 } },
                    { "cast(cast(cast(IntProp,Edm.Int64),Edm.Int16),Edm.Double) gt 1.5", new int[] { 2, 3 } },
                    // Cast fails.
                    { "cast(IntProp,Edm.DateTimeOffset) eq null", new int[] { 1, 2, 3 } },
                    { "cast(NullableIntProp,Edm.Guid) eq null", new int[] { 1, 2, 3 } },
                    { "cast(StringProp,Edm.Double) eq null", new int[] { 1, 2, 3 } },
                    { "cast(StringProp,Microsoft.TestCommon.Types.SimpleEnum) eq null", new int[] { 1, 2, 3 } },
                    { "cast(DateTimeOffsetProp,Edm.Int32) eq null", new int[] { 1, 2, 3 } },
                    { "cast(Edm.Int32) eq null", new int[] { 1, 2, 3 } },
                    { "cast($it,Edm.String) ne null", new int[] { } },
                    { "cast(ComplexProp,Edm.Double) ne null", new int[] { } },
                    { "cast(ComplexProp,Edm.String) ne null", new int[] { } },
                };
            }
        }

        // Test data used by CastModelTestFilters TheoryDataSet
        public static List<DataTypes> CastModelTestData
        {
            get
            {
                return new List<DataTypes>()
                {
                    new DataTypes()
                    {
                        IntProp = 1,
                        NullableIntProp = 1,
                        StringProp = "String 1",
                        FlagsEnumProp = FlagsEnum.One | FlagsEnum.Four,
                        NullableSimpleEnumProp = SimpleEnum.First,
                        DateTimeOffsetProp = new DateTimeOffset(new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
                        GuidProp = Guid.Empty,
                        ComplexProp = new Expressions.Address()
                    },
                    new DataTypes()
                    {
                        IntProp = 2,
                        NullableIntProp = 2,
                        FlagsEnumProp = FlagsEnum.Four,
                        NullableSimpleEnumProp = SimpleEnum.Second,
                        DateTimeOffsetProp = new DateTimeOffset(new DateTime(2002, 2, 2, 2, 2, 2, DateTimeKind.Utc)),
                        EntityProp = new Product(),
                        GuidProp = Guid.NewGuid(),
                        ComplexProp = new Expressions.Address()
                    },
                    new DataTypes()
                    {
                        IntProp = 3,
                        StringProp = "String 3",
                        FlagsEnumProp = FlagsEnum.Two | FlagsEnum.Four,
                        NullableSimpleEnumProp = SimpleEnum.Third,
                        DateTimeOffsetProp = new DateTimeOffset(new DateTime(2003, 3, 3, 3, 3, 3, DateTimeKind.Utc)),
                        GuidProp = Guid.Empty,
                        EntityProp = new Product()
                    }
                };
            }
        }

        // Legal filter queries usable against ParameterAliasTestData.
        // Tuple is: filter, parameter alias value, expected list of Id's
        public static TheoryDataSet<string, string, int[]> ParameterAliasTestFilters
        {
            get
            {
                return new TheoryDataSet<string, string, int[]>
                {
                    { "IntProp gt @p", "1.5", new int[] { 2, 3 }},
                    { "NullableIntProp lt @p", "1.5", new int[] { 1}},
                    { "contains(StringProp,@p)", "'3'", new int[] { 3 } },
                    { "FlagsEnumProp has @p", "Microsoft.TestCommon.Types.FlagsEnum'Two'", new int[] { 2, 3 } },
                    { "NullableSimpleEnumProp ne @p", "Microsoft.TestCommon.Types.SimpleEnum'1'", new int[] { 1, 3 } },
                    { "DateTimeOffsetProp ne @p", "2001-01-01T01:01:01.000+00:00", new int[] { 2, 3 } },
                    { "GuidProp eq @p", "00000000-0000-0000-0000-000000000000", new int[] { 1, 3 } },
                    { "EntityProp/AlternateAddresses/all(a: a/City ne @p)", "'bc'", new int[] { 1 } },
                    { "EntityProp/AlternateAddresses/any(a: a/City eq @p)", "'bc'", new int[] { 2, 3 } }
                };
            }
        }

        // Test data used by ParameterAliasTestFilters TheoryDataSet
        public static List<DataTypes> ParameterAliasTestData
        {
            get
            {
                return new List<DataTypes>()
                {
                    new DataTypes()
                    {
                        IntProp = 1,
                        NullableIntProp = 1,
                        StringProp = "String 1",
                        FlagsEnumProp = FlagsEnum.One | FlagsEnum.Four,
                        NullableSimpleEnumProp = SimpleEnum.First,
                        DateTimeOffsetProp = new DateTimeOffset(new DateTime(2001, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
                        GuidProp = Guid.Empty,
                        EntityProp = new Product { AlternateAddresses = new[]
                        {
                            new Expressions.Address { City = "a" },
                        }}
                    },
                    new DataTypes()
                    {
                        IntProp = 2,
                        NullableIntProp = 2,
                        FlagsEnumProp = FlagsEnum.Two,
                        NullableSimpleEnumProp = SimpleEnum.Second,
                        DateTimeOffsetProp = new DateTimeOffset(new DateTime(2002, 2, 2, 2, 2, 2, DateTimeKind.Utc)),
                        GuidProp = Guid.NewGuid(),
                        EntityProp = new Product { AlternateAddresses = new[]
                        {
                            new Expressions.Address { City = "a" },
                            new Expressions.Address { City = "bc" },
                        }}
                    },
                    new DataTypes()
                    {
                        IntProp = 3,
                        StringProp = "String 3",
                        FlagsEnumProp = FlagsEnum.Two | FlagsEnum.Four,
                        NullableSimpleEnumProp = SimpleEnum.Third,
                        DateTimeOffsetProp = new DateTimeOffset(new DateTime(2003, 3, 3, 3, 3, 3, DateTimeKind.Utc)),
                        GuidProp = Guid.Empty,
                        EntityProp = new Product { AlternateAddresses = new[]
                        {
                            new Expressions.Address { City = "bc" },
                        }}
                    }
                };
            }
        }

        // Legal filter queries usable against PropertyAliasTestData.
        // Tuple is: filter, expected list of IDs
        public static TheoryDataSet<string, int[]> PropertyAliasTestFilters
        {
            get
            {
                return new TheoryDataSet<string, int[]>
                {
                    { "FirstNameAlias eq 'abc'", new int[] { 2 } },
                    { "'abc' eq FirstNameAlias", new int[] { 2 } },
                    { "FirstNameAlias eq null", new int[] { } },
                    { "null eq FirstNameAlias", new int[] { } },
                    { "FirstNameAlias ne 'abc'", new int[] { 1, 3 } },
                    { "FirstNameAlias eq 'abc' and Id eq 2", new int[] { 2 } },
                    { "FirstNameAlias eq 'abc' and Id eq 1", new int[] { } },
                    { "FirstNameAlias gt 'abc'", new int[] { 1, 3 } },
                    { "FirstNameAlias ge 'def'", new int[] { 1, 3 } },
                };
            }
        }

        // Test data used by PropertyAliasTestFilters TheoryDataSet
        public static List<PropertyAlias> PropertyAliasTestData
        {
            get
            {
                return new List<PropertyAlias>()
                {
                    new PropertyAlias()
                    {
                        Id = 1,
                        FirstName = "def"
                    },
                    new PropertyAlias()
                    {
                        Id = 2,
                        FirstName = "abc"
                    },
                    new PropertyAlias()
                    {
                        Id = 3,
                        FirstName = "xyz"
                    },
                };
            }
        }

        [Fact]
        public void ConstructorNullContextThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FilterQueryOption("Name eq 'MSFT'", null));
        }

        [Fact]
        public void ConstructorNullRawValueThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new FilterQueryOption(null, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new FilterQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorNullQueryOptionParserThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            Assert.ThrowsArgumentNull(() =>
                new FilterQueryOption("test", new ODataQueryContext(model, typeof(Customer)), queryOptionParser: null),
                "queryOptionParser");
        }

        [Theory]
        [InlineData("Name eq 'MSFT'")]
        [InlineData("''")]
        public void CanConstructValidFilterQuery(string filterValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption(filterValue, context);

            Assert.Same(context, filter.Context);
            Assert.Equal(filterValue, filter.RawValue);
        }

        [Fact]
        public void GetQueryNodeParsesQuery()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Name eq 'MSFT'", context);
            var node = filter.FilterClause;

            Assert.Equal(QueryNodeKind.BinaryOperator, node.Expression.Kind);
            var binaryNode = node.Expression as BinaryOperatorNode;
            Assert.Equal(BinaryOperatorKind.Equal, binaryNode.OperatorKind);
            Assert.Equal(QueryNodeKind.Constant, binaryNode.Right.Kind);
            Assert.Equal("MSFT", ((ConstantNode)binaryNode.Right).Value);
            Assert.Equal(QueryNodeKind.SingleValuePropertyAccess, binaryNode.Left.Kind);
            var propertyAccessNode = binaryNode.Left as SingleValuePropertyAccessNode;
            Assert.Equal("Name", propertyAccessNode.Property.Name);
        }

        [Fact]
        public void CanConstructValidAnyQueryOverPrimitiveCollectionProperty()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Aliases/any(a: a eq 'alias')", context);
            var node = filter.FilterClause;
            var anyNode = node.Expression as AnyNode;
            var aParameter = anyNode.RangeVariables.SingleOrDefault(p => p.Name == "a");
            var aParameterType = aParameter.TypeReference.Definition as IEdmPrimitiveType;

            Assert.NotNull(aParameter);

            Assert.NotNull(aParameterType);
            Assert.Equal("a", aParameter.Name);
        }

        [Fact]
        public void CanConstructValidAnyQueryOverComplexCollectionProperty()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);
            var node = filter.FilterClause;
            var anyNode = node.Expression as AnyNode;
            var aParameter = anyNode.RangeVariables.SingleOrDefault(p => p.Name == "a");
            var aParameterType = aParameter.TypeReference.Definition as IEdmComplexType;

            Assert.NotNull(aParameter);

            Assert.NotNull(aParameterType);
            Assert.Equal("a", aParameter.Name);
        }

        [Fact]
        public void CanTurnOffValidationForFilter()
        {
            ODataValidationSettings settings = new ODataValidationSettings() { AllowedFunctions = AllowedFunctions.AllDateTimeFunctions };
            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
            FilterQueryOption option = new FilterQueryOption("substring(Name,8,1) eq '7'", context);

            Assert.Throws<ODataException>(() =>
                option.Validate(settings),
                "Function 'substring' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");

            option.Validator = null;
            Assert.DoesNotThrow(() => option.Validate(settings));
        }

        [Fact]
        public void ApplyTo_Throws_Null_Query()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => filter.ApplyTo(null, new ODataQuerySettings()), "query");
        }

        [Fact]
        public void ApplyTo_Throws_Null_QuerySettings()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => filter.ApplyTo(new Customer[0].AsQueryable(), null), "querySettings");
        }

        [Fact]
        public void ApplyTo_Throws_Null_AssembliesResolver()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType_With_CollectionProperties().Add_Customers_EntitySet().Add_Address_ComplexType().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filter = new FilterQueryOption("Addresses/any(a: a/HouseNumber eq 1)", context);

            // Act & Assert
            Assert.ThrowsArgumentNull(() => filter.ApplyTo(new Customer[0].AsQueryable(), new ODataQuerySettings(), null), "assembliesResolver");
        }

        [Theory]
        [PropertyData("CustomerTestFilters")]
        public void ApplyTo_Returns_Correct_Queryable(string filter, int[] customerIds)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<Customer> customers = CustomerFilterTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(
                customerIds,
                actualCustomers.Select(customer => customer.CustomerId));
        }

        [Theory]
        [PropertyData("EnumModelTestFilters")]
        public void ApplyToEnums_ReturnsCorrectQueryable(string filter, int[] enumModelIds)
        {
            // Arrange
            var model = GetEnumModel();
            var context = new ODataQueryContext(model, typeof(EnumModel));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<EnumModel> enumModels = EnumModelTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(enumModels.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<EnumModel> actualCustomers = Assert.IsAssignableFrom<IEnumerable<EnumModel>>(queryable);
            Assert.Equal(
                enumModelIds,
                actualCustomers.Select(enumModel => enumModel.Id));
        }

        [Theory]
        [InlineData("Simple has null", typeof(ODataException))]
        [InlineData("null has Microsoft.TestCommon.Types.SimpleEnum'First'", typeof(ODataException))]
        [InlineData("Id has Microsoft.TestCommon.Types.SimpleEnum'First'", typeof(ODataException))]
        [InlineData("null has null", typeof(NotSupportedException))]
        [InlineData("Simple has 23", typeof(ODataException))]
        [InlineData("'Some string' has 0", typeof(ODataException))]
        public void ApplyToEnums_Throws_WithInvalidFilter(string filter, Type exceptionType)
        {
            // Arrange
            var model = GetEnumModel();
            var context = new ODataQueryContext(model, typeof(EnumModel));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<EnumModel> enumModels = EnumModelTestData;

            // Act & Assert
            Assert.Throws(
                exceptionType,
                () => filterOption.ApplyTo(
                    enumModels.AsQueryable(),
                    new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True }));
        }

        [Theory]
        [InlineData("Simple eq Microsoft.TestCommon.Types.SimpleEnum'4'")]
        [InlineData("Flag eq Microsoft.TestCommon.Types.FlagsEnum'8'")]
        public void ApplyToEnums_DoesnotThrow_ForUndefinedValue(string filter)
        {
            // Arrange
            var model = GetEnumModel();
            var context = new ODataQueryContext(model, typeof(EnumModel));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<EnumModel> enumModels = EnumModelTestData;

            // Act
            Assert.DoesNotThrow(
                () => filterOption.ApplyTo(enumModels.AsQueryable(),
                    new ODataQuerySettings
                    {
                        HandleNullPropagation = HandleNullPropagationOption.True
                    })
            );
        }

        [Theory]
        [PropertyData("CastModelTestFilters")]
        public void ApplyWithCast_ReturnsCorrectQueryable(string filter, int[] castModelIds)
        {
            // Arrange
            var model = GetCastModel();
            var context = new ODataQueryContext(model, typeof(DataTypes));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<DataTypes> castModels = CastModelTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(castModels.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<DataTypes> actualProducts = Assert.IsAssignableFrom<IEnumerable<DataTypes>>(queryable);
            Assert.Equal(
                castModelIds,
                actualProducts.Select(product => product.IntProp));
        }

        [Theory]
        [InlineData("cast(NoSuchProperty,Edm.String) eq null", typeof(ODataException))]
        [InlineData("cast(ProductId,Edm.NoSuchType) eq null", typeof(ODataException))]
        [InlineData("cast(ProductId,Edm.String) eq 123", typeof(ODataException))]
        [InlineData("cast('123',Microsoft.TestCommon.Types.SimpleEnum) ne 'First'", typeof(ODataException))]
        [InlineData("cast(Edm.Int32) eq '123'", typeof(ODataException))]
        public void ApplyWithCast_Throws_WithInvalidFilter(string filter, Type exceptionType)
        {
            // Arrange
            var model = GetCastModel();
            var context = new ODataQueryContext(model, typeof(DataTypes));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<DataTypes> castModels = CastModelTestData;

            // Act & Assert
            Assert.Throws(
                exceptionType,
                () => filterOption.ApplyTo(
                    castModels.AsQueryable(),
                    new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True }));
        }

        [Theory]
        [InlineData(
            "length(Simple) eq 5",
            "No function signature for the function with name 'length' matches the specified arguments. The function signatures considered are: length(Edm.String Nullable=true).")]
        [InlineData(
            "length(SimpleNullable) eq 5",
            "No function signature for the function with name 'length' matches the specified arguments. The function signatures considered are: length(Edm.String Nullable=true).")]
        [InlineData(
            "length(Flag) eq 5",
            "No function signature for the function with name 'length' matches the specified arguments. The function signatures considered are: length(Edm.String Nullable=true).")]
        [InlineData(
            "length(FlagNullable) eq 5",
            "No function signature for the function with name 'length' matches the specified arguments. The function signatures considered are: length(Edm.String Nullable=true).")]
        [InlineData(
            "contains(Simple, 'foo') eq true",
            "No function signature for the function with name 'contains' matches the specified arguments. The function signatures considered are: contains(Edm.String Nullable=true, Edm.String Nullable=true).")]
        [InlineData(
            "startswith(Simple, 'foo') eq true",
            "No function signature for the function with name 'startswith' matches the specified arguments. The function signatures considered are: startswith(Edm.String Nullable=true, Edm.String Nullable=true).")]
        [InlineData(
            "endswith(Simple, 'foo') eq true",
            "No function signature for the function with name 'endswith' matches the specified arguments. The function signatures considered are: endswith(Edm.String Nullable=true, Edm.String Nullable=true).")]
        [InlineData(
            "tolower(Simple) eq 'foo'",
            "No function signature for the function with name 'tolower' matches the specified arguments. The function signatures considered are: tolower(Edm.String Nullable=true).")]
        [InlineData(
            "toupper(Simple) eq 'foo'",
            "No function signature for the function with name 'toupper' matches the specified arguments. The function signatures considered are: toupper(Edm.String Nullable=true).")]
        [InlineData(
            "trim(Simple) eq 'foo'",
            "No function signature for the function with name 'trim' matches the specified arguments. The function signatures considered are: trim(Edm.String Nullable=true).")]
        [InlineData(
            "indexof(Simple, 'foo') eq 2",
            "No function signature for the function with name 'indexof' matches the specified arguments. The function signatures considered are: indexof(Edm.String Nullable=true, Edm.String Nullable=true).")]
        [InlineData(
            "substring(Simple, 3) eq 'foo'",
            "No function signature for the function with name 'substring' matches the specified arguments. The function signatures considered are: substring(Edm.String Nullable=true, Edm.Int32); substring(Edm.String Nullable=true, Edm.Int32 Nullable=true); substring(Edm.String Nullable=true, Edm.Int32, Edm.Int32); substring(Edm.String Nullable=true, Edm.Int32 Nullable=true, Edm.Int32); substring(Edm.String Nullable=true, Edm.Int32, Edm.Int32 Nullable=true); substring(Edm.String Nullable=true, Edm.Int32 Nullable=true, Edm.Int32 Nullable=true).")]
        [InlineData(
            "substring(Simple, 1, 3) eq 'foo'",
            "No function signature for the function with name 'substring' matches the specified arguments. The function signatures considered are: substring(Edm.String Nullable=true, Edm.Int32); substring(Edm.String Nullable=true, Edm.Int32 Nullable=true); substring(Edm.String Nullable=true, Edm.Int32, Edm.Int32); substring(Edm.String Nullable=true, Edm.Int32 Nullable=true, Edm.Int32); substring(Edm.String Nullable=true, Edm.Int32, Edm.Int32 Nullable=true); substring(Edm.String Nullable=true, Edm.Int32 Nullable=true, Edm.Int32 Nullable=true).")]
        [InlineData(
            "concat(Simple, 'bar') eq 'foo'",
            "No function signature for the function with name 'concat' matches the specified arguments. The function signatures considered are: concat(Edm.String Nullable=true, Edm.String Nullable=true).")]
        public void ApplyToEnums_ThrowsNotSupported_ForStringFunctions(string filter, string exceptionMessage)
        {
            // Arrange
            var model = GetEnumModel();
            var context = new ODataQueryContext(model, typeof(EnumModel));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<EnumModel> enumModels = EnumModelTestData;

            // Act
            Assert.Throws<ODataException>(
                () => filterOption.ApplyTo(enumModels.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True }),
                exceptionMessage
            );
        }

        [Theory]
        [PropertyData("ParameterAliasTestFilters")]
        public void ApplyWithParameterAlias_ReturnsCorrectQueryable(string filter, string parameterAliasValue, int[] parameterAliasModelIds)
        {
            // Arrange
            var model = GetParameterAliasModel();
            var context = new ODataQueryContext(model, typeof(DataTypes));
            IEdmType targetEdmType = model.FindType("System.Web.OData.Query.Expressions.DataTypes");
            IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("System.Web.OData.Query.Expressions.Products");

            ODataQueryOptionParser parser = new ODataQueryOptionParser(
                model,
                targetEdmType,
                targetNavigationSource,
                new Dictionary<string, string> { { "$filter", filter }, { "@p", parameterAliasValue } });

            var filterOption = new FilterQueryOption(filter, context, parser);
            IEnumerable<DataTypes> parameterAliasModels = ParameterAliasTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(parameterAliasModels.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<DataTypes> actualResult = Assert.IsAssignableFrom<IEnumerable<DataTypes>>(queryable);
            Assert.Equal(
                parameterAliasModelIds,
                actualResult.Select(result => result.IntProp));
        }

        [Theory]
        [PropertyData("PropertyAliasTestFilters")]
        public void ApplyTo_ReturnsCorrectQueryable_PropertyAlias(string filter, int[] propertyAliasIds)
        {
            // Arrange
            var model = GetPropertyAliasModel();
            var context = new ODataQueryContext(model, typeof(PropertyAlias));
            var filterOption = new FilterQueryOption(filter, context);
            IEnumerable<PropertyAlias> propertyAliases = PropertyAliasTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(
                propertyAliases.AsQueryable(),
                new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<PropertyAlias> actualPropertyAliases = Assert.IsAssignableFrom<IEnumerable<PropertyAlias>>(queryable);
            Assert.Equal(
                propertyAliasIds,
                actualPropertyAliases.Select(propertyAlias => propertyAlias.Id));
        }

        [Fact]
        public void Property_FilterClause_WorksWithUnTypedContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            FilterQueryOption filter = new FilterQueryOption("ID eq 42", context);

            // Act & Assert
            Assert.NotNull(filter.FilterClause);
        }

        [Fact]
        public void ApplyTo_WithUnTypedContext_Throws_InvalidOperation()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            FilterQueryOption filter = new FilterQueryOption("Id eq 42", context);
            IQueryable queryable = new Mock<IQueryable>().Object;

            Assert.Throws<NotSupportedException>(() => filter.ApplyTo(queryable, new ODataQuerySettings()),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }

        private static IEdmModel GetEnumModel()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(EnumModel)));
            var builder = new ODataConventionModelBuilder(config);
            builder.EntitySet<EnumModel>("EnumModels");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetCastModel()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(DataTypes)));
            var builder = new ODataConventionModelBuilder(config);
            builder.EntitySet<DataTypes>("CastModels");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetParameterAliasModel()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(DataTypes)));
            var builder = new ODataConventionModelBuilder(config);
            builder.EntitySet<DataTypes>("ParameterAliasModels");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetPropertyAliasModel()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(typeof(PropertyAlias)));
            var builder = new ODataConventionModelBuilder(config) { ModelAliasingEnabled = true };
            builder.EntitySet<PropertyAlias>("PropertyAliases");
            return builder.GetEdmModel();
        }
    }
}
