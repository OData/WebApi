using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Extensions;
using Microsoft.OData.Client;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.DollarId
{
    [NuwaFramework]
    public class DollarIdClientTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(SingersController), typeof(AlbumsController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.MapODataServiceRoute("clientTest", "clientTest", DollarIdEdmModel.GetModel());
            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task DeleteNavigationLink()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var ClientContext = new Client.Default.Container(new Uri(serviceRoot));
            ClientContext.MergeOption = MergeOption.OverwriteChanges;

            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Singers/WebStack.QA.Test.OData.DollarId.ResetDataSource"), "POST");
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Albums/WebStack.QA.Test.OData.DollarId.ResetDataSource"), "POST");

            var singer = ClientContext.Singers.Where(s => s.ID == 0).Single();
            ClientContext.LoadProperty(singer, "Albums");
            Assert.Equal(3, singer.Albums.Count);

            var album = ClientContext.Albums.Where(s => s.ID == 0).Single();
            ClientContext.DeleteLink(singer, "Albums", album);
            await ClientContext.SaveChangesAsync();

            ClientContext.LoadProperty(singer, "Albums");
            Assert.Equal(2, singer.Albums.Count);
        }

        [Fact]
        public async Task DeleteContainedNavigationLink()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var ClientContext = new Client.Default.Container(new Uri(serviceRoot));
            ClientContext.MergeOption = MergeOption.OverwriteChanges;

            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Singers/WebStack.QA.Test.OData.DollarId.ResetDataSource"), "POST");
            await ClientContext.ExecuteAsync(new Uri(serviceRoot + "Albums/WebStack.QA.Test.OData.DollarId.ResetDataSource"), "POST");

            const int albumKey = 5;
            var album = ClientContext.Albums.Where(a => a.ID == albumKey).Single();
            ClientContext.LoadProperty(album, "Sales");
            Assert.Equal(2, album.Sales.Count);

            var sales = album.Sales.Where(s => s.ID == albumKey + 1).Single();
            ClientContext.DeleteLink(album, "Sales", sales);
            await ClientContext.SaveChangesAsync();

            ClientContext.LoadProperty(album, "Sales");
            Assert.Equal(1, album.Sales.Count);
        }

        [Fact(Skip = "Used to generate csdl file")]
        public void GetMetadata()
        {
            var directory = Directory.GetCurrentDirectory();
            var strArray = directory.Split(new string[] { "bin" }, StringSplitOptions.None);
            var filePath = Path.Combine(strArray[0], @"src\WebStack.QA.Test.OData\DollarId\Metadata.csdl");

            var request = (HttpWebRequest)WebRequest.Create(new Uri(this.BaseAddress + "/clientTest/$metadata"));
            request.Accept = "application/xml";
            var response = request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            string csdl = streamReader.ReadToEnd();

            File.WriteAllText(filePath, csdl);
        }
    }
}
