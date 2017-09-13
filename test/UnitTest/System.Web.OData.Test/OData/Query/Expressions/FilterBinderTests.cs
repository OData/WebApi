// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Xml.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;

namespace System.Web.OData.Query.Expressions
{
    public class FilterBinderTests
    {
        private const string NotTesting = "";

        private static readonly Uri _serviceBaseUri = new Uri("http://server/service/");

        private static Dictionary<Type, IEdmModel> _modelCache = new Dictionary<Type, IEdmModel>();

        public static TheoryDataSet<decimal?, bool, object> MathRoundDecimal_DataSet
        {
            get
            {
                return new TheoryDataSet<decimal?, bool, object>
                {
                    { null, false, typeof(InvalidOperationException) },
                    { 5.9m, true, true },
                    { 5.4m, false, false },
                };
            }
        }

        public static TheoryDataSet<decimal?, bool, object> MathFloorDecimal_DataSet
        {
            get
            {
                return new TheoryDataSet<decimal?, bool, object>
                {
                    { null, false, typeof(InvalidOperationException) },
                    { 5.4m, true, true },
                    { 4.4m, false, false },
                };
            }
        }

        public static TheoryDataSet<decimal?, bool, object> MathCeilingDecimal_DataSet
        {
            get
            {
                return new TheoryDataSet<decimal?, bool, object>
                {
                    { null, false, typeof(InvalidOperationException) },
                    { 4.1m, true, true },
                    { 5.9m, false, false },
                };
            }
        }

        #region Inequalities
        [Theory]
        [InlineData(null, true, true)]
        [InlineData("", false, false)]
        [InlineData("Doritos", false, false)]
        public void EqualityOperatorWithNull(string productName, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "ProductName eq null",
                "$it => ($it.ProductName == null)");

            RunFilters(filters,
                new Product { ProductName = productName },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, false)]
        [InlineData("", false, false)]
        [InlineData("Doritos", true, true)]
        public void EqualityOperator(string productName, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "ProductName eq 'Doritos'",
                "$it => ($it.ProductName == \"Doritos\")");

