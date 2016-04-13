using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class EntryMetadataTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute("Relationships", "Relationships", GetRelationshipsModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("Inheritance", "Inheritance", GetInheritanceModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("CustomNavigationPropertyConventions", "CustomNavigationPropertyConventions", GetCustomNavigationPropertyConventionsModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("CustomReadLinkConventions", "CustomReadLinkConventions", GetCustomReadLinkConventionsModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("CustomEditLinkConventions", "CustomEditLinkConventions", GetCustomEditLinkConventionsModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("CustomIdLinkConventions", "CustomIdLinkConventions", GetCustomIdLinkConventionsModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.AddODataQueryFilter();
        }

        private static IEdmModel GetInheritanceModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var baseEntitySet = builder.EntitySet<BaseEntity>("BaseEntity");
            var derivedEntityType = builder.EntityType<DerivedEntity>().DerivesFrom<BaseEntity>();
            return builder.GetEdmModel();
        }

        private static IEdmModel GetRelationshipsModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var oneToOneParentSet = builder.EntitySet<OneToOneParent>("OneToOneParent");
            var oneToOneChildSet = builder.EntitySet<OneToOneChild>("OneToOneChild");
            oneToOneParentSet.HasOptionalBinding(x => x.Child, "OneToOneChild");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetCustomNavigationPropertyConventionsModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var oneToOneChildSet = builder.EntitySet<OneToOneChild>("OneToOneChild");
            var oneToOneParentSet = builder.EntitySet<OneToOneParent>("OneToOneParent");
            var oneToOneParentEntity = oneToOneParentSet.EntityType;
            NavigationPropertyConfiguration childProperty = oneToOneParentEntity.HasOptional(x => x.Child);
            Func<EntityContext<OneToOneParent>, IEdmNavigationProperty, Uri> linkFactory = (eic, np) => new Uri("http://localhost:50000/CustomNavigationProperty");
            oneToOneParentSet.HasNavigationPropertyLink(childProperty, linkFactory, false);
            return builder.GetEdmModel();
        }

        private static IEdmModel GetCustomReadLinkConventionsModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var oneToOneParentSet = builder.EntitySet<OneToOneParent>("OneToOneParent");
            oneToOneParentSet.EntityType.Ignore(x => x.Child);
            oneToOneParentSet.HasReadLink(eic => new Uri("http://localhost:5000/CustomReadLink"), followsConventions: false);
            return builder.GetEdmModel();
        }

        private static IEdmModel GetCustomEditLinkConventionsModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var oneToOneParentSet = builder.EntitySet<OneToOneParent>("OneToOneParent");
            oneToOneParentSet.EntityType.Ignore(x => x.Child);
            oneToOneParentSet.HasEditLink(eic => new Uri("http://localhost:5000/CustomEditLink"), followsConventions: false);
            return builder.GetEdmModel();
        }

        private static IEdmModel GetCustomIdLinkConventionsModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var oneToOneParentSet = builder.EntitySet<OneToOneParent>("OneToOneParent");
            oneToOneParentSet.EntityType.Ignore(x => x.Child);
            oneToOneParentSet.HasIdLink(eic => new Uri("http://localhost:5000/CustomIdLink"), followsConventions: false);
            return builder.GetEdmModel();
        }

        public static TheoryDataSet<string> AllAcceptHeaders
        {
            get
            {
                var headers = new TheoryDataSet<string>();

                headers.Add("application/json;odata.metadata=full");
                headers.Add("application/json;odata.metadata=full;odata.streaming=true");
                headers.Add("application/json;odata.metadata=full;odata.streaming=false");
                headers.Add("application/json;odata.metadata=minimal");
                headers.Add("application/json;odata.metadata=minimal;odata.streaming=true");
                headers.Add("application/json;odata.metadata=minimal;odata.streaming=false");
                headers.Add("application/json;odata.metadata=none");
                headers.Add("application/json;odata.metadata=none;odata.streaming=true");
                headers.Add("application/json;odata.metadata=none;odata.streaming=false");
                headers.Add("application/json");
                headers.Add("application/json;odata.streaming=true");
                headers.Add("application/json;odata.streaming=false");

                return headers;
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public void ODataTypeAnnotationAppearsForAllEntitiesInFullMetadataAndForDerivedEntityTypesInFullAndMinimalMetadata(
            string acceptHeader)
        {
            //Arrange
            BaseEntity[] baseEntities = new BaseEntity[] { new BaseEntity(1), new BaseEntity(2), new BaseEntity(3), new BaseEntity(4), new BaseEntity(5), new BaseEntity(6), new BaseEntity(7) }; //InstanceCreator.CreateInstanceOf<BaseEntity[]>(new Random(RandomSeedGenerator.GetRandomSeed()));
            DerivedEntity[] derivedEntities = new DerivedEntity[] { new DerivedEntity(8), new DerivedEntity(9), new DerivedEntity(10), new DerivedEntity(11), new DerivedEntity(12), new DerivedEntity(13), new DerivedEntity(14) };  //InstanceCreator.CreateInstanceOf<DerivedEntity[]>(new Random(RandomSeedGenerator.GetRandomSeed()));
            BaseEntity[] entities = baseEntities.Union(derivedEntities, new BaseEntity.IdEqualityComparer()).ToArray();
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Inheritance/BaseEntity/";
            string expectedTypeName;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;

            //Assert
            JArray returnedEntities = (JArray)result["value"];
            for (int i = 0; i < returnedEntities.Count; i++)
            {
                JObject returnedEntity = (JObject)returnedEntities[i];
                BaseEntity originalEntity = entities.FirstOrDefault(x => x.Id == (int)returnedEntity["Id"]);
                Assert.NotNull(originalEntity);
                if (acceptHeader.Contains("odata.metadata=none"))
                {
                    JsonAssert.DoesNotContainProperty("@odata.type", returnedEntity);
                }
                else if (acceptHeader.Contains("odata.metadata=full"))
                {
                    expectedTypeName = "#" + originalEntity.GetType().FullName;
                    JsonAssert.PropertyEquals(expectedTypeName, "@odata.type", returnedEntity);
                }
                else
                {
                    if (originalEntity is DerivedEntity)
                    {
                        expectedTypeName = "#" + originalEntity.GetType().FullName;
                        JsonAssert.PropertyEquals(expectedTypeName, "@odata.type", returnedEntity);
                    }
                    else
                    {
                        JsonAssert.DoesNotContainProperty("@odata.type", returnedEntity);
                    }
                }
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public void NavigationLinksAppearOnlyInFullMetadata(string acceptHeader)
        {
            //Arrange
            OneToOneParent[] entities = MetadataTestHelpers.CreateInstances<OneToOneParent[]>();
            var entity = entities.First();
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Relationships/OneToOneParent(" + entity.Id + ")";
            string expectedNavigationLink = BaseAddress.ToLowerInvariant() + "/Relationships/OneToOneParent(" + entity.Id + ")/Child";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;

            //Assert
            if (acceptHeader.Contains("odata.metadata=full"))
            {
                JsonAssert.PropertyEquals(expectedNavigationLink, "Child@odata.navigationLink", result);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("Child@odata.navigationLinkUrl", result);
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public void CustomEditLinksAppearInFullAndMinimalMetadata(string acceptHeader)
        {
            //Arrange
            OneToOneParent[] entities = MetadataTestHelpers.CreateInstances<OneToOneParent[]>();
            var entity = entities.First();
            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomEditLinkConventions/OneToOneParent(" + entity.Id + ")";
            string expectedEditLinkUrl = "http://localhost:5000/CustomEditLink";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/CustomEditLinkConventions/OneToOneParent(" + entity.Id + ")");
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;

            //Assert
            if (!acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.PropertyEquals(expectedEditLinkUrl, "@odata.editLink", result);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("@odata.editLink", result);
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public void CustomIdLinksAppearInFullAndMinimalMetadata(string acceptHeader)
        {
            //Arrange
            OneToOneParent[] entities = MetadataTestHelpers.CreateInstances<OneToOneParent[]>();
            var entity = entities.First();
            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomIdLinkConventions/OneToOneParent(" + entity.Id + ")";
            string expectedEditLinkUrl = "http://localhost:5000/CustomIdLink";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/CustomIdLinkConventions/OneToOneParent(" + entity.Id + ")");
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;

            //Assert
            if (!acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.PropertyEquals(expectedEditLinkUrl, "@odata.id", result);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("@odata.id", result);
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public void CustomReadLinksAppearInFullAndMinimalMetadata(string acceptHeader)
        {
            //Arrange
            OneToOneParent[] entities = MetadataTestHelpers.CreateInstances<OneToOneParent[]>();
            var entity = entities.First();
            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomReadLinkConventions/OneToOneParent(" + entity.Id + ")";
            string expectedReadLinkUrl = "http://localhost:5000/CustomReadLink";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/CustomReadLinkConventions/OneToOneParent(" + entity.Id + ")");
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;

            //Assert
            if (!acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.PropertyEquals(expectedReadLinkUrl, "@odata.readLink", result);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("@odata.readLink", result);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application/json;odata.metadata=full;odata.streaming=false")]
        public void CanFollowGeneratedNavigationLinks(string acceptHeader)
        {
            //Arrange
            OneToOneParent[] entities = MetadataTestHelpers.CreateInstances<OneToOneParent[]>();
            OneToOneChild[] childEntities = entities.Select(x => x.Child).ToArray();
            JArray returnedChildEntities = new JArray();
            JArray returnedParentEntities;
            int[] returnedChildrenIdentities;
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Relationships/OneToOneParent";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsAsync<JObject>().Result;
            returnedParentEntities = (JArray)result["value"];
            for (int i = 0; i < returnedParentEntities.Count; i++)
            {
                string childUrl = (string)returnedParentEntities[i]["Child@odata.navigationLink"];
                HttpRequestMessage childRequest = new HttpRequestMessage(HttpMethod.Get, childUrl);
                HttpResponseMessage childResponse = Client.SendAsync(childRequest).Result;
                JObject childEntry = childResponse.Content.ReadAsAsync<JObject>().Result;
                returnedChildEntities.Add(childEntry);
            }
            returnedChildrenIdentities = returnedChildEntities.Select(x => (int)x["Id"]).ToArray();

            //Assert
            foreach (var returnedChildEntityId in returnedChildrenIdentities)
            {
                Assert.True(childEntities.Any(x => x.Id == returnedChildEntityId));
            }
        }
    }
}
