// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Query.Expressions
{
    public class FilterBinderTests
    {
        private const string NotTesting = "";

        private static readonly Uri _serviceBaseUri = new Uri("http://server/service/");

        #region Inequalities
        [Theory]
        [InlineData(null, true, true)]
        [InlineData("", false, false)]
        [InlineData("Doritos", false, false)]
        public void EqualityOperatorWithNull(string productName, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "$filter=ProductName eq null",
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
                "$filter=ProductName eq 'Doritos'",
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
                "$filter=ProductName ne 'Doritos'",
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
                "$filter=UnitPrice gt 5.00m",
                Error.Format("$it => ($it.UnitPrice > Convert({0:0.00}))", 5.0),
                Error.Format("$it => IIF((($it.UnitPrice > Convert({0:0.00})) == null), False, Convert(($it.UnitPrice > Convert({0:0.00}))))", 5.0));

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
                "$filter=UnitPrice ge 5.00m",
                Error.Format("$it => ($it.UnitPrice >= Convert({0:0.00}))", 5.0),
                Error.Format("$it => IIF((($it.UnitPrice >= Convert({0:0.00})) == null), False, Convert(($it.UnitPrice >= Convert({0:0.00}))))", 5.0));

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
                "$filter=UnitPrice lt 5.00m",
                Error.Format("$it => ($it.UnitPrice < Convert({0:0.00}))", 5.0),
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
                "$filter=UnitPrice le 5.00m",
                Error.Format("$it => ($it.UnitPrice <= Convert({0:0.00}))", 5.0),
                NotTesting);

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void NegativeNumbers()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice le -5.00m",
                Error.Format("$it => ($it.UnitPrice <= Convert({0:0.00}))", -5.0),
                NotTesting);
        }

        [Theory(Skip = "Enable once ODataUriParser handles DateTimeOffsets")]
        [InlineData("DateTimeOffsetProp eq DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp == $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp ne DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp != $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp ge DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp >= $it.DateTimeOffsetProp)")]
        [InlineData("DateTimeOffsetProp le DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp <= $it.DateTimeOffsetProp)")]
        public void DateTimeOffsetInEqualities(string clause, string expectedExpression)
        {
            VerifyQueryDeserialization<DataTypes>(
                "$filter=" + clause,
                expectedExpression);
        }

        [Theory]
        [InlineData("DateTimeProp eq DateTimeProp", "$it => ($it.DateTimeProp == $it.DateTimeProp)")]
        [InlineData("DateTimeProp ne DateTimeProp", "$it => ($it.DateTimeProp != $it.DateTimeProp)")]
        [InlineData("DateTimeProp ge DateTimeProp", "$it => ($it.DateTimeProp >= $it.DateTimeProp)")]
        [InlineData("DateTimeProp le DateTimeProp", "$it => ($it.DateTimeProp <= $it.DateTimeProp)")]
        public void DateInEqualities(string clause, string expectedExpression)
        {
            VerifyQueryDeserialization<DataTypes>(
                "$filter=" + clause,
                expectedExpression);
        }

        #endregion

        #region Logical Operators

        [Fact]
        public void BooleanOperatorNullableTypes()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice eq 5.00m or CategoryID eq 0",
                Error.Format("$it => (($it.UnitPrice == Convert(5.00)) OrElse ($it.CategoryID == 0))", 5.0, 0),
                NotTesting);
        }

        [Theory]
        [InlineData(null, null, false, false)]
        [InlineData(5.0, 0, true, true)]
        [InlineData(null, 1, false, false)]
        public void OrOperator(object unitPrice, object unitsInStock, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "$filter=UnitPrice eq 5.00m or UnitsInStock eq 0",
                Error.Format("$it => (($it.UnitPrice == Convert({0:0.00})) OrElse (Convert($it.UnitsInStock) == Convert({1})))", 5.0, 0),
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
                "$filter=UnitPrice eq 5.00m and UnitsInStock eq 10.00m",
                Error.Format("$it => (($it.UnitPrice == Convert({0:0.00})) AndAlso (Convert($it.UnitsInStock) == Convert({1:0.00})))", 5.0, 10.0),
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
                "$filter=not (UnitPrice eq 5.00m)",
                Error.Format("$it => Not(($it.UnitPrice == Convert({0:0.00})))", 5.0),
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
                "$filter=not Discontinued",
                "$it => Convert(Not($it.Discontinued))",
                "$it => IIF((Not($it.Discontinued) == null), False, Convert(Not($it.Discontinued)))");

            RunFilters(filters,
                new Product { Discontinued = ToNullable<bool>(discontinued) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void NestedNegation()
        {
            VerifyQueryDeserialization(
                "$filter=not (not(not    (Discontinued)))",
                "$it => Convert(Not(Not(Not($it.Discontinued))))",
                "$it => IIF((Not(Not(Not($it.Discontinued))) == null), False, Convert(Not(Not(Not($it.Discontinued)))))");
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
                "$filter=UnitPrice sub 1.00m lt 5.00m",
                Error.Format("$it => (($it.UnitPrice - Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                Error.Format("$it => IIF(((($it.UnitPrice - Convert(1.00)) < Convert(5.00)) == null), False, Convert((($it.UnitPrice - Convert({0:0.00})) < Convert({1:0.00}))))", 1.0, 5.0));

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void Addition()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice add 1.00m lt 5.00m",
                Error.Format("$it => (($it.UnitPrice + Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                NotTesting);
        }

        [Fact]
        public void Multiplication()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice mul 1.00m lt 5.00m",
                Error.Format("$it => (($it.UnitPrice * Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                NotTesting);
        }

        [Fact]
        public void Division()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice div 1.00m lt 5.00m",
                Error.Format("$it => (($it.UnitPrice / Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
                NotTesting);
        }

        [Fact]
        public void Modulo()
        {
            VerifyQueryDeserialization(
                "$filter=UnitPrice mod 1.00m lt 5.00m",
                Error.Format("$it => (($it.UnitPrice % Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
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
            var filters = VerifyQueryDeserialization("$filter=" + filter);

            RunFilters(filters,
                new Product { UnitsInStock = ToNullable<short>(unitsInStock), UnitsOnOrder = ToNullable<short>(unitsOnOrder) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData("UnitsInStock eq null", null, true, true)] // NULL == constant NULL is true when null propagation is enabled
        [InlineData("UnitsInStock ne null", null, false, false)]  // NULL != constant NULL is false when null propagation is enabled
        public void NullHandling_LiteralNull(string filter, object unitsInStock, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization("$filter=" + filter);

            RunFilters(filters,
                new Product { UnitsInStock = ToNullable<short>(unitsInStock) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }
        #endregion

        [Theory]
        [InlineData(null, null, true, true)]
        [InlineData("not doritos", 0, true, true)]
        [InlineData("Doritos", 1, false, false)]
        public void Grouping(string productName, object unitsInStock, bool withNullPropagation, bool withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "$filter=((ProductName ne 'Doritos') or (UnitPrice lt 5.00m))",
                Error.Format("$it => (($it.ProductName != \"Doritos\") OrElse ($it.UnitPrice < Convert({0:0.00})))", 5.0),
                NotTesting);

            RunFilters(filters,
                new Product { ProductName = productName, UnitsInStock = ToNullable<short>(unitsInStock) },
                new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Fact]
        public void MemberExpressions()
        {
            var filters = VerifyQueryDeserialization(
                "$filter=Category/CategoryName eq 'Snacks'",
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
                "$filter=Category/Product/Category/CategoryName eq 'Snacks'",
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
                "$filter=SupplierAddress/City eq 'Redmond'",
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
               "$filter=Category/EnumerableProducts/any(P: P/ProductName eq 'Snacks')",
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
               "$filter=Category/QueryableProducts/any(P: P/ProductName eq 'Snacks')",
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
               "$filter=Category/EnumerableProducts/any(P: P/ProductName eq 'Snacks')",
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
               "$filter=Category/EnumerableProducts/all(P: P/ProductName eq 'Snacks')",
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
        public void AnyOnNavigationEnumerableCollections_EmptyFilter()
        {
            VerifyQueryDeserialization(
               "$filter=Category/EnumerableProducts/any()",
               "$it => $it.Category.EnumerableProducts.Any()",
               NotTesting);
        }

        [Fact]
        public void AnyOnNavigationQueryableCollections_EmptyFilter()
        {
            VerifyQueryDeserialization(
               "$filter=Category/QueryableProducts/any()",
               "$it => $it.Category.QueryableProducts.Any()",
               NotTesting);
        }

        [Fact]
        public void AllOnNavigationEnumerableCollections()
        {
            VerifyQueryDeserialization(
               "$filter=Category/EnumerableProducts/all(P: P/ProductName eq 'Snacks')",
               "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))",
               NotTesting);
        }

        [Fact]
        public void AllOnNavigationQueryableCollections()
        {
            VerifyQueryDeserialization(
               "$filter=Category/QueryableProducts/all(P: P/ProductName eq 'Snacks')",
               "$it => $it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\"))",
               NotTesting);
        }

        [Fact]
        public void RecursiveAllAny()
        {
            var filter = VerifyQueryDeserialization(
               "$filter=Category/QueryableProducts/all(P: P/Category/EnumerableProducts/any(PP: PP/ProductName eq 'Snacks'))",
               "$it => $it.Category.QueryableProducts.All(P => P.Category.EnumerableProducts.Any(PP => (PP.ProductName == \"Snacks\")))",
               NotTesting);
        }

        #endregion

        #region String Functions
        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("Abcd", true, true)]
        [InlineData("Abd", false, false)]
        public void StringSubstringOf(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            // In OData, the order of parameters is actually reversed in the resulting
            // string.Contains expression

            var filters = VerifyQueryDeserialization(
                "$filter=substringof('Abc', ProductName)",
                "$it => $it.ProductName.Contains(\"Abc\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });

            filters = VerifyQueryDeserialization(
                "$filter=substringof(ProductName, 'Abc')",
                "$it => \"Abc\".Contains($it.ProductName)",
                NotTesting);
        }

        [Theory]
        [InlineData(null, false, typeof(NullReferenceException))]
        [InlineData("Abcd", true, true)]
        [InlineData("Abd", false, false)]
        public void StringStartsWith(string productName, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "$filter=startswith(ProductName, 'Abc')",
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
                "$filter=endswith(ProductName, 'Abc')",
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
                "$filter=length(ProductName) gt 0",
                "$it => ($it.ProductName.Length > 0)",
                "$it => IIF(((IIF(($it.ProductName == null), null, Convert($it.ProductName.Length)) > Convert(0)) == null), False, Convert((IIF(($it.ProductName == null), null, Convert($it.ProductName.Length)) > Convert(0))))");

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
                "$filter=indexof(ProductName, 'Abc') eq 5",
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
                "$filter=substring(ProductName, 3) eq 'uctName'",
                "$it => ($it.ProductName.Substring(3) == \"uctName\")",
                NotTesting);

            RunFilters(filters,
              new Product { ProductName = productName },
              new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });

            VerifyQueryDeserialization(
                "$filter=substring(ProductName, 3, 4) eq 'uctN'",
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
                "$filter=tolower(ProductName) eq 'tasty treats'",
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
                "$filter=toupper(ProductName) eq 'TASTY TREATS'",
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
                "$filter=trim(ProductName) eq 'Tasty Treats'",
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
                "$filter=concat('Food', 'Bar') eq 'FoodBar'",
                "$it => (Concat(\"Food\", \"Bar\") == \"FoodBar\")");

            RunFilters(filters,
              new Product { },
              new { WithNullPropagation = true, WithoutNullPropagation = true });
        }
        #endregion

        #region Date Functions
        [Fact]
        public void DateDay()
        {
            var filters = VerifyQueryDeserialization(
                "$filter=day(DiscontinuedDate) eq 8",
                "$it => ($it.DiscontinuedDate.Value.Day == 8)",
                NotTesting);

            RunFilters(filters,
               new Product { },
               new { WithNullPropagation = false, WithoutNullPropagation = typeof(InvalidOperationException) });

            RunFilters(filters,
               new Product { DiscontinuedDate = new DateTime(2000, 10, 8) },
               new { WithNullPropagation = true, WithoutNullPropagation = true });
        }

        public void DateDayNonNullable()
        {
            VerifyQueryDeserialization(
                "$filter=day(NonNullableDiscontinuedDate) eq 8",
                "$it => ($it.NonNullableDiscontinuedDate.Day == 8)");
        }

        [Fact]
        public void DateMonth()
        {
            VerifyQueryDeserialization(
                "$filter=month(DiscontinuedDate) eq 8",
                "$it => ($it.DiscontinuedDate.Value.Month == 8)",
                NotTesting);
        }

        [Fact]
        public void DateYear()
        {
            VerifyQueryDeserialization(
                "$filter=year(DiscontinuedDate) eq 1974",
                "$it => ($it.DiscontinuedDate.Value.Year == 1974)",
                NotTesting);
        }

        [Fact]
        public void DateHour()
        {
            VerifyQueryDeserialization("$filter=hour(DiscontinuedDate) eq 8",
                "$it => ($it.DiscontinuedDate.Value.Hour == 8)",
                NotTesting);
        }

        [Fact]
        public void DateMinute()
        {
            VerifyQueryDeserialization(
                "$filter=minute(DiscontinuedDate) eq 12",
                "$it => ($it.DiscontinuedDate.Value.Minute == 12)",
                NotTesting);
        }

        [Fact]
        public void DateSecond()
        {
            VerifyQueryDeserialization(
                "$filter=second(DiscontinuedDate) eq 33",
                "$it => ($it.DiscontinuedDate.Value.Second == 33)",
                NotTesting);
        }
        #endregion

        #region Math Functions
        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(5.9, true, true)]
        [InlineData(5.4, false, false)]
        public void MathRound(object unitPrice, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "$filter=round(UnitPrice) gt 5.00m",
                Error.Format("$it => (Round($it.UnitPrice.Value) > {0:0.00})", 5.0),
                NotTesting);

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(5.4, true, true)]
        [InlineData(4.4, false, false)]
        public void MathFloor(object unitPrice, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "$filter=floor(UnitPrice) eq 5m",
                "$it => (Floor($it.UnitPrice.Value) == 5)",
                NotTesting);

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }

        [Theory]
        [InlineData(null, false, typeof(InvalidOperationException))]
        [InlineData(4.1, true, true)]
        [InlineData(5.9, false, false)]
        public void MathCeiling(object unitPrice, bool withNullPropagation, object withoutNullPropagation)
        {
            var filters = VerifyQueryDeserialization(
                "$filter=ceiling(UnitPrice) eq 5m",
                "$it => (Ceiling($it.UnitPrice.Value) == 5)",
                NotTesting);

            RunFilters(filters,
               new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
               new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
        }
        #endregion

        #region Data Types
        [Fact]
        public void GuidExpression()
        {
            VerifyQueryDeserialization<DataTypes>(
                "$filter=GuidProp eq guid'0EFDAECF-A9F0-42F3-A384-1295917AF95E'",
                "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");

            // verify case insensitivity
            VerifyQueryDeserialization<DataTypes>(
                "$filter=GuidProp eq GuiD'0EFDAECF-A9F0-42F3-A384-1295917AF95E'",
                "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");
        }

        [Theory]
        [InlineData("DateTimeProp eq datetime'2000-12-12T12:00:00'", "$it => ($it.DateTimeProp == {0})")]
        [InlineData("DateTimeProp lt datetime'2000-12-12T12:00:00'", "$it => ($it.DateTimeProp < {0})")]
        // TODO: [InlineData("DateTimeProp ge datetime'2000-12-12T12:00'", "$it => ($it.DateTimeProp >= {0})")] (uriparser fails on optional seconds)
        public void DateTimeExpression(string clause, string expectedExpression)
        {
            var dateTime = new DateTime(2000, 12, 12, 12, 0, 0);
            VerifyQueryDeserialization<DataTypes>(
                "$filter=" + clause,
                Error.Format(expectedExpression, dateTime));
        }

        [Theory(Skip = "No DateTimeOffset parsing in ODataUriParser")]
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
            VerifyQueryDeserialization<DataTypes>(
                "$filter=" + clause,
                Error.Format(expectedExpression, dateTimeOffset));
        }

        [Fact]
        public void IntegerLiteralSuffix()
        {
            // long L
            VerifyQueryDeserialization<DataTypes>(
                "$filter=LongProp lt 987654321L and LongProp gt 123456789l",
                "$it => (($it.LongProp < 987654321) AndAlso ($it.LongProp > 123456789))");

            VerifyQueryDeserialization<DataTypes>(
                "$filter=LongProp lt -987654321L and LongProp gt -123456789l",
                "$it => (($it.LongProp < -987654321) AndAlso ($it.LongProp > -123456789))");
        }

        [Fact]
        public void RealLiteralSuffixes()
        {
            // Float F
            VerifyQueryDeserialization<DataTypes>(
                "$filter=FloatProp lt 4321.56F and FloatProp gt 1234.56f",
                Error.Format("$it => (($it.FloatProp < {0:0.00}) AndAlso ($it.FloatProp > {1:0.00}))", 4321.56, 1234.56));

            // Decimal M
            VerifyQueryDeserialization<DataTypes>(
                "$filter=DecimalProp lt 4321.56M and DecimalProp gt 1234.56m",
                Error.Format("$it => (($it.DecimalProp < {0:0.00}) AndAlso ($it.DecimalProp > {1:0.00}))", 4321.56, 1234.56));
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
                "$filter=ProductName eq " + literal,
                String.Format("$it => ($it.ProductName == \"{0}\")", expected));
        }
        #endregion

        #region Negative Tests

        [Fact]
        public void TypeMismatchInComparison()
        {
            Assert.Throws<ODataException>(() =>
                VerifyQueryDeserialization(
                "$filter=length(123) eq 12",
                ""));
        }

        #endregion

        private void RunFilters(dynamic filters, Product product, dynamic expectedValue)
        {
            var filterWithNullPropagation = filters.WithNullPropagation as Expression<Func<Product, bool>>;
            if (expectedValue.WithNullPropagation is Type)
            {
                Assert.Throws(expectedValue.WithNullPropagation as Type, () => RunFilter(filterWithNullPropagation, product));
            }
            else
            {
                Assert.Equal(RunFilter(filterWithNullPropagation, product), expectedValue.WithNullPropagation);
            }

            var filterWithoutNullPropagation = filters.WithoutNullPropagation as Expression<Func<Product, bool>>;
            if (expectedValue.WithoutNullPropagation is Type)
            {
                Assert.Throws(expectedValue.WithoutNullPropagation as Type, () => RunFilter(filterWithoutNullPropagation, product));
            }
            else
            {
                Assert.Equal(RunFilter(filterWithoutNullPropagation, product), expectedValue.WithoutNullPropagation);
            }
        }

        private bool RunFilter(Expression<Func<Product, bool>> filter, Product product)
        {
            return filter.Compile().Invoke(product);
        }

        private dynamic VerifyQueryDeserialization(string filter, string expectedResult = null, string expectedResultWithNullPropagation = null)
        {
            return VerifyQueryDeserialization<Product>(filter, expectedResult, expectedResultWithNullPropagation);
        }

        private dynamic VerifyQueryDeserialization<T>(string filter, string expectedResult = null, string expectedResultWithNullPropagation = null) where T : class
        {
            IEdmModel model = GetModel<T>();
            var queryUri = new Uri(_serviceBaseUri, string.Format("Products?{0}", filter));

            var semanticTree = SemanticTree.ParseUri(queryUri, _serviceBaseUri, model);

            var filterExpr = FilterBinder.Bind<T>(semanticTree.Query as FilterQueryNode, model, handleNullPropagation: false);

            if (!String.IsNullOrEmpty(expectedResult))
            {
                VerifyExpression(filterExpr, expectedResult);
            }

            expectedResultWithNullPropagation = expectedResultWithNullPropagation ?? expectedResult;

            var filterExprWithNullPropagation = FilterBinder.Bind<T>(semanticTree.Query as FilterQueryNode, model, handleNullPropagation: true);

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
            string resultExpression = filter.ToString();
            Assert.True(resultExpression == expectedExpression,
                String.Format("Expected expression '{0}' but the deserializer produced '{1}'", expectedExpression, resultExpression));
        }

        private IEdmModel GetModel<T>() where T : class
        {
            var model = new ODataConventionModelBuilder();
            model.EntitySet<T>("Products");
            return model.GetEdmModel();
        }

        private T? ToNullable<T>(object value) where T : struct
        {
            return value == null ? null : (T?)Convert.ChangeType(value, typeof(T));
        }
    }
}