            RunFilters(filters,
                new Product { ProductName = productName },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, true, true)]
        [InlineData("", true, true)]
        [InlineData("Doritos", false, false)]
        public void NotEqualOperator(string productName, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "ProductName ne 'Doritos'",
                "$it => ($it.ProductName != \"Doritos\")");

            RunFilters(filters,
                new Product { ProductName = productName },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, false)]
        [InlineData(5.01, true, true)]
        [InlineData(4.99, false, false)]
        public void GreaterThanOperator(object unitPrice, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "UnitPrice gt 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice > Convert({0:0.00}))", 5.0),
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice > Convert({0:0.00})) == True)", 5.0));

            RunFilters(filters,
                new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, false)]
        [InlineData(5.0, true, true)]
        [InlineData(4.99, false, false)]
        public void GreaterThanEqualOperator(object unitPrice, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "UnitPrice ge 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice >= Convert({0:0.00}))", 5.0),
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice >= Convert({0:0.00})) == True)", 5.0));

            RunFilters(filters,
                new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, false)]
        [InlineData(4.99, true, true)]
        [InlineData(5.01, false, false)]
        public void LessThanOperator(object unitPrice, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "UnitPrice lt 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice < Convert({0:0.00}))", 5.0),
                NotTesting);

            RunFilters(filters,
                new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, false)]
        [InlineData(5.0, true, true)]
        [InlineData(5.01, false, false)]
        public void LessThanOrEqualOperator(object unitPrice, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "UnitPrice le 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", 5.0),
                NotTesting);

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void NegativeNumbers()
        {
            VerifyQueryDeserialization(
                "UnitPrice le -5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", -5.0),
                NotTesting);
        }

        [Theory]
        [InlineData("DateTimeOffsetProp eq DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp == $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp ne DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp != $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp ge DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp >= $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp le DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp <= $it.DateTimeOffsetProp)")]
        public void DateTimeOffsetInEqualities(string clause, string expectedExpression)
        {
            // There's currently a bug here. For now, the test checks for the presence of the bug (as a reminder to fix
            // the test once the bug is fixed).
            // The following assert shows the behavior with the bug and should be removed once the bug is fixed.
            Assert.Throws<ODataException>(() => Bind("" + clause));

            // TODO: Enable once ODataUriParser handles DateTimeOffsets
            // The following call shows the behavior without the bug, and should be enabled once the bug is fixed.
            //VerifyQueryDeserialization<DataTypes>("" + clause, expectedExpression);
        }

        [Theory]
        [InlineData("DateTimeProp eq DateTimeProp", "$it => ($it.DateTimeProp == $it.DateTimeProp)")]
        [InlineData("DateTimeProp ne DateTimeProp", "$it => ($it.DateTimeProp != $it.DateTimeProp)")]
        [InlineData("DateTimeProp ge DateTimeProp", "$it => ($it.DateTimeProp >= $it.DateTimeProp)")]
        [InlineData("DateTimeProp le DateTimeProp", "$it => ($it.DateTimeProp <= $it.DateTimeProp)")]
        public void DateInEqualities(string clause, string expectedExpression)
        {
            VerifyQueryDeserialization<DataTypes>(
                "" + clause,
                expectedExpression);
        }

        #endregion

        #region Logical Operators

        [Fact]
        [ReplaceCulture]
        public void BooleanOperatorNullableTypes()
        {
            VerifyQueryDeserialization(
                "UnitPrice eq 5.00m or CategoryID eq 0",
                "$it => (($it.UnitPrice == Convert(5.00)) OrElse ($it.CategoryID == 0))",
                NotTesting);
        }

        [Fact]
        public void BooleanComparisonOnNullableAndNonNullableType()
        {
            VerifyQueryDeserialization(
                "Discontinued eq true",
                "$it => ($it.Discontinued == Convert(True))",
                "$it => (($it.Discontinued == Convert(True)) == True)");
        }

        [Fact]
        public void BooleanComparisonOnNullableType()
        {
            VerifyQueryDeserialization(
                "Discontinued eq Discontinued",
                "$it => ($it.Discontinued == $it.Discontinued)",
                "$it => (($it.Discontinued == $it.Discontinued) == True)");
        }

        [Theory]
        [InlineData(null, null, false, false)]
        [InlineData(5.0, 0, true, true)]
        [InlineData(null, 1, false, false)]
        public void OrOperator(object unitPrice, object unitsInStock, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "UnitPrice eq 5.00m or UnitsInStock eq 0",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice == Convert({0:0.00})) OrElse (Convert($it.UnitsInStock) == Convert({1})))", 5.0, 0),
                NotTesting);

            RunFilters(filters,
                new Product { UnitPrice = ToNullable<decimal>(unitPrice), UnitsInStock = ToNullable<short>(unitsInStock) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, null, false, false)]
        [InlineData(5.0, 10, true, true)]
        [InlineData(null, 1, false, false)]
        public void AndOperator(object unitPrice, object unitsInStock, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "UnitPrice eq 5.00m and UnitsInStock eq 10.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice == Convert({0:0.00})) AndAlso (Convert($it.UnitsInStock) == Convert({1:0.00})))", 5.0, 10.0),
                NotTesting);

            RunFilters(filters,
                new Product { UnitPrice = ToNullable<decimal>(unitPrice), UnitsInStock = ToNullable<short>(unitsInStock) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, true)] // This is an interesting cas for null propagation.
        [InlineData(5.0, false, false)]
        [InlineData(5.5, true, true)]
        public void Negation(object unitPrice, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "not (UnitPrice eq 5.00m)",
                String.Format(CultureInfo.InvariantCulture, "$it => Not(($it.UnitPrice == Convert({0:0.00})))", 5.0),
                NotTesting);

            RunFilters(filters,
                new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, true, true)] // This is an interesting cas for null propagation.
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        public void BoolNegation(bool discontinued, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "not Discontinued",
                "$it => Convert(Not($it.Discontinued))",
                "$it => (Not($it.Discontinued) == True)");

            RunFilters(filters,
                new Product { Discontinued = ToNullable<bool>(discontinued) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void NestedNegation()
        {
            VerifyQueryDeserialization(
                "not (not(not    (Discontinued)))",
                "$it => Convert(Not(Not(Not($it.Discontinued))))",
                "$it => (Not(Not(Not($it.Discontinued))) == True)");
        }
        #endregion

        #region Arithmetic Operators
        [Theory]
        [InlineData(null, false, false)]
        [InlineData(5.0, true, true)]
        [InlineData(15.01, false, false)]
        public void Subtraction(object unitPrice, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "UnitPrice sub 1.00m lt 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice - Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                String.Format(CultureInfo.InvariantCulture, "$it => ((($it.UnitPrice - Convert({0:0.00})) < Convert({1:0.00})) == True)", 1.0, 5.0));

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void Addition()
        {
            VerifyQueryDeserialization(
                "UnitPrice add 1.00m lt 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice + Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                NotTesting);
        }

        [Fact]
        public void Multiplication()
        {
            VerifyQueryDeserialization(
                "UnitPrice mul 1.00m lt 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice * Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                NotTesting);
        }

        [Fact]
        public void Division()
        {
            VerifyQueryDeserialization(
                "UnitPrice div 1.00m lt 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice / Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                NotTesting);
        }

        [Fact]
        public void Modulo()
        {
            VerifyQueryDeserialization(
                "UnitPrice mod 1.00m lt 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice % Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                NotTesting);
        }
        #endregion

        # region NULL  handling
        [Theory]
        [InlineData("UnitsInStock eq UnitsOnOrder", null, null, false, true)]
        [InlineData("UnitsInStock ne UnitsOnOrder", null, null, false, false)]
        [InlineData("UnitsInStock gt UnitsOnOrder", null, null, false, false)]
        [InlineData("UnitsInStock ge UnitsOnOrder", null, null, false, false)]
        [InlineData("UnitsInStock lt UnitsOnOrder", null, null, false, false)]
        [InlineData("UnitsInStock le UnitsOnOrder", null, null, false, false)]
        [InlineData("(UnitsInStock add UnitsOnOrder) eq UnitsInStock", null, null, false, true)]
        [InlineData("(UnitsInStock sub UnitsOnOrder) eq UnitsInStock", null, null, false, true)]
        [InlineData("(UnitsInStock mul UnitsOnOrder) eq UnitsInStock", null, null, false, true)]
        [InlineData("(UnitsInStock div UnitsOnOrder) eq UnitsInStock", null, null, false, true)]
        [InlineData("(UnitsInStock mod UnitsOnOrder) eq UnitsInStock", null, null, false, true)]
        [InlineData("UnitsInStock eq UnitsOnOrder", 1, null, false, false)]
        [InlineData("UnitsInStock eq UnitsOnOrder", 1, 1, true, true)]
        public void NullHandling(string filter, object unitsInStock, object unitsOnOrder, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization("" + filter);

            RunFilters(filters,
                new Product { UnitsInStock = ToNullable<short>(unitsInStock), UnitsOnOrder = ToNullable<short>(unitsOnOrder) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData("UnitsInStock eq null", null, true, true)] // NULL == constant NULL is true when null propagation is enabled
        [InlineData("UnitsInStock ne null", null, false, false)]  // NULL != constant NULL is false when null propagation is enabled
        public void NullHandling_LiteralNull(string filter, object unitsInStock, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization("" + filter);

            RunFilters(filters,
                new Product { UnitsInStock = ToNullable<short>(unitsInStock) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void NullHandling_StringFunctionWithStringParameret()
        {
            VerifyQueryDeserialization(
                "startswith(ProductName, 'Abc')",
                NotTesting,
                "$it => (IIF((($it.ProductName == null) OrElse (\"Abc\" == null)), null, Convert($it.ProductName.StartsWith(\"Abc\"))) == True)");
        }
        #endregion

        [Theory]
        [InlineData("StringProp gt 'Middle'", "Middle", false)]
        [InlineData("StringProp ge 'Middle'", "Middle", true)]
        [InlineData("StringProp lt 'Middle'", "Middle", false)]
        [InlineData("StringProp le 'Middle'", "Middle", true)]
        [InlineData("StringProp ge StringProp", "", true)]
        [InlineData("StringProp gt null", "", true)]
        [InlineData("null gt StringProp", "", false)]
        [InlineData("'Middle' gt StringProp", "Middle", false)]
        [InlineData("'a' lt 'b'", "", true)]
        public void StringComparisons_Work(string filter, string value, bool expectedResult)
        {
            var filters = VerifyQueryDeserialization<DataTypes>(filter);
            var result = RunFilter(filters.WithoutNullPropagation, new DataTypes { StringProp = value });

            Assert.Equal(expectedResult, result);
        }

        // Issue: 477
        [Theory]
        [InlineData("indexof('hello', StringProp) gt UIntProp")]
        [InlineData("indexof('hello', StringProp) gt ULongProp")]
        [InlineData("indexof('hello', StringProp) gt UShortProp")]
        [InlineData("indexof('hello', StringProp) gt NullableUShortProp")]
        [InlineData("indexof('hello', StringProp) gt NullableUIntProp")]
        [InlineData("indexof('hello', StringProp) gt NullableULongProp")]
        public void ComparisonsInvolvingCastsAndNullableValues(string filter)
        {
            var filters = VerifyQueryDeserialization<DataTypes>(filter);

            RunFilters(filters,
              new DataTypes(),
              new { WithNullPropagation = false, WithoutNullPropagation = typeof(ArgumentNullException) });
        }

        [Theory]
        [InlineData(null, null, true, true)]
        [InlineData("not doritos", 0, true, true)]
        [InlineData("Doritos", 1, false, false)]
        public void Grouping(string productName, object unitsInStock, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "((ProductName ne 'Doritos') or (UnitPrice lt 5.00m))",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.ProductName != \"Doritos\") OrElse ($it.UnitPrice < Convert({0:0.00})))", 5.0),
                NotTesting);

            RunFilters(filters,
                new Product { ProductName = productName, UnitsInStock = ToNullable<short>(unitsInStock) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void MemberExpressions()
        {
            var filters = VerifyQueryDeserialization(
                "Category/CategoryName eq 'Snacks'",
                "$it => ($it.Category.CategoryName == \"Snacks\")",
                "$it => (IIF(($it.Category == null), null, $it.Category.CategoryName) == \"Snacks\")");

            RunFilters(filters,
                new Product { },
                new { WithNullPropagation = false, WithoutNullPropagation = typeof(NullReferenceException) });

            RunFilters(filters,
                new Product { Category = new Category { CategoryName = "Snacks" } },
                new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        [Fact]
        public void MemberExpressionsRecursive()
        {
            var filters = VerifyQueryDeserialization(
                "Category/Product/Category/CategoryName eq 'Snacks'",
                "$it => ($it.Category.Product.Category.CategoryName == \"Snacks\")",
                NotTesting);

            RunFilters(filters,
               new Product { },
               new { WithNullPropagation = false, WithoutNullPropagation = typeof(NullReferenceException) });
        }

        [Fact]
        public void ComplexPropertyNavigation()
        {
            var filters = VerifyQueryDeserialization(
                "SupplierAddress/City eq 'Redmond'",
                "$it => ($it.SupplierAddress.City == \"Redmond\")",
                "$it => (IIF(($it.SupplierAddress == null), null, $it.SupplierAddress.City) == \"Redmond\")");

            RunFilters(filters,
               new Product { },
               new { WithNullPropagation = false, WithoutNullPropagation = typeof(NullReferenceException) });

            RunFilters(filters,
               new Product { SupplierAddress = new Address { City = "Redmond" } },
               new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        #region Any/All

        [Fact]
        public void AnyOnNavigationEnumerableCollections()
        {
            var filters = VerifyQueryDeserialization(
               "Category/EnumerableProducts/any(P: P/ProductName eq 'Snacks')",
               "$it => $it.Category.EnumerableProducts.Any(P => (P.ProductName == \"Snacks\"))",
               NotTesting);

            RunFilters(filters,
                     new Product
                     {
                         Category = new Category
                         {
                             EnumerableProducts = new Product[]
                        {
                            new Product { ProductName = "Snacks" },
                            new Product { ProductName = "NonSnacks" }
                        }
                         }
                     },
                     new { WithNullPropagation = true, WithoutNullPropagation = true });

            RunFilters(filters,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts = new Product[]
                        {
                            new Product { ProductName = "NonSnacks" }
                        }
                    }
                },
                new { WithNullPropagation = false, WithoutNullPropagation = false });
        }

        [Fact]
        public void AnyOnNavigationQueryableCollections()
        {
            var filters = VerifyQueryDeserialization(
               "Category/QueryableProducts/any(P: P/ProductName eq 'Snacks')",
               "$it => $it.Category.QueryableProducts.Any(P => (P.ProductName == \"Snacks\"))",
               NotTesting);

            RunFilters(filters,
                    new Product
                    {
                        Category = new Category
                        {
                            QueryableProducts = new Product[]
                        {
                            new Product { ProductName = "Snacks" },
                            new Product { ProductName = "NonSnacks" }
                        }.AsQueryable()
                        }
                    },
                new { WithNullPropagation = true, WithoutNullPropagation = true });

            RunFilters(filters,
                new Product
                {
                    Category = new Category
                    {
                        QueryableProducts = new Product[]
                        {
                            new Product { ProductName = "NonSnacks" }
                        }.AsQueryable()
                    }
                },
            new { WithNullPropagation = false, WithoutNullPropagation = false });
        }

        [Fact]
        public void AnyOnNavigation_NullCollection()
        {
            var filters = VerifyQueryDeserialization(
               "Category/EnumerableProducts/any(P: P/ProductName eq 'Snacks')",
               "$it => $it.Category.EnumerableProducts.Any(P => (P.ProductName == \"Snacks\"))",
               NotTesting);

            RunFilters(filters,
                     new Product
                     {
                         Category = new Category
                         {
                         }
                     },
                     new { WithNullPropagation = false, WithoutNullPropagation = typeof(ArgumentNullException) });

            RunFilters(filters,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts = new Product[]
                        {
                            new Product { ProductName = "Snacks" }
                        }
                    }
                },
                new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        [Fact]
        public void AllOnNavigation_NullCollection()
        {
            var filters = VerifyQueryDeserialization(
               "Category/EnumerableProducts/all(P: P/ProductName eq 'Snacks')",
               "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))",
               NotTesting);

            RunFilters(filters,
                     new Product
                     {
                         Category = new Category
                         {
                         }
                     },
                     new { WithNullPropagation = false, WithoutNullPropagation = typeof(ArgumentNullException) });

            RunFilters(filters,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts = new Product[]
                        {
                            new Product { ProductName = "Snacks" }
                        }
                    }
                },
                new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        [Fact]
        public void MultipleAnys_WithSameRangeVariableName()
        {
            VerifyQueryDeserialization(
               "AlternateIDs/any(n: n eq 42) and AlternateAddresses/any(n : n/City eq 'Redmond')",
               "$it => ($it.AlternateIDs.Any(n => (n == 42)) AndAlso $it.AlternateAddresses.Any(n => (n.City == \"Redmond\")))",
               NotTesting);
        }

        [Fact]
        public void MultipleAlls_WithSameRangeVariableName()
        {
            VerifyQueryDeserialization(
               "AlternateIDs/all(n: n eq 42) and AlternateAddresses/all(n : n/City eq 'Redmond')",
               "$it => ($it.AlternateIDs.All(n => (n == 42)) AndAlso $it.AlternateAddresses.All(n => (n.City == \"Redmond\")))",
               NotTesting);
        }

        [Fact]
        public void AnyOnNavigationEnumerableCollections_EmptyFilter()
        {
            VerifyQueryDeserialization(
               "Category/EnumerableProducts/any()",
               "$it => $it.Category.EnumerableProducts.Any()",
               NotTesting);
        }

        [Fact]
        public void AnyOnNavigationQueryableCollections_EmptyFilter()
        {
            VerifyQueryDeserialization(
               "Category/QueryableProducts/any()",
               "$it => $it.Category.QueryableProducts.Any()",
               NotTesting);
        }

        [Fact]
        public void AllOnNavigationEnumerableCollections()
        {
            VerifyQueryDeserialization(
               "Category/EnumerableProducts/all(P: P/ProductName eq 'Snacks')",
               "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))",
               NotTesting);
        }

        [Fact]
        public void AllOnNavigationQueryableCollections()
        {
            VerifyQueryDeserialization(
               "Category/QueryableProducts/all(P: P/ProductName eq 'Snacks')",
               "$it => $it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\"))",
               NotTesting);
        }

        [Fact]
        public void AnyInSequenceNotNested()
        {
            VerifyQueryDeserialization(
               "Category/QueryableProducts/any(P: P/ProductName eq 'Snacks') or Category/QueryableProducts/any(P2: P2/ProductName eq 'Snacks')",
               "$it => ($it.Category.QueryableProducts.Any(P => (P.ProductName == \"Snacks\")) OrElse $it.Category.QueryableProducts.Any(P2 => (P2.ProductName == \"Snacks\")))",
               NotTesting);
        }

        [Fact]
        public void AllInSequenceNotNested()
        {
            VerifyQueryDeserialization(
               "Category/QueryableProducts/all(P: P/ProductName eq 'Snacks') or Category/QueryableProducts/all(P2: P2/ProductName eq 'Snacks')",
               "$it => ($it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\")) OrElse $it.Category.QueryableProducts.All(P2 => (P2.ProductName == \"Snacks\")))",
               NotTesting);
        }

        [Fact]
        public void AnyOnPrimitiveCollection()
        {
            var filters = VerifyQueryDeserialization(
               "AlternateIDs/any(id: id eq 42)",
               "$it => $it.AlternateIDs.Any(id => (id == 42))",
               NotTesting);

            RunFilters(
                filters,
                new Product { AlternateIDs = new[] { 1, 2, 42 } },
                new { WithNullPropagation = true, WithoutNullPropagation = true });

            RunFilters(
                filters,
                new Product { AlternateIDs = new[] { 1, 2 } },
                new { WithNullPropagation = false, WithoutNullPropagation = false });
        }

        [Fact]
        public void AllOnPrimitiveCollection()
        {
            VerifyQueryDeserialization(
               "AlternateIDs/all(id: id eq 42)",
               "$it => $it.AlternateIDs.All(id => (id == 42))",
               NotTesting);
        }

        [Fact]
        public void AnyOnComplexCollection()
        {
            var filters = VerifyQueryDeserialization(
               "AlternateAddresses/any(address: address/City eq 'Redmond')",
               "$it => $it.AlternateAddresses.Any(address => (address.City == \"Redmond\"))",
               NotTesting);

            RunFilters(
                filters,
                new Product { AlternateAddresses = new[] { new Address { City = "Redmond" } } },
                new { WithNullPropagation = true, WithoutNullPropagation = true });

            RunFilters(
                filters,
                new Product(),
                new { WithNullPropagation = false, WithoutNullPropagation = typeof(ArgumentNullException) });
        }

        [Fact]
        public void AllOnComplexCollection()
        {
            VerifyQueryDeserialization(
               "AlternateAddresses/all(address: address/City eq 'Redmond')",
               "$it => $it.AlternateAddresses.All(address => (address.City == \"Redmond\"))",
               NotTesting);
        }

        [Fact]
        public void RecursiveAllAny()
        {
            VerifyQueryDeserialization(
               "Category/QueryableProducts/all(P: P/Category/EnumerableProducts/any(PP: PP/ProductName eq 'Snacks'))",
               "$it => $it.Category.QueryableProducts.All(P => P.Category.EnumerableProducts.Any(PP => (PP.ProductName == \"Snacks\")))",
               NotTesting);
        }

        #endregion

        #region String Functions

        [Theory]
        [InlineData("Abcd", -1, "Abcd", true, typeof(ArgumentOutOfRangeException))]
        [InlineData("Abcd", 0, "Abcd", true, true)]
        [InlineData("Abcd", 1, "bcd", true, true)]
        [InlineData("Abcd", 3, "d", true, true)]
        [InlineData("Abcd", 4, "", true, true)]
        [InlineData("Abcd", 5, "", true, typeof(ArgumentOutOfRangeException))]
        public void StringSubstringStart(string productName, int startIndex, string compareString, bool withNullPropagation, object withoutNullPropagation)
        {
            string filter = String.Format("substring(ProductName, {0}) eq '{1}'", startIndex, compareString);
            var filters = VerifyQueryDeserialization(filter);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData("Abcd", -1, 4, "Abcd", true, typeof(ArgumentOutOfRangeException))]
        [InlineData("Abcd", -1, 3, "Abc", true, typeof(ArgumentOutOfRangeException))]
        [InlineData("Abcd", 0, 1, "A", true, true)]
        [InlineData("Abcd", 0, 4, "Abcd", true, true)]
        [InlineData("Abcd", 0, 3, "Abc", true, true)]
        [InlineData("Abcd", 0, 5, "Abcd", true, typeof(ArgumentOutOfRangeException))]
        [InlineData("Abcd", 1, 3, "bcd", true, true)]
        [InlineData("Abcd", 1, 5, "bcd", true, typeof(ArgumentOutOfRangeException))]
        [InlineData("Abcd", 2, 1, "c", true, true)]
        [InlineData("Abcd", 3, 1, "d", true, true)]
        [InlineData("Abcd", 4, 1, "", true, typeof(ArgumentOutOfRangeException))]
        [InlineData("Abcd", 0, -1, "", true, typeof(ArgumentOutOfRangeException))]
        [InlineData("Abcd", 5, -1, "", true, typeof(ArgumentOutOfRangeException))]
        public void StringSubstringStartAndLength(string productName, int startIndex, int length, string compareString, bool withNullPropagation, object withoutNullPropagation)
        {
            string filter = String.Format("substring(ProductName, {0}, {1}) eq '{2}'", startIndex, length, compareString);
            var filters = VerifyQueryDeserialization(filter);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("Abcd", true, true)]
        [InlineData("Abd", false, false)]
        public void StringContains(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            // In OData, the order of parameters is actually reversed in the resulting
            // String.Contains expression

            var filters = VerifyQueryDeserialization(
                "contains(ProductName, 'Abc')",
                "$it => $it.ProductName.Contains(\"Abc\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });

            filters = VerifyQueryDeserialization(
                "contains(ProductName, 'Abc')",
                "$it => $it.ProductName.Contains(\"Abc\")",
                NotTesting);
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("Abcd", true, true)]
        [InlineData("Abd", false, false)]
        public void StringStartsWith(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "startswith(ProductName, 'Abc')",
                "$it => $it.ProductName.StartsWith(\"Abc\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("AAbc", true, true)]
        [InlineData("Abcd", false, false)]
        public void StringEndsWith(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "endswith(ProductName, 'Abc')",
                "$it => $it.ProductName.EndsWith(\"Abc\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("AAbc", true, true)]
        [InlineData("", false, false)]
        public void StringLength(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "length(ProductName) gt 0",
                "$it => ($it.ProductName.Length > 0)",
                "$it => ((IIF(($it.ProductName == null), null, Convert($it.ProductName.Length)) > Convert(0)) == True)");

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("12345Abc", true, true)]
        [InlineData("1234Abc", false, false)]
        public void StringIndexOf(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "indexof(ProductName, 'Abc') eq 5",
                "$it => ($it.ProductName.IndexOf(\"Abc\") == 5)",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("123uctName", true, true)]
        [InlineData("1234Abc", false, false)]
        public void StringSubstring(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "substring(ProductName, 3) eq 'uctName'",
                "$it => ($it.ProductName.Substring(3) == \"uctName\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });

            VerifyQueryDeserialization(
                "substring(ProductName, 3, 4) eq 'uctN'",
                "$it => ($it.ProductName.Substring(3, 4) == \"uctN\")",
                NotTesting);
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("Tasty Treats", true, true)]
        [InlineData("Tasty Treatss", false, false)]
        public void StringToLower(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "tolower(ProductName) eq 'tasty treats'",
                "$it => ($it.ProductName.ToLower() == \"tasty treats\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("Tasty Treats", true, true)]
        [InlineData("Tasty Treatss", false, false)]
        public void StringToUpper(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "toupper(ProductName) eq 'TASTY TREATS'",
                "$it => ($it.ProductName.ToUpper() == \"TASTY TREATS\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("Tasty Treats", true, true)]
        [InlineData("Tasty Treatss", false, false)]
        public void StringTrim(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "trim(ProductName) eq 'Tasty Treats'",
                "$it => ($it.ProductName.Trim() == \"Tasty Treats\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void StringConcat()
        {
            var filters = VerifyQueryDeserialization(
                "concat('Food', 'Bar') eq 'FoodBar'",
                "$it => (\"Food\".Concat(\"Bar\") == \"FoodBar\")",
                NotTesting);

            RunFilters(filters,
              new Product { },
              new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        [Fact]
        public void RecursiveMethodCall()
        {
            var filters = VerifyQueryDeserialization(
                "floor(floor(UnitPrice)) eq 123m",
                "$it => ($it.UnitPrice.Value.Floor().Floor() == 123)",
                NotTesting);

            RunFilters(filters,
              new Product { },
              new { WithNullPropagation = false, WithoutNullPropagation = typeof(InvalidOperationException) });
        }

        #endregion

        #region Date Functions
        [Fact]
        public void DateDay()
        {
            var filters = VerifyQueryDeserialization(
                "day(DiscontinuedDate) eq 8",
                "$it => ($it.DiscontinuedDate.Value.Day == 8)",
                NotTesting);

            RunFilters(filters,
               new Product { },
               new { WithNullPropagation = false, WithoutNullPropagation = typeof(InvalidOperationException) });

            RunFilters(filters,
               new Product { DiscontinuedDate = new DateTime(2000, 10, 8) },
               new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        [Fact]
        public void DateDayNonNullable()
        {
            VerifyQueryDeserialization(
                "day(NonNullableDiscontinuedDate) eq 8",
                "$it => ($it.NonNullableDiscontinuedDate.Day == 8)");
        }

        [Fact]
        public void DateMonth()
        {
            VerifyQueryDeserialization(
                "month(DiscontinuedDate) eq 8",
                "$it => ($it.DiscontinuedDate.Value.Month == 8)",
                NotTesting);
        }

        [Fact]
        public void DateYear()
        {
            VerifyQueryDeserialization(
                "year(DiscontinuedDate) eq 1974",
                "$it => ($it.DiscontinuedDate.Value.Year == 1974)",
                NotTesting);
        }

        [Fact]
        public void DateHour()
        {
            VerifyQueryDeserialization("hour(DiscontinuedDate) eq 8",
                "$it => ($it.DiscontinuedDate.Value.Hour == 8)",
                NotTesting);
        }

        [Fact]
        public void DateMinute()
        {
            VerifyQueryDeserialization(
                "minute(DiscontinuedDate) eq 12",
                "$it => ($it.DiscontinuedDate.Value.Minute == 12)",
                NotTesting);
        }

        [Fact]
        public void DateSecond()
        {
            VerifyQueryDeserialization(
                "second(DiscontinuedDate) eq 33",
                "$it => ($it.DiscontinuedDate.Value.Second == 33)",
                NotTesting);
        }

        [Theory]
        [InlineData("year(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Year == 100)")]
        [InlineData("month(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Month == 100)")]
        [InlineData("day(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Day == 100)")]
        [InlineData("hour(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Hour == 100)")]
        [InlineData("minute(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Minute == 100)")]
        [InlineData("second(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Second == 100)")]
        public void DateTimeOffsetFunctions(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression);
        }

        [Theory]
        [InlineData("year(Birthday) eq 100", "$it => {0}.Year == 100)")]
        [InlineData("month(Birthday) eq 100", "$it => {0}.Month == 100)")]
        [InlineData("day(Birthday) eq 100", "$it => {0}.Day == 100)")]
        [InlineData("hour(Birthday) eq 100", "$it => {0}.Hour == 100)")]
        [InlineData("minute(Birthday) eq 100", "$it => {0}.Minute == 100)")]
        [InlineData("second(Birthday) eq 100", "$it => {0}.Second == 100)")]
        public void DateTimeFunctions(string filter, string expression)
        {
            TimeZoneInfoHelper.TimeZone = TimeZoneInfo.Utc;
            string expect = String.Format(expression, "($it.Birthday");
            VerifyQueryDeserialization(filter, expect);
        }

        [Theory]
        [InlineData("year(NullableDateProperty) eq 2015", "$it => ($it.NullableDateProperty.Value.Year == 2015)")]
        [InlineData("month(NullableDateProperty) eq 12", "$it => ($it.NullableDateProperty.Value.Month == 12)")]
        [InlineData("day(NullableDateProperty) eq 23", "$it => ($it.NullableDateProperty.Value.Day == 23)")]
        public void DateFunctions_Nullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression, NotTesting);
        }

        [Theory]
        [InlineData("year(DateProperty) eq 2015", "$it => ($it.DateProperty.Year == 2015)")]
        [InlineData("month(DateProperty) eq 12", "$it => ($it.DateProperty.Month == 12)")]
        [InlineData("day(DateProperty) eq 23", "$it => ($it.DateProperty.Day == 23)")]
        public void DateFunctions_NonNullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression);
        }

        [Theory]
        [InlineData("hour(NullableTimeOfDayProperty) eq 10", "$it => ($it.NullableTimeOfDayProperty.Value.Hours == 10)")]
        [InlineData("minute(NullableTimeOfDayProperty) eq 20", "$it => ($it.NullableTimeOfDayProperty.Value.Minutes == 20)")]
        [InlineData("second(NullableTimeOfDayProperty) eq 30", "$it => ($it.NullableTimeOfDayProperty.Value.Seconds == 30)")]
        public void TimeOfDayFunctions_Nullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression, NotTesting);
        }

        [Theory]
        [InlineData("hour(TimeOfDayProperty) eq 10", "$it => ($it.TimeOfDayProperty.Hours == 10)")]
        [InlineData("minute(TimeOfDayProperty) eq 20", "$it => ($it.TimeOfDayProperty.Minutes == 20)")]
        [InlineData("second(TimeOfDayProperty) eq 30", "$it => ($it.TimeOfDayProperty.Seconds == 30)")]
        public void TimeOfDayFunctions_NonNullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression);
        }

        [Theory]
        [InlineData("fractionalseconds(DiscontinuedDate) eq 0.2", "$it => ((Convert($it.DiscontinuedDate.Value.Millisecond) / 1000) == 0.2)")]
        [InlineData("fractionalseconds(NullableTimeOfDayProperty) eq 0.2", "$it => ((Convert($it.NullableTimeOfDayProperty.Value.Milliseconds) / 1000) == 0.2)")]
        public void FractionalsecondsFunction_Nullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression, NotTesting);
        }

        [Theory]
        [InlineData("fractionalseconds(NonNullableDiscontinuedDate) eq 0.2", "$it => ((Convert($it.NonNullableDiscontinuedDate.Millisecond) / 1000) == 0.2)")]
        [InlineData("fractionalseconds(TimeOfDayProperty) eq 0.2", "$it => ((Convert($it.TimeOfDayProperty.Milliseconds) / 1000) == 0.2)")]
        public void FractionalsecondsFunction_NonNullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression);
        }

        [Theory]
        [InlineData("date(DiscontinuedDate) eq 2015-02-26",
            "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))")]
        [InlineData("date(DiscontinuedDate) lt 2016-02-26",
            "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))")]
        [InlineData("2015-02-26 ge date(DiscontinuedDate)",
            "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day))")]
        [InlineData("null ne date(DiscontinuedDate)", "$it => (null != $it.DiscontinuedDate)")]
        [InlineData("date(DiscontinuedDate) eq null", "$it => ($it.DiscontinuedDate == null)")]
        public void DateFunction_Nullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression, NotTesting);
        }

        [Theory]
        [InlineData("date(NonNullableDiscontinuedDate) eq 2015-02-26",
            "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))")]
        [InlineData("date(NonNullableDiscontinuedDate) lt 2016-02-26",
            "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))")]
        [InlineData("2015-02-26 ge date(NonNullableDiscontinuedDate)",
            "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day))")]
        public void DateFunction_NonNullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression);
        }

        [Theory]
        [InlineData("time(DiscontinuedDate) eq 01:02:03.0040000",
            "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) == ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
        [InlineData("time(DiscontinuedDate) ge 01:02:03.0040000",
            "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) >= ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
        [InlineData("01:02:03.0040000 le time(DiscontinuedDate)",
            "$it => (((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))) <= ((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))))")]
        [InlineData("null ne time(DiscontinuedDate)", "$it => (null != $it.DiscontinuedDate)")]
        [InlineData("time(DiscontinuedDate) eq null", "$it => ($it.DiscontinuedDate == null)")]
        public void TimeFunction_Nullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression, NotTesting);
        }

        [Theory]
        [InlineData("time(NonNullableDiscontinuedDate) eq 01:02:03.0040000",
            "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) == ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
        [InlineData("time(NonNullableDiscontinuedDate) ge 01:02:03.0040000",
            "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) >= ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
        [InlineData("01:02:03.0040000 le time(NonNullableDiscontinuedDate)",
            "$it => (((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))) <= ((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))))")]
        public void TimeFunction_NonNullable(string filter, string expression)
        {
            VerifyQueryDeserialization(filter, expression);
        }

        #endregion

        #region Math Functions
        [Theory, PropertyData("MathRoundDecimal_DataSet")]
        public void MathRoundDecimal(decimal? unitPrice, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "round(UnitPrice) gt 5.00m",
                String.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice.Value.Round() > {0:0.00})", 5.0),
                NotTesting);

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(5.9d, true, true)]
        [InlineData(5.4d, false, false)]
        public void MathRoundDouble(double? weight, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "round(Weight) gt 5d",
                String.Format(CultureInfo.InvariantCulture, "$it => ($it.Weight.Value.Round() > {0})", 5),
                NotTesting);

            RunFilters(filters,
               new Product { Weight = ToNullable<double>(weight) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(5.9f, true, true)]
        [InlineData(5.4f, false, false)]
        public void MathRoundFloat(float? width, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "round(Width) gt 5f",
                String.Format(CultureInfo.InvariantCulture, "$it => (Convert($it.Width).Value.Round() > {0})", 5),
                NotTesting);

            RunFilters(filters,
               new Product { Width = ToNullable<float>(width) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory, PropertyData("MathFloorDecimal_DataSet")]
        public void MathFloorDecimal(decimal? unitPrice, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "floor(UnitPrice) eq 5",
                "$it => ($it.UnitPrice.Value.Floor() == 5)",
                NotTesting);

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(5.4d, true, true)]
        [InlineData(4.4d, false, false)]
        public void MathFloorDouble(double? weight, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "floor(Weight) eq 5",
                "$it => ($it.Weight.Value.Floor() == 5)",
                NotTesting);

            RunFilters(filters,
               new Product { Weight = ToNullable<double>(weight) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(5.4f, true, true)]
        [InlineData(4.4f, false, false)]
        public void MathFloorFloat(float? width, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "floor(Width) eq 5",
                "$it => (Convert($it.Width).Value.Floor() == 5)",
                NotTesting);

            RunFilters(filters,
               new Product { Width = ToNullable<float>(width) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory, PropertyData("MathCeilingDecimal_DataSet")]
        public void MathCeilingDecimal(object unitPrice, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "ceiling(UnitPrice) eq 5",
                "$it => ($it.UnitPrice.Value.Ceiling() == 5)",
                NotTesting);

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(4.1d, true, true)]
        [InlineData(5.9d, false, false)]
        public void MathCeilingDouble(double? weight, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "ceiling(Weight) eq 5",
                "$it => ($it.Weight.Value.Ceiling() == 5)",
                NotTesting);

            RunFilters(filters,
               new Product { Weight = ToNullable<double>(weight) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(4.1f, true, true)]
        [InlineData(5.9f, false, false)]
        public void MathCeilingFloat(float? width, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "ceiling(Width) eq 5",
                "$it => (Convert($it.Width).Value.Ceiling() == 5)",
                NotTesting);

            RunFilters(filters,
               new Product { Width = ToNullable<float>(width) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData("floor(FloatProp) eq floor(FloatProp)")]
        [InlineData("round(FloatProp) eq round(FloatProp)")]
        [InlineData("ceiling(FloatProp) eq ceiling(FloatProp)")]
        [InlineData("floor(DoubleProp) eq floor(DoubleProp)")]
        [InlineData("round(DoubleProp) eq round(DoubleProp)")]
        [InlineData("ceiling(DoubleProp) eq ceiling(DoubleProp)")]
        [InlineData("floor(DecimalProp) eq floor(DecimalProp)")]
        [InlineData("round(DecimalProp) eq round(DecimalProp)")]
        [InlineData("ceiling(DecimalProp) eq ceiling(DecimalProp)")]
        public void MathFunctions_VariousTypes(string filter)
        {
            var filters = VerifyQueryDeserialization<DataTypes>(filter);
            RunFilters(filters, new DataTypes(), new { WithNullPropagation = true, WithoutNullPropagation = true });
        }
        #endregion

        #region Custom Functions

        [Fact]
        public void CustomMethod_InstanceMethodOfDeclaringType()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction =
                    new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                     EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });
            const string padrightMethodName = "padright";
            try
            {
                const string productName = "Abcd";
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
                UriFunctionsBinder.BindUriFunctionName(padrightMethodName, padRightStringMethodInfo);

                string filter = String.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);
                var filters = VerifyQueryDeserialization(filter);

                RunFilters(filters,
                  new Product { ProductName = productName },
                  new { WithNullPropagation = true, WithoutNullPropagation = true });
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void CustomMethod_InstanceMethodNotOfDeclaringType()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
                        EdmCoreModel.Instance.GetString(true),
                        EdmCoreModel.Instance.GetString(true),
                        EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(FilterBinderTests).GetMethod("PadRightInstance", BindingFlags.NonPublic | BindingFlags.Instance);

            const string padrightMethodName = "padright";
            try
            {
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
                UriFunctionsBinder.BindUriFunctionName(padrightMethodName, padRightStringMethodInfo);

                string filter = String.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);

                Action filterToExpression = () => VerifyQueryDeserialization(filter);
                Assert.Throws(typeof(NotImplementedException),filterToExpression);
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void CustomMethod_StaticExtensionMethod()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(StringExtender).GetMethod("PadRightExStatic", BindingFlags.Public | BindingFlags.Static);

            const string padrightMethodName = "padright";
            try
            {
                const string productName = "Abcd";
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
                UriFunctionsBinder.BindUriFunctionName(padrightMethodName, padRightStringMethodInfo);

                string filter = String.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);
                var filters = VerifyQueryDeserialization(filter);

                RunFilters(filters,
                  new Product { ProductName = productName },
                  new { WithNullPropagation = true, WithoutNullPropagation = true });
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void CustomMethod_StaticMethodNotOfDeclaringType()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(FilterBinderTests).GetMethod("PadRightStatic", BindingFlags.NonPublic | BindingFlags.Static);

            const string padrightMethodName = "padright";
            try
            {
                const string productName = "Abcd";
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
                UriFunctionsBinder.BindUriFunctionName(padrightMethodName, padRightStringMethodInfo);

                string filter = String.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);
                var filters = VerifyQueryDeserialization(filter);

                RunFilters(filters,
                  new Product { ProductName = productName },
                  new { WithNullPropagation = true, WithoutNullPropagation = true });
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void CustomMethod_AddSignatureAndBindFunctionWithShortcut()
        {
            FunctionSignatureWithReturnType padrightStringEdmFunction =
                    new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                     EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false));

            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });
            const string padrightMethodName = "padright";
            try
            {
                const string productName = "Abcd";
                const int totalWidth = 5;
                const string expectedProductName = "Abcd ";

                // Add the custom function
                ODataUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction, padRightStringMethodInfo);

                string filter = String.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);
                var filters = VerifyQueryDeserialization(filter);

                RunFilters(filters,
                  new Product { ProductName = productName },
                  new { WithNullPropagation = true, WithoutNullPropagation = true });
            }
            finally
            {
                Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
                Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
            }
        }

        #endregion

        #region Data Types
        [Fact]
        public void GuidExpression()
        {
            VerifyQueryDeserialization<DataTypes>(
                "GuidProp eq 0EFDAECF-A9F0-42F3-A384-1295917AF95E",
                "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");

            // verify case insensitivity
            VerifyQueryDeserialization<DataTypes>(
                "GuidProp eq 0EFDAECF-A9F0-42F3-A384-1295917AF95E",
                "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");
        }

        [Theory]
        [InlineData("DateTimeProp eq 2000-12-12T12:00:00Z", "$it => ($it.DateTimeProp == {0})")]
        [InlineData("DateTimeProp lt 2000-12-12T12:00:00Z", "$it => ($it.DateTimeProp < {0})")]
        // TODO: [InlineData("DateTimeProp ge datetime'2000-12-12T12:00'", "$it => ($it.DateTimeProp >= {0})")] (uriparser fails on optional seconds)
        public void DateTimeExpression(string clause, string expectedExpression)
        {
            var dateTime = new DateTimeOffset(new DateTime(2000, 12, 12, 12, 0, 0), TimeSpan.Zero);
            VerifyQueryDeserialization<DataTypes>(
                "" + clause,
                String.Format(CultureInfo.InvariantCulture, expectedExpression, dateTime));
        }

        [Theory]
        [InlineData("DateTimeOffsetProp eq datetimeoffset'2002-10-10T17:00:00Z'", "$it => ($it.DateTimeOffsetProp == {0})", 0)]
        [InlineData("DateTimeOffsetProp ge datetimeoffset'2002-10-10T17:00:00Z'", "$it => ($it.DateTimeOffsetProp >= {0})", 0)]
        [InlineData("DateTimeOffsetProp le datetimeoffset'2002-10-10T17:00:00-07:00'", "$it => ($it.DateTimeOffsetProp <= {0})", -7)]
        [InlineData("DateTimeOffsetProp eq datetimeoffset'2002-10-10T17:00:00-0600'", "$it => ($it.DateTimeOffsetProp == {0})", -6)]
        [InlineData("DateTimeOffsetProp lt datetimeoffset'2002-10-10T17:00:00-05'", "$it => ($it.DateTimeOffsetProp < {0})", -5)]
        [InlineData("DateTimeOffsetProp ne datetimeoffset'2002-10-10T17:00:00%2B09:30'", "$it => ($it.DateTimeOffsetProp != {0})", 9.5)]
        [InlineData("DateTimeOffsetProp gt datetimeoffset'2002-10-10T17:00:00%2B0545'", "$it => ($it.DateTimeOffsetProp > {0})", 5.75)]
        public void DateTimeOffsetExpression(string clause, string expectedExpression, double offsetHours)
        {
            var dateTimeOffset = new DateTimeOffset(2002, 10, 10, 17, 0, 0, TimeSpan.FromHours(offsetHours));

            // There's currently a bug here. For now, the test checks for the presence of the bug (as a reminder to fix
            // the test once the bug is fixed).
            // The following assert shows the behavior with the bug and should be removed once the bug is fixed.
            Assert.Throws<ODataException>(() => Bind("" + clause));

            // TODO: No DateTimeOffset parsing in ODataUriParser
            // The following call shows the behavior without the bug, and should be enabled once the bug is fixed.
            //VerifyQueryDeserialization<DataTypes>(
            //    "" + clause,
            //    String.Format(CultureInfo.InvariantCulture, expectedExpression, dateTimeOffset));
        }

        [Fact]
        public void IntegerLiteralSuffix()
        {
            // long L
            VerifyQueryDeserialization<DataTypes>(
                "LongProp lt 987654321L and LongProp gt 123456789l",
                "$it => (($it.LongProp < 987654321) AndAlso ($it.LongProp > 123456789))");

            VerifyQueryDeserialization<DataTypes>(
                "LongProp lt -987654321L and LongProp gt -123456789l",
                "$it => (($it.LongProp < -987654321) AndAlso ($it.LongProp > -123456789))");
        }

        [Fact]
        public void RealLiteralSuffixes()
        {
            // Float F
            VerifyQueryDeserialization<DataTypes>(
                "FloatProp lt 4321.56F and FloatProp gt 1234.56f",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.FloatProp < {0:0.00}) AndAlso ($it.FloatProp > {1:0.00}))", 4321.56, 1234.56));

            // Decimal M
            VerifyQueryDeserialization<DataTypes>(
                "DecimalProp lt 4321.56M and DecimalProp gt 1234.56m",
                String.Format(CultureInfo.InvariantCulture, "$it => (($it.DecimalProp < {0:0.00}) AndAlso ($it.DecimalProp > {1:0.00}))", 4321.56, 1234.56));
        }

        [Theory]
        [InlineData("'hello,world'", "hello,world")]
        [InlineData("'''hello,world'", "'hello,world")]
        [InlineData("'hello,world'''", "hello,world'")]
        [InlineData("'hello,''wor''ld'", "hello,'wor'ld")]
        [InlineData("'hello,''''''world'", "hello,'''world")]
        [InlineData("'\"hello,world\"'", "\"hello,world\"")]
        [InlineData("'\"hello,world'", "\"hello,world")]
        [InlineData("'hello,world\"'", "hello,world\"")]
        [InlineData("'hello,\"world'", "hello,\"world")]
        [InlineData("'México D.F.'", "México D.F.")]
        [InlineData("'æææøøøååå'", "æææøøøååå")]
        [InlineData("'いくつかのテキスト'", "いくつかのテキスト")]
        public void StringLiterals(string literal, string expected)
        {
            VerifyQueryDeserialization<Product>(
                "ProductName eq " + literal,
                String.Format("$it => ($it.ProductName == \"{0}\")", expected));
        }

        [Theory]
        [InlineData('$')]
        [InlineData('&')]
        [InlineData('+')]
        [InlineData(',')]
        [InlineData('/')]
        [InlineData(':')]
        [InlineData(';')]
        [InlineData('=')]
        [InlineData('?')]
        [InlineData('@')]
        [InlineData(' ')]
        [InlineData('<')]
        [InlineData('>')]
        [InlineData('#')]
        [InlineData('%')]
        [InlineData('{')]
        [InlineData('}')]
        [InlineData('|')]
        [InlineData('\\')]
        [InlineData('^')]
        [InlineData('~')]
        [InlineData('[')]
        [InlineData(']')]
        [InlineData('`')]
        public void SpecialCharactersInStringLiteral(char c)
        {
            var filters = VerifyQueryDeserialization<Product>(
                "ProductName eq '" + c + "'",
                String.Format("$it => ($it.ProductName == \"{0}\")", c));

            RunFilters(
                filters,
                new Product { ProductName = c.ToString() },
                new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        #endregion

        #region Casts

        [Fact]
        public void NSCast_OnEnumerableEntityCollection_GeneratesExpression_WithOfTypeOnEnumerable()
        {
            var filters = VerifyQueryDeserialization(
                "Category/EnumerableProducts/System.Web.OData.Query.Expressions.DerivedProduct/any(p: p/ProductName eq 'ProductName')",
                "$it => $it.Category.EnumerableProducts.OfType().Any(p => (p.ProductName == \"ProductName\"))",
                NotTesting);

            Assert.NotNull(filters.WithoutNullPropagation);
        }

        [Fact]
        public void NSCast_OnQueryableEntityCollection_GeneratesExpression_WithOfTypeOnQueryable()
        {
            var filters = VerifyQueryDeserialization(
                "Category/QueryableProducts/System.Web.OData.Query.Expressions.DerivedProduct/any(p: p/ProductName eq 'ProductName')",
                "$it => $it.Category.QueryableProducts.OfType().Any(p => (p.ProductName == \"ProductName\"))",
                NotTesting);
        }

        [Fact]
        public void NSCast_OnEntityCollection_CanAccessDerivedInstanceProperty()
        {
            var filters = VerifyQueryDeserialization(
                "Category/Products/System.Web.OData.Query.Expressions.DerivedProduct/any(p: p/DerivedProductName eq 'DerivedProductName')");

            RunFilters(
                filters,
                new Product { Category = new Category { Products = new Product[] { new DerivedProduct { DerivedProductName = "DerivedProductName" } } } },
                new { WithNullPropagation = true, WithoutNullPropagation = true });

            RunFilters(
                filters,
                new Product { Category = new Category { Products = new Product[] { new DerivedProduct { DerivedProductName = "NotDerivedProductName" } } } },
                new { WithNullPropagation = false, WithoutNullPropagation = false });
        }

        [Fact]
        public void NSCast_OnSingleEntity_GeneratesExpression_WithAsOperator()
        {
            var filters = VerifyQueryDeserialization(
                "System.Web.OData.Query.Expressions.Product/ProductName eq 'ProductName'",
                "$it => (($it As Product).ProductName == \"ProductName\")",
                NotTesting);
        }

        [Theory]
        [InlineData("System.Web.OData.Query.Expressions.Product/ProductName eq 'ProductName'")]
        [InlineData("System.Web.OData.Query.Expressions.DerivedProduct/DerivedProductName eq 'DerivedProductName'")]
        [InlineData("System.Web.OData.Query.Expressions.DerivedProduct/Category/CategoryID eq 123")]
        [InlineData("System.Web.OData.Query.Expressions.DerivedProduct/Category/System.Web.OData.Query.Expressions.DerivedCategory/CategoryID eq 123")]
        public void Inheritance_WithDerivedInstance(string filter)
        {
            var filters = VerifyQueryDeserialization<DerivedProduct>(filter);

            RunFilters<DerivedProduct>(filters,
              new DerivedProduct { Category = new DerivedCategory { CategoryID = 123 }, ProductName = "ProductName", DerivedProductName = "DerivedProductName" },
              new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        [Theory]
        [InlineData("System.Web.OData.Query.Expressions.DerivedProduct/DerivedProductName eq 'ProductName'")]
        [InlineData("System.Web.OData.Query.Expressions.DerivedProduct/Category/CategoryID eq 123")]
        [InlineData("System.Web.OData.Query.Expressions.DerivedProduct/Category/System.Web.OData.Query.Expressions.DerivedCategory/CategoryID eq 123")]
        public void Inheritance_WithBaseInstance(string filter)
        {
            var filters = VerifyQueryDeserialization<Product>(filter);

            RunFilters<Product>(filters,
              new Product(),
              new { WithNullPropagation = false, WithoutNullPropagation = typeof(NullReferenceException) });
        }

        [Fact]
        public void CastToNonDerivedType_Throws()
        {
            Assert.Throws<ODataException>(
                () => VerifyQueryDeserialization<Product>("System.Web.OData.Query.Expressions.DerivedCategory/CategoryID eq 123"),
                "Encountered invalid type cast. 'System.Web.OData.Query.Expressions.DerivedCategory' is not assignable from 'System.Web.OData.Query.Expressions.Product'.");
        }

        [Theory]
        [InlineData("Edm.Int32 eq 123", "A binary operator with incompatible types was detected. Found operand types 'Edm.String' and 'Edm.Int32' for operator kind 'Equal'.")]
        [InlineData("ProductName/Edm.String eq 123", "A binary operator with incompatible types was detected. Found operand types 'Edm.String' and 'Edm.Int32' for operator kind 'Equal'.")]
        public void CastToNonEntityType_Throws(string filter, string error)
        {
            Assert.Throws<ODataException>(
                () => VerifyQueryDeserialization<Product>(filter), error);
        }

        [Theory]
        [InlineData("Edm.NonExistentType eq 123")]
        [InlineData("Category/Edm.NonExistentType eq 123")]
        [InlineData("Category/Products/Edm.NonExistentType eq 123")]
        public void CastToNonExistantType_Throws(string filter)
        {
            Assert.Throws<ODataException>(
                () => VerifyQueryDeserialization<Product>(filter),
                "The child type 'Edm.NonExistentType' in a cast was not an entity type. Casts can only be performed on entity types.");
        }

        #endregion

        #region cast in query option

        [Theory]
        [InlineData("cast(null,Edm.Int16) eq null", "$it => (null == null)")]
        [InlineData("cast(null,Edm.Int32) eq 123", "$it => (null == Convert(123))")]
        [InlineData("cast(null,Edm.Int64) ne 123", "$it => (null != Convert(123))")]
        [InlineData("cast(null,Edm.Single) ne 123", "$it => (null != Convert(123))")]
        [InlineData("cast(null,Edm.Double) ne 123", "$it => (null != Convert(123))")]
        [InlineData("cast(null,Edm.Decimal) ne 123", "$it => (null != Convert(123))")]
        [InlineData("cast(null,Edm.Boolean) ne true", "$it => (null != Convert(True))")]
        [InlineData("cast(null,Edm.Byte) ne 1", "$it => (null != Convert(1))")]
        [InlineData("cast(null,Edm.Guid) eq 00000000-0000-0000-0000-000000000000", "$it => (null == Convert(00000000-0000-0000-0000-000000000000))")]
        [InlineData("cast(null,Edm.String) ne '123'", "$it => (null != \"123\")")]
        [InlineData("cast(null,Edm.DateTimeOffset) eq 2001-01-01T12:00:00.000+08:00", "$it => (null == Convert(01/01/2001 12:00:00 +08:00))")]
        [InlineData("cast(null,Edm.Duration) eq duration'P8DT23H59M59.9999S'", "$it => (null == Convert(8.23:59:59.9999000))")]
        [InlineData("cast(null,'Microsoft.TestCommon.Types.SimpleEnum') eq null", "$it => (null == null)")]
        [InlineData("cast(null,'Microsoft.TestCommon.Types.FlagsEnum') eq null", "$it => (null == null)")]
        [InlineData("cast(IntProp,Edm.String) eq '123'", "$it => (Convert($it.IntProp.ToString()) == \"123\")")]
        [InlineData("cast(LongProp,Edm.String) eq '123'", "$it => (Convert($it.LongProp.ToString()) == \"123\")")]
        [InlineData("cast(SingleProp,Edm.String) eq '123'", "$it => (Convert($it.SingleProp.ToString()) == \"123\")")]
        [InlineData("cast(DoubleProp,Edm.String) eq '123'", "$it => (Convert($it.DoubleProp.ToString()) == \"123\")")]
        [InlineData("cast(DecimalProp,Edm.String) eq '123'", "$it => (Convert($it.DecimalProp.ToString()) == \"123\")")]
        [InlineData("cast(BoolProp,Edm.String) eq '123'", "$it => (Convert($it.BoolProp.ToString()) == \"123\")")]
        [InlineData("cast(ByteProp,Edm.String) eq '123'", "$it => (Convert($it.ByteProp.ToString()) == \"123\")")]
        [InlineData("cast(GuidProp,Edm.String) eq '123'", "$it => (Convert($it.GuidProp.ToString()) == \"123\")")]
        [InlineData("cast(StringProp,Edm.String) eq '123'", "$it => (Convert($it.StringProp) == \"123\")")]
        [InlineData("cast(DateTimeOffsetProp,Edm.String) eq '123'", "$it => (Convert($it.DateTimeOffsetProp.ToString()) == \"123\")")]
        [InlineData("cast(TimeSpanProp,Edm.String) eq '123'", "$it => (Convert($it.TimeSpanProp.ToString()) == \"123\")")]
        [InlineData("cast(SimpleEnumProp,Edm.String) eq '123'", "$it => (Convert(Convert($it.SimpleEnumProp).ToString()) == \"123\")")]
        [InlineData("cast(FlagsEnumProp,Edm.String) eq '123'", "$it => (Convert(Convert($it.FlagsEnumProp).ToString()) == \"123\")")]
        [InlineData("cast(LongEnumProp,Edm.String) eq '123'", "$it => (Convert(Convert($it.LongEnumProp).ToString()) == \"123\")")]
        [InlineData("cast(NullableIntProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableIntProp.HasValue, $it.NullableIntProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableIntProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableIntProp.HasValue, $it.NullableIntProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableLongProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableLongProp.HasValue, $it.NullableLongProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableSingleProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableSingleProp.HasValue, $it.NullableSingleProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableDoubleProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableDoubleProp.HasValue, $it.NullableDoubleProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableDecimalProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableDecimalProp.HasValue, $it.NullableDecimalProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableBoolProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableBoolProp.HasValue, $it.NullableBoolProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableByteProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableByteProp.HasValue, $it.NullableByteProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableGuidProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableGuidProp.HasValue, $it.NullableGuidProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableDateTimeOffsetProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableDateTimeOffsetProp.HasValue, $it.NullableDateTimeOffsetProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableTimeSpanProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableTimeSpanProp.HasValue, $it.NullableTimeSpanProp.Value.ToString(), null)) == \"123\")")]
        [InlineData("cast(NullableSimpleEnumProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableSimpleEnumProp.HasValue, Convert($it.NullableSimpleEnumProp.Value).ToString(), null)) == \"123\")")]
        [InlineData("cast(IntProp,Edm.Int64) eq 123", "$it => (Convert($it.IntProp) == 123)")]
        [InlineData("cast(NullableLongProp,Edm.Double) eq 1.23", "$it => (Convert($it.NullableLongProp) == Convert(1.23))")]
        [InlineData("cast(2147483647,Edm.Int16) ne null", "$it => (Convert(Convert(2147483647)) != null)")]
        [InlineData("cast(Microsoft.TestCommon.Types.SimpleEnum'1',Edm.String) eq '1'", "$it => (Convert(Convert(Second).ToString()) == \"1\")")]
        [InlineData("cast(cast(cast(IntProp,Edm.Int64),Edm.Int16),Edm.String) eq '123'", "$it => (Convert(Convert(Convert($it.IntProp)).ToString()) == \"123\")")]
        [InlineData("cast('123',Microsoft.TestCommon.Types.SimpleEnum) ne null", "$it => (Convert(123) != null)")]
        public void CastMethod_Succeeds(string filter, string expectedResult)
        {
            // Arrange & Act & Assert
            VerifyQueryDeserialization<DataTypes>(
                filter,
                expectedResult,
                NotTesting);
        }

        [Theory]
        [InlineData("cast(NoSuchProperty,Edm.Int32) ne null",
            "Could not find a property named 'NoSuchProperty' on type 'System.Web.OData.Query.Expressions.DataTypes'.")]
        public void Cast_UndefinedSource_ThrowsODataException(string filter, string errorMessage)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataException>(() => Bind<DataTypes>(filter), errorMessage);
        }

        public static TheoryDataSet<string, string> CastToUnquotedUndefinedTarget
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    { "cast(Edm.DateTime) eq null", "Edm.DateTime" },
                    { "cast(Edm.Unknown) eq null", "Edm.Unknown" },
                    { "cast(null,Edm.DateTime) eq null", "Edm.DateTime" },
                    { "cast(null,Edm.Unknown) eq null", "Edm.Unknown" },
                    { "cast('2001-01-01T12:00:00.000',Edm.DateTime) eq null", "Edm.DateTime" },
                    { "cast('2001-01-01T12:00:00.000',Edm.Unknown) eq null", "Edm.Unknown" },
                    { "cast(DateTimeProp,Edm.DateTime) eq null", "Edm.DateTime" },
                    { "cast(DateTimeProp,Edm.Unknown) eq null", "Edm.Unknown" },
                };
            }
        }

        // Exception messages here and in CastQuotedUndefinedTarget_ThrowsODataException should be consistent.
        // Worse, this message is incorrect -- casts can be performed on most types but _not_ entity types.
        [Theory]
        [PropertyData("CastToUnquotedUndefinedTarget")]
        public void CastToUnquotedUndefinedTarget_ThrowsODataException(string filter, string typeName)
        {
            // Arrange
            var expectedMessage = string.Format(
                "The child type '{0}' in a cast was not an entity type. Casts can only be performed on entity types.",
                typeName);

            // Act & Assert
            Assert.Throws<ODataException>(() => Bind<DataTypes>(filter), expectedMessage);
        }

        public static TheoryDataSet<string> CastToQuotedUndefinedTarget
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "cast('Edm.DateTime') eq null" },
                    { "cast('Edm.Unknown') eq null" },
                    { "cast(null,'Edm.DateTime') eq null" },
                    { "cast(null,'Edm.Unknown') eq null" },
                    { "cast('2001-01-01T12:00:00.000','Edm.DateTime') eq null" },
                    { "cast('','Edm.Unknown') eq null" },
                    { "cast(DateTimeProp,'Edm.DateTime') eq null" },
                    { "cast(IntProp,'Edm.Unknown') eq null" },
                };
            }
        }

        [Theory]
        [PropertyData("CastToQuotedUndefinedTarget")]
        public void CastToQuotedUndefinedTarget_ThrowsODataException(string filter)
        {
            // Arrange
            var expectedMessage = "Cast or IsOf Function must have a type in its arguments.";

            // Act & Assert
            Assert.Throws<ODataException>(() => Bind<DataTypes>(filter), expectedMessage);
        }

        [Theory]
        [InlineData("cast(Microsoft.TestCommon.Types.SimpleEnum) ne null")]
        [InlineData("cast(Microsoft.TestCommon.Types.FlagsEnum) ne null")]
        [InlineData("cast(0,Microsoft.TestCommon.Types.SimpleEnum) ne null")]
        [InlineData("cast(0,Microsoft.TestCommon.Types.FlagsEnum) ne null")]
        [InlineData("cast(Microsoft.TestCommon.Types.SimpleEnum'0',Microsoft.TestCommon.Types.SimpleEnum) ne null")]
        [InlineData("cast(Microsoft.TestCommon.Types.FlagsEnum'0',Microsoft.TestCommon.Types.FlagsEnum) ne null")]
        [InlineData("cast(SimpleEnumProp,Microsoft.TestCommon.Types.SimpleEnum) ne null")]
        [InlineData("cast(FlagsEnumProp,Microsoft.TestCommon.Types.FlagsEnum) ne null")]
        [InlineData("cast(NullableSimpleEnumProp,Microsoft.TestCommon.Types.SimpleEnum) ne null")]
        [InlineData("cast(IntProp,Microsoft.TestCommon.Types.SimpleEnum) ne null")]
        [InlineData("cast(DateTimeOffsetProp,Microsoft.TestCommon.Types.SimpleEnum) ne null")]
        [InlineData("cast(Microsoft.TestCommon.Types.SimpleEnum'1',Edm.Int32) eq 1")]
        [InlineData("cast(Microsoft.TestCommon.Types.FlagsEnum'1',Edm.Int32) eq 1")]
        [InlineData("cast(SimpleEnumProp,Edm.Int32) eq 123")]
        [InlineData("cast(FlagsEnumProp,Edm.Int32) eq 123")]
        [InlineData("cast(NullableSimpleEnumProp,Edm.Guid) ne null")]

        [InlineData("cast('Microsoft.TestCommon.Types.SimpleEnum') ne null")]
        [InlineData("cast('Microsoft.TestCommon.Types.FlagsEnum') ne null")]
        [InlineData("cast(0,'Microsoft.TestCommon.Types.SimpleEnum') ne null")]
        [InlineData("cast(0,'Microsoft.TestCommon.Types.FlagsEnum') ne null")]
        [InlineData("cast(Microsoft.TestCommon.Types.SimpleEnum'0','Microsoft.TestCommon.Types.SimpleEnum') ne null")]
        [InlineData("cast(Microsoft.TestCommon.Types.FlagsEnum'0','Microsoft.TestCommon.Types.FlagsEnum') ne null")]
        [InlineData("cast(SimpleEnumProp,'Microsoft.TestCommon.Types.SimpleEnum') ne null")]
        [InlineData("cast(FlagsEnumProp,'Microsoft.TestCommon.Types.FlagsEnum') ne null")]
        [InlineData("cast(NullableSimpleEnumProp,'Microsoft.TestCommon.Types.SimpleEnum') ne null")]
        [InlineData("cast(IntProp,'Microsoft.TestCommon.Types.SimpleEnum') ne null")]
        [InlineData("cast(DateTimeOffsetProp,'Microsoft.TestCommon.Types.SimpleEnum') ne null")]
        [InlineData("cast(Microsoft.TestCommon.Types.SimpleEnum'1','Edm.Int32') eq 1")]
        [InlineData("cast(Microsoft.TestCommon.Types.FlagsEnum'1','Edm.Int32') eq 1")]
        [InlineData("cast(SimpleEnumProp,'Edm.Int32') eq 123")]
        [InlineData("cast(FlagsEnumProp,'Edm.Int32') eq 123")]
        [InlineData("cast(NullableSimpleEnumProp,'Edm.Guid') ne null")]
        public void Cast_UnsupportedSourceOrTargetForEnumCast_Throws(string filter)
        {
            // Arrange & Act & Assert
            // TODO : 1824 Should not throw exception for invalid enum cast in query option.
            Assert.Throws<ODataException>(() => Bind<DataTypes>(filter), "Enumeration type value can only be casted to or from string.");
        }

        [Theory]
        [InlineData("cast(IntProp,Edm.DateTimeOffset) eq null")]
        [InlineData("cast(ByteProp,Edm.Guid) eq null")]
        [InlineData("cast(NullableLongProp,Edm.Duration) eq null")]
        [InlineData("cast(StringProp,Edm.Double) eq null")]
        [InlineData("cast(StringProp,Edm.Int16) eq null")]
        [InlineData("cast(DateTimeOffsetProp,Edm.Int32) eq null")]
        [InlineData("cast(NullableGuidProp,Edm.Int64) eq null")]
        [InlineData("cast(Edm.Int32) eq null")]
        [InlineData("cast($it,Edm.String) eq null")]
        [InlineData("cast(ComplexProp,Edm.Double) eq null")]
        [InlineData("cast(ComplexProp,Edm.String) eq null")]
        [InlineData("cast(StringProp,Microsoft.TestCommon.Types.SimpleEnum) eq null")]
        [InlineData("cast(StringProp,Microsoft.TestCommon.Types.FlagsEnum) eq null")]
        public void Cast_UnsupportedTarget_ReturnsNull(string filter)
        {
            // Arrange & Act & Assert
            VerifyQueryDeserialization<DataTypes>(filter, "$it => (null == null)");
        }

        // See OtherFunctions_SomeTwoParameterCasts_ThrowODataException and OtherFunctions_SomeSingleParameterCasts_ThrowODataException
        // in FilterQueryValidatorTest.  ODL's ODataQueryOptionParser and FunctionCallBinder call the code throwing these exceptions.
        [Theory]
        [InlineData("cast(null,System.Web.OData.Query.Expressions.Address) ne null",
            "Encountered invalid type cast. " +
            "'System.Web.OData.Query.Expressions.Address' is not assignable from 'System.Web.OData.Query.Expressions.DataTypes'.")]
        [InlineData("cast(null,System.Web.OData.Query.Expressions.DataTypes) ne null",
            "Cast or IsOf Function must have a type in its arguments.")]
        public void Cast_NonPrimitiveTarget_ThrowsODataException(string filter, string expectErrorMessage)
        {
            // Arrange & Act & Assert
            // TODO : 1827 Should not throw when the target type of cast is not primitive or enumeration type.
            Assert.Throws<ODataException>(() => Bind<DataTypes>(filter), expectErrorMessage);
        }

        [Theory]
        [InlineData("cast(null,'Edm.Int32') ne null")]
        [InlineData("cast(StringProp,'Microsoft.TestCommon.Types.SimpleEnum') eq null")]
        [InlineData("cast(IntProp,'Edm.String') eq '123'")]
        [InlineData("cast('System.Web.OData.Query.Expressions.DataTypes') eq null")]
        [InlineData("cast($it,'System.Web.OData.Query.Expressions.DataTypes') eq null")]
        public void SingleQuotesOnTypeNameOfCast_WorksForNow(string filter)
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<DataTypes>("Customers");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.FindDeclaredEntitySet("Customers");
            IEdmEntityType entityType = entitySet.EntityType();
            var parser = new ODataQueryOptionParser(model, entityType, entitySet,
                new Dictionary<string, string> { { "$filter", filter } });

            // Act & Assert
            // TODO : 1927 ODL parser should throw when there are single quotes on type name of cast.
            Assert.NotNull(parser.ParseFilter());
        }

        [Fact]
        public void SingleQuotesOnEnumTypeNameOfCast_WorksForNow()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<DataTypes>("Customers");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.FindDeclaredEntitySet("Customers");
            IEdmEntityType entityType = entitySet.EntityType();
            var parser = new ODataQueryOptionParser(model, entityType, entitySet,
                new Dictionary<string, string>
                {
                    { "$filter", "cast(StringProp,'Microsoft.TestCommon.Types.SimpleEnum') eq null" }
                });

            // Act
            // TODO : 1927 ODL parser should throw when there are single quotes on type name of cast.
            FilterClause filterClause = parser.ParseFilter();

            // Assert
            Assert.NotNull(filterClause);
            var castNode = Assert.IsType<SingleValueFunctionCallNode>(((BinaryOperatorNode)filterClause.Expression).Left);
            Assert.Equal("cast", castNode.Name);
            Assert.Equal("Microsoft.TestCommon.Types.SimpleEnum", ((ConstantNode)castNode.Parameters.Last()).Value);
        }

        public static TheoryDataSet<string> CastToQuotedPrimitiveType
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "cast('Edm.Binary') eq null" },
                    { "cast('Edm.Boolean') eq null" },
                    { "cast('Edm.Byte') eq null" },
                    { "cast('Edm.DateTimeOffset') eq null" },
                    { "cast('Edm.Decimal') eq null" },
                    { "cast('Edm.Double') eq null" },
                    { "cast('Edm.Duration') eq null" },
                    { "cast('Edm.Guid') eq null" },
                    { "cast('Edm.Int16') eq null" },
                    { "cast('Edm.Int32') eq null" },
                    { "cast('Edm.Int64') eq null" },
                    { "cast('Edm.SByte') eq null" },
                    { "cast('Edm.Single') eq null" },
                    { "cast('Edm.String') eq null" },

                    { "cast(null,'Edm.Binary') eq null" },
                    { "cast(null,'Edm.Boolean') eq null" },
                    { "cast(null,'Edm.Byte') eq null" },
                    { "cast(null,'Edm.DateTimeOffset') eq null" },
                    { "cast(null,'Edm.Decimal') eq null" },
                    { "cast(null,'Edm.Double') eq null" },
                    { "cast(null,'Edm.Duration') eq null" },
                    { "cast(null,'Edm.Guid') eq null" },
                    { "cast(null,'Edm.Int16') eq null" },
                    { "cast(null,'Edm.Int32') eq null" },
                    { "cast(null,'Edm.Int64') eq null" },
                    { "cast(null,'Edm.SByte') eq null" },
                    { "cast(null,'Edm.Single') eq null" },
                    { "cast(null,'Edm.String') eq null" },

                    { "cast(binary'T0RhdGE=','Edm.Binary') eq binary'T0RhdGE='" },
                    { "cast(false,'Edm.Boolean') eq false" },
                    { "cast(23,'Edm.Byte') eq 23" },
                    { "cast(2001-01-01T12:00:00.000+08:00,'Edm.DateTimeOffset') eq 2001-01-01T12:00:00.000+08:00" },
                    { "cast(23,'Edm.Decimal') eq 23" },
                    { "cast(23,'Edm.Double') eq 23" },
                    { "cast(duration'PT12H','Edm.Duration') eq duration'PT12H'" },
                    { "cast(00000000-0000-0000-0000-000000000000,'Edm.Guid') eq 00000000-0000-0000-0000-000000000000" },
                    { "cast(23,'Edm.Int16') eq 23" },
                    { "cast(23,'Edm.Int32') eq 23" },
                    { "cast(23,'Edm.Int64') eq 23" },
                    { "cast(23,'Edm.SByte') eq 23" },
                    { "cast(23,'Edm.Single') eq 23" },
                    { "cast('hello','Edm.String') eq 'hello'" },

                    { "cast(ByteArrayProp,'Edm.Binary') eq null" },
                    { "cast(BoolProp,'Edm.Boolean') eq true" },
                    { "cast(DateTimeOffsetProp,'Edm.DateTimeOffset') eq 2001-01-01T12:00:00.000+08:00" },
                    { "cast(DecimalProp,'Edm.Decimal') eq 23" },
                    { "cast(DoubleProp,'Edm.Double') eq 23" },
                    { "cast(TimeSpanProp,'Edm.Duration') eq duration'PT23H'" },
                    { "cast(GuidProp,'Edm.Guid') eq 0EFDAECF-A9F0-42F3-A384-1295917AF95E" },
                    { "cast(NullableShortProp,'Edm.Int16') eq 23" },
                    { "cast(IntProp,'Edm.Int32') eq 23" },
                    { "cast(LongProp,'Edm.Int64') eq 23" },
                    { "cast(FloatProp,'Edm.Single') eq 23" },
                    { "cast(StringProp,'Edm.String') eq 'hello'" },
                };
            }
        }

        [Theory]
        [PropertyData("CastToQuotedPrimitiveType")]
        public void CastToQuotedPrimitiveType_Succeeds(string filter)
        {
            // Arrange
            var model = new DataTypes
            {
                BoolProp = true,
                DateTimeOffsetProp = DateTimeOffset.Parse("2001-01-01T12:00:00.000+08:00"),
                DecimalProp = 23,
                DoubleProp = 23,
                GuidProp = Guid.Parse("0EFDAECF-A9F0-42F3-A384-1295917AF95E"),
                NullableShortProp = 23,
                IntProp = 23,
                LongProp = 23,
                FloatProp = 23,
                StringProp = "hello",
                TimeSpanProp = TimeSpan.FromHours(23),
            };

            // Act & Assert
            var filters = VerifyQueryDeserialization<DataTypes>(
                filter,
                expectedResult: NotTesting,
                expectedResultWithNullPropagation: NotTesting);
            RunFilters(filters, model, expectedValue: new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        public static TheoryDataSet<string> CastToUnquotedComplexType
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "cast(System.Web.OData.Query.Expressions.Address) eq null" },
                    { "cast(null, System.Web.OData.Query.Expressions.Address) eq null" },
                    { "cast('', System.Web.OData.Query.Expressions.Address) eq null" },
                    { "cast(SupplierAddress, System.Web.OData.Query.Expressions.Address) eq null" },
                };
            }
        }

        [Theory]
        [PropertyData("CastToUnquotedComplexType")]
        public void CastToUnquotedComplexType_ThrowsODataException(string filter)
        {
            // Arrange
            var expectedMessage =
                "Encountered invalid type cast. " +
                "'System.Web.OData.Query.Expressions.Address' is not assignable from 'System.Web.OData.Query.Expressions.Product'.";

            // Act & Assert
            Assert.Throws<ODataException>(() => Bind<Product>(filter), expectedMessage);
        }

        public static TheoryDataSet<string> CastToQuotedComplexType
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "cast('System.Web.OData.Query.Expressions.Address') eq null" },
                    { "cast(null, 'System.Web.OData.Query.Expressions.Address') eq null" },
                    { "cast('', 'System.Web.OData.Query.Expressions.Address') eq null" },
                    { "cast(SupplierAddress, 'System.Web.OData.Query.Expressions.Address') ne null" },
                };
            }
        }

        [Theory]
        [PropertyData("CastToQuotedComplexType")]
        public void CastToQuotedComplexType_Succeeds(string filter)
        {
            // Arrange
            var model = new Product
            {
                SupplierAddress = new Address { City = "Redmond", },
            };

            // Act & Assert
            var filters = VerifyQueryDeserialization<Product>(
                filter,
                expectedResult: NotTesting,
                expectedResultWithNullPropagation: NotTesting);
            RunFilters(filters, model, expectedValue: new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        public static TheoryDataSet<string, string> CastToUnquotedEntityType
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {
                        "cast(System.Web.OData.Query.Expressions.DerivedProduct)/DerivedProductName eq null",
                        "Cast or IsOf Function must have a type in its arguments."
                    },
                    {
                        "cast(null, System.Web.OData.Query.Expressions.DerivedCategory)/DerivedCategoryName eq null",
                        "Encountered invalid type cast. " +
                        "'System.Web.OData.Query.Expressions.DerivedCategory' is not assignable from 'System.Web.OData.Query.Expressions.Product'."
                    },
                    {
                        "cast(Category, System.Web.OData.Query.Expressions.DerivedCategory)/DerivedCategoryName eq null",
                        "Encountered invalid type cast. " +
                        "'System.Web.OData.Query.Expressions.DerivedCategory' is not assignable from 'System.Web.OData.Query.Expressions.Product'."
                    },
                };
            }
        }

        [Theory]
        [PropertyData("CastToUnquotedEntityType")]
        public void CastToUnquotedEntityType_ThrowsODataException(string filter, string expectedMessage)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataException>(() => Bind<Product>(filter), expectedMessage);
        }

        // Demonstrates a bug in FilterBinder.
        [Theory]
        [InlineData("cast('System.Web.OData.Query.Expressions.DerivedProduct')/DerivedProductName eq null", "DerivedProductName")]
        [InlineData("cast(Category,'System.Web.OData.Query.Expressions.DerivedCategory')/DerivedCategoryName eq null", "DerivedCategoryName")]
        [InlineData("cast(Category, 'System.Web.OData.Query.Expressions.DerivedCategory')/DerivedCategoryName eq null", "DerivedCategoryName")]
        public void CastToQuotedEntityType_ThrowsArgumentException(string filter, string propertyName)
        {
            // Arrange
            var expectedMessage = string.Format(
                "Instance property '{0}' is not defined for type '{1}'",
                propertyName,
                typeof(object).FullName);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => Bind<Product>(filter), expectedMessage);
        }

        [Theory]
        [InlineData("cast(null,'System.Web.OData.Query.Expressions.DerivedCategory')/DerivedCategoryName eq null")]
        [InlineData("cast(null, 'System.Web.OData.Query.Expressions.DerivedCategory')/DerivedCategoryName eq null")]
        public void CastNullToQuotedEntityType_ThrowsArgumentException(string filter)
        {
            // Arrange
            var expectedMessage =
                "Instance property 'DerivedCategoryName' is not defined for type 'System.Object'";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => Bind<Product>(filter), expectedMessage);
        }

        #endregion

        #region 'isof' in query option

        [Theory]
        [InlineData("isof(Edm.Int16)", "$it => IIF(($it Is System.Int16), True, False)")]
        [InlineData("isof('System.Web.OData.Query.Expressions.Product')", "$it => IIF(($it Is System.Web.OData.Query.Expressions.Product), True, False)")]
        [InlineData("isof(ProductName,Edm.String)", "$it => IIF(($it.ProductName Is System.String), True, False)")]
        [InlineData("isof(Category,'System.Web.OData.Query.Expressions.Category')", "$it => IIF(($it.Category Is System.Web.OData.Query.Expressions.Category), True, False)")]
        [InlineData("isof(Category,'System.Web.OData.Query.Expressions.DerivedCategory')", "$it => IIF(($it.Category Is System.Web.OData.Query.Expressions.DerivedCategory), True, False)")]
        [InlineData("isof(Ranking, 'Microsoft.TestCommon.Types.SimpleEnum')", "$it => IIF(($it.Ranking Is Microsoft.TestCommon.Types.SimpleEnum), True, False)")]
        public void IsofMethod_Succeeds(string filter, string expectedResult)
        {
            // Arrange & Act & Assert
            VerifyQueryDeserialization<Product>(
                filter,
                expectedResult,
                NotTesting);
        }

        [Theory]
        [InlineData("isof(null)")]
        [InlineData("isof(ProductName,null)")]
        public void Isof_WithNullTypeName_ThrowsArgumentNullException(string filter)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => Bind<Product>(filter),
                "Value cannot be null.\r\nParameter name: qualifiedName");
        }

        [Theory]
        [InlineData("isof(NoSuchProperty,Edm.Int32)",
            "Could not find a property named 'NoSuchProperty' on type 'System.Web.OData.Query.Expressions.DataTypes'.")]
        public void IsOfUndefinedSource_ThrowsODataException(string filter, string errorMessage)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataException>(() => Bind<DataTypes>(filter), errorMessage);
        }

        [Theory]
        [InlineData("isof(null,Edm.Binary)")]
        [InlineData("isof(null,Edm.Boolean)")]
        [InlineData("isof(null,Edm.Byte)")]
        [InlineData("isof(null,Edm.DateTimeOffset)")]
        [InlineData("isof(null,Edm.Decimal)")]
        [InlineData("isof(null,Edm.Double)")]
        [InlineData("isof(null,Edm.Duration)")]
        [InlineData("isof(null,Edm.Guid)")]
        [InlineData("isof(null,Edm.Int16)")]
        [InlineData("isof(null,Edm.Int32)")]
        [InlineData("isof(null,Edm.Int64)")]
        [InlineData("isof(null,Edm.SByte)")]
        [InlineData("isof(null,Edm.Single)")]
        [InlineData("isof(null,Edm.Stream)")]
        [InlineData("isof(null,Edm.String)")]
        [InlineData("isof(null,Microsoft.TestCommon.Types.SimpleEnum)")]
        [InlineData("isof(null,Microsoft.TestCommon.Types.FlagsEnum)")]

        [InlineData("isof(ByteArrayProp,Edm.Binary)")] // ByteArrayProp == null
        [InlineData("isof(IntProp,Microsoft.TestCommon.Types.SimpleEnum)")]
        [InlineData("isof(NullableShortProp,'Edm.Int16')")] // NullableShortProp == null

        [InlineData("isof('Edm.Binary')")]
        [InlineData("isof('Edm.Boolean')")]
        [InlineData("isof('Edm.Byte')")]
        [InlineData("isof('Edm.DateTimeOffset')")]
        [InlineData("isof('Edm.Decimal')")]
        [InlineData("isof('Edm.Double')")]
        [InlineData("isof('Edm.Duration')")]
        [InlineData("isof('Edm.Guid')")]
        [InlineData("isof('Edm.Int16')")]
        [InlineData("isof('Edm.Int32')")]
        [InlineData("isof('Edm.Int64')")]
        [InlineData("isof('Edm.SByte')")]
        [InlineData("isof('Edm.Single')")]
        [InlineData("isof('Edm.Stream')")]
        [InlineData("isof('Edm.String')")]
        [InlineData("isof('Microsoft.TestCommon.Types.SimpleEnum')")]
        [InlineData("isof('Microsoft.TestCommon.Types.FlagsEnum')")]

        [InlineData("isof(23,'Edm.Byte')")]
        [InlineData("isof(23,'Edm.Decimal')")]
        [InlineData("isof(23,'Edm.Double')")]
        [InlineData("isof(23,'Edm.Int16')")]
        [InlineData("isof(23,'Edm.Int64')")]
        [InlineData("isof(23,'Edm.SByte')")]
        [InlineData("isof(23,'Edm.Single')")]
        [InlineData("isof('hello','Edm.Stream')")]
        [InlineData("isof(0,'Microsoft.TestCommon.Types.FlagsEnum')")]
        [InlineData("isof(0,'Microsoft.TestCommon.Types.SimpleEnum')")]

        [InlineData("isof('2001-01-01T12:00:00.000+08:00','Edm.DateTimeOffset')")] // source is string
        [InlineData("isof('00000000-0000-0000-0000-000000000000','Edm.Guid')")] // source is string
        [InlineData("isof('23','Edm.Byte')")]
        [InlineData("isof('23','Edm.Int16')")]
        [InlineData("isof('23','Edm.Int32')")]
        [InlineData("isof('false','Edm.Boolean')")]
        [InlineData("isof('OData','Edm.Binary')")]
        [InlineData("isof('PT12H','Edm.Duration')")]
        [InlineData("isof(23,'Edm.String')")]
        [InlineData("isof('0','Microsoft.TestCommon.Types.FlagsEnum')")]
        [InlineData("isof('0','Microsoft.TestCommon.Types.SimpleEnum')")]
        public void IsOfPrimitiveType_Succeeds_WithFalse(string filter)
        {
            // Arrange
            var model = new DataTypes();

            // Act & Assert
            var filters = VerifyQueryDeserialization<DataTypes>(
                filter,
                expectedResult: NotTesting,
                expectedResultWithNullPropagation: NotTesting);
            RunFilters(filters, model, expectedValue: new { WithNullPropagation = false, WithoutNullPropagation = false });
        }

        public static TheoryDataSet<string, string> IsOfUndefinedTarget
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    { "isof(Edm.DateTime)", "Edm.DateTime" },
                    { "isof(Edm.Unknown)", "Edm.Unknown" },
                    { "isof(null,Edm.DateTime)", "Edm.DateTime" },
                    { "isof(null,Edm.Unknown)", "Edm.Unknown" },
                    { "isof('2001-01-01T12:00:00.000',Edm.DateTime)", "Edm.DateTime" },
                    { "isof('',Edm.Unknown)", "Edm.Unknown" },
                    { "isof(DateTimeProp,Edm.DateTime)", "Edm.DateTime" },
                    { "isof(IntProp,Edm.Unknown)", "Edm.Unknown" },
                };
            }
        }

        // Exception messages here and in IsOfQuotedUndefinedTarget_ThrowsODataException should be consistent.
        // Worse, this message is incorrect -- casts can be performed on most types but _not_ entity types and
        // isof can't be performed.
        [Theory]
        [PropertyData("IsOfUndefinedTarget")]
        public void IsOfUndefinedTarget_ThrowsODataException(string filter, string typeName)
        {
            // Arrange
            var expectedMessage = string.Format(
                "The child type '{0}' in a cast was not an entity type. Casts can only be performed on entity types.",
                typeName);

            // Act & Assert
            Assert.Throws<ODataException>(() => Bind<DataTypes>(filter), expectedMessage);
        }

        public static TheoryDataSet<string> IsOfQuotedUndefinedTarget
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "isof('Edm.DateTime')" },
                    { "isof('Edm.Unknown')" },
                    { "isof(null,'Edm.DateTime')" },
                    { "isof(null,'Edm.Unknown')" },
                    { "isof('2001-01-01T12:00:00.000','Edm.DateTime')" },
                    { "isof('','Edm.Unknown')" },
                    { "isof(DateTimeProp,'Edm.DateTime')" },
                    { "isof(IntProp,'Edm.Unknown')" },
                };
            }
        }

        [Theory]
        [PropertyData("IsOfQuotedUndefinedTarget")]
        public void IsOfQuotedUndefinedTarget_ThrowsODataException(string filter)
        {
            // Arrange
            var expectedMessage = "Cast or IsOf Function must have a type in its arguments.";

            // Act & Assert
            Assert.Throws<ODataException>(() => Bind<DataTypes>(filter), expectedMessage);
        }

        public static TheoryDataSet<string> IsOfUnquotedComplexType
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "isof(System.Web.OData.Query.Expressions.Address)" },
                    { "isof(null,System.Web.OData.Query.Expressions.Address)" },
                    { "isof(null, System.Web.OData.Query.Expressions.Address)" },
                    { "isof(SupplierAddress,System.Web.OData.Query.Expressions.Address)" },
                    { "isof(SupplierAddress, System.Web.OData.Query.Expressions.Address)" },
                };
            }
        }

        [Theory]
        [PropertyData("IsOfUnquotedComplexType")]
        public void IsOfUnquotedComplexType_ThrowsODataException(string filter)
        {
            // Arrange
            var expectedMessage =
                "Encountered invalid type cast. " +
                "'System.Web.OData.Query.Expressions.Address' is not assignable from 'System.Web.OData.Query.Expressions.Product'.";

            // Act & Assert
            Assert.Throws<ODataException>(() => Bind<Product>(filter), expectedMessage);
        }

        public static TheoryDataSet<string, string> IsOfUnquotedEntityType
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {
                        "isof(System.Web.OData.Query.Expressions.DerivedProduct)",
                        "Cast or IsOf Function must have a type in its arguments."
                    },
                    {
                        "isof(null,System.Web.OData.Query.Expressions.DerivedCategory)",
                        "Encountered invalid type cast. " +
                        "'System.Web.OData.Query.Expressions.DerivedCategory' is not assignable from 'System.Web.OData.Query.Expressions.Product'."
                    },
                    {
                        "isof(null, System.Web.OData.Query.Expressions.DerivedCategory)",
                        "Encountered invalid type cast. " +
                        "'System.Web.OData.Query.Expressions.DerivedCategory' is not assignable from 'System.Web.OData.Query.Expressions.Product'."
                    },
                    {
                        "isof(Category,System.Web.OData.Query.Expressions.DerivedCategory)",
                        "Encountered invalid type cast. " +
                        "'System.Web.OData.Query.Expressions.DerivedCategory' is not assignable from 'System.Web.OData.Query.Expressions.Product'."
                    },
                    {
                        "isof(Category, System.Web.OData.Query.Expressions.DerivedCategory)",
                        "Encountered invalid type cast. " +
                        "'System.Web.OData.Query.Expressions.DerivedCategory' is not assignable from 'System.Web.OData.Query.Expressions.Product'."
                    },
                };
            }
        }

        [Theory]
        [PropertyData("IsOfUnquotedEntityType")]
        public void IsOfUnquotedEntityType_ThrowsODataException(string filter, string expectedMessage)
        {
            // Arrange & Act & Assert
            Assert.Throws<ODataException>(() => Bind<Product>(filter), expectedMessage);
        }

        public static TheoryDataSet<string> IsOfQuotedNonPrimitiveType
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { "isof('System.Web.OData.Query.Expressions.DerivedProduct')" },
                    { "isof(SupplierAddress,'System.Web.OData.Query.Expressions.Address')" },
                    { "isof(SupplierAddress, 'System.Web.OData.Query.Expressions.Address')" },
                    { "isof(Category,'System.Web.OData.Query.Expressions.DerivedCategory')" },
                    { "isof(Category, 'System.Web.OData.Query.Expressions.DerivedCategory')" },
                };
            }
        }

        [Theory]
        [PropertyData("IsOfQuotedNonPrimitiveType")]
        public void IsOfQuotedNonPrimitiveType_Succeeds(string filter)
        {
            // Arrange
            var model = new DerivedProduct
            {
                SupplierAddress = new Address { City = "Redmond", },
                Category = new DerivedCategory { DerivedCategoryName = "DerivedCategory" }
            };

            // Act & Assert
           var filters = VerifyQueryDeserialization<Product>(
                filter,
                expectedResult: NotTesting,
                expectedResultWithNullPropagation: NotTesting);
            RunFilters<Product>(filters, model, expectedValue: new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        [Theory]
        [InlineData("isof(null,'System.Web.OData.Query.Expressions.Address')")]
        [InlineData("isof(null, 'System.Web.OData.Query.Expressions.Address')")]
        [InlineData("isof(null,'System.Web.OData.Query.Expressions.DerivedCategory')")]
        [InlineData("isof(null, 'System.Web.OData.Query.Expressions.DerivedCategory')")]
        public void IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalse(string filter)
        {
            // Arrange
            var model = new DerivedProduct
            {
                SupplierAddress = new Address { City = "Redmond", },
                Category = new DerivedCategory { DerivedCategoryName = "DerivedCategory" }
            };

            // Act & Assert
            var filters = VerifyQueryDeserialization<Product>(
                filter,
                expectedResult: NotTesting,
                expectedResultWithNullPropagation: NotTesting);
            RunFilters<Product>(filters, model, expectedValue: new { WithNullPropagation = false, WithoutNullPropagation = false });
        }

        #endregion

        #region parameter alias for filter query option

        [Theory]
        // Parameter alias value is not null.
        [InlineData("IntProp eq @p", "1", "$it => ($it.IntProp == 1)")]
        [InlineData("BoolProp eq @p", "true", "$it => ($it.BoolProp == True)")]
        [InlineData("LongProp eq @p", "-123", "$it => ($it.LongProp == Convert(-123))")]
        [InlineData("FloatProp eq @p", "1.23", "$it => ($it.FloatProp == 1.23)")]
        [InlineData("DoubleProp eq @p", "4.56", "$it => ($it.DoubleProp == Convert(4.56))")]
        [InlineData("StringProp eq @p", "'abc'", "$it => ($it.StringProp == \"abc\")")]
        [InlineData("DateTimeOffsetProp eq @p", "2001-01-01T12:00:00.000+08:00", "$it => ($it.DateTimeOffsetProp == 01/01/2001 12:00:00 +08:00)")]
        [InlineData("TimeSpanProp eq @p", "duration'P8DT23H59M59.9999S'", "$it => ($it.TimeSpanProp == 8.23:59:59.9999000)")]
        [InlineData("GuidProp eq @p", "00000000-0000-0000-0000-000000000000", "$it => ($it.GuidProp == 00000000-0000-0000-0000-000000000000)")]
        [InlineData("SimpleEnumProp eq @p", "Microsoft.TestCommon.Types.SimpleEnum'First'", "$it => (Convert($it.SimpleEnumProp) == 0)")]
        // Parameter alias value is null.
        [InlineData("NullableIntProp eq @p", "null", "$it => ($it.NullableIntProp == null)")]
        [InlineData("NullableBoolProp eq @p", "null", "$it => ($it.NullableBoolProp == null)")]
        [InlineData("NullableLongProp eq @p", "null", "$it => ($it.NullableLongProp == null)")]
        [InlineData("NullableSingleProp eq @p", "null", "$it => ($it.NullableSingleProp == null)")]
        [InlineData("NullableDoubleProp eq @p", "null", "$it => ($it.NullableDoubleProp == null)")]
        [InlineData("StringProp eq @p", "null", "$it => ($it.StringProp == null)")]
        [InlineData("NullableDateTimeOffsetProp eq @p", "null", "$it => ($it.NullableDateTimeOffsetProp == null)")]
        [InlineData("NullableTimeSpanProp eq @p", "null", "$it => ($it.NullableTimeSpanProp == null)")]
        [InlineData("NullableGuidProp eq @p", "null", "$it => ($it.NullableGuidProp == null)")]
        [InlineData("NullableSimpleEnumProp eq @p", "null", "$it => (Convert($it.NullableSimpleEnumProp) == null)")]
        // Parameter alias value is property.
        [InlineData("@p eq 1", "IntProp", "$it => ($it.IntProp == 1)")]
        [InlineData("@p eq true", "NullableBoolProp", "$it => ($it.NullableBoolProp == Convert(True))")]
        [InlineData("@p eq -123", "LongProp", "$it => ($it.LongProp == -123)")]
        [InlineData("@p eq 1.23", "FloatProp", "$it => ($it.FloatProp == 1.23)")]
        [InlineData("@p eq 4.56", "NullableDoubleProp", "$it => ($it.NullableDoubleProp == Convert(4.56))")]
        [InlineData("@p eq 'abc'", "StringProp", "$it => ($it.StringProp == \"abc\")")]
        [InlineData("@p eq 2001-01-01T12:00:00.000+08:00", "DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp == 01/01/2001 12:00:00 +08:00)")]
        [InlineData("@p eq duration'P8DT23H59M59.9999S'", "TimeSpanProp", "$it => ($it.TimeSpanProp == 8.23:59:59.9999000)")]
        [InlineData("@p eq 00000000-0000-0000-0000-000000000000", "GuidProp", "$it => ($it.GuidProp == 00000000-0000-0000-0000-000000000000)")]
        [InlineData("@p eq Microsoft.TestCommon.Types.SimpleEnum'First'", "SimpleEnumProp", "$it => (Convert($it.SimpleEnumProp) == 0)")]
        // Parameter alias value has built-in functions.
        [InlineData("@p eq 'abc'", "substring(StringProp,5)", "$it => ($it.StringProp.Substring(5) == \"abc\")")]
        [InlineData("2 eq @p", "IntProp add 1", "$it => (2 == ($it.IntProp + 1))")]
        [InlineData("EntityProp/AlternateAddresses/all(a: a/City ne @p)", "'abc'", "$it => $it.EntityProp.AlternateAddresses.All(a => (a.City != \"abc\"))")]
        public void ParameterAlias_Succeeds(string filter, string parameterAliasValue, string expectedResult)
        {
            // Arrange
            IEdmModel model = GetModel<DataTypes>();
            IEdmType targetEdmType = model.FindType("System.Web.OData.Query.Expressions.DataTypes");
            IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("System.Web.OData.Query.Expressions.Products");
            IDictionary<string, string> queryOptions = new Dictionary<string, string> { { "$filter", filter } };
            queryOptions.Add("@p", parameterAliasValue);
            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, targetEdmType, targetNavigationSource, queryOptions);
            ODataQueryContext context = new ODataQueryContext(model, typeof(DataTypes));
            context.RequestContainer = new MockContainer();
            FilterClause filterClause = new FilterQueryOption(filter, context, parser).FilterClause;

            // Act
            Expression actualExpression = FilterBinder.Bind(
                filterClause,
                typeof(DataTypes),
                model,
                CreateFakeAssembliesResolver(),
                new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False });

            // Assert
            VerifyExpression(actualExpression, expectedResult);
        }

        [Theory]
        [InlineData("NullableIntProp eq @p", "$it => ($it.NullableIntProp == null)")]
        [InlineData("NullableBoolProp eq @p", "$it => ($it.NullableBoolProp == null)")]
        [InlineData("NullableDoubleProp eq @p", "$it => ($it.NullableDoubleProp == null)")]
        [InlineData("StringProp eq @p", "$it => ($it.StringProp == null)")]
        [InlineData("NullableDateTimeOffsetProp eq @p", "$it => ($it.NullableDateTimeOffsetProp == null)")]
        [InlineData("NullableSimpleEnumProp eq @p", "$it => (Convert($it.NullableSimpleEnumProp) == null)")]
        [InlineData("EntityProp/AlternateAddresses/any(a: a/City eq @p)", "$it => $it.EntityProp.AlternateAddresses.Any(a => (a.City == null))")]
        public void ParameterAlias_AssumedToBeNull_ValueNotFound(string filter, string expectedResult)
        {
            // Arrange
            IEdmModel model = GetModel<DataTypes>();
            IEdmType targetEdmType = model.FindType("System.Web.OData.Query.Expressions.DataTypes");
            IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("System.Web.OData.Query.Expressions.Products");
            IDictionary<string, string> queryOptions = new Dictionary<string, string> { { "$filter", filter } };
            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, targetEdmType, targetNavigationSource, queryOptions);
            ODataQueryContext context = new ODataQueryContext(model, typeof(DataTypes));
            context.RequestContainer = new MockContainer();
            FilterClause filterClause = new FilterQueryOption(filter, context, parser).FilterClause;

            // Act
            Expression actualExpression = FilterBinder.Bind(
                filterClause,
                typeof(DataTypes),
                model,
                CreateFakeAssembliesResolver(),
                new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False });

            // Assert
            VerifyExpression(actualExpression, expectedResult);
        }

        [Fact]
        public void ParameterAlias_NestedCase_Succeeds()
        {
            // Arrange
            IEdmModel model = GetModel<DataTypes>();
            IEdmType targetEdmType = model.FindType("System.Web.OData.Query.Expressions.DataTypes");
            IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("System.Web.OData.Query.Expressions.Products");

            ODataQueryOptionParser parser = new ODataQueryOptionParser(
                model,
                targetEdmType,
                targetNavigationSource,
                new Dictionary<string, string> { { "$filter", "IntProp eq @p1" }, { "@p1", "@p2" }, { "@p2", "123" } });

            ODataQueryContext context = new ODataQueryContext(model, typeof(DataTypes));
            context.RequestContainer = new MockContainer();
            FilterClause filterClause = new FilterQueryOption("IntProp eq @p1", context, parser).FilterClause;

            // Act
            Expression actualExpression = FilterBinder.Bind(
                filterClause,
                typeof(DataTypes),
                model,
                CreateFakeAssembliesResolver(),
                new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False });

            // Assert
            VerifyExpression(actualExpression, "$it => ($it.IntProp == 123)");
        }

        [Fact]
        public void ParameterAlias_Throws_NotStartWithAt()
        {
            // Arrange
            IEdmModel model = GetModel<DataTypes>();
            IEdmType targetEdmType = model.FindType("System.Web.OData.Query.Expressions.DataTypes");
            IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("System.Web.OData.Query.Expressions.Products");

            ODataQueryOptionParser parser = new ODataQueryOptionParser(
                model,
                targetEdmType,
                targetNavigationSource,
                new Dictionary<string, string> { { "$filter", "IntProp eq #p" }, { "#p", "123" } });

            // Act & Assert
            Assert.Throws<ODataException>(
                () => parser.ParseFilter(),
                "Syntax error: character '#' is not valid at position 11 in 'IntProp eq #p'.");
        }

        #endregion

        [Theory]
        [InlineData("UShortProp eq 12", "$it => (Convert($it.UShortProp) == 12)")]
        [InlineData("ULongProp eq 12L", "$it => (Convert($it.ULongProp) == 12)")]
        [InlineData("UIntProp eq 12", "$it => (Convert($it.UIntProp) == 12)")]
        [InlineData("CharProp eq 'a'", "$it => (Convert($it.CharProp.ToString()) == \"a\")")]
        [InlineData("CharArrayProp eq 'a'", "$it => (new String($it.CharArrayProp) == \"a\")")]
        [InlineData("BinaryProp eq binary'TWFu'", "$it => ($it.BinaryProp.ToArray() == System.Byte[])")]
        [InlineData("XElementProp eq '<name />'", "$it => ($it.XElementProp.ToString() == \"<name />\")")]
        public void NonstandardEdmPrimtives(string filter, string expression)
        {
            var filters = VerifyQueryDeserialization<DataTypes>(filter, expression, NotTesting);

            RunFilters(filters,
                new DataTypes
                {
                    UShortProp = 12,
                    ULongProp = 12,
                    UIntProp = 12,
                    CharProp = 'a',
                    CharArrayProp = new[] { 'a' },
                    BinaryProp = new Binary(new byte[] { 77, 97, 110 }),
                    XElementProp = new XElement("name")
                },
                new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        [Theory]
        [InlineData("BinaryProp eq binary'I6v/'", "$it => ($it.BinaryProp.ToArray() == System.Byte[])", true, true)]
        [InlineData("BinaryProp ne binary'I6v/'", "$it => ($it.BinaryProp.ToArray() != System.Byte[])", false, false)]
        [InlineData("ByteArrayProp eq binary'I6v/'", "$it => ($it.ByteArrayProp == System.Byte[])", true, true)]
        [InlineData("ByteArrayProp ne binary'I6v/'", "$it => ($it.ByteArrayProp != System.Byte[])", false, false)]
        [InlineData("binary'I6v/' eq binary'I6v/'", "$it => (System.Byte[] == System.Byte[])", true, true)]
        [InlineData("binary'I6v/' ne binary'I6v/'", "$it => (System.Byte[] != System.Byte[])", false, false)]
        [InlineData("ByteArrayPropWithNullValue ne binary'I6v/'", "$it => ($it.ByteArrayPropWithNullValue != System.Byte[])", true, true)]
        [InlineData("ByteArrayPropWithNullValue ne ByteArrayPropWithNullValue", "$it => ($it.ByteArrayPropWithNullValue != $it.ByteArrayPropWithNullValue)", false, false)]
        [InlineData("ByteArrayPropWithNullValue ne null", "$it => ($it.ByteArrayPropWithNullValue != null)", false, false)]
        [InlineData("ByteArrayPropWithNullValue eq null", "$it => ($it.ByteArrayPropWithNullValue == null)", true, true)]
        [InlineData("null ne ByteArrayPropWithNullValue", "$it => (null != $it.ByteArrayPropWithNullValue)", false, false)]
        [InlineData("null eq ByteArrayPropWithNullValue", "$it => (null == $it.ByteArrayPropWithNullValue)", true, true)]
        public void ByteArrayComparisons(string filter, string expression, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization<DataTypes>(filter, expression, NotTesting);
            RunFilters(filters,
                new DataTypes
                {
                    BinaryProp = new Binary(new byte[] { 35, 171, 255 }),
                    ByteArrayProp = new byte[] { 35, 171, 255 }
                },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData("binary'AP8Q' ge binary'AP8Q'", "GreaterThanOrEqual")]
        [InlineData("binary'AP8Q' le binary'AP8Q'", "LessThanOrEqual")]
        [InlineData("binary'AP8Q' lt binary'AP8Q'", "LessThan")]
        [InlineData("binary'AP8Q' gt binary'AP8Q'", "GreaterThan")]
        [InlineData("binary'AP8Q' add binary'AP8Q'", "Add")]
        [InlineData("binary'AP8Q' sub binary'AP8Q'", "Subtract")]
        [InlineData("binary'AP8Q' mul binary'AP8Q'", "Multiply")]
        [InlineData("binary'AP8Q' div binary'AP8Q'", "Divide")]
        public void DisAllowed_ByteArrayComparisons(string filter, string op)
        {
            Assert.Throws<ODataException>(
                () => Bind<DataTypes>(filter),
                String.Format(CultureInfo.InvariantCulture, "A binary operator with incompatible types was detected. Found operand types 'Edm.Binary' and 'Edm.Binary' for operator kind '{0}'.", op));
        }

        [Theory]
        [InlineData("NullableUShortProp eq 12", "$it => (Convert($it.NullableUShortProp.Value) == Convert(12))")]
        [InlineData("NullableULongProp eq 12L", "$it => (Convert($it.NullableULongProp.Value) == Convert(12))")]
        [InlineData("NullableUIntProp eq 12", "$it => (Convert($it.NullableUIntProp.Value) == Convert(12))")]
        [InlineData("NullableCharProp eq 'a'", "$it => ($it.NullableCharProp.Value.ToString() == \"a\")")]
        public void Nullable_NonstandardEdmPrimitives(string filter, string expression)
        {
            var filters = VerifyQueryDeserialization<DataTypes>(filter, expression, NotTesting);

            RunFilters(filters,
                new DataTypes(),
                new { WithNullPropagation = false, WithoutNullPropagation = typeof(InvalidOperationException) });
        }

        [Fact]
        public void MultipleConstants_Are_Parameterized()
        {
            VerifyQueryDeserialization("ProductName eq '1' or ProductName eq '2' or ProductName eq '3' or ProductName eq '4'",
                "$it => (((($it.ProductName == \"1\") OrElse ($it.ProductName == \"2\")) OrElse ($it.ProductName == \"3\")) OrElse ($it.ProductName == \"4\"))",
                NotTesting);
        }

        [Fact]
        public void Constants_Are_Not_Parameterized_IfDisabled()
        {
            var filters = VerifyQueryDeserialization("ProductName eq '1'", settingsCustomizer: (settings) =>
                {
                    settings.EnableConstantParameterization = false;
                });

            Assert.Equal("$it => ($it.ProductName == \"1\")", (filters.WithoutNullPropagation as Expression).ToString());
        }

        [Fact]
        public void FilterByDynamicProperty()
        {
            VerifyQueryDeserialization<DynamicProduct>("Token eq '1'",
                "$it => (Convert(IIF($it.ProductProperties.ContainsKey(Token), $it.ProductPropertiesToken, null)) == \"1\")",
                "$it => (Convert(IIF((($it.ProductProperties != null) AndAlso $it.ProductProperties.ContainsKey(Token)), $it.ProductPropertiesToken, null)) == \"1\")");
        }

        #region Negative Tests

        [Fact]
        public void TypeMismatchInComparison()
        {
            Assert.Throws<ODataException>(() => Bind("length(123) eq 12"));
        }

        #endregion

        private Expression<Func<Product, bool>> Bind(string filter, ODataQuerySettings querySettings = null)
        {
            return Bind<Product>(filter, querySettings);
        }

        private Expression<Func<T, bool>> Bind<T>(string filter, ODataQuerySettings querySettings = null) where T : class
        {
            IEdmModel model = GetModel<T>();
            FilterClause filterNode = CreateFilterNode(filter, model, typeof(T));

            if (querySettings == null)
            {
                querySettings = CreateSettings();
            }

            return Bind<T>(filterNode, model, CreateFakeAssembliesResolver(), querySettings);
        }

        private static Expression<Func<TEntityType, bool>> Bind<TEntityType>(FilterClause filterNode, IEdmModel model, IAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
        {
            return FilterBinder.Bind<TEntityType>(filterNode, model, assembliesResolver, querySettings);
        }

        private IAssembliesResolver CreateFakeAssembliesResolver()
        {
            return new NoAssembliesResolver();
        }

        private FilterClause CreateFilterNode(string filter, IEdmModel model, Type entityType)
        {
            IEdmEntityType productType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == entityType.Name);
            Assert.NotNull(productType); // Guard

            IEdmEntitySet products = model.EntityContainer.FindEntitySet("Products");
            Assert.NotNull(products); // Guard

            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, productType, products,
                new Dictionary<string, string> { { "$filter", filter } });

            return parser.ParseFilter();
        }

        private static ODataQuerySettings CreateSettings()
        {
            return new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.False // A value other than Default is required for calls to Bind.
            };
        }

        private void RunFilters<T>(dynamic filters, T product, dynamic expectedValue)
        {
            var filterWithNullPropagation = filters.WithNullPropagation as Expression<Func<T, bool>>;
            if (expectedValue.WithNullPropagation is Type)
            {
                Assert.Throws(expectedValue.WithNullPropagation as Type, () => RunFilter(filterWithNullPropagation, product));
            }
            else
            {
                Assert.Equal(expectedValue.WithNullPropagation, RunFilter(filterWithNullPropagation, product));
            }

            var filterWithoutNullPropagation = filters.WithoutNullPropagation as Expression<Func<T, bool>>;
            if (expectedValue.WithoutNullPropagation is Type)
            {
                Assert.Throws(expectedValue.WithoutNullPropagation as Type, () => RunFilter(filterWithoutNullPropagation, product));
            }
            else
            {
                Assert.Equal(expectedValue.WithoutNullPropagation, RunFilter(filterWithoutNullPropagation, product));
            }
        }

        private bool RunFilter<T>(Expression<Func<T, bool>> filter, T instance)
        {
            return filter.Compile().Invoke(instance);
        }

        private dynamic VerifyQueryDeserialization(string filter, string expectedResult = null, string expectedResultWithNullPropagation = null, Action<ODataQuerySettings> settingsCustomizer = null)
        {
            return VerifyQueryDeserialization<Product>(filter, expectedResult, expectedResultWithNullPropagation, settingsCustomizer);
        }

        private dynamic VerifyQueryDeserialization<T>(string filter, string expectedResult = null, string expectedResultWithNullPropagation = null, Action<ODataQuerySettings> settingsCustomizer = null) where T : class
        {
            IEdmModel model = GetModel<T>();
            FilterClause filterNode = CreateFilterNode(filter, model, typeof(T));
            IAssembliesResolver assembliesResolver = CreateFakeAssembliesResolver();

            Func<ODataQuerySettings, ODataQuerySettings> customizeSettings = (settings) =>
            {
                if (settingsCustomizer != null)
                {
                    settingsCustomizer.Invoke(settings);
                }

                return settings;
            };

            var filterExpr = Bind<T>(
                filterNode,
                model,
                assembliesResolver,
                customizeSettings(new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }));

            if (!String.IsNullOrEmpty(expectedResult))
            {
                VerifyExpression(filterExpr, expectedResult);
            }

            expectedResultWithNullPropagation = expectedResultWithNullPropagation ?? expectedResult;

            var filterExprWithNullPropagation = Bind<T>(
                filterNode,
                model,
                assembliesResolver,
                customizeSettings(new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True }));

            if (!String.IsNullOrEmpty(expectedResultWithNullPropagation))
            {
                VerifyExpression(filterExprWithNullPropagation, expectedResultWithNullPropagation ?? expectedResult);
            }

            return new
            {
                WithNullPropagation = filterExprWithNullPropagation,
                WithoutNullPropagation = filterExpr
            };
        }

        private void VerifyExpression(Expression filter, string expectedExpression)
        {
            // strip off the beginning part of the expression to get to the first
            // actual query operator
            string resultExpression = ExpressionStringBuilder.ToString(filter);
            Assert.True(resultExpression == expectedExpression,
                String.Format("Expected expression '{0}' but the deserializer produced '{1}'", expectedExpression, resultExpression));
        }

        private IEdmModel GetModel<T>() where T : class
        {
            Type key = typeof(T);
            IEdmModel value;

            if (!_modelCache.TryGetValue(key, out value))
            {
                ODataModelBuilder model = new ODataConventionModelBuilder();
                model.EntitySet<T>("Products");
                if (key == typeof(Product))
                {
                    model.EntityType<DerivedProduct>().DerivesFrom<Product>();
                    model.EntityType<DerivedCategory>().DerivesFrom<Category>();
                }

                value = _modelCache[key] = model.GetEdmModel();
            }
            return value;
        }

        private T? ToNullable<T>(object value) where T : struct
        {
            return value == null ? null : (T?)Convert.ChangeType(value, typeof(T));
        }

        // Used by Custom Method binder tests - by reflection
        private string PadRightInstance(string str, int number)
        {
            return str.PadRight(number);
        }

        // Used by Custom Method binder tests - by reflection
        private static string PadRightStatic(string str, int number)
        {
            return str.PadRight(number);
        }

        private class NoAssembliesResolver : IAssembliesResolver
        {
            public ICollection<Assembly> GetAssemblies()
            {
                return new Assembly[0];
            }
        }

    }

    // Used by Custom Method binder tests - by reflection
    public static class StringExtender
    {
        public static string PadRightExStatic(this string str, int width)
        {
            return str.PadRight(width);
        }
    }

}
