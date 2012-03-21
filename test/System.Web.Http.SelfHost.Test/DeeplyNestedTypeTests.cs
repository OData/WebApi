using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;

namespace System.Web.Http.SelfHost
{
    public class DeepNestedTypeTests
    {
        private HttpSelfHostServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;

        public DeepNestedTypeTests()
        {
            this.SetupHost();
        }

        public void SetupHost()
        {
            baseAddress = String.Format("http://localhost/");

            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new {controller = "DeepNestedType"});

            server = new HttpSelfHostServer(config);

            httpClient = new HttpClient(server);
        }

        [Fact]
        public void PostDeeplyNestedTypeInXmlThrows()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostNest"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetNestedObjectInXml(8000), UTF8Encoding.UTF8, "application/xml");

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void PostNotTooDeeplyNestedTypeInXmlWorks()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostNest"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetNestedObjectInXml(20), UTF8Encoding.UTF8, "application/xml");

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">success from PostNest</string>";
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectedResponseValue, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void PostDeeplyNestedTypeInFormUrlEncodedThrows()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostJToken"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetNestedObjectInFormUrl(5000), UTF8Encoding.UTF8, "application/x-www-form-urlencoded");

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public void PostNotTooDeeplyNestedTypeInFormUrlEncodedWorks()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostJToken"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(GetNestedObjectInFormUrl(20), UTF8Encoding.UTF8, "application/x-www-form-urlencoded");

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">success from PostJToken</string>";
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectedResponseValue, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void PostNestedListInFormUrlEncodedWorks()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "DeepNestedType/PostNestedList"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Method = HttpMethod.Post;

            // Changing the number below to 70K will eat about 5GB memory and causing the process to spin. 
            // We have a bug to track the fix.
            request.Content = new StringContent(GetBigListInFormUrl(10000), Encoding.UTF8, "application/x-www-form-urlencoded");
            
            // Act
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            
            // Assert
            string expectedResponseValue = @"<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">success from PostNestedList</string>";
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(expectedResponseValue, response.Content.ReadAsStringAsync().Result);
        }

        private string GetNestedObjectInXml(int depth)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                sb.Insert(0, "<A>");
                sb.Append("</A>");
            }

            sb.Insert(0, "<Nest xmlns=\"http://schemas.datacontract.org/2004/07/System.Web.Http.SelfHost\">");
            sb.Append("</Nest>");
            sb.Insert(0, "<?xml version=\"1.0\"?>");

            return sb.ToString();
        }

        private string GetNestedObjectInFormUrl(int depth)
        {
            StringBuilder sb = new StringBuilder("a");
            for (int i = 0; i < depth; i++)
            {
                sb.Append("[a]");
            }
            sb.Append("=1");
            return sb.ToString();
        }

        private string GetBigListInFormUrl(int depth)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("a");
            for (int i = 0; i < depth; i++)
            {
                sb.Append("[N]");
            }
            sb.Append("[D]=1");
            return sb.ToString();
        }
    }

    public class DeepNestedTypeController : ApiController
    {
        public string PostNest(Nest a)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            return "success from PostNest";
        }

        public string PostJToken(JToken token)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            return "success from PostJToken";
        }

        public string PostNestedList(MyList a)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            return "success from PostNestedList";
        }
    }

    public class Nest
    {
        public Nest A { get; set; }
    }

    public class MyList
    {
        public int D { get; set; }
        public MyList N { get; set; }
    }

}
