// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
using System.Web.OData.Query;

namespace System.Web.OData.Test.OData.Query
{
    public class ApplyQueryOptionTest
    {
        // Legal apply queries usable against CustomerApplyTestData.
        // Tuple is: apply, expected number
        public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestApplies
        {
            get
            {
                return new TheoryDataSet<string, List<Dictionary<string, object>>>
                {
                    {
                        "aggregate(CustomerId with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", "10"} }
                        }
                    },
                    {
                        "groupby(Name,aggregate(CustomerId with sum as CustomerId))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"}, { "CustomerId", "5"} },
                            new Dictionary<string, object> { { "Name", "Highest"}, { "CustomerId", "2" } },
                            new Dictionary<string, object> { { "Name", "Middle"}, { "CustomerId", "3"} }
                        }
                    },
                    {
                        "groupby(Name)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"} },
                            new Dictionary<string, object> { { "Name", "Highest"} },
                            new Dictionary<string, object> { { "Name", "Middle"} }
                        }
                    },
                };
            }
        }

        // Test data used by CustomerTestApplies TheoryDataSet
        public static List<Customer> CustomerApplyTestData
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
                    Name = "Lowest",
                    Aliases = new List<string> { "alias34", "alias4" }
                };
                customerList.Add(c);

                return customerList;
            }
        }

        [Theory]
        [PropertyData("CustomerTestApplies")]
        public void ApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
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
            var applyOption = new ApplyQueryOption(filter, context);
            IEnumerable<Customer> customers = CustomerApplyTestData;

            // Act
            IQueryable queryable = applyOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });


            // Assert
            Assert.NotNull(queryable);
            IList<GroupingWrapper<Customer>> actualCustomers = Assert.IsAssignableFrom<IEnumerable<GroupingWrapper<Customer>>>(queryable).ToList();

            Assert.Equal(aggregation.Count(), actualCustomers.Count());

            var aggEnum = actualCustomers.GetEnumerator();

            foreach (var expected in aggregation)
            {
                aggEnum.MoveNext();
                var agg = aggEnum.Current;
                foreach (var key in expected.Keys)
                {
                    object value;
                    Assert.True(agg.TryGetPropertyValue(key, out value), "Property " + key + "not found");
                    Assert.Equal(expected[key], value);
                }

            }

        }
    }
}
