// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData
{
    public class ODataContainmentTest
    {
        private const string BaseAddress = @"http://localhost";
        private HttpConfiguration _configuration;
        private HttpClient _client;

        public ODataContainmentTest()
        {
            var controllers = new[] { typeof(MyOrdersController) };
            var resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            _configuration = new HttpConfiguration { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            _configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            _configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            var server = new HttpServer(_configuration);
            _client = new HttpClient(server);
        }

        [Fact]
        public void Get_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#MyOrders(1)/OrderLines", (string)result["@odata.context"]);
        }

        [Fact]
        public void GetSpecialOrderLines_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines/System.Web.OData.Builder.TestModels.SpecialOrderLine";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains(
                "http://localhost/odata/$metadata#MyOrders(1)/OrderLines/System.Web.OData.Builder.TestModels.SpecialOrderLine",
                (string)result["@odata.context"]);
        }
        
        [Fact]
        public void GetOrderLine_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines(2)";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#MyOrders(1)/OrderLines/$entity", (string)result["@odata.context"]);
        }

        [Fact]
        public void GetOrderLine_OperationAdvertised_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines(2)";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.TryParseAdd("application/json;odata.metadata=full");
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(
                "http://localhost/odata/$metadata#MyOrders(1)/OrderLines/$entity",
                (string)result["@odata.context"]);
            var tag = result["#ns.Tag"];
            Assert.NotNull(tag);
            Assert.Equal("ns.Tag", tag["title"]);
            Assert.Equal("http://localhost/odata/MyOrders(1)/OrderLines(2)/ns.Tag", tag["target"]);
        }
        
        [Fact]
        public void GetMyOrders_WithContainmentProperties()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#MyOrders", (string)result["@odata.context"]);
        }

        [Fact]
        public void GetMyOrders_HasLinks_WithContainmentPropertiesAndJsonFullMetadata()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.TryParseAdd("application/json;odata.metadata=full");
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#MyOrders", (string)result["@odata.context"]);
            var array = result["value"];
            Assert.Equal(2, array.Count());
            var myOrder = array[0];
            Assert.Equal("http://localhost/odata/MyOrders(1)", myOrder["@odata.id"]);
            Assert.Equal("http://localhost/odata/MyOrders(1)", myOrder["@odata.editLink"]);
            Assert.Equal("http://localhost/odata/MyOrders(1)/OrderLines", myOrder["OrderLines@odata.navigationLink"]);
            Assert.Equal("http://localhost/odata/MyOrders(1)/OrderLines/$ref", myOrder["OrderLines@odata.associationLink"]);
            var mySpecialOrder = array[1];
            Assert.Equal("http://localhost/odata/MyOrders(2)", mySpecialOrder["@odata.id"]);
            Assert.Equal("http://localhost/odata/MyOrders(2)/System.Web.OData.Builder.TestModels.MySpecialOrder", mySpecialOrder["@odata.editLink"]);
            Assert.Equal("http://localhost/odata/MyOrders(2)/OrderLines", mySpecialOrder["OrderLines@odata.navigationLink"]);
            Assert.Equal("http://localhost/odata/MyOrders(2)/System.Web.OData.Builder.TestModels.MySpecialOrder/OrderLines/$ref", mySpecialOrder["OrderLines@odata.associationLink"]);
        }
        
        [Fact]
        public void ExpandOrderLines_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)?$expand=OrderLines";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#MyOrders/$entity", (string)result["@odata.context"]);
        }

        [Fact]
        public void ExpandOrderLines_Containment_FullMetadata()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)?$expand=OrderLines";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.TryParseAdd("application/json;odata.metadata=full");
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#MyOrders/$entity", (string)result["@odata.context"]);
            var myOrder = result;
            Assert.Equal("http://localhost/odata/MyOrders(1)", myOrder["@odata.id"]);
            Assert.Equal("http://localhost/odata/MyOrders(1)", myOrder["@odata.editLink"]);
            Assert.Equal("http://localhost/odata/MyOrders(1)/OrderLines", myOrder["OrderLines@odata.navigationLink"]);
            Assert.Equal(
                "http://localhost/odata/MyOrders(1)/OrderLines/$ref",
                myOrder["OrderLines@odata.associationLink"]);
            var orderLines = myOrder["OrderLines"];
            Assert.Equal(2, orderLines.Count());
            var orderLine = orderLines[0];
            Assert.Equal("MyOrders(1)/OrderLines(2)", orderLine["@odata.id"]);
            Assert.Equal("MyOrders(1)/OrderLines(2)", orderLine["@odata.editLink"]);
            var tag = orderLine["#ns.Tag"];
            Assert.Equal("ns.Tag", tag["title"]);
            Assert.Equal("http://localhost/odata/MyOrders(1)/OrderLines(2)/ns.Tag", tag["target"]);
            orderLine = orderLines[1];
            Assert.Equal("#System.Web.OData.Builder.TestModels.SpecialOrderLine", orderLine["@odata.type"]);
            Assert.Equal("MyOrders(1)/OrderLines(22)", orderLine["@odata.id"]);
            Assert.Equal(
                "MyOrders(1)/OrderLines(22)/System.Web.OData.Builder.TestModels.SpecialOrderLine",
                orderLine["@odata.editLink"]);
            tag = orderLine["#ns.Tag"];
            Assert.Equal("ns.Tag", tag["title"]);
            Assert.Equal(
                "http://localhost/odata/MyOrders(1)/OrderLines(22)/System.Web.OData.Builder.TestModels.SpecialOrderLine/ns.Tag",
                tag["target"]);
        }

        [Fact]
        public void Post_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            string payload = @"{ 
                ""ID"": 3, 
                ""Name"": ""def"" 
            }";
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json"); 
            
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/odata/MyOrders(1)/OrderLines(3)", response.Headers.Location.ToString());
            Assert.Equal("http://localhost/odata/$metadata#MyOrders(1)/OrderLines/$entity", (string)result["@odata.context"]);
        }

        [Fact]
        public void Put_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines(2)";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            string payload = @"{ 
                ""ID"": 2, 
                ""Name"": ""xyz"" 
            }";
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = _client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void Patch_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines(2)";

            // Act
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            string payload = @"{ 
                ""Name"": ""xyz"" 
            }";
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = _client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void Delete_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines(2)";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            var response = _client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void GetMostExpensiveOrderLine_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders(1)/OrderLines/ns.MostExpensive()";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#Edm.Double", (string)result["@odata.context"]);
            Assert.Equal(1.0, result["value"]);
        }

        [Fact]
        public void TopOrder_Containment()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders/ns.TopOrder()/OrderLines";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#OrderLines", (string)result["@odata.context"]);
        }

        [Fact]
        public void Post_MyOrders()
        {
            // Arrange
            var requestUri = BaseAddress + "/odata/MyOrders";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            string payload = @"{ 
                ""ID"": 3, 
                ""Name"": ""def"" 
            }";
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = _client.SendAsync(request).Result;
            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/odata/MyOrders(3)", response.Headers.Location.ToString());
            Assert.Equal("http://localhost/odata/$metadata#MyOrders/$entity", (string)result["@odata.context"]);
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder { Namespace = "ns" };
            builder.EntitySet<Client>("Clients");
            builder.EntitySet<MyOrder>("MyOrders");
            var orderLine = builder.EntityType<OrderLine>();
            orderLine.Collection.Function("MostExpensive").Returns<double>();
            orderLine.Action("Tag").Returns<int>();
            var myOrder = builder.EntityType<MyOrder>();
            myOrder.Collection.Function("TopOrder")
                .ReturnsEntityViaEntitySetPath<MyOrder>("bindingParameter")
                .IsComposable = true;
            return builder.GetEdmModel();
        }

        public class ClientsController : ODataController
        {
            private OrderHeader _orderHeader;

            private OrderLine[] _orderLines;

            private MyOrder[] _myOrders;

            public ClientsController()
            {
                _orderHeader = new OrderHeader { ID = 3, Name = "header", OrderId = 1 };

                _orderLines = new[] { new OrderLine { ID = 2, Name = "abc", OrderId = 1 } };

                _myOrders = new[]
                {
                    new MyOrder { ID = 1, Name = "Book", OrderLines = _orderLines, OrderHeader = _orderHeader },
                    new MySpecialOrder { ID = 2, Name = "CD", OrderLines = _orderLines, OrderHeader = _orderHeader }
                };
            }

            [ODataRoute("Clients({clientId})/MyOrders({orderId})/OrderLines")]
            IQueryable<OrderLine> GetOrderLines(int clientId, int orderId)
            {
                return _orderLines.AsQueryable().Where(orderLine => orderLine.OrderId == orderId);
            }
        }

        public class MyOrdersController : ODataController
        {
            private OrderHeader _orderHeader;

            private OrderLine[] _orderLines;

            private MyOrder[] _myOrders;

            public MyOrdersController()
            {
                _orderHeader = new OrderHeader { ID = 3, Name = "header", OrderId = 1 };

                _orderLines = new[]
                {
                    new OrderLine { ID = 2, Name = "abc", OrderId = 1 },
                    new SpecialOrderLine { ID = 22, Name = "def", OrderId = 1, MoonCake = "GS"}
                };

                _myOrders = new[]
                {
                    new MyOrder { ID = 1, Name = "Book", OrderLines = _orderLines, OrderHeader = _orderHeader },
                    new MySpecialOrder { ID = 2, Name = "CD", OrderLines = _orderLines, OrderHeader = _orderHeader }
                };
            }

            [ODataRoute("MyOrders")]
            public IHttpActionResult PostToOrders(MyOrder order)
            {
                return Created(order);
            }

            [EnableQuery]
            [ODataRoute("MyOrders")]
            public IQueryable<MyOrder> Get()
            {
                return _myOrders.AsQueryable();
            }

            [EnableQuery]
            [ODataRoute("MyOrders({orderId})")]
            public SingleResult<MyOrder> Get(int orderId)
            {
                var result = _myOrders.AsQueryable().Where(mo => mo.ID == orderId);
                return SingleResult.Create(result);
            }

            [ODataRoute("MyOrders({orderId})/OrderLines")]
            public IQueryable<OrderLine> GetOrderLines(int orderId)
            {
                return _orderLines.AsQueryable().Where(orderLine => orderLine.OrderId == orderId);
            }

            [ODataRoute("MyOrders({orderId})/OrderLines/System.Web.OData.Builder.TestModels.SpecialOrderLine")]
            public IQueryable<SpecialOrderLine> GetSpecialOrderLines(int orderId)
            {
                return _orderLines.AsQueryable().Where(orderLine => orderLine.OrderId == orderId).OfType<SpecialOrderLine>();
            }

            [EnableQuery]
            [ODataRoute("MyOrders({orderId})/OrderLines({lineId})")]
            public SingleResult<OrderLine> GetOrderLine(int orderId, int lineId)
            {
                var result = _orderLines.AsQueryable().Where(orderLine => orderLine.OrderId == orderId && orderLine.ID == lineId);
                return SingleResult.Create(result);
            }

            [ODataRoute("MyOrders({orderId})/OrderLines")]
            public IHttpActionResult Post(int orderId, OrderLine orderLine)
            {
                orderLine.OrderId = orderId;
                return Created(orderLine);
            }

            [ODataRoute("MyOrders({orderId})/OrderLines({lineId})")]
            public IHttpActionResult Put(int orderId, int lineId, OrderLine orderLine)
            {
                orderLine.OrderId = orderId;
                orderLine.ID = lineId;
                return Updated(orderLine);
            }

            [ODataRoute("MyOrders({orderId})/OrderLines({lineId})")]
            public IHttpActionResult Patch(int orderId, int lineId, Delta<OrderLine> patch)
            {
                var orderLine = _orderLines.FirstOrDefault(o => o.ID == lineId && o.OrderId == orderId);
                if (orderLine == null)
                {
                    return NotFound();
                }

                patch.Patch(orderLine);
                return Updated(orderLine);
            }

            [ODataRoute("MyOrders({orderId})/OrderLines({lineId})")]
            public IHttpActionResult Delete(int orderId, int lineId)
            {
                var orderLine = _orderLines.FirstOrDefault(o => o.ID == lineId && o.OrderId == orderId);
                if (orderLine == null)
                {
                    return NotFound();
                }

                return StatusCode(HttpStatusCode.NoContent);
            }

            [HttpGet]
            [ODataRoute("MyOrders({orderId})/OrderLines({lineId})/ns.Tag()")]
            public IHttpActionResult Tag(int orderId, int lineId)
            {
                return Ok(1);
            }

            [HttpGet]
            [ODataRoute("MyOrders({orderId})/OrderLines/ns.MostExpensive()")]
            public IHttpActionResult MostExpensive(int orderId)
            {
                return Ok(1.0);
            }

            [HttpGet]
            [ODataRoute("MyOrders/ns.TopOrder()/OrderLines")]
            public IQueryable<OrderLine> TopOrder()
            {
                return _orderLines.AsQueryable().Where(orderLine => orderLine.OrderId == 1);
            }
        }
    }
}
