using System;
using System.Collections.Generic;
using System.Data.Services.Client;
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

namespace WebStack.QA.Test.OData.Formatter.Atom
{
    [NuwaFramework]
    [NwHost(HostType.WcfSelf)]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class AtomRandomModelTests : RandomModelTests
    {
        public static TheoryDataSet<Type, string> EntityTypes
        {
            get
            {
                var data = new TheoryDataSet<Type, string>();
                foreach (var type in Creator.EntityClientTypes)
                {
                    data.Add(type, type.Name);
                    var baseType = type.BaseType;
                    while (baseType != typeof(object))
                    {
                        data.Add(type, baseType.Name);
                        baseType = baseType.BaseType;
                    }
                }
                return data;
            }
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
        public void TestRandomEntityTypesAtom(Type entityType, string entitySetName)
        {
            TestRandomEntityTypes(entityType, entitySetName);
        }
    }
}
