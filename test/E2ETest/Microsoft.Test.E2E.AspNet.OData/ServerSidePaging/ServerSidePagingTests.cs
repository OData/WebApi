//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ServerSidePaging
{
    public class ServerSidePagingTests : WebHostTestBase
    {
        public ServerSidePagingTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Select().Filter().OrderBy().Expand().Count().MaxTop(null);
            // NOTE: Brackets in prefix to force a call into `RouteCollection`'s `GetVirtualPath`
            configuration.MapODataServiceRoute(
                routeName: "bracketsInPrefix",
                routePrefix: "{a}",
                model: GetEdmModel(configuration),
                pathHandler: new DefaultODataPathHandler(),
                routingConventions: ODataRoutingConventions.CreateDefault());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<ServerSidePagingOrder>("ServerSidePagingOrders").EntityType.HasRequired(d => d.ServerSidePagingCustomer);
            builder.EntitySet<ServerSidePagingCustomer>("ServerSidePagingCustomers").EntityType.HasMany(d => d.ServerSidePagingOrders);
            builder.EntitySet<ContainmentPagingCustomer>("ContainmentPagingCustomers");
            builder.Singleton<ContainmentPagingCustomer>("ContainmentPagingCompany");
            builder.EntitySet<NoContainmentPagingCustomer>("NoContainmentPagingCustomers");
            builder.EntitySet<NoContainmentPagingOrder>("NoContainmentPagingOrders");
            builder.EntitySet<NoContainmentPagingOrderItem>("NoContainmentPagingOrderItems");
            builder.EntitySet<ContainmentPagingMenu>("ContainmentPagingMenus");
            builder.EntitySet<ContainmentPagingPanel>("ContainmentPagingPanels");
            builder.Singleton<ContainmentPagingMenu>("ContainmentPagingRibbon");

            var getEmployeesHiredInPeriodFunction = builder.EntitySet<ServerSidePagingEmployee>(
                "ServerSidePagingEmployees").EntityType.Collection.Function("GetEmployeesHiredInPeriod");
            getEmployeesHiredInPeriodFunction.Parameter(typeof(DateTime), "fromDate");
            getEmployeesHiredInPeriodFunction.Parameter(typeof(DateTime), "toDate");
            getEmployeesHiredInPeriodFunction.ReturnsCollectionFromEntitySet<ServerSidePagingEmployee>("ServerSidePagingEmployees");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ValidNextLinksGenerated()
        {
            var requestUri = this.BaseAddress + "/prefix/ServerSidePagingCustomers?$expand=ServerSidePagingOrders";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Customer 1 => 6 Orders, Customer 2 => 5 Orders, Customer 3 => 4 Orders, ...
            // NextPageLink will be expected on the Customers collection as well as
            // the Orders child collection on Customer 1
            Assert.Contains("@odata.nextLink", content);
            Assert.Contains("/prefix/ServerSidePagingCustomers?$expand=ServerSidePagingOrders&$skip=5",
                content);
            // Orders child collection
            Assert.Contains("ServerSidePagingOrders@odata.nextLink", content);
            Assert.Contains("/prefix/ServerSidePagingCustomers(1)/ServerSidePagingOrders?$skip=5",
                content);
        }

        [Fact]
        public async Task VerifyParametersInNextPageLinkInEdmFunctionResponseBodyAreInSameCaseAsInRequestUrl()
        {
            // Arrange
            var requestUri = this.BaseAddress + "/prefix/ServerSidePagingEmployees/" +
                "GetEmployeesHiredInPeriod(fromDate=@fromDate,toDate=@toDate)" +
                "?@fromDate=2023-01-07T00:00:00%2B00:00&@toDate=2023-05-07T00:00:00%2B00:00";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("\"@odata.nextLink\":", content);
            Assert.Contains(
                "/prefix/ServerSidePagingEmployees/GetEmployeesHiredInPeriod(fromDate=@fromDate,toDate=@toDate)" +
                "?%40fromDate=2023-01-07T00%3A00%3A00%2B00%3A00&%40toDate=2023-05-07T00%3A00%3A00%2B00%3A00&$skip=3",
                content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForNestedExpandInContainmentScenario()
        {
            // Arrange
            var requestUri = this.BaseAddress + "/prefix/ContainmentPagingCustomers?$expand=Orders($expand=Items)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("/prefix/ContainmentPagingCustomers(1)/Orders(1)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCustomers(1)/Orders(2)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCustomers(1)/Orders?$expand=Items&$skip=2", content);

            Assert.Contains("/prefix/ContainmentPagingCustomers(2)/Orders(4)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCustomers(2)/Orders(5)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCustomers(2)/Orders?$expand=Items&$skip=2", content);

            Assert.Contains("/prefix/ContainmentPagingCustomers?$expand=Orders", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForContainedNavigationPropertyAsODataPathSegment()
        {
            // Arrange
            var requestUri = this.BaseAddress + "/prefix/ContainmentPagingCustomers(2)/Orders?$expand=Items";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("/prefix/ContainmentPagingCustomers(2)/Orders(4)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCustomers(2)/Orders(5)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCustomers(2)/Orders?$expand=Items&$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForContainedNavigationPropertyInSingletonScenario()
        {
            // Arrange
            var requestUri = this.BaseAddress + "/prefix/ContainmentPagingCompany?$expand=Orders($expand=Items)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("/prefix/ContainmentPagingCompany/Orders(1)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCompany/Orders(2)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCompany/Orders?$expand=Items&$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForContainedNavigationPropertyAsODataPathSegmentInSingletonScenario()
        {
            // Arrange
            var requestUri = this.BaseAddress + "/prefix/ContainmentPagingCompany/Orders?$expand=Items";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("/prefix/ContainmentPagingCompany/Orders(1)/Items?$skip=2", content);
            Assert.Contains("/prefix/ContainmentPagingCompany/Orders(2)/Items?$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForNestedExpandInNoContainmentScenario()
        {
            // Arrange
            var requestUri = this.BaseAddress + "/prefix/NoContainmentPagingCustomers?$expand=Orders($expand=Items)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("/prefix/NoContainmentPagingOrders(1)/Items?$skip=2", content);
            Assert.Contains("/prefix/NoContainmentPagingOrders(2)/Items?$skip=2", content);
            Assert.Contains("/prefix/NoContainmentPagingCustomers(1)/Orders?$expand=Items&$skip=2", content);

            Assert.Contains("/prefix/NoContainmentPagingOrders(4)/Items?$skip=2", content);
            Assert.Contains("/prefix/NoContainmentPagingOrders(5)/Items?$skip=2", content);
            Assert.Contains("/prefix/NoContainmentPagingCustomers(2)/Orders?$expand=Items&$skip=2", content);

            Assert.Contains("/prefix/NoContainmentPagingCustomers?$expand=Orders", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForNonContainedNavigationPropertyAsODataPathSegment()
        {
            // Arrange
            var requestUri = this.BaseAddress + "/prefix/NoContainmentPagingCustomers(2)/Orders?$expand=Items";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("/prefix/NoContainmentPagingOrders(4)/Items?$skip=2", content);
            Assert.Contains("/prefix/NoContainmentPagingOrders(5)/Items?$skip=2", content);
            Assert.Contains("/prefix/NoContainmentPagingCustomers(2)/Orders?$expand=Items&$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForContainedNavigationPropertyDeclaredOnDerivedType()
        {
            // Arrange
            var extendedMenuTypeName = typeof(ContainmentPagingExtendedMenu).FullName;
            var extendedTabTypeName = typeof(ContainedPagingExtendedTab).FullName;
            var extendedItemTypeName = typeof(ContainedPagingExtendedItem).FullName;
            var menusResourcePath = $"/prefix/ContainmentPagingMenus";
            var menu1ResourcePath = $"/prefix/ContainmentPagingMenus(1)/{extendedMenuTypeName}";
            var menu2ResourcePath = $"/prefix/ContainmentPagingMenus(2)/{extendedMenuTypeName}";

            var requestUri = $"{this.BaseAddress}{menusResourcePath}?$expand={extendedMenuTypeName}/Tabs($expand={extendedTabTypeName}/Items($expand={extendedItemTypeName}/Notes))";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains($"{menu1ResourcePath}/Tabs(1)/{extendedTabTypeName}/Items(1)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(1)/{extendedTabTypeName}/Items(2)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(1)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(2)/{extendedTabTypeName}/Items(4)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(2)/{extendedTabTypeName}/Items(5)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(2)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs?$expand={extendedTabTypeName}%2FItems%28%24expand%3D{extendedItemTypeName}%2FNotes%29&$skip=2", content);

            Assert.Contains($"{menu2ResourcePath}/Tabs(4)/{extendedTabTypeName}/Items(10)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu2ResourcePath}/Tabs(4)/{extendedTabTypeName}/Items(11)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu2ResourcePath}/Tabs(4)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu2ResourcePath}/Tabs(5)/{extendedTabTypeName}/Items(13)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu2ResourcePath}/Tabs(5)/{extendedTabTypeName}/Items(14)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu2ResourcePath}/Tabs(5)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu2ResourcePath}/Tabs?$expand={extendedTabTypeName}%2FItems%28%24expand%3D{extendedItemTypeName}%2FNotes%29&$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForContainedNavigationPropertyDeclaredOnDerivedTypeAsODataPathSegment()
        {
            // Arrange
            var extendedMenuTypeName = typeof(ContainmentPagingExtendedMenu).FullName;
            var extendedTabTypeName = typeof(ContainedPagingExtendedTab).FullName;
            var extendedItemTypeName = typeof(ContainedPagingExtendedItem).FullName;
            var menu1ResourcePath = $"/prefix/ContainmentPagingMenus(1)/{extendedMenuTypeName}";

            var requestUri = $"{this.BaseAddress}{menu1ResourcePath}/Tabs?$expand={extendedTabTypeName}/Items($expand={extendedItemTypeName}/Notes)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains($"{menu1ResourcePath}/Tabs(1)/{extendedTabTypeName}/Items(1)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(1)/{extendedTabTypeName}/Items(2)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(1)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(2)/{extendedTabTypeName}/Items(4)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(2)/{extendedTabTypeName}/Items(5)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs(2)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Tabs?$expand={extendedTabTypeName}%2FItems%28%24expand%3D{extendedItemTypeName}%2FNotes%29&$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForContainedNavigationPropertyDeclaredOnDerivedTypeInSingletonScenario()
        {
            // Arrange
            var extendedMenuTypeName = typeof(ContainmentPagingExtendedMenu).FullName;
            var extendedTabTypeName = typeof(ContainedPagingExtendedTab).FullName;
            var extendedItemTypeName = typeof(ContainedPagingExtendedItem).FullName;
            var ribbonResourcePath = $"/prefix/ContainmentPagingRibbon";

            var requestUri = $"{this.BaseAddress}{ribbonResourcePath}?$expand={extendedMenuTypeName}/Tabs($expand={extendedTabTypeName}/Items($expand={extendedItemTypeName}/Notes))";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains($"{ribbonResourcePath}/{extendedMenuTypeName}/Tabs(1)/{extendedTabTypeName}/Items(1)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/{extendedMenuTypeName}/Tabs(1)/{extendedTabTypeName}/Items(2)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/{extendedMenuTypeName}/Tabs(1)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/{extendedMenuTypeName}/Tabs(2)/{extendedTabTypeName}/Items(4)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/{extendedMenuTypeName}/Tabs(2)/{extendedTabTypeName}/Items(5)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/{extendedMenuTypeName}/Tabs(2)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/{extendedMenuTypeName}/Tabs?$expand={extendedTabTypeName}%2FItems%28%24expand%3D{extendedItemTypeName}%2FNotes%29&$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForContainedNavigationPropertyDeclaredOnDerivedTypeAsODataPathSegmentInSingletonScenario()
        {
            // Arrange
            var extendedMenuTypeName = typeof(ContainmentPagingExtendedMenu).FullName;
            var extendedTabTypeName = typeof(ContainedPagingExtendedTab).FullName;
            var extendedItemTypeName = typeof(ContainedPagingExtendedItem).FullName;
            var ribbonResourcePath = $"/prefix/ContainmentPagingRibbon/{extendedMenuTypeName}";

            var requestUri = $"{this.BaseAddress}{ribbonResourcePath}/Tabs?$expand={extendedTabTypeName}/Items($expand={extendedItemTypeName}/Notes)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains($"{ribbonResourcePath}/Tabs(1)/{extendedTabTypeName}/Items(1)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/Tabs(1)/{extendedTabTypeName}/Items(2)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/Tabs(1)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/Tabs(2)/{extendedTabTypeName}/Items(4)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/Tabs(2)/{extendedTabTypeName}/Items(5)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/Tabs(2)/{extendedTabTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{ribbonResourcePath}/Tabs?$expand={extendedTabTypeName}%2FItems%28%24expand%3D{extendedItemTypeName}%2FNotes%29&$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForNonContainedNavigationPropertyDeclaredOnDerivedType()
        {
            // Arrange
            var extendedMenuTypeName = typeof(ContainmentPagingExtendedMenu).FullName;
            var extendedPanelTypeName = typeof(ContainmentPagingExtendedPanel).FullName;
            var extendedItemTypeName = typeof(ContainedPagingExtendedItem).FullName;
            var menusResourcePath = $"/prefix/ContainmentPagingMenus";
            var menu1ResourcePath = $"/prefix/ContainmentPagingMenus(1)/{extendedMenuTypeName}";
            var menu2ResourcePath = $"/prefix/ContainmentPagingMenus(2)/{extendedMenuTypeName}";

            var requestUri = $"{this.BaseAddress}{menusResourcePath}?$expand={extendedMenuTypeName}/Panels($expand={extendedPanelTypeName}/Items($expand={extendedItemTypeName}/Notes))";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains($"/prefix/ContainmentPagingPanels(1)/{extendedPanelTypeName}/Items(1)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(1)/{extendedPanelTypeName}/Items(2)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(1)/{extendedPanelTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(2)/{extendedPanelTypeName}/Items(4)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(2)/{extendedPanelTypeName}/Items(5)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(2)/{extendedPanelTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Panels?$expand={extendedPanelTypeName}%2FItems%28%24expand%3D{extendedItemTypeName}%2FNotes%29&$skip=2", content);

            Assert.Contains($"/prefix/ContainmentPagingPanels(4)/{extendedPanelTypeName}/Items(10)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(4)/{extendedPanelTypeName}/Items(11)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(4)/{extendedPanelTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(5)/{extendedPanelTypeName}/Items(13)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(5)/{extendedPanelTypeName}/Items(14)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(5)/{extendedPanelTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu2ResourcePath}/Panels?$expand={extendedPanelTypeName}%2FItems%28%24expand%3D{extendedItemTypeName}%2FNotes%29&$skip=2", content);
        }

        [Fact]
        public async Task VerifyExpectedNextLinksGeneratedForNonContainedNavigationPropertyDeclaredOnDerivedTypeAsODataPathSegment()
        {
            // Arrange
            var extendedMenuTypeName = typeof(ContainmentPagingExtendedMenu).FullName;
            var extendedPanelTypeName = typeof(ContainmentPagingExtendedPanel).FullName;
            var extendedItemTypeName = typeof(ContainedPagingExtendedItem).FullName;
            var menu1ResourcePath = $"/prefix/ContainmentPagingMenus(1)/{extendedMenuTypeName}";

            var requestUri = $"{this.BaseAddress}{menu1ResourcePath}/Panels?$expand={extendedPanelTypeName}/Items($expand={extendedItemTypeName}/Notes)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains($"/prefix/ContainmentPagingPanels(1)/{extendedPanelTypeName}/Items(1)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(1)/{extendedPanelTypeName}/Items(2)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(1)/{extendedPanelTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(2)/{extendedPanelTypeName}/Items(4)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(2)/{extendedPanelTypeName}/Items(5)/{extendedItemTypeName}/Notes?$skip=2", content);
            Assert.Contains($"/prefix/ContainmentPagingPanels(2)/{extendedPanelTypeName}/Items?$expand={extendedItemTypeName}%2FNotes&$skip=2", content);
            Assert.Contains($"{menu1ResourcePath}/Panels?$expand={extendedPanelTypeName}%2FItems%28%24expand%3D{extendedItemTypeName}%2FNotes%29&$skip=2", content);
        }
    }

    public class SkipTokenPagingTests : WebHostTestBase
    {
        public SkipTokenPagingTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Select().Filter().OrderBy().Expand().Count().MaxTop(null).SkipToken();
            // NOTE: Brackets in prefix to force a call into `RouteCollection`'s `GetVirtualPath`
            configuration.MapODataServiceRoute(
                routeName: "bracketsInPrefix",
                routePrefix: "{a}",
                model: GetEdmModel(configuration),
                pathHandler: new DefaultODataPathHandler(),
                routingConventions: ODataRoutingConventions.CreateDefault());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<SkipTokenPagingCustomer>("SkipTokenPagingS1Customers");
            builder.EntitySet<SkipTokenPagingCustomer>("SkipTokenPagingS2Customers");
            builder.EntitySet<SkipTokenPagingCustomer>("SkipTokenPagingS3Customers");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullableProperty()
        {
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, decimal?, int, decimal?>>
            {
                Tuple.Create<int, decimal?, int, decimal?> (1, null, 3, null),
                Tuple.Create<int, decimal?, int, decimal?> (5, null, 2, 2),
                Tuple.Create<int, decimal?, int, decimal?> (7, 5, 9, 25),
                Tuple.Create<int, decimal?, int, decimal?>(4, 30, 6, 35)
            };

            string requestUri = this.BaseAddress + "/prefix/SkipTokenPagingS1Customers?$orderby=CreditLimit";

            foreach (var testData in skipTokenTestData)
            {
                int idAt0 = testData.Item1;
                decimal? creditLimitAt0 = testData.Item2;
                int idAt1 = testData.Item3;
                decimal? creditLimitAt1 = testData.Item4;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=CreditLimit-", creditLimitAt1 != null ? creditLimitAt1.ToString() : "null", ",Id-", idAt1);

                // Act
                response = await this.Client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);
                Assert.Equal(idAt0, (pageResult[0] as JObject)["Id"].ToObject<int>());
                Assert.Equal(creditLimitAt0, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                Assert.Equal(creditLimitAt1, (pageResult[1] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingS1Customers?$orderby=CreditLimit&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await this.Client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(8, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Equal(50, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullablePropertyDescending()
        {
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, decimal?>>
            {
                Tuple.Create<int, decimal ?> (6, 35),
                Tuple.Create<int, decimal?> (9, 25),
                Tuple.Create<int, decimal?> (2, 2),
                Tuple.Create<int, decimal?>(3, null)
            };

            string requestUri = this.BaseAddress + "/prefix/SkipTokenPagingS1Customers?$orderby=CreditLimit desc";

            foreach (var testData in skipTokenTestData)
            {
                int idAt1 = testData.Item1;
                decimal? creditLimitAt1 = testData.Item2;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=CreditLimit-", creditLimitAt1 != null ? creditLimitAt1.ToString() : "null", ",Id-", idAt1);

                // Act
                response = await this.Client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);
                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                Assert.Equal(creditLimitAt1, (pageResult[1] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingS1Customers?$orderby=CreditLimit%20desc&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await this.Client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(5, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Null((pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNonNullablePropertyThenByNullableProperty()
        {
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, string, decimal?>>
            {
                Tuple.Create<int, string, decimal?> (2, "B", null),
                Tuple.Create<int, string, decimal?> (6, "C", null),
                Tuple.Create<int, string, decimal?> (11, "F", 35),
            };

            string requestUri = this.BaseAddress + "/prefix/SkipTokenPagingS2Customers?$orderby=Grade,CreditLimit";

            foreach (var testData in skipTokenTestData)
            {
                int idAt3 = testData.Item1;
                string gradeAt3 = testData.Item2;
                decimal? creditLimitAt3 = testData.Item3;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=Grade-%27", gradeAt3, "%27,CreditLimit-", creditLimitAt3 != null ? creditLimitAt3.ToString() : "null", ",Id-", idAt3);

                // Act
                response = await this.Client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(4, pageResult.Count);
                Assert.Equal(idAt3, (pageResult[3] as JObject)["Id"].ToObject<int>());
                Assert.Equal(gradeAt3, (pageResult[3] as JObject)["Grade"].ToObject<string>());
                Assert.Equal(creditLimitAt3, (pageResult[3] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingS2Customers?$orderby=Grade%2CCreditLimit&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await this.Client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(13, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Equal("F", (pageResult[0] as JObject)["Grade"].ToObject<string>());
            Assert.Equal(55, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullablePropertyThenByNonNullableProperty()
        {
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, string, decimal?>>
            {
                Tuple.Create<int, string, decimal?> (6, "C", null),
                Tuple.Create<int, string, decimal?> (5, "A", 30),
                Tuple.Create<int, string, decimal?> (10, "D", 50),
            };

            string requestUri = this.BaseAddress + "/prefix/SkipTokenPagingS2Customers?$orderby=CreditLimit,Grade";

            foreach (var testData in skipTokenTestData)
            {
                int idAt3 = testData.Item1;
                string gradeAt3 = testData.Item2;
                decimal? creditLimitAt3 = testData.Item3;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=CreditLimit-", creditLimitAt3 != null ? creditLimitAt3.ToString() : "null", ",Grade-%27", gradeAt3, "%27", ",Id-", idAt3);

                // Act
                response = await this.Client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(4, pageResult.Count);
                Assert.Equal(idAt3, (pageResult[3] as JObject)["Id"].ToObject<int>());
                Assert.Equal(gradeAt3, (pageResult[3] as JObject)["Grade"].ToObject<string>());
                Assert.Equal(creditLimitAt3, (pageResult[3] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingS2Customers?$orderby=CreditLimit%2CGrade&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await this.Client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(13, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Equal("F", (pageResult[0] as JObject)["Grade"].ToObject<string>());
            Assert.Equal(55, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullableDateTimeProperty()
        {
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, DateTime?, int, DateTime?>>
            {
                Tuple.Create<int, DateTime?, int, DateTime?> (1, null, 3, null),
                Tuple.Create<int, DateTime?, int, DateTime?> (5, null, 2, new DateTime(2023, 1, 2)),
                Tuple.Create<int, DateTime?, int, DateTime?> (7, new DateTime(2023, 1, 5), 9, new DateTime(2023, 1, 25)),
                Tuple.Create<int, DateTime?, int, DateTime?>(4, new DateTime(2023, 1, 30), 6, new DateTime(2023, 2, 4))
            };

            string requestUri = this.BaseAddress + "/prefix/SkipTokenPagingS3Customers?$orderby=CustomerSince";
            DateTime? customerSince;

            foreach (var testData in skipTokenTestData)
            {
                int idAt0 = testData.Item1;
                DateTime? customerSinceAt0 = testData.Item2;
                int idAt1 = testData.Item3;
                DateTime? customerSinceAt1 = testData.Item4;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipTokenStart = string.Concat(
                    "$skiptoken=CustomerSince-",
                    customerSinceAt1 != null ? customerSinceAt1.Value.ToString("yyyy-MM-dd") : "null");
                string skipTokenEnd = string.Concat(",Id-", idAt1);

                // Act
                response = await this.Client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);

                Assert.Equal(idAt0, (pageResult[0] as JObject)["Id"].ToObject<int>());
                customerSince = (pageResult[0] as JObject)["CustomerSince"].ToObject<DateTime?>();
                if (customerSinceAt0 == null)
                {
                    Assert.Null(customerSince);
                }
                else
                {
                    Assert.Equal(customerSinceAt0.Value.Date, customerSince.Value.Date);
                }

                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                customerSince = (pageResult[1] as JObject)["CustomerSince"].ToObject<DateTime?>();
                if (customerSinceAt1 == null)
                {
                    Assert.Null(customerSince);
                }
                else
                {
                    Assert.Equal(customerSinceAt1.Value.Date, customerSince.Value.Date);
                }

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.Contains("/prefix/SkipTokenPagingS3Customers?$orderby=CustomerSince&" + skipTokenStart, nextPageLink);
                Assert.EndsWith(skipTokenEnd, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await this.Client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(8, (pageResult[0] as JObject)["Id"].ToObject<int>());
            customerSince = (pageResult[0] as JObject)["CustomerSince"].ToObject<DateTime?>();
            Assert.NotNull(customerSince);
            Assert.Equal(new DateTime(2023, 2, 19).Date, customerSince.Value.Date);
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullableDateTimePropertyDescending()
        {
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, DateTime?>>
            {
                Tuple.Create<int, DateTime ?> (6, new DateTime(2023, 2, 4)),
                Tuple.Create<int, DateTime?> (9, new DateTime(2023, 1, 25)),
                Tuple.Create<int, DateTime?> (2, new DateTime(2023, 1, 2)),
                Tuple.Create<int, DateTime?>(3, null)
            };

            string requestUri = this.BaseAddress + "/prefix/SkipTokenPagingS3Customers?$orderby=CustomerSince desc";

            foreach (var testData in skipTokenTestData)
            {
                int idAt1 = testData.Item1;
                DateTime? customerSinceAt1 = testData.Item2;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipTokenStart = string.Concat(
                    "$skiptoken=CustomerSince-",
                    customerSinceAt1 != null ? customerSinceAt1.Value.ToString("yyyy-MM-dd") : "null");
                string skipTokenEnd = string.Concat(",Id-", idAt1);

                // Act
                response = await this.Client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);
                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                DateTime? customerSince = (pageResult[1] as JObject)["CustomerSince"].ToObject<DateTime?>();
                if (customerSinceAt1 == null)
                {
                    Assert.Null(customerSince);
                }
                else
                {
                    Assert.Equal(customerSinceAt1.Value.Date, customerSince.Value.Date);
                }

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.Contains("/prefix/SkipTokenPagingS3Customers?$orderby=CustomerSince%20desc&" + skipTokenStart, nextPageLink);
                Assert.EndsWith(skipTokenEnd, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await this.Client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(5, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Null((pageResult[0] as JObject)["CustomerSince"].ToObject<DateTime?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }
    }

    public class SkipTokenPagingEdgeCaseTests : WebHostTestBase
    {
        public SkipTokenPagingEdgeCaseTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Select().Filter().OrderBy().Expand().Count().MaxTop(null).SkipToken();
            // NOTE: Brackets in prefix to force a call into `RouteCollection`'s `GetVirtualPath`
            configuration.MapODataServiceRoute(
                routeName: "bracketsInPrefix",
                routePrefix: "{a}",
                model: GetEdmModel(configuration),
                pathHandler: new DefaultODataPathHandler(),
                routingConventions: ODataRoutingConventions.CreateDefault());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var csdl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">" +
                "<edmx:DataServices>" +
                "<Schema Namespace=\"" + typeof(SkipTokenPagingEdgeCase1Customer).Namespace + "\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<EntityType Name=\"SkipTokenPagingEdgeCase1Customer\">" +
                "<Key>" +
                "<PropertyRef Name=\"Id\" />" +
                "</Key>" +
                "<Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                "<Property Name=\"CreditLimit\" Type=\"Edm.Decimal\" Scale=\"Variable\" Nullable=\"false\" />" + // Property is nullable on CLR type
                "</EntityType>" +
                "</Schema>" +
                "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<EntityContainer Name=\"Container\">" +
                "<EntitySet Name=\"SkipTokenPagingEdgeCase1Customers\" EntityType=\"" + typeof(SkipTokenPagingEdgeCase1Customer).FullName + "\" />" +
                "</EntityContainer>" +
                "</Schema>" +
                "</edmx:DataServices>" +
                "</edmx:Edmx>";

            IEdmModel model;

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(csdl)))
            using (var reader = XmlReader.Create(memoryStream))
            {
                model = CsdlReader.Parse(reader);
            }

            return model;
        }

        [Fact]
        public async Task VerifySkipTokenPagingForPropertyNullableOnClrTypeButNotNullableOnEdmType()
        {
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, decimal?, int, decimal?>>
            {
                Tuple.Create<int, decimal?, int, decimal?>(2, 2, 7, 5),
                Tuple.Create<int, decimal?, int, decimal?>(9, 25, 4, 30),
            };

            string requestUri = this.BaseAddress + "/prefix/SkipTokenPagingEdgeCase1Customers?$orderby=CreditLimit";

            foreach (var testData in skipTokenTestData)
            {
                int idAt0 = testData.Item1;
                decimal? creditLimitAt0 = testData.Item2;
                int idAt1 = testData.Item3;
                decimal? creditLimitAt1 = testData.Item4;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=CreditLimit-", creditLimitAt1 != null ? creditLimitAt1.ToString() : "null", ",Id-", idAt1);

                // Act
                response = await this.Client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);
                Assert.Equal(idAt0, (pageResult[0] as JObject)["Id"].ToObject<int>());
                Assert.Equal(creditLimitAt0, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                Assert.Equal(creditLimitAt1, (pageResult[1] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingEdgeCase1Customers?$orderby=CreditLimit&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await this.Client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(6, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Equal(35, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }
    }
}
