// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class ODataEntityReferenceLinkE2ETests : WebHostTestBase
    {
        public ODataEntityReferenceLinkE2ETests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }
        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(BooksController)};
            configuration.AddControllers(controllers);
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("odata", "odata", BuildEdmModel(configuration));
        }
        private static IEdmModel BuildEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Book>("Books");
            builder.EntitySet<Author>("Authors");
            builder.Action("ResetDataSource");
            builder.Action("RelateToExistingEntityAndUpdate").ReturnsFromEntitySet<Book>("Books").EntityParameter<Book>("book");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanCreate_ANewEntityAndRelateToAnExistingEntity_UsingODataBind()
        {
            await ResetDataSource();
            // Arrange
            const string Payload = "{" +
            "\"Id\":\"1\"," +
            "\"Name\":\"BookA\"," +
            "\"Author@odata.bind\":\"Authors(1)\"}";

            string Uri = BaseAddress + "/odata/Books";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Uri);

            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Content);

            //Get the above saved entity from the database
            //and expand the navigation property to see if
            //it was correctly created with the existing entity
            //attached to it. 
            string query = string.Format("{0}/odata/Books?$expand=Author", BaseAddress);
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, query);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            
            // Act
            HttpResponseMessage res = await Client.SendAsync(requestMessage);

            // Assert
            Assert.True(res.IsSuccessStatusCode);
            var responseObject = JObject.Parse(await res.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;
            var expandProp = result[0]["Author"] as JObject;
            Assert.Equal(1, expandProp["Id"]);

        }

        [Fact]
        public async Task CanUpdate_TheRelatedEntitiesProperties()
        {
            await ResetDataSource();
            // Arrange
            const string Payload = "{" +
               "\"book\":{" +
                   "\"Id\":\"1\"," +
                   "\"Name\":\"BookA\"," +
                   "\"Author\":{" +
                       "\"@odata.id\":\"Authors(1)\"," +
                       "\"Name\":\"UpdatedAuthor\"}}}";

            string Uri = BaseAddress + "/odata/RelateToExistingEntityAndUpdate";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Uri);

            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Content);

            //Get the above saved entity from the database
            //and expand its navigation property to see if it was created with
            //the existing entity correctly.
            //Also note that we were able to update the name property 
            //of the existing entity
            string query = string.Format("{0}/odata/Books?$expand=Author", BaseAddress);
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, query);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage res = await Client.SendAsync(requestMessage);

            // Assert
            Assert.True(res.IsSuccessStatusCode);
            var responseObject = JObject.Parse(await res.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;
            var expandProp = result[0]["Author"] as JObject;
            Assert.Equal(1, expandProp["Id"]);
            Assert.Equal("UpdatedAuthor", expandProp["Name"]);
        }

        private async Task ResetDataSource()
        {
            string requestUri = BaseAddress + "/odata/ResetDataSource";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
        }
    }

    public class BooksController : TestODataController
#if NETCORE
        , IDisposable
#endif
    { 
        private ODataEntityReferenceLinkContext db = new ODataEntityReferenceLinkContext();

        [EnableQuery]
        public ITestActionResult Get()
        {        
            return Ok(db.Books);
        }
        public ITestActionResult Post([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            db.Authors.Attach(book.Author);
            db.Books.Add(book);
            db.SaveChanges();

            return Created(book);
        }

        [HttpPost]
        [ODataRoute("RelateToExistingEntityAndUpdate")]
        public ITestActionResult RelateToExistingEntityAndUpdate(ODataActionParameters odataActionParameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Book book = (Book)odataActionParameters["book"];
            string authorName = book.Author.Name;
            Author author = new Author()
            {
                Id = book.Author.Id
            };
            db.Authors.Attach(author);
            book.Author = author;
            book.Author.Name = authorName;
            db.Books.Add(book);

            db.SaveChanges();
            return Created(book);
        }

        [HttpGet]
        [ODataRoute("ResetDataSource")]
        public ITestActionResult ResetDataSource()
        {
            db.Database.Delete();  // Start from scratch so that tests aren't broken by schema changes.
            CreateDatabase();
            return Ok();
        }

        private static void CreateDatabase()
        {
            using (ODataEntityReferenceLinkContext db = new ODataEntityReferenceLinkContext())
            {
                if (!db.Authors.Any())
                {
                    IList<Author> authors = new List<Author>()
                    {
                        new Author()
                        {
                            Id = 1,
                            Name = "AuthorA"
                        },
                        new Author()
                        {
                            Id = 2,
                            Name = "AuthorB"
                        },
                        new Author()
                        {
                            Id = 3,
                            Name = "AuthorC"
                        }
                    };

                    foreach (var author in authors)
                    {
                        db.Authors.Add(author);
                    }
                    db.SaveChanges();
                }
            }
        }

#if NETCORE
        public void Dispose()
        {
             //_db.Dispose();
        }
#endif

    }

    public class Book
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public Author Author { get; set; }
        public IList<Author> AuthorList { get; set; }
    }

    public class Author
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ODataEntityReferenceLinkContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=ODataEntityReferenceLinkContext";
        public ODataEntityReferenceLinkContext()
            : base(ConnectionString)
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
    }
}
