using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class ActionMetadataTests
    {
        private string _baseAddress;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get { return _baseAddress; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.MapODataServiceRoute("Actions", "Actions", GetActionsModel(configuration));
            configuration.AddODataQueryFilter();
        }

        public static IEdmModel GetActionsModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var baseEntitySet = builder.EntitySet<BaseEntity>("BaseEntity");
            var alwaysAvailableActionBaseType = baseEntitySet.EntityType.Action("AlwaysAvailableActionBaseType");
            var transientActionBaseType = baseEntitySet.EntityType.TransientAction("TransientActionBaseType");
            Func<EntityInstanceContext, Uri> transientActionBaseTypeLinkFactory = eic =>
            {
                IEdmEntityType baseType = eic.EdmModel.FindType(typeof(BaseEntity).FullName) as IEdmEntityType;
                object id;
                eic.EdmObject.TryGetPropertyValue("Id", out id);
                if (!eic.EntityType.IsOrInheritsFrom(baseType) || (int)id % 2 == 1)
                {
                    return null;
                }
                else
                {
                    IList<ODataPathSegment> segments = new List<ODataPathSegment>();
                    segments.Add(new EntitySetPathSegment(eic.EntitySet));
                    segments.Add(new KeyValuePathSegment(id.ToString()));
                    segments.Add(new ActionPathSegment("TransientActionBaseType"));
                    string link = eic.Url.CreateODataLink("Actions", eic.Request.ODataProperties().PathHandler, segments);
                    return new Uri(link);
                }
            };

            transientActionBaseType.HasActionLink(transientActionBaseTypeLinkFactory, true);
            var derivedEntityType = builder.Entity<DerivedEntity>().DerivesFrom<BaseEntity>();
            var alwaysAvailableActionDerivedType = derivedEntityType.Action("AlwaysAvailableActionDerivedType");
            var transientActionDerivedType = derivedEntityType.TransientAction("TransientActionDerivedType");
            Func<EntityInstanceContext, Uri> transientActionDerivedTypeLinkFactory = eic =>
            {
                IEdmEntityType derivedType = eic.EdmModel.FindType(typeof(DerivedEntity).FullName) as IEdmEntityType;
                object id;
                eic.EdmObject.TryGetPropertyValue("Id", out id);
                if (!eic.EntityType.IsOrInheritsFrom(derivedType) || (int)id % 2 == 1)
                {
                    return null;
                }
                else
                {
                    IList<ODataPathSegment> segments = new List<ODataPathSegment>();
                    segments.Add(new EntitySetPathSegment(eic.EntitySet));
                    segments.Add(new KeyValuePathSegment(id.ToString()));
                    segments.Add(new CastPathSegment(derivedType.FullName()));
                    segments.Add(new ActionPathSegment("TransientActionDerivedType"));
                    string link = eic.Url.CreateODataLink("Actions", eic.Request.ODataProperties().PathHandler, segments);
                    return new Uri(link);
                }
            };
            transientActionDerivedType.HasActionLink(transientActionDerivedTypeLinkFactory, true);
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void AlwaysAvailableActionsGetAdvertisedOnFullMetadataOnly(string acceptHeader)
        {
            //Arrange
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(1)/";
            string expectedTargetUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(1)/AlwaysAvailableActionBaseType";
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";
            string expectedAlwaysAvailableActionName = "AlwaysAvailableActionBaseType";
            string expectedAlwaysAvailableActionContainer = expectedContainerNamePrefix + expectedAlwaysAvailableActionName;
            JObject container;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.ContainsProperty(expectedAlwaysAvailableActionContainer, result);
                container = (JObject)result[expectedAlwaysAvailableActionContainer];
                JsonAssert.Equal(expectedTargetUrl, "target", container);
                JsonAssert.Equal(expectedAlwaysAvailableActionName, "title", container);
            }
            else
            {
                JsonAssert.DoesNotContainProperty(expectedAlwaysAvailableActionContainer, result);
            }
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void TransientActionsDontGetAdvertisedWhenTheyArentAvailable(string acceptHeader)
        {
            //Arrange
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(1)/";
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";
            string expectedTransientActionName = "TransientActionBaseType";
            string expectedTransientActionContainer = expectedContainerNamePrefix + expectedTransientActionName;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.DoesNotContainProperty(expectedTransientActionContainer, result);
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void TransientActionsGetAdvertisedWhenTheyAreAvailable(string acceptHeader)
        {
            //Arrange
            JObject container;
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(2)/";
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";
            string expectedTransientActionName = "TransientActionBaseType";
            string expectedTransientActionContainer = expectedContainerNamePrefix + expectedTransientActionName;
            string expectedTargetUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(2)/TransientActionBaseType";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.ContainsProperty(expectedTransientActionContainer, result);
            container = (JObject)result[expectedTransientActionContainer];
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(expectedTargetUrl, "target", container);
                JsonAssert.Equal(expectedTransientActionName, "title", container);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("target", container);
                JsonAssert.DoesNotContainProperty("title", container);
            }
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void AlwaysAvailableActionsGetAdvertisedForDerivedTypesOnFullMetadataOnly(string acceptHeader)
        {
            //Arrange
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(9)/";
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";

            string expectedTargetUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(9)/AlwaysAvailableActionBaseType";
            string expectedAlwaysAvailableActionName = "AlwaysAvailableActionBaseType";
            string expectedAlwaysAvailableActionContainer = expectedContainerNamePrefix + expectedAlwaysAvailableActionName;

            string expectedDerivedTypeTargetUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(9)/WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model.DerivedEntity/AlwaysAvailableActionDerivedType";
            string expectedAlwaysAvailableDerivedTypeActionName = "AlwaysAvailableActionDerivedType";
            string expectedAlwaysAvailableDerivedTypeActionContainer = expectedContainerNamePrefix + expectedAlwaysAvailableDerivedTypeActionName;
            JObject container;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.ContainsProperty(expectedAlwaysAvailableActionContainer, result);
                container = (JObject)result[expectedAlwaysAvailableActionContainer];
                JsonAssert.Equal(expectedTargetUrl, "target", container);
                JsonAssert.Equal(expectedAlwaysAvailableActionName, "title", container);

                JsonAssert.ContainsProperty(expectedAlwaysAvailableDerivedTypeActionContainer, result);
                container = (JObject)result[expectedAlwaysAvailableDerivedTypeActionContainer];
                JsonAssert.Equal(expectedDerivedTypeTargetUrl, "target", container);
                JsonAssert.Equal(expectedAlwaysAvailableDerivedTypeActionName, "title", container);
            }
            else
            {
                JsonAssert.DoesNotContainProperty(expectedAlwaysAvailableActionContainer, result);
                JsonAssert.DoesNotContainProperty(expectedAlwaysAvailableDerivedTypeActionContainer, result);
            }
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void TransientActionsDontGetAdvertisedForDerivedTypesWhenTheyArentAvailable(string acceptHeader)
        {
            //Arrange
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(9)/";
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";
            string expectedTransientActionName = "TransientActionBaseType";
            string expectedTransientActionContainer = expectedContainerNamePrefix + expectedTransientActionName;
            string expectedDerivedTypeTransientActionName = "TransientActionDerivedType";
            string expectedDerivedTypeTransientActionContainer = expectedContainerNamePrefix + expectedDerivedTypeTransientActionName;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.DoesNotContainProperty(expectedTransientActionContainer, result);
            JsonAssert.DoesNotContainProperty(expectedDerivedTypeTransientActionContainer, result);
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void TransientActionsGetAdvertisedForDerivedTypesWhenTheyAreAvailable(string acceptHeader)
        {
            //Arrange
            JObject container;
            JObject derivedContainer;
            string requestUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(8)/";
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";
            string expectedTransientActionName = "TransientActionBaseType";
            string expectedTransientActionContainer = expectedContainerNamePrefix + expectedTransientActionName;
            string expectedTargetUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(8)/TransientActionBaseType";

            string expectedDerivedTypeTransientActionName = "TransientActionDerivedType";
            string expectedDerivedTypeTransientActionContainer = expectedContainerNamePrefix + expectedDerivedTypeTransientActionName;
            string expectedDerivedTypeTargetUrl = BaseAddress.ToLowerInvariant() + "/Actions/BaseEntity(8)/WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model.DerivedEntity/TransientActionDerivedType";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.ContainsProperty(expectedTransientActionContainer, result);
            JsonAssert.ContainsProperty(expectedDerivedTypeTransientActionContainer, result);

            container = (JObject)result[expectedTransientActionContainer];
            derivedContainer = (JObject)result[expectedDerivedTypeTransientActionContainer];
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(expectedTargetUrl, "target", container);
                JsonAssert.Equal(expectedTransientActionName, "title", container);

                JsonAssert.Equal(expectedDerivedTypeTargetUrl, "target", derivedContainer);
                JsonAssert.Equal(expectedDerivedTypeTransientActionName, "title", derivedContainer);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("target", container);
                JsonAssert.DoesNotContainProperty("title", container);

                JsonAssert.DoesNotContainProperty("target", container);
                JsonAssert.DoesNotContainProperty("title", container);
            }
        }
    }
}
