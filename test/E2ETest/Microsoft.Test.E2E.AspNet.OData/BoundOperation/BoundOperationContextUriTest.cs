// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BoundOperation
{
    public class BoundOperationContextUriTest : WebHostTestBase
    {
        public BoundOperationContextUriTest(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(ProjectsController) };
            configuration.AddControllers(controllers);
            configuration.Routes.Clear();

            // for Operation context Uri testing
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
        }

        [Fact]
        public async Task BoundFunctionWithNonContainmentReturnTypeWorksWithCorrectContextUri()
        {
            // Arrange
            var requestUri = string.Format("{0}/odata/Projects/Default.GetProjectTask()", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("odata/$metadata#ProjectTasks/$entity", responseString);
        }

        [Fact]
        public async Task BoundFunctionWithContainmentReturnTypeWorksWithCorrectContextUri()
        {
            // Arrange
            var requestUri = string.Format("{0}/odata/Projects/Default.GetAssigments()", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("odata/$metadata#ProjectTasks/Assignments", responseString);
        }

        [Fact]
        public async Task BoundActionWorksWithNonContainmentReturnTypeWorksWithCorrectContextUri()
        {
            // Arrange
            var requestUri = string.Format("{0}/odata/Projects/Default.UpdateProjectTasks", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains("odata/$metadata#ProjectTasks", responseString);
        }

        [Fact]
        public async Task BoundActionWithContainmentReturnTypeWorksWithCorrectContextUri()
        {
            // Arrange
            var requestUri = string.Format("{0}/odata/Projects/Default.UpdateAssignment", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains("odata/$metadata#ProjectTasks/Assignment/$entity", responseString);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ProjectTask>("ProjectTasks");
            builder.EntityType<ProjectTaskAssignment>();
            var projectsConfig = builder.EntitySet<Project>("Projects").EntityType;

            // Action returns "non-Containment"
            var updateAction = projectsConfig.Collection.Action("UpdateProjectTasks");
            updateAction.CollectionEntityParameter<ProjectTask>("projectTasks");
            updateAction.ReturnsCollectionFromEntitySet<ProjectTask>("ProjectTasks");

            // Function returns "non-Containment"
            var getFunction = projectsConfig.Collection.Function("GetProjectTask");
            getFunction.ReturnsFromEntitySet<ProjectTask>("ProjectTasks");

            // Action returns "Containment"
            var action = projectsConfig.Collection.Action("UpdateAssignment");
            action.ReturnsEntityViaEntitySetPath<ProjectTaskAssignment>("bindingParameter/Task/Assignment");

            // Function returns "Containment"
            var function = projectsConfig.Collection.Function("GetAssigments");
            function.ReturnsEntityViaEntitySetPath<ProjectTaskAssignment>("bindingParameter/Tasks/Assignments");

            return builder.GetEdmModel();
        }
    }

    #region Controller
    public class ProjectsController : TestODataController
    {
        [HttpGet]
        public ITestActionResult GetProjectTask()
        {
            ProjectTask task = new ProjectTask
            {
                Id = "1",
                ProjectId = "11",
                Assignments = new ProjectTaskAssignment[]
                {
                    new ProjectTaskAssignment { Name = "James", Id = "111" }
                }
            };

            return Ok(task);
        }

        [HttpGet]
        public ITestActionResult GetAssigments()
        {
            var assignments = new ProjectTaskAssignment[]
            {
                new ProjectTaskAssignment { Name = "Kerry", Id = "1" },
                new ProjectTaskAssignment { Name = "Xu", Id = "1" }
            };

            return Ok(assignments);
        }

        [HttpPost]
        public ITestActionResult UpdateAssignment([FromBody]ODataActionParameters parameters)
        {
            ProjectTaskAssignment assignment = new ProjectTaskAssignment
            {
                Id = "2",
                Name = "Peter",
            };

            return Ok(assignment);
        }

        [HttpPost]
        public ITestActionResult UpdateProjectTasks([FromBody]ODataActionParameters parameters)
        {
            var tasks = new ProjectTask[]
            {
                new ProjectTask
                {
                    Id = "1",
                    ProjectId = "11",
                    Assignments = new ProjectTaskAssignment[]
                    {
                        new ProjectTaskAssignment { Name = "John", Id = "1" }
                    }
                }
            };

            return Ok(tasks);
        }
    }
    #endregion

    #region ProjectsModel

    public class Project
    {
        public string Id { get; set; }

        public ProjectTask Task { get; set; }

        public virtual ICollection<ProjectTask> Tasks { get; set; }
    }

    public class ProjectTask
    {
        public string Id { get; set; }

        public string ProjectId { get; set; }

        [Contained]
        public ProjectTaskAssignment Assignment { get; set; }

        [Contained]
        public virtual ICollection<ProjectTaskAssignment> Assignments { get; set; }
    }

    public class ProjectTaskAssignment
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    #endregion
}
