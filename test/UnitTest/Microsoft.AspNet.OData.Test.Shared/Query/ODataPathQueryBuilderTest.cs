using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Query.Expressions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class ODataPathQueryBuilderTest
    {
        public static TheoryDataSet<string, string> TestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {
                        "Customers",
                        "ODataPathQuery_Customer[]"
                    },
                    {
                        "Customers(1)",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1))"
                    },
                    {
                        "Customers(1)/Name",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Select( => .Name)"
                    },
                    // collection navigation properties
                    {
                        "Customers(1)/Products",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Products)"
                    },
                    {
                        "Customers(1)/Products(2)",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Products).Where(entity => (entity.Id == 2))"
                    },
                    {
                        "Customers(1)/Products(2)/Name",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Products).Where(entity => (entity.Id == 2)).Select( => .Name)"
                    },
                    // non-collection navigation property
                    {
                        "Customers(1)/FavoriteProduct",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Where( => (.FavoriteProduct != null)).Select( => .FavoriteProduct)"
                    },
                    {
                        "Customers(1)/FavoriteProduct/Name",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Where( => (.FavoriteProduct != null)).Select( => .FavoriteProduct).Select( => .Name)"
                    },
                    // collection complex properties
                    {
                        "Customers(1)/Addresses",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Addresses)"
                    },
                    // non-collection complex properties
                    {
                        "Customers(1)/HomeAddress",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Select( => .HomeAddress)"
                    },
                    {
                        "Customers(1)/HomeAddress/City",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Select( => .HomeAddress).Select( => .City)"
                    },
                    // key segments with composite-keys
                    {
                        "Customers(1)/Projects(2, 'abc')",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Projects).Where(entity => ((entity.KeyOne == 2) AndAlso (entity.KeyTwo == abc)))"
                    },
                    {
                        "Customers(1)/Projects(2, 'abc')/IsDone",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Projects).Where(entity => ((entity.KeyOne == 2) AndAlso (entity.KeyTwo == abc))).Select( => .IsDone)"
                    },
                    // paths with singleton navigation source
                    {
                        "TopCustomer",
                        "ODataPathQuery_Customer[]"
                    },
                    {
                        "TopCustomer/Name",
                        "ODataPathQuery_Customer[].Select( => .Name)"
                    },
                    {
                        "TopCustomer/HomeAddress",
                        "ODataPathQuery_Customer[].Select( => .HomeAddress)"
                    },
                    {
                        "TopCustomer/Addresses",
                        "ODataPathQuery_Customer[].SelectMany( => .Addresses)"
                    },
                    {
                        "TopCustomer/HomeAddress/City",
                        "ODataPathQuery_Customer[].Select( => .HomeAddress).Select( => .City)"
                    },
                    {
                        "TopCustomer/Products",
                        "ODataPathQuery_Customer[].SelectMany( => .Products)"
                    },
                    {
                        "TopCustomer/Products(2)",
                        "ODataPathQuery_Customer[].SelectMany( => .Products).Where(entity => (entity.Id == 2))"
                    },
                    {
                        "TopCustomer/Products(2)/Name",
                        "ODataPathQuery_Customer[].SelectMany( => .Products).Where(entity => (entity.Id == 2)).Select( => .Name)"
                    },
                    {
                        "TopCustomer/FavoriteProduct",
                        "ODataPathQuery_Customer[].Where( => (.FavoriteProduct != null)).Select( => .FavoriteProduct)"
                    },
                    {
                        "TopCustomer/FavoriteProduct/Name",
                        "ODataPathQuery_Customer[].Where( => (.FavoriteProduct != null)).Select( => .FavoriteProduct).Select( => .Name)"
                    },

                };
            }
        }

        public static TheoryDataSet<string, string> CountTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {
                        "Customers/$count",
                        "ODataPathQuery_Customer[]"
                    },
                    {
                        "Customers(1)/Products/$count",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Products)"
                    },
                    {
                        "Customers(1)/Addresses/$count",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Addresses)"
                    }
                };
            }
        }

        public static TheoryDataSet<string, string> ValueTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {
                        "Customers(1)/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1))"
                    },
                    {
                        "Customers(1)/Name/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Select( => .Name)"
                    },
                    {
                        "Customers(1)/Products(2)/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Products).Where(entity => (entity.Id == 2))"
                    },
                    {
                        "Customers(1)/Products(2)/Name/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Products).Where(entity => (entity.Id == 2)).Select( => .Name)"
                    },
                    {
                        "Customers(1)/FavoriteProduct/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Where( => (.FavoriteProduct != null)).Select( => .FavoriteProduct)"
                    },
                    {
                        "Customers(1)/FavoriteProduct/Name/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Where( => (.FavoriteProduct != null)).Select( => .FavoriteProduct).Select( => .Name)"
                    },
                    {
                        "Customers(1)/HomeAddress/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Select( => .HomeAddress)"
                    },
                    {
                        "Customers(1)/HomeAddress/City/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).Select( => .HomeAddress).Select( => .City)"
                    },
                    {
                        "Customers(1)/Projects(2, 'abc')/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Projects).Where(entity => ((entity.KeyOne == 2) AndAlso (entity.KeyTwo == abc)))"
                    },
                    {
                        "Customers(1)/Projects(2, 'abc')/IsDone/$value",
                        "ODataPathQuery_Customer[].Where(entity => (entity.Id == 1)).SelectMany( => .Projects).Where(entity => ((entity.KeyOne == 2) AndAlso (entity.KeyTwo == abc))).Select( => .IsDone)"
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void BuildQuery_TransformsQueryBasedOnPathSegments(string path, string expectedQuery)
        {
            var model = ODataPathQueryModel.GetModel();
            var pathHandler = new DefaultODataPathHandler();
            var odataPath = pathHandler.Parse(model, "http://any/", path);
            IQueryable source = Array.CreateInstance(typeof(ODataPathQuery_Customer), 0).AsQueryable();
            var queryBuilder = new ODataPathQueryBuilder(source, model, odataPath);
            ODataPathQueryResult result = queryBuilder.BuildQuery();

            string queryExpression = ExpressionStringBuilder.ToString(result.Result.Expression);
            queryExpression = RemoveNameSpace(queryExpression);

            Assert.Equal(expectedQuery, queryExpression);
            Assert.False(result.HasCountSegment);
            Assert.False(result.HasValueSegment);
        }

        [Theory]
        [MemberData(nameof(CountTestData))]
        public void BuildQuery_SetsCountFlagToTrue_IfPathHasCountSegment(string path, string expectedQuery)
        {
            var model = ODataPathQueryModel.GetModel();
            var pathHandler = new DefaultODataPathHandler();
            var odataPath = pathHandler.Parse(model, "http://any/", path);
            IQueryable source = Array.CreateInstance(typeof(ODataPathQuery_Customer), 0).AsQueryable();
            var queryBuilder = new ODataPathQueryBuilder(source, model, odataPath);
            ODataPathQueryResult result = queryBuilder.BuildQuery();

            string queryExpression = ExpressionStringBuilder.ToString(result.Result.Expression);
            queryExpression = RemoveNameSpace(queryExpression);

            Assert.Equal(expectedQuery, queryExpression);
            Assert.False(result.HasValueSegment);
            Assert.True(result.HasCountSegment);
        }

        [Theory]
        [MemberData(nameof(ValueTestData))]
        public void BuildQuery_SetValueFlagToTrue_IfPathHasValueSegment(string path, string expectedQuery)
        {
            var model = ODataPathQueryModel.GetModel();
            var pathHandler = new DefaultODataPathHandler();
            var odataPath = pathHandler.Parse(model, "http://any/", path);
            IQueryable source = Array.CreateInstance(typeof(ODataPathQuery_Customer), 0).AsQueryable();
            var queryBuilder = new ODataPathQueryBuilder(source, model, odataPath);
            ODataPathQueryResult result = queryBuilder.BuildQuery();

            string queryExpression = ExpressionStringBuilder.ToString(result.Result.Expression);
            queryExpression = RemoveNameSpace(queryExpression);

            Assert.Equal(expectedQuery, queryExpression);
            Assert.True(result.HasValueSegment);
            Assert.False(result.HasCountSegment);
        }

        private string RemoveNameSpace(string fullTypeName)
        {
            string ns = typeof(ODataPathQuery_Customer).Namespace;
            return fullTypeName.Replace($"{ns}.", "");
        }
    }

    public static class ODataPathQueryModel
    {
        public static IEdmModel GetModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ODataPathQuery_Customer>("Customers");
            builder.EntitySet<ODataPathQuery_Product>("Products");
            builder.EntitySet<ODataPathQuery_Project>("Projects");
            builder.ComplexType<ODataPathQuery_Address>();
            builder.Singleton<ODataPathQuery_Customer>("TopCustomer");

            return builder.GetEdmModel();
        }
    }


    public class ODataPathQuery_Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ODataPathQuery_Address HomeAddress { get; set; }
        public List<string> Emails { get; set; }
        public List<ODataPathQuery_Address> Addresses { get; set; }
        public ODataPathQuery_Product FavoriteProduct { get; set; }
        public List<ODataPathQuery_Product> Products { get; set; }
        public List<ODataPathQuery_Project> Projects { get; set; }
    }

    public class ODataPathQuery_Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
    }

    public class ODataPathQuery_Project
    {
        [Key]
        public int KeyOne { get; set; }
        [Key]
        public string KeyTwo { get; set; }
        public string Description { get; set; }
        public bool IsDone { get; set; }
    }

    public class ODataPathQuery_Address
    {
        public string City { get; set; }
        public string Street { get; set; }
    }
}
