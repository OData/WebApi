//-----------------------------------------------------------------------------
// <copyright file="InheritanceTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class InheritanceTests_Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Photo { get; set; }
    }

    public class InheritanceTests_Manager : InheritanceTests_Employee
    {
        public List<InheritanceTests_Employee> DirectReports { get; set; }
    }

    public class InheritanceTests : WebHostTestBase
    {
        private WebRouteConfiguration _configuration;

        public InheritanceTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Fact]
        public void IgnoredBaseTypePropertyShouldBeIgnoredInDeriveTypeAsWell()
        {
            ODataConventionModelBuilder builder = _configuration.CreateConventionModelBuilder();
            var employees = builder.EntitySet<InheritanceTests_Employee>("InheritanceTests_Employee");
            employees.EntityType.Ignore(e => e.Photo);
            var model = builder.GetEdmModel();

            var manager = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "InheritanceTests_Manager");
            Assert.DoesNotContain(manager.Properties(), (p) => p.Name == "Photo");
        }
    }
}
