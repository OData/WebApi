using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
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
            HttpConfiguration config = new HttpConfiguration();
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(config);
            var employees = builder.EntitySet<InheritanceTests_Employee>("InheritanceTests_Employee");
            employees.EntityType.Ignore(e => e.Photo);            
            var model = builder.GetEdmModel();

            var manager = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "InheritanceTests_Manager");
            Assert.False(manager.Properties().Any(p => p.Name == "Photo"));
        }
    }
}
