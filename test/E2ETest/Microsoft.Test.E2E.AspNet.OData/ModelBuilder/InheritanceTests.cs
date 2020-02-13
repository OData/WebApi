// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
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

    public class InheritanceTests
    {
        [Fact]
        public void IgnoredBaseTypePropertyShouldBeIgnoredInDeriveTypeAsWell()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var employees = builder.EntitySet<InheritanceTests_Employee>("InheritanceTests_Employee");
            employees.EntityType.Ignore(e => e.Photo);
            var model = builder.GetEdmModel();

            var manager = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "InheritanceTests_Manager");
            Assert.DoesNotContain(manager.Properties(), (p) => p.Name == "Photo");
        }
    }
}
