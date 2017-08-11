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
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight
{
    public class JsonLightCollectionPropertyTests : CollectionPropertyTests
    {
        public string AcceptHeader { get; set; }

        public override DataServiceContext WriterClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            var ctx = base.WriterClient(serviceRoot, protocolVersion);
            new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            return ctx;
        }

        public override DataServiceContext ReaderClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            var ctx = base.ReaderClient(serviceRoot, protocolVersion);
            new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            return ctx;
        }

        //[Theory]
        //[InlineData("application/json;odata=minimalmetadata;streaming=true")]
        //[InlineData("application/json;odata=minimalmetadata;streaming=false")]
        //[InlineData("application/json;odata=minimalmetadata")]
        //[InlineData("application/json;odata=fullmetadata;streaming=true")]
        //[InlineData("application/json;odata=fullmetadata;streaming=false")]
        //[InlineData("application/json;odata=fullmetadata")]
        //[InlineData("application/json;streaming=true")]
        //[InlineData("application/json;streaming=false")]
        //[InlineData("application/json")]
        //[Trait("Category", "LocalOnly")]
        public void SupportPostCollectionPropertyByEntityPayloadJSonLight(string accept)
        {
            AcceptHeader = accept;
            SupportPostCollectionPropertyByEntityPayload();
        }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Formatters.Clear();
            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }
    }
}
