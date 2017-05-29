using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
{
    [EntitySet("ComplexTypeTests_Entity")]
    [DataServiceKey("ID")]
    public class ComplexTypeTests_Entity
    {
        public int ID { get; set; }
        public ComplexTypeTests_ComplexType ComplexType { get; set; }
    }

    public class ComplexTypeTests_ComplexTypeBase
    {
        public string BaseProperty { get; set; }
    }
    public class ComplexTypeTests_ComplexType : ComplexTypeTests_ComplexTypeBase
    {
        public string ChildProperty { get; set; }
    }

    public class ComplexTypeTests_EntityController : InMemoryEntitySetController<ComplexTypeTests_Entity, int>
    {
        public ComplexTypeTests_EntityController()
            : base("ID")
        {
        }
    }

    public class ComplexTypeTests : ODataFormatterTestBase
    {
        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<ComplexTypeTests_Entity>("ComplexTypeTests_Entity");
            return mb.GetEdmModel();
        }

        public void ShouldSupportDerivedComplexType()
        {
            CreatorSettings settings = new CreatorSettings()
            {
                NullValueProbability = 0.0
            };

            // clear respository
            this.ClearRepository("ComplexTypeTests_Entity");

            //this.Client.GetStringAsync(this.BaseAddress + "/$metadata").Wait();

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            var expected = InstanceCreator.CreateInstanceOf<ComplexTypeTests_Entity>(r, settings);
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject("ComplexTypeTests_Entity", expected);
            ctx.SaveChanges();

            int id = expected.ID;
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            var actual = ctx.CreateQuery<ComplexTypeTests_Entity>("ComplexTypeTests_Entity").Where(t => t.ID == id).First();

            AssertExtension.DeepEqual(expected, actual);

            expected = actual;
            ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AttachTo("ComplexTypeTests_Entity", expected);
            expected.ComplexType = InstanceCreator.CreateInstanceOf<ComplexTypeTests_ComplexType>(r, settings);
            ctx.UpdateObject(expected);
            ctx.SaveChanges();

            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            actual = ctx.CreateQuery<ComplexTypeTests_Entity>("ComplexTypeTests_Entity").Where(t => t.ID == id).First();

            AssertExtension.DeepEqual(expected, actual);

            // clear respository
            this.ClearRepository("ComplexTypeTests_Entity");
        }
    }
}
