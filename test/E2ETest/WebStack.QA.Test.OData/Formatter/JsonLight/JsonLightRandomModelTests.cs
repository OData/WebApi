// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using Microsoft.OData.Client;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.TypeCreator;

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
                    "application/json;odata.metadata=minimal;odata.streaming=true",
                    "application/json;odata.metadata=minimal;odata.streaming=false",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full;odata.streaming=true",
                    "application/json;odata.metadata=full;odata.streaming=false",
                    "application/json;odata.metadata=full",
                    "application/json;odata.streaming=true",
                    "application/json;odata.streaming=false",
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

        public override DataServiceContext WriterClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            var ctx = base.WriterClient(serviceRoot, protocolVersion);
            new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            return ctx;
        }

        public override DataServiceContext ReaderClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
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

            configuration.EnableODataSupport(GetEdmModel(configuration));
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

        // [Theory(Skip = "github Issue #324 random deadlock")]
        // [PropertyData("EntityTypes")]
        public async Task TestRandomEntityTypesJsonLight(string acceptHeader, Type entityType, string entitySetName)
        {
            AcceptHeader = acceptHeader;
            await TestRandomEntityTypes(entityType, entitySetName);
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
