using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Client;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.Singleton
{
    [NuwaFramework]
    public class SingletonClientTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            HttpServer server = configuration.Properties["Nuwa.HttpServerKey"] as HttpServer;

            if (server == null)
            {
                throw new Exception("Inappropriate http server");
            }

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute(
                "explicit",
                "clientTest",
                SingletonEdmModel.GetExplicitModel("Umbrella"),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler(server));
        }

        [Fact]
        public async Task SingletonClientQueryTest()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var ClientContext = new Client.Container(new Uri(serviceRoot));
            ClientContext.MergeOption = MergeOption.OverwriteChanges;

            // Reset data source
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Umbrella/WebStack.QA.Test.OData.Singleton.ResetDataSource"), "POST");
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Partners/WebStack.QA.Test.OData.Singleton.ResetDataSource"),
                                  "POST");

            // Query
            var umbrella = ClientContext.Umbrella.Single();
            Assert.Equal("Umbrella", umbrella.Name);

            // Update and verify
            umbrella.Name = "UpdatedName";
            ClientContext.UpdateObject(umbrella);
            await ClientContext.SaveChangesAsync();

            var name = ClientContext.Execute<string>(new Uri("Umbrella/Name", UriKind.Relative)).Single();
            Assert.Equal("UpdatedName", name);

            // $select
            var category = ClientContext.Umbrella.Select(u => u.Category).Single();
            Assert.Equal(Client.CompanyCategory.Communication, category);

            // Add navigation link
            var partner = new Client.Partner() { ID = 111, Name = "NewPartner1" };
            ClientContext.AddToPartners(partner);
            ClientContext.AddLink(umbrella, "Partners", partner);
            await ClientContext.SaveChangesAsync();

            var partner2 = new Client.Partner() { ID = 222, Name = "NewPartner2" };
            ClientContext.AddRelatedObject(umbrella, "Partners", partner2);
            await ClientContext.SaveChangesAsync();

            // Load navigation property
            ClientContext.LoadProperty(umbrella, "Partners");
            Assert.NotNull(umbrella.Partners);

            // Add navigation target which is a singleton to entity
            partner = ClientContext.Partners.Where(p => p.ID == partner2.ID).Single();
            ClientContext.SetLink(partner, "Company", umbrella);
            await ClientContext.SaveChangesAsync();

            ClientContext.LoadProperty(partner, "Company");
            Assert.NotNull(partner.Company);

            // Update singleton
            var navigatedCompany = partner.Company;
            navigatedCompany.Revenue = 100;
            ClientContext.UpdateRelatedObject(partner, "Company", navigatedCompany);
            await ClientContext.SaveChangesAsync();

            ClientContext.LoadProperty(partner, "Company");
            navigatedCompany = partner.Company;
            Assert.Equal(100, navigatedCompany.Revenue);

            ClientContext.DeleteLink(umbrella, "Partners", partner);
            await ClientContext.SaveChangesAsync();

            umbrella = ClientContext.Umbrella.Expand(u => u.Partners).Single();
            Assert.Equal(1, umbrella.Partners.Count);
        }

        [Fact]
        public void SingletonQueryInBatchTest()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var ClientContext = new Client.Container(new Uri(serviceRoot));

            // Reset data source
            ClientContext.Execute(new Uri(serviceRoot + "Umbrella/WebStack.QA.Test.OData.Singleton.ResetDataSource"), "POST");
            ClientContext.Execute(new Uri(serviceRoot + "Partners/WebStack.QA.Test.OData.Singleton.ResetDataSource"),
                                  "POST");

            DataServiceRequest[] requests = new DataServiceRequest[] {
                        ClientContext.CreateSingletonQuery<Client.Company>("Umbrella"),
                        ClientContext.CreateQuery<Client.Partner>("Partners")
            };

            DataServiceResponse responses = ClientContext.ExecuteBatch(requests);

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
            ClientContext.Execute(new Uri(serviceRoot + "Umbrella/WebStack.QA.Test.OData.Singleton.ResetDataSource"), "POST");
            ClientContext.Execute(new Uri(serviceRoot + "Partners/WebStack.QA.Test.OData.Singleton.ResetDataSource"),
                                  "POST");

            var umbrella = ClientContext.Umbrella.Single();
            Client.Partner newPartner = new Client.Partner() { ID = 110, Name = "NewPartner" };

            umbrella.Name = "UpdatedCompanyName";
            ClientContext.UpdateObject(umbrella);
            ClientContext.AddRelatedObject(umbrella, "Partners", newPartner);

            await ClientContext.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);
        }

        [Fact(Skip = "Used to generate csdl file")]
        public void GetMetadata()
        {
            var directory = Directory.GetCurrentDirectory();
            var strArray = directory.Split(new string[] { "bin" }, StringSplitOptions.None);
            var filePath = Path.Combine(strArray[0], @"src\WebStack.QA.Test.OData\Singleton\Metadata.csdl");

            var request = (HttpWebRequest)WebRequest.Create(new Uri(this.BaseAddress + "/clientTest/$metadata"));
            request.Accept = "application/xml";
            var response = request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            string csdl = streamReader.ReadToEnd();

            File.WriteAllText(filePath, csdl);
        }
    }
}
