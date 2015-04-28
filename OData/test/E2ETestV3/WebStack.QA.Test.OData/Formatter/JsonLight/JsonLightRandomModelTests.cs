using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.SelfHost;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.TypeCreator;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight
{
    public class JsonLightRandomModelTests : RandomModelTests
    {      
        public virtual string AcceptHeader { get; set; }

        public static TheoryDataSet<string, Type, string> EntityTypes
        {
            get
            {
                var data = new TheoryDataSet<string, Type, string>();
                var acceptHeaders = new List<string> 
                {
                    "application/json;odata=minimalmetadata;streaming=true",
                    "application/json;odata=minimalmetadata;streaming=false",
                    "application/json;odata=minimalmetadata",
                    "application/json;odata=fullmetadata;streaming=true",
                    "application/json;odata=fullmetadata;streaming=false",
                    "application/json;odata=fullmetadata",
                    "application/json;streaming=true",
                    "application/json;streaming=false",
                    "application/json",
                };
                foreach (var acceptHeader in acceptHeaders)
                {
                    foreach (var type in Creator.EntityClientTypes)
                    {
                        data.Add(acceptHeader, type, type.Name);
                        var baseType = type.BaseType;
                        while (baseType != typeof(object))
                        {
                            data.Add(acceptHeader, type, baseType.Name);
                            baseType = baseType.BaseType;
                        }
                    }
                }
                return data;
            }
        }

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

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //TraceConfig.Register(configuration);

            configuration.EnableODataSupport(GetEdmModel(configuration));
            //configuration.Services.Replace(typeof(ModelBinderProvider), new ODataModelBinderProvider());
            //configuration.Services.Replace(typeof(IHttpActionSelector), new ODataActionSelector());
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), new DynamicHttpControllerTypeResolver(
                controllers =>
                {
                    Creator.ControllerTypes.ForEach(t => controllers.Add(t));
                    return controllers;
                }));

            var selfHostConfig = configuration as HttpSelfHostConfiguration;
            if (selfHostConfig != null)
            {
                selfHostConfig.MaxReceivedMessageSize = selfHostConfig.MaxBufferSize = int.MaxValue;
            }
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [Theory]
        [PropertyData("EntityTypes")]
        public void TestRandomEntityTypesJsonLight(string acceptHeader, Type entityType, string entitySetName)
        {
            AcceptHeader = acceptHeader;
            TestRandomEntityTypes(entityType, entitySetName);
        }

        private object GetIDValue(object o)
        {
            var idProperty = o.GetType().GetProperty("ID");
            return idProperty.GetValue(o, null);
        }

        private PropertyInfo UpdateNonIDProperty(object o, Random rndGen)
        {
            var properties =
                o.GetType().GetProperties().Where(p =>
                    !p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase)
                    && p.PropertyType.IsPrimitive);
            if (!properties.Any())
            {
                return null;
            }

            var newObj = Creator.GenerateClientRandomData(o.GetType(), rndGen);
            var property = properties.Skip(rndGen.Next(properties.Count())).First();
            property.SetValue(o, property.GetValue(newObj, null), null);
            return property;
        }
    }
}
