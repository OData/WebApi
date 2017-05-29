using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter
{
    [EntitySet("MaliciousCharactorsAttackTests_Entity")]
    [DataServiceKey("ID")]
    public class MaliciousCharactorsAttackTests_Entity
    {
        public string ID { get; set; }
        public string StringProperty { get; set; }
    }
    public class MaliciousCharactorsAttackTests_EntityController : InMemoryEntitySetController<MaliciousCharactorsAttackTests_Entity, string>
    {
        public MaliciousCharactorsAttackTests_EntityController()
            : base("ID")
        {
        }
    }

    [NuwaFramework]
    [NwHost(HostType.WcfSelf)]
    //[NwHost(HostType.IIS)]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class MaliciousCharactorsAttackTests : IODataFormatterTestBase, IODataTestBase
    {
        private string baseAddress = null;

        public static TheoryDataSet<string> TestCharactors
        {
            get
            {
                TheoryDataSet<string> data = new TheoryDataSet<string>();
                for (char c = char.MinValue; c < char.MaxValue; c++)
                {
                    data.Add(c.ToString());
                }

                return data;
            }
        }

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        public virtual DataServiceContext ReaderClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            //By default reader uses the same configuration as writer. Reading is a more important scenario than writing
            //so this configuration allows for partial support for reading while using a standard configuration for writing.
            return WriterClient(serviceRoot, protocolVersion);
        }

        public virtual DataServiceContext WriterClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            DataServiceContext ctx = new DataServiceContext(serviceRoot, protocolVersion);
            return ctx;
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            var entity = mb.EntitySet<MaliciousCharactorsAttackTests_Entity>("MaliciousCharactorsAttackTests_Entity").EntityType;
            return mb.GetEdmModel();
        }

        public virtual void PostAndGetShouldNotFailInSerialization(string data)
        {
            var entitySetName = "MaliciousCharactorsAttackTests_Entity";
            // clear respository
            this.ClearRepository(entitySetName);

            MaliciousCharactorsAttackTests_Entity entity = new MaliciousCharactorsAttackTests_Entity();
            entity.ID = data;
            entity.StringProperty = data;

            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject(entitySetName, entity);

            try
            {
                ctx.SaveChanges(SaveChangesOptions.ContinueOnError);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    Assert.False(ex.InnerException.Message.Contains("The server did not return a response for this request."),
                        string.Format("{0} ({1}) will cause server to close connection.", data, (int)data.First()));
                }
            }

            // clear repository
            this.ClearRepository(entitySetName);
        }
    }
}
