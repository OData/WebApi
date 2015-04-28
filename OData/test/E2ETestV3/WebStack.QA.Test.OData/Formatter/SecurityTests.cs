using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.SelfHost;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter
{
    [EntitySet("Security_NestedModel")]
    [DataServiceKey("ID")]
    public class Security_NestedModel
    {
        public int ID { get; set; }
        public Security_NestedModel Nest { get; set; }
    }

    [EntitySet("Security_ArrayModel")]
    [DataServiceKey("ID")]
    public class Security_ArrayModel
    {
        public int ID { get; set; }
        public List<string> StringArray { get; set; }
        public List<Security_ComplexModel> ComplexArray { get; set; }
        public List<Security_NestedModel> NavigationCollection { get; set; }
    }

    public class Security_ComplexModel
    {
        public string Name { get; set; }
    }
        
    public class Security_NestedModelController : InMemoryEntitySetController<Security_NestedModel, int>
    {
        public Security_NestedModelController()
            : base("ID")
        { 
        }
    }

    public class Security_ArrayModelController : InMemoryEntitySetController<Security_ArrayModel, int>
    {
        public Security_ArrayModelController()
            : base("ID")
        { 
        }
    }

    public class DosSecurityTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            var selfConfig = configuration as HttpSelfHostConfiguration;
            if (selfConfig != null)
            {
                selfConfig.MaxReceivedMessageSize = selfConfig.MaxBufferSize = int.MaxValue;
            }

            configuration.Formatters.Clear();
            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
            var nestType = builder.EntitySet<Security_NestedModel>("Security_NestedModel").EntityType;
            var arrayType = builder.EntitySet<Security_ArrayModel>("Security_ArrayModel").EntityType;
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json;odata=verbose")]
        [InlineData("application/json")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=fullmetadata")]
        public void DeepNestedObjectShouldBeBlockedByRecursionCheck(string mediaType)
        {
            var root = new Security_NestedModel();
            var nest = root;
            for (int i = 0; i < 1000; i++)
            {
                nest.Nest = new Security_NestedModel();
                nest = nest.Nest;
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.BaseAddress + "/Security_NestedModel");
            request.Content = new ObjectContent<Security_NestedModel>(root, new JsonMediaTypeFormatter(), MediaTypeHeaderValue.Parse(mediaType));
            var response = this.Client.SendAsync(request).Result;

            var content = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("The depth limit for entries in nested expanded navigation links was reached", content);
        }

        [Fact]
        public void BigArrayObjectWithAtomShoulNotdMakeServerHang()
        {
            //Console.WriteLine(this.Client.GetStringAsync(this.BaseAddress + "/$metadata").Result);

            var model = new Security_ArrayModel();
            List<string> stringList = new List<string>();
            List<Security_ComplexModel> complexList = new List<Security_ComplexModel>();
            List<Security_NestedModel> navigationList = new List<Security_NestedModel>();
            for (int i = 0; i < 10000; i++)
            {
                stringList.Add(string.Empty);
                complexList.Add(new Security_ComplexModel());
                navigationList.Add(new Security_NestedModel());
            }
            model.StringArray = stringList;
            model.ComplexArray = complexList;
            model.NavigationCollection = navigationList;

            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject("Security_ArrayModel", model);
            ctx.SaveChanges();
        }

        [Theory]
        //[InlineData("application/json;odata=verbose")]
        [InlineData("application/json")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=fullmetadata")]
        public void BigArrayObjectWithJsonShoulNotdMakeServerHang(string mediaType)
        {
            //Console.WriteLine(this.Client.GetStringAsync(this.BaseAddress + "/$metadata").Result);

            var model = new Security_ArrayModel();
            List<string> stringList = new List<string>();
            List<Security_ComplexModel> complexList = new List<Security_ComplexModel>();
            List<Security_NestedModel> navigationList = new List<Security_NestedModel>();
            for (int i = 0; i < 10000; i++)
            {
                stringList.Add(string.Empty);
                complexList.Add(new Security_ComplexModel());
                navigationList.Add(new Security_NestedModel());
            }
            model.StringArray = stringList;
            model.ComplexArray = complexList;
            model.NavigationCollection = navigationList;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.BaseAddress + "/Security_ArrayModel");
            request.Content = new ObjectContent<Security_ArrayModel>(model, new JsonMediaTypeFormatter(), MediaTypeHeaderValue.Parse(mediaType));
            var response = this.Client.SendAsync(request).Result;

            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData(@"<id />")]
        [InlineData(@"<content type=""application/xml""/>")]
        //[InlineData(@"<idasdfasdfasd />")]
        public void DuplicateAtomEntryElementsShouldBeReject(string element)
        {
            AttackStringBuilder asb = new AttackStringBuilder();
            asb.Append(@"<?xml version=""1.0"" encoding=""utf-8""?><entry xmlns=""http://www.w3.org/2005/Atom"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">");
            asb.Repeat(element, 100000);
            asb.Append(@"<id /><title /><updated>2013-01-11T00:45:34Z</updated><author><name /></author><content type=""application/xml""><m:properties></m:properties></content></entry>");
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.BaseAddress + "/Security_ArrayModel");
            request.Content = new StringContent(asb.ToString(), Encoding.Unicode, "application/atom+xml");
            var response = this.Client.SendAsync(request).Result;

            //Assert.False(response.IsSuccessStatusCode);
        }

        [Theory]
        [InlineData(@"<d:ID m:type=""Edm.Int32"">1232</d:ID>")]
        public void DuplicateAtomContentPropertiesShouldBeReject(string element)
        {
            AttackStringBuilder asb = new AttackStringBuilder();
            asb.Append(@"<?xml version=""1.0"" encoding=""utf-8""?><entry xmlns=""http://www.w3.org/2005/Atom"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata""><id /><title /><updated>2013-01-11T00:45:34Z</updated><author><name /></author><content type=""application/xml""><m:properties>");
            asb.Repeat(element, 10000);
            asb.Append(@"</m:properties></content></entry>");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.BaseAddress + "/Security_ArrayModel");
            request.Content = new StringContent(asb.ToString(), Encoding.Unicode, "application/atom+xml");
            var response = this.Client.SendAsync(request).Result;

            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public void BigDataServiceVersionHeaderShouldBeRejected()
        {
            AttackStringBuilder asb = new AttackStringBuilder();
            asb.Append("3.0").Repeat("0", 100000);

            var payload = @"
<?xml version=""1.0"" encoding=""utf-8""?><entry xmlns=""http://www.w3.org/2005/Atom"" 
xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" 
xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
<id /><title /><updated>2013-01-11T00:45:34Z</updated><author><name /></author><content type=""application/xml""><m:properties></m:properties></content></entry>";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.BaseAddress + "/Security_ArrayModel");
            request.Content = new StringContent(payload, Encoding.Unicode, "application/atom+xml");
            request.Headers.Add("DataServiceVersion", asb.ToString());
            var response = this.Client.SendAsync(request).Result;

            Assert.False(response.IsSuccessStatusCode);
        }
    }

    public class InformationDisclosureTests : ODataTestBase
    {
    }
}
