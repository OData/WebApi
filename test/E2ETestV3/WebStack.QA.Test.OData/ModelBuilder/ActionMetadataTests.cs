using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Nuwa;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Xml;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    [NuwaFramework]
    public class ActionMetadataTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Routes.MapODataServiceRoute("odata", "odata", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ActionProduct> products = builder.EntitySet<ActionProduct>("Products");
            ActionConfiguration productsByCategory = products.EntityType.Action("GetProductsByCategory");
            ActionConfiguration getSpecialProduct = products.EntityType.Action("GetSpecialProduct");
            productsByCategory.ReturnsCollectionFromEntitySet<ActionProduct>(products);
            getSpecialProduct.ReturnsFromEntitySet<ActionProduct>(products);
            return builder.GetEdmModel();
        }

        [Fact]
        public void ProvideOverloadToSupplyEntitySetConfiguration()
        {
            IEdmModel model = null;
            Stream stream = Client.GetStreamAsync(BaseAddress + "/odata/$metadata").Result;
            using (XmlReader reader = XmlReader.Create(stream))
            {
                model = EdmxReader.Parse(reader);
            }
            IEdmFunctionImport collection = model.FindDeclaredEntityContainer("Container").FindFunctionImports("GetProductsByCategory").SingleOrDefault();
            IEdmEntityType expectedReturnType = model.FindDeclaredType(typeof(ActionProduct).FullName) as IEdmEntityType;
            Assert.NotNull(expectedReturnType);
            Assert.NotNull(collection);
            Assert.True(collection.IsBindable);
            Assert.NotNull(collection.ReturnType.AsCollection());
            Assert.NotNull(collection.ReturnType.AsCollection().ElementType().AsEntity());
            Assert.Equal(expectedReturnType, collection.ReturnType.AsCollection().ElementType().AsEntity().EntityDefinition());

            IEdmFunctionImport single = model.FindDeclaredEntityContainer("Container").FindFunctionImports("GetSpecialProduct").SingleOrDefault();
            Assert.NotNull(single);
            Assert.True(single.IsBindable);
            Assert.NotNull(single.ReturnType.AsEntity());
            Assert.Equal(expectedReturnType, single.ReturnType.AsEntity().EntityDefinition());
        }
    }

    [NuwaFramework]
    [NwHost(HostType.IIS)]
    public class ThrowErrorWhenUserRegistersEntitytypeAsReturnTypeWithoutEntitySet
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Routes.MapODataServiceRoute("odata", "odata", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ActionProduct> products = builder.EntitySet<ActionProduct>("Products");
            ActionConfiguration extendsSupportDate = products.EntityType.Action("ExtendsSupportDate");
            extendsSupportDate.Returns<ActionProduct>();
            return builder.GetEdmModel();
        }

        [Fact]
        public void ThrowsError()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/$metadata");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.NotNull(response);
            string content = response.Content.ReadAsStringAsync().Result;
            Assert.NotNull(content);
            Assert.Contains("The EDM type 'WebStack.QA.Test.OData.ModelBuilder.ActionProduct' is already declared as an entity type. Use the method 'ReturnsFromEntitySet' if the return type is an entity.", content);
        }
    }

    public class ActionProduct
    {
        public int Id { get; set; }
    }
}
