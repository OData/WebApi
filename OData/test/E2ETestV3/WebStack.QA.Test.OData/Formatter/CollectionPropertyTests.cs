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
    [EntitySet("CollectionProperty_Entity")]
    [DataServiceKey("ID")]
    public class CollectionProperty_Entity
    {
        public int ID { get; set; }
        public List<string> StringList { get; set; }
        public Collection<CollectionProperty_ComplexType> ComplexTypeCollection { get; set; }
    }

    public class CollectionProperty_ComplexType
    {
        public List<string> StringList { get; set; }
        public Collection<CollectionProperty_ComplexType1> ComplexTypeCollection { get; set; }
    }
    public class CollectionProperty_ComplexType1
    {
        public List<string> StringList { get; set; }
    }

    public class CollectionProperty_EntityController : InMemoryEntitySetController<CollectionProperty_Entity, int>
    {
        public CollectionProperty_EntityController()
            : base("ID")
        {
        }
    }

    public abstract class CollectionPropertyTests : ODataFormatterTestBase
    {
        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<CollectionProperty_Entity>("CollectionProperty_Entity");
            return mb.GetEdmModel();
        }

        public void SupportPostCollectionPropertyByEntityPayload()
        {
            CreatorSettings settings = new CreatorSettings() 
            {
                NullValueProbability = 0.0
            };

            // clear respository
            this.ClearRepository("CollectionProperty_Entity");

            //this.Client.GetStringAsync(this.BaseAddress + "/$metadata").Wait();

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            var expected = InstanceCreator.CreateInstanceOf<CollectionProperty_Entity>(r, settings);
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject("CollectionProperty_Entity", expected);
            ctx.SaveChanges();

            int id = expected.ID;
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            var actual = ctx.CreateQuery<CollectionProperty_Entity>("CollectionProperty_Entity").Where(t => t.ID == id).First();

            AssertExtension.DeepEqual(expected, actual);

            expected = actual;
            ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AttachTo("CollectionProperty_Entity", expected);
            expected.StringList = InstanceCreator.CreateInstanceOf<List<string>>(r, settings);
            expected.ComplexTypeCollection = InstanceCreator.CreateInstanceOf<Collection<CollectionProperty_ComplexType>>(r, settings);
            ctx.UpdateObject(expected);
            ctx.SaveChanges();

            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            actual = ctx.CreateQuery<CollectionProperty_Entity>("CollectionProperty_Entity").Where(t => t.ID == id).First();

            AssertExtension.DeepEqual(expected, actual);

            // clear respository
            this.ClearRepository("CollectionProperty_Entity");
        }
    }
}
