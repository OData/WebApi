// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class AsyncEntitySetControllerTest
    {
        private readonly HttpServer _server;
        private readonly HttpClient _client;
        private readonly IEdmModel _model;

        public AsyncEntitySetControllerTest()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<EmployeesController.Employee>("AsyncEmployees");
            _model = builder.GetEdmModel();
            configuration.Routes.MapODataServiceRoute(_model);

            _server = new HttpServer(configuration);
            _client = new HttpClient(_server);
        }

        [Fact]
        public void ODataPath_ReturnsRequestODataPath()
        {
            var controller = new Mock<AsyncEntitySetController<FormatterPerson, int>>().Object;
            var request = new HttpRequestMessage();
            var path = new ODataPath(new EntitySetPathSegment("Customers"));
            request.ODataProperties().Path = path;
            controller.Request = request;

            Assert.Equal(path, controller.ODataPath);
        }

        [Fact]
        public void GetQueryOptions_ReturnsRequestQueryOptions()
        {
            var controller = new Mock<AsyncEntitySetController<FormatterPerson, int>>().Object;
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            var model = ODataTestUtil.GetEdmModel();
            request.ODataProperties().Model = model;
            controller.Request = request;
            controller.Configuration = configuration;

            var queryOptions = controller.QueryOptions;

            Assert.Equal(request, queryOptions.Request);
            Assert.Equal(model, queryOptions.Context.Model);
            Assert.Equal(typeof(FormatterPerson), queryOptions.Context.ElementClrType);
        }

        [Fact]
        public void AsyncEntitySetController_SupportsODataUriParameters()
        {
            Guid guid = Guid.Parse("835ef7c7-ff60-4ecf-8c47-92ceacaf6a19");
            string uri = "http://localhost/AsyncEmployees(guid'835ef7c7-ff60-4ecf-8c47-92ceacaf6a19')";

            HttpResponseMessage response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri)).Result;

            response.EnsureSuccessStatusCode();
            EmployeesController.Employee employee = (response.Content as ObjectContent).Value as EmployeesController.Employee;
            Assert.Equal(guid, employee.EmployeeID);
        }

        [Fact]
        public void GetByKey_ReturnsNotFound_IfGetEntityByKeyReturnsNull()
        {
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.Setup(c => c.GetEntityByKeyAsync(It.IsAny<int>())).Returns(Task.FromResult<FormatterPerson>(null));
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            controller.Request = new HttpRequestMessage();

            var response = controller.Get(5).Result;

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Null(response.Content);
        }

        [Fact]
        public void GetByKey_ReturnsOk_IfGetEntityByKeyReturnsEntity()
        {
            var entity = new FormatterPerson();
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.Setup(c => c.GetEntityByKeyAsync(It.IsAny<int>())).Returns(Task.FromResult(entity));
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            SetupController(controller);

            var response = controller.Get(5).Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(entity, (response.Content as ObjectContent).Value as FormatterPerson);
        }

        [Fact]
        public void Post_ReturnsCreated()
        {
            var entity = new FormatterPerson();
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.Setup(c => c.CreateEntityAsync(It.IsAny<FormatterPerson>())).Returns(Task.FromResult(entity));
            controllerMock.Setup(c => c.GetKey(It.IsAny<FormatterPerson>())).Returns(5);
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            SetupController(controller);

            var response = controller.Post(entity).Result;

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(entity, (response.Content as ObjectContent).Value as FormatterPerson);
            Assert.Equal("http://localhost/People(5)", response.Headers.Location.ToString());
        }

        [Fact]
        public void Post_ReturnsNoContent_IfRequestPrefers()
        {
            var entity = new FormatterPerson();
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.Setup(c => c.CreateEntityAsync(It.IsAny<FormatterPerson>())).Returns(Task.FromResult(entity));
            controllerMock.Setup(c => c.GetKey(It.IsAny<FormatterPerson>())).Returns(5);
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            SetupController(controller);
            controller.Request.Headers.Add("Prefer", "return-no-content");

            var response = controller.Post(entity).Result;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("http://localhost/People(5)", response.Headers.Location.ToString());
            Assert.Equal("return-no-content", response.Headers.GetValues("Preference-Applied").First());
        }

        [Fact]
        public void Put_ReturnsNoContent()
        {
            var entity = new FormatterPerson();
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.Setup(c => c.UpdateEntityAsync(It.IsAny<int>(), It.IsAny<FormatterPerson>())).Returns(Task.FromResult(entity));
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            SetupController(controller);

            var response = controller.Put(5, entity).Result;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void Put_ReturnsContent_IfRequestPrefers()
        {
            var entity = new FormatterPerson();
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.Setup(c => c.UpdateEntityAsync(It.IsAny<int>(), It.IsAny<FormatterPerson>())).Returns(Task.FromResult(entity));
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            SetupController(controller);
            controller.Request.Headers.Add("Prefer", "return-content");

            var response = controller.Put(5, entity).Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(entity, (response.Content as ObjectContent).Value as FormatterPerson);
            Assert.Equal("return-content", response.Headers.GetValues("Preference-Applied").First());
        }

        [Fact]
        public void Patch_ReturnsNoContent()
        {
            var entity = new FormatterPerson();
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.Setup(c => c.PatchEntityAsync(It.IsAny<int>(), It.IsAny<Delta<FormatterPerson>>())).Returns(Task.FromResult(entity));
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            SetupController(controller);

            var response = controller.Patch(5, new Delta<FormatterPerson>()).Result;

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void Patch_ReturnsContent_IfRequestPrefers()
        {
            var entity = new FormatterPerson();
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.Setup(c => c.PatchEntityAsync(It.IsAny<int>(), It.IsAny<Delta<FormatterPerson>>())).Returns(Task.FromResult(entity));
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            SetupController(controller);
            controller.Request.Headers.Add("Prefer", "return-content");

            var response = controller.Patch(5, new Delta<FormatterPerson>()).Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(entity, (response.Content as ObjectContent).Value as FormatterPerson);
            Assert.Equal("return-content", response.Headers.GetValues("Preference-Applied").First());
        }

        [Fact]
        public void NotImplementedDeleteLink_ThrowsHttpResponseException()
        {
            var controllerMock = new Mock<AsyncEntitySetController<FormatterPerson, int>>();
            controllerMock.CallBase = true;
            var controller = controllerMock.Object;
            SetupController(controller);

            var exception = Assert.Throws<HttpResponseException>(() => controller.DeleteLink(5, "6", "Products"));
            Assert.Equal("Deleting a 'Products' link is not supported for this entity set.",
                ((ODataError)((ObjectContent)exception.Response.Content).Value).Message);
        }

        [Fact]
        public void Controller_DoesNotAppear_InApiDescriptions()
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{action}");
            config.Routes.MapODataServiceRoute(new ODataConventionModelBuilder().GetEdmModel());
            var explorer = config.Services.GetApiExplorer();

            var apis = explorer.ApiDescriptions.Select(api => api.ActionDescriptor.ControllerDescriptor.ControllerName);

            Assert.DoesNotContain("AsyncEmployees", apis);
        }

        private static void SetupController(AsyncEntitySetController<FormatterPerson, int> controller)
        {
            var config = new HttpConfiguration();
            IEdmModel model = ODataTestUtil.GetEdmModel();
            string routeName = "Route";
            config.Routes.MapODataServiceRoute(routeName, null, model);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Mock"), "http://localhost/People");
            request.SetConfiguration(config);
            request.ODataProperties().Path = new DefaultODataPathHandler().Parse(model, "People");
            request.ODataProperties().RouteName = routeName;
            controller.Request = request;
            controller.Configuration = config;
            controller.Url = new UrlHelper(request);
        }
    }

    public class AsyncEmployeesController : AsyncEntitySetController<EmployeesController.Employee, Guid>
    {
        protected internal override Task<EmployeesController.Employee> GetEntityByKeyAsync(Guid key)
        {
            return Task.FromResult(new EmployeesController.Employee() { EmployeeID = key, EmployeeName = "Bob" });
        }
    }
}
