// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class FunctionActionAdvertisedTest
    {
        private const string _requestRooturl = "http://localhost/odata/";

        [Fact]
        public async Task AGet_Full()
        {
            // Arrange
            var controllers = new[] { typeof(AccountsController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null);
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            HttpClient client = TestServerFactory.CreateClient(server);

            // Act
            HttpResponseMessage response = await client.GetAsync(_requestRooturl + "Accounts?$expand=PayoutPI&$format=application/json;odata.metadata=full");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task AGet_Minimial()
        {
            // Arrange
            var controllers = new[] { typeof(AccountsController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null);
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            HttpClient client = TestServerFactory.CreateClient(server);

            // Act
            HttpResponseMessage response = await client.GetAsync(_requestRooturl + "Accounts?$expand=PayoutPI&$format=application/json;odata.metadata=minimal");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Account>("Accounts");
            builder.EntitySet<PaymentInstrument>("Payments");

            builder.EntityType<Account>().Collection.Action("Clear").Parameter<int>("p1");
            builder.EntityType<Account>()
                .Collection.Function("MyCollectionFunction")
                .Returns<string>()
                .Parameter<int>("land");

            builder.EntityType<PreminumAccount>().Collection.Function("MyCollectionFunction")
                .Returns<string>()
                .Parameter<int>("land");

            builder.EntityType<Account>().Action("ClearSingle").Parameter<string>("pa");

            builder.EntityType<Account>().Function("MyFunction").Returns<string>().Parameter<string>("p1");
            return builder.GetEdmModel();
        }
    }

    // Controller
    public class AccountsController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(_accounts);
        }

        private IList<Account> _accounts = new List<Account>
        {
            new Account
            {
                Id = 1,
                PayinPIs = new List<PaymentInstrument>(),
                PayoutPI = new PaymentInstrument { Id = 11 }
            },
            new PreminumAccount
            {
                Id = 2,
                PayinPIs = new List<PaymentInstrument>(),
                PayoutPI = new PaymentInstrument { Id = 22 },
                Name = "Sam"
            }
        };
    }

    public class Account
    {
        public int Id { get; set; }

        public PaymentInstrument PayoutPI { get; set; }

        public IList<PaymentInstrument> PayinPIs { get; set; }
    }

    public class PreminumAccount : Account
    {
        public string Name { get; set; }
    }

    public class PaymentInstrument
    {
        public int Id { get; set; }
    }
}
