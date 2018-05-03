﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Threading.Tasks;
#else
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataSingletonRoutingTest
    {
        //private HttpServer _server;
        private HttpClient _client;

        public ODataSingletonRoutingTest()
        {
            var controllers = new[] { typeof(VipCustomerController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "", new CustomersModelWithInheritance().Model);
            });

            _client = TestServerFactory.CreateClient(server);
        }

        [Theory]
        [InlineData("GET", "Get")]
        [InlineData("PUT", "Put")]
        [InlineData("PATCH", "Patch")]
        [InlineData("MERGE", "Patch")]
        public async Task SingletonPath_RoutesCorrectly_ForValidHttpMethods(string httpMethod, string expectedResponse)
        {
            // Arrange
            HttpResponseMessage response = await _client.SendAsync(
                new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/VipCustomer"));

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }

        [Theory]
        [InlineData("Post")]
        [InlineData("Delete")]
        public async Task SingletonPath_ReturnsBadResponse_ForInvalidHttpMethods(string httpMethod)
        {
            // Arrange
            HttpResponseMessage response = await _client.SendAsync(
                new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/VipCustomer"));

            // Act & Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
#if NETFX
            Assert.Contains("No HTTP resource was found that matches the request URI 'http://localhost/VipCustomer'.",
                await response.Content.ReadAsStringAsync());
#endif
            // AspNetCore: Fails since SelectCandidates does not return error.
        }

        [Theory]
        [InlineData("GET", "GetFromSpecialCustomer")]
        [InlineData("PUT", "PutFromSpecialCustomer")]
        [InlineData("PATCH", "PatchFromSpecialCustomer")]
        [InlineData("MERGE", "PatchFromSpecialCustomer")]
        public async Task SingletonPath_ToCast_RoutesCorrectly(string httpMethod, string expectedResponse)
        {
            // Arrange
            HttpResponseMessage response = await _client.SendAsync(
                new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/VipCustomer/NS.SpecialCustomer"));

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }

        [Theory]
        [InlineData("GET", "GetOrders")]
        [InlineData("POST", "PostToOrders")]
        public async Task SingletonPath_ToNavigationProperty_RoutesCorrectly(string httpMethod, string expectedResponse)
        {
            // Arrange
            HttpResponseMessage response = await _client.SendAsync(
                new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/VipCustomer/Orders"));

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }

        [Theory]
        [InlineData("VipCustomer/Name", "GetName")]
        [InlineData("VipCustomer/Address", "GetAddress")]
        [InlineData("VipCustomer/NS.SpecialCustomer/SpecialCustomerProperty", "GetSpecialCustomerProperty")]
        [InlineData("VipCustomer/NS.SpecialCustomer/SpecialAddress", "GetSpecialAddress")]
        [InlineData("VipCustomer/Name/$value", "GetName")]
        [InlineData("VipCustomer/NS.SpecialCustomer/SpecialCustomerProperty/$value", "GetSpecialCustomerProperty")]
        public async Task SingletonPath_ToAccessProperty_RoutesCorrectly(string path, string expectedResponse)
        {
            // Arrange
            HttpResponseMessage response = await _client.GetAsync("http://localhost/" + path);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }

        [Theory]
        [InlineData("VipCustomer/Orders/$ref", "POST", "CreateRef(Orders)")]
        [InlineData("VipCustomer/Orders/$ref", "PUT", "CreateRef(Orders)")]
        [InlineData("VipCustomer/Orders/$ref", "DELETE", "DeleteRef(Orders)")]
        [InlineData("VipCustomer/Orders(2)/$ref", "DELETE", "DeleteRef(Orders)ByKey(2)")]
        [InlineData("VipCustomer/Orders/$ref?$id=http://localhost/Orders(2)", "DELETE", "DeleteRef(Orders)ByKey(2)")]
        [InlineData("VipCustomer/Orders/$ref?$id=../../Orders(2)", "DELETE", "DeleteRef(Orders)ByKey(2)")]
        [InlineData("VipCustomer/NS.SpecialCustomer/SpecialOrders/$ref", "POST", "CreateRef(SpecialOrders)")]
        [InlineData("VipCustomer/NS.SpecialCustomer/SpecialOrders/$ref", "PUT", "CreateRef(SpecialOrders)")]
        [InlineData("VipCustomer/NS.SpecialCustomer/SpecialOrders/$ref", "DELETE", "DeleteRef(SpecialOrders)")]
        [InlineData("VipCustomer/NS.SpecialCustomer/SpecialOrders(7)/$ref", "DELETE", "DeleteRef(SpecialOrders)ByKey(7)")]
        public async Task SingletonPath_ToDollaRef_RoutesCorrectly(string path, string httpMethod, string expectedResponse)
        {
            // Arrange
            HttpResponseMessage response = await _client.SendAsync(
                 new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + path));

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }

        [Theory]
        [InlineData("VipCustomer/NS.upgrade", "Upgrade")]
        [InlineData("VipCustomer/NS.SpecialCustomer/NS.specialUpgrade", "SpecialUpgrade")]
        public async Task SingletonPath_OnAction_RoutesCorrectly(string path, string expectedResponse)
        {
            // Arrange
            HttpResponseMessage response = await _client.PostAsync("http://localhost/" + path, new StringContent(""));

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }

        [Theory]
        [InlineData("VipCustomer/NS.IsUpgraded()", "IsUpgraded")]
        [InlineData("VipCustomer/NS.SpecialCustomer/NS.IsSpecialUpgraded()", "IsSpecialUpgraded")]
        [InlineData("VipCustomer/NS.IsUpgradedWithParam(city='Shanghai')", "IsUpgradedWithParam(Shanghai)")]
        [InlineData("VipCustomer/NS.OrderByCityAndAmount(city=@city,amount=@amount)?@city='Shanghai'&@amount=9", "OrderByCityAndAmount(Shanghai, 9)")]
        public async Task SingletonPath_OnFunction_RoutesCorrectly(string path, string expectedResponse)
        {
            // Arrange
            HttpResponseMessage response = await _client.GetAsync("http://localhost/" + path);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expectedResponse, response.Content.AsObjectContentValue());
        }
    }

    public class VipCustomerController : ODataController
    {
#region Singleton access
        public string Get()
        {
            return "Get";
        }

        public string Post()
        {
            return "Post";
        }

        public string Put()
        {
            return "Put";
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public string Patch()
        {
            return "Patch";
        }

        public string Delete()
        {
            return "Delete";
        }

        public string GetFromSpecialCustomer()
        {
            return "GetFromSpecialCustomer";
        }

        public string PostFromSpecialCustomer()
        {
            return "PostFromSpecialCustomer";
        }

        public string PutFromSpecialCustomer()
        {
            return "PutFromSpecialCustomer";
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public string PatchFromSpecialCustomer()
        {
            return "PatchFromSpecialCustomer";
        }
#endregion

#region Navigation property
        public string GetOrders()
        {
            return "GetOrders";
        }

        public string PostToOrdersFromCustomer()
        {
            return "PostToOrders";
        }

        public string PutToOrdersFromCustomer()
        {
            return "PutToOrders";
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public string PatchToOrders()
        {
            return "PatchToOrders";
        }

        public string DeleteOrders()
        {
            return "DeleteOrders";
        }

#endregion

#region Property access
        public string GetName()
        {
            return "GetName";
        }

        public string GetAddress()
        {
            return "GetAddress";
        }

        public string GetSpecialCustomerProperty()
        {
            return "GetSpecialCustomerProperty";
        }

        public string GetSpecialAddressFromSpecialCustomer()
        {
            return "GetSpecialAddress";
        }
#endregion

#region $ref
        [AcceptVerbs("POST", "PUT")]
        public string CreateRef(string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "CreateRef({0})", navigationProperty);
        }

        public string DeleteRef(string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "DeleteRef({0})", navigationProperty);
        }

        public string DeleteRef(int relatedKey, string navigationProperty)
        {
            return String.Format(CultureInfo.InvariantCulture, "DeleteRef({0})ByKey({1})", navigationProperty, relatedKey);
        }
#endregion

#region actions
        public string upgrade()
        {
            return "Upgrade";
        }

        public string specialUpgradeOnSpecialCustomer()
        {
            return "SpecialUpgrade";
        }
#endregion

#region functions
        [HttpGet]
        public string IsUpgraded()
        {
            return "IsUpgraded";
        }

        [HttpGet]
        public string IsSpecialUpgradedOnSpecialCustomer()
        {
            return "IsSpecialUpgraded";
        }

        [HttpGet]
        public string IsUpgradedWithParam(string city)
        {
            return "IsUpgradedWithParam(" + city + ")";
        }

        [HttpGet]
        public string OrderByCityAndAmount(string city, int amount)
        {
            return "OrderByCityAndAmount(" + city + ", " + amount + ")";
        }
#endregion
    }
}
