//-----------------------------------------------------------------------------
// <copyright file="SingletonClientTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Client;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Singleton
{
    public class SingletonClientTest : WebHostTestBase
    {
        public SingletonClientTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute(
                "explicit",
                "clientTest",
                SingletonEdmModel.GetExplicitModel("Umbrella"),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                configuration.CreateDefaultODataBatchHandler());
        }

        [Fact]
        public async Task SingletonClientQueryTest()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var ClientContext = new Client.Container(new Uri(serviceRoot));
            ClientContext.MergeOption = MergeOption.OverwriteChanges;

            // Reset data source
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Umbrella/Microsoft.Test.E2E.AspNet.OData.Singleton.ResetDataSource"), "POST");
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Partners/Microsoft.Test.E2E.AspNet.OData.Singleton.ResetDataSource"),
                                  "POST");

            // Query
            var umbrella = await Task.Factory.FromAsync(ClientContext.Umbrella.BeginExecute(null, null), (asyncResult) =>
            {
                return ClientContext.Umbrella.EndExecute(asyncResult).Single();
            });

            Assert.Equal("Umbrella", umbrella.Name);

            // Update and verify
            umbrella.Name = "UpdatedName";
            ClientContext.UpdateObject(umbrella);
            await ClientContext.SaveChangesAsync();

            var name = (await ClientContext.ExecuteAsync<string>(new Uri("Umbrella/Name", UriKind.Relative))).Single();
            Assert.Equal("UpdatedName", name);

            // $select
            var category = await Task.Factory.FromAsync(ClientContext.Umbrella.BeginExecute(null, null), (asyncResult) =>
            {
                return ClientContext.Umbrella.EndExecute(asyncResult).Select(u => u.Category).Single();
            });

            Assert.Equal(Microsoft.Test.E2E.AspNet.OData.Singleton.Client.CompanyCategory.Communication, category);

            // Add navigation link
            var partner = new Client.Partner() { ID = 111, Name = "NewPartner1" };
            ClientContext.AddToPartners(partner);
            ClientContext.AddLink(umbrella, "Partners", partner);
            await ClientContext.SaveChangesAsync();

            var partner2 = new Client.Partner() { ID = 222, Name = "NewPartner2" };
            ClientContext.AddRelatedObject(umbrella, "Partners", partner2);
            await ClientContext.SaveChangesAsync();

            // Load navigation property
            await ClientContext.LoadPropertyAsync(umbrella, "Partners");
            Assert.NotNull(umbrella.Partners);

            // Add navigation target which is a singleton to entity
            partner = await Task.Factory.FromAsync(ClientContext.Partners.BeginExecute(null, null), (asyncResult) =>
            {
                return ClientContext.Partners.EndExecute(asyncResult).Where(p => p.ID == partner2.ID).Single();
            });

            ClientContext.SetLink(partner, "Company", umbrella);
            await ClientContext.SaveChangesAsync();

            await ClientContext.LoadPropertyAsync(partner, "Company");
            Assert.NotNull(partner.Company);

            // Update singleton
            var navigatedCompany = partner.Company;
            navigatedCompany.Revenue = 100;
            ClientContext.UpdateRelatedObject(partner, "Company", navigatedCompany);
            await ClientContext.SaveChangesAsync();

            await ClientContext.LoadPropertyAsync(partner, "Company");
            navigatedCompany = partner.Company;
            Assert.Equal(100, navigatedCompany.Revenue);

            ClientContext.DeleteLink(umbrella, "Partners", partner);
            await ClientContext.SaveChangesAsync();

            var umbrellaQuery = ClientContext.Umbrella.Expand(u => u.Partners);
            umbrella = await Task.Factory.FromAsync(umbrellaQuery.BeginExecute(null, null), (asyncResult) =>
            {
                return umbrellaQuery.EndExecute(asyncResult).Single();
            });

            Assert.Single(umbrella.Partners);
        }

        [Fact]
        public async Task SingletonQueryInBatchTest()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var ClientContext = new Client.Container(new Uri(serviceRoot));

            // Reset data source
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Umbrella/Microsoft.Test.E2E.AspNet.OData.Singleton.ResetDataSource"), "POST");
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Partners/Microsoft.Test.E2E.AspNet.OData.Singleton.ResetDataSource"),
                                  "POST");

            DataServiceRequest[] requests = new DataServiceRequest[] {
                        ClientContext.CreateSingletonQuery<Client.Company>("Umbrella"),
                        ClientContext.CreateQuery<Client.Partner>("Partners")
            };

            DataServiceResponse responses = await ClientContext.ExecuteBatchAsync(requests);

            foreach (var response in responses)
            {
                var companyResponse = response as QueryOperationResponse<Client.Company>;
                var partnerResponse = response as QueryOperationResponse<Client.Partner>;

                if (companyResponse != null)
                {
                    Assert.Equal("Umbrella", companyResponse.Single().Name);
                }

                if (partnerResponse != null)
                {
                    Assert.Equal(10, partnerResponse.ToArray().Count());
                }
            }
        }

        [Fact]
        public async Task SingletonUpdateInBatchTest()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var ClientContext = new Client.Container(new Uri(serviceRoot));

            // Reset data source
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Umbrella/Microsoft.Test.E2E.AspNet.OData.Singleton.ResetDataSource"), "POST");
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Partners/Microsoft.Test.E2E.AspNet.OData.Singleton.ResetDataSource"),
                                  "POST");

            var umbrella = await Task.Factory.FromAsync(ClientContext.Umbrella.BeginExecute(null, null), (asyncResult) =>
            {
                return ClientContext.Umbrella.EndExecute(asyncResult).Single();
            });

            Client.Partner newPartner = new Client.Partner() { ID = 110, Name = "NewPartner" };

            umbrella.Name = "UpdatedCompanyName";
            ClientContext.UpdateObject(umbrella);
            ClientContext.AddRelatedObject(umbrella, "Partners", newPartner);

            await ClientContext.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);
        }

        //[Fact(Skip = "Used to generate csdl file")]
        internal void GetMetadata()
        {
            var directory = Directory.GetCurrentDirectory();
            var strArray = directory.Split(new string[] { "bin" }, StringSplitOptions.None);
            var filePath = Path.Combine(strArray[0], @"src\Microsoft.Test.E2E.AspNet.OData\Singleton\Metadata.csdl");

            var request = (HttpWebRequest)WebRequest.Create(new Uri(this.BaseAddress + "/clientTest/$metadata"));
            request.Accept = "application/xml";
            var response = request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            string csdl = streamReader.ReadToEnd();

            File.WriteAllText(filePath, csdl);
        }
    }
}
