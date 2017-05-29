using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Web.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Models.Vehicle;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight
{
    public class JsonLightInheritanceTests : InheritanceTests
    {
        public string AcceptHeader { get; set; }

        public static TheoryDataSet<Type, string, string> PostGetUpdateAndDeleteData
        {
            get
            {
                var data = new TheoryDataSet<Type, string, string>();
                object[][] testCases = new object[][]
                {
                    new object[] { typeof(Car), "InheritanceTests_MovingObjects" },
                    new object[] { typeof(Vehicle), "InheritanceTests_MovingObjects" },
                    new object[] { typeof(Vehicle), "InheritanceTests_Vehicles" },
                    new object[] { typeof(Car), "InheritanceTests_Cars" },
                    new object[] { typeof(Car), "InheritanceTests_Vehicles" },
                    new object[] { typeof(SportBike), "InheritanceTests_Vehicles" }
                };
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
                foreach (dynamic testCase in testCases)
                {
                    foreach (var header in acceptHeaders)
                    {
                        data.Add(testCase[0], testCase[1], header);
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
            
            var conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new NavigationRoutingConvention2());
            conventions.Insert(0, new LinkRoutingConvention2());

            configuration.Routes.MapODataServiceRoute(
                ODataTestConstants.DefaultRouteName,
                null,
                GetEdmModel(configuration),
                new DefaultODataPathHandler(),
                conventions);

            configuration.AddODataQueryFilter();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [Theory]
        [PropertyData("PostGetUpdateAndDeleteData")]
        public void PostGetUpdateAndDeleteJsonLight(Type entityType, string entitySetName, string acceptHeader)
        {
            AcceptHeader = acceptHeader;
            PostGetUpdateAndDelete(entityType, entitySetName);
        }

        [Theory]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        [InlineData("application/json")]
        public void AddAndRemoveBaseNavigationPropertyInDerivedTypeJsonLight(string acceptHeader)
        {
            AcceptHeader = acceptHeader;
            AddAndRemoveBaseNavigationPropertyInDerivedType();
        }

        [Theory]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        [InlineData("application/json")]
        public void AddAndRemoveDerivedNavigationPropertyInDerivedTypeJsonLight(string acceptHeader)
        {
            AcceptHeader = acceptHeader;
            AddAndRemoveDerivedNavigationPropertyInDerivedType();
        }

        [Theory]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        [InlineData("application/json")]
        public void CreateAndDeleteLinkToDerivedNavigationPropertyOnBaseEntitySetJsonLight(string acceptHeader)
        {
            AcceptHeader = acceptHeader;
            CreateAndDeleteLinkToDerivedNavigationPropertyOnBaseEntitySet();
        }
    }
}
