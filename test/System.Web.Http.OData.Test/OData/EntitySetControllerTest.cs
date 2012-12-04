// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class EntitySetControllerTest
    {
        private HttpServer _server;
        private HttpClient _client;

        public EntitySetControllerTest()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<EmployeesController.Employee>("Employees");
            IEdmModel model = builder.GetEdmModel();
            configuration.EnableOData(model);

            _server = new HttpServer(configuration);
            _client = new HttpClient(_server);
        }

        [Fact]
        public void EntitySetController_SupportsODataUriParameters()
        {
            Guid guid = Guid.Parse("835ef7c7-ff60-4ecf-8c47-92ceacaf6a19");
            string uri = "http://localhost/Employees(guid'835ef7c7-ff60-4ecf-8c47-92ceacaf6a19')";

            HttpResponseMessage response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri)).Result;

            response.EnsureSuccessStatusCode();
            EmployeesController.Employee employee = (response.Content as ObjectContent<EmployeesController.Employee>).Value as EmployeesController.Employee;
            Assert.Equal(guid, employee.EmployeeID);
        }
    }

    public class EmployeesController : EntitySetController<EmployeesController.Employee, Guid>
    {
        protected override EmployeesController.Employee GetEntityByKey(Guid key)
        {
            return new EmployeesController.Employee() { EmployeeID = key, EmployeeName = "Bob" };
        }

        public class Employee
        {
            public Guid EmployeeID { get; set; }
            public string EmployeeName { get; set; }
        }
    }
}
