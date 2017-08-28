// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;

namespace Microsoft.Test.AspNet.OData
{
    public class FunctionActionAdvertisedTest
    {
        private const string _requestRooturl = "http://localhost/odata/";

        [Fact]
        public void AGet_Full()
        {
            // Arrange
            var configuration = new[] { typeof(AccountsController) }.GetHttpConfiguration();
            configuration.Count().OrderBy().Filter().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpResponseMessage response = client.GetAsync(_requestRooturl + "Accounts?$expand=PayoutPI&$format=application/json;odata.metadata=full").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void AGet_Minimial()
        {
            // Arrange
            var configuration = new[] { typeof(AccountsController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpResponseMessage response = client.GetAsync(_requestRooturl + "Accounts?$expand=PayoutPI&$format=application/json;odata.metadata=minimal").Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
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
    public class AccountsController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
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
