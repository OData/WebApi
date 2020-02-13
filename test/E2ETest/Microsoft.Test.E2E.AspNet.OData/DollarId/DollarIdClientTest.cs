// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Client;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DollarId
{
    public class DollarIdClientTest : WebHostTestBase<DollarIdClientTest>
    {
        public DollarIdClientTest(WebHostTestFixture<DollarIdClientTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(SingersController), typeof(AlbumsController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("clientTest", "clientTest", DollarIdEdmModel.GetModel(configuration));
            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task DeleteNavigationLink()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var clientContext = new Client.Default.Container(new Uri(serviceRoot));
            clientContext.MergeOption = MergeOption.OverwriteChanges;

            var singer = await Task.Factory.FromAsync(clientContext.Singers.BeginExecute(null, null), (asyncResult) =>
            {
                return clientContext.Singers.EndExecute(asyncResult).Where(s => s.ID == 0).Single();
            });

            await clientContext.LoadPropertyAsync(singer, "Albums");
            Assert.Equal(3, singer.Albums.Count);

            var album = await Task.Factory.FromAsync(clientContext.Albums.BeginExecute(null, null), (asyncResult) =>
            {
                return clientContext.Albums.EndExecute(asyncResult).Where(s => s.ID == 0).Single();
            });

            clientContext.DeleteLink(singer, "Albums", album);
            await clientContext.SaveChangesAsync();

            await clientContext.LoadPropertyAsync(singer, "Albums");
            Assert.Equal(2, singer.Albums.Count);
        }

        [Fact]
        public async Task DeleteContainedNavigationLink()
        {
            var serviceRoot = this.BaseAddress + "/clientTest/";
            var clientContext = new Client.Default.Container(new Uri(serviceRoot));
            clientContext.MergeOption = MergeOption.OverwriteChanges;

            const int albumKey = 0;
            var album = await Task.Factory.FromAsync(clientContext.Albums.BeginExecute(null, null), (asyncResult) =>
            {
                return clientContext.Albums.EndExecute(asyncResult).Where(a => a.ID == albumKey).Single();
            });

            await clientContext.LoadPropertyAsync(album, "Sales");
            Assert.Equal(2, album.Sales.Count);

            var sales = album.Sales.Where(s => s.ID == albumKey + 1).Single();
            clientContext.DeleteLink(album, "Sales", sales);
            await clientContext.SaveChangesAsync();

            await clientContext.LoadPropertyAsync(album, "Sales");
            Assert.Single(album.Sales);
        }

        // [Fact(Skip = "Used to generate csdl file")]
        internal void GetMetadata()
        {
            var directory = Directory.GetCurrentDirectory();
            var strArray = directory.Split(new string[] { "bin" }, StringSplitOptions.None);
            var filePath = Path.Combine(strArray[0], @"src\Microsoft.Test.E2E.AspNet.OData\DollarId\Metadata.csdl");

            var request = (HttpWebRequest)WebRequest.Create(new Uri(this.BaseAddress + "/clientTest/$metadata"));
            request.Accept = "application/xml";
            var response = request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            string csdl = streamReader.ReadToEnd();

            File.WriteAllText(filePath, csdl);
        }
    }
}
