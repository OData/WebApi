using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
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
    public class CustomConventionActionMetadataTests
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

            var conventions = ODataRoutingConventions.CreateDefault();
            configuration.Routes.MapODataServiceRoute("CustomActionConventions", "CustomActionConventions", GetCustomActionConventionsModel(configuration));
            configuration.AddODataQueryFilter();
        }

        public static IEdmModel GetCustomActionConventionsModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var baseEntitySet = builder.EntitySet<BaseEntity>("BaseEntity");

            var alwaysAvailableActionBaseType = baseEntitySet.EntityType.Action("AlwaysAvailableActionBaseType");
            Func<EntityInstanceContext, Uri> alwaysAvailableActionBaseTypeLinkFactory = eic =>
            {
                object id;
                eic.EdmObject.TryGetPropertyValue("Id",out id);
                IList<ODataPathSegment> segments = new List<ODataPathSegment>();
                segments.Add(new EntitySetPathSegment(eic.EntitySet));
                segments.Add(new KeyValuePathSegment(id.ToString()));
                segments.Add(new ActionPathSegment("AlwaysAvailableActionBaseType"));
                string link = eic.Url.CreateODataLink("CustomActionConventions", eic.Request.ODataProperties().PathHandler, segments);
                return new Uri(link);
            };
            alwaysAvailableActionBaseType.HasActionLink(alwaysAvailableActionBaseTypeLinkFactory, false);

            var transientActionBaseType = baseEntitySet.EntityType.TransientAction("TransientActionBaseType");
            Func<EntityInstanceContext, Uri> transientActionBaseTypeLinkFactory = eic =>
            {
                IEdmType baseType = eic.EdmModel.FindType(typeof(BaseEntity).FullName);
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
                    string link = eic.Url.CreateODataLink("CustomActionConventions", eic.Request.ODataProperties().PathHandler, segments);
                    return new Uri(link);
                }
            };
            transientActionBaseType.HasActionLink(transientActionBaseTypeLinkFactory, false);

            var derivedEntityType = builder.Entity<DerivedEntity>().DerivesFrom<BaseEntity>();
            var alwaysAvailableActionDerivedType = derivedEntityType.Action("AlwaysAvailableActionDerivedType");
            Func<EntityInstanceContext, Uri> alwaysAvailableActionDerivedTypeLinkFactory = eic =>
            {
                string entityName = eic.EntityType.FullName();
                object id;
                eic.EdmObject.TryGetPropertyValue("Id", out id);
                IList<ODataPathSegment> segments = new List<ODataPathSegment>();
                segments.Add(new EntitySetPathSegment(eic.EntitySet));
                segments.Add(new KeyValuePathSegment(id.ToString()));
                segments.Add(new CastPathSegment(entityName));
                segments.Add(new ActionPathSegment("AlwaysAvailableActionDerivedType"));
                string link = eic.Url.CreateODataLink("CustomActionConventions", eic.Request.ODataProperties().PathHandler, segments);
                return new Uri(link);
            };
            alwaysAvailableActionDerivedType.HasActionLink(alwaysAvailableActionDerivedTypeLinkFactory, false);

            var transientActionDerivedType = derivedEntityType.TransientAction("TransientActionDerivedType");
            Func<EntityInstanceContext, Uri> transientActionDerivedTypeLinkFactory = eic =>
            {
                IEdmType derivedType = eic.EdmModel.FindType(typeof(DerivedEntity).FullName);
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
                    segments.Add(new CastPathSegment(eic.EntityType.FullName()));
                    segments.Add(new ActionPathSegment("TransientActionDerivedType"));
                    string link = eic.Url.CreateODataLink("CustomActionConventions", eic.Request.ODataProperties().PathHandler, segments);
                    return new Uri(link);
                }
            };
            transientActionDerivedType.HasActionLink(transientActionDerivedTypeLinkFactory, false);
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
        public void AlwaysAvailableCustomActionsGetAlwaysAdvertised(string acceptHeader)
        {
            //Arrange
            JObject container;
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";

            string expectedTargetUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(1)/AlwaysAvailableActionBaseType";
            string expectedActionName = "AlwaysAvailableActionBaseType";
            string expectedContainerName = expectedContainerNamePrefix + expectedActionName;
            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(1)/";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.ContainsProperty(expectedContainerName, result);
            container = (JObject)result[expectedContainerName];
            JsonAssert.Equal(expectedTargetUrl, "target", container);
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(expectedActionName, "title", container);
            }
            else
            {
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
        public void TransientCustomActionsGetAdvertisedWithTheTargetWhenAvailable(string acceptHeader)
        {
            //Arrange
            JObject container;
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";

            string expectedTargetUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(2)/TransientActionBaseType";
            string expectedActionName = "TransientActionBaseType";
            string expectedContainerName = expectedContainerNamePrefix + expectedActionName;
            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(2)/";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.ContainsProperty(expectedContainerName, result);
            container = (JObject)result[expectedContainerName];
            JsonAssert.Equal(expectedTargetUrl, "target", container);
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(expectedActionName, "title", container);
            }
            else
            {
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
        public void TransientCustomActionsDontGetAdvertisedWhenNotAvailable(string acceptHeader)
        {
            //Arrange
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";
            string expectedActionName = "TransientActionBaseType";
            string expectedContainerName = expectedContainerNamePrefix + expectedActionName;
            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(1)/";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.DoesNotContainProperty(expectedContainerName, result);
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
        public void AlwaysAvailableCustomActionsInDerivedTypesGetAlwaysAdvertised(string acceptHeader)
        {
            //Arrange
            JObject container;
            JObject derivedContainer;
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";

            string expectedTargetUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(8)/AlwaysAvailableActionBaseType";
            string expectedActionName = "AlwaysAvailableActionBaseType";
            string expectedContainerName = expectedContainerNamePrefix + expectedActionName;

            string expectedDerivedTypeTargetUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(8)/WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model.DerivedEntity/AlwaysAvailableActionDerivedType";
            string expectedDerivedTypeActionName = "AlwaysAvailableActionDerivedType";
            string expectedDerivedTypeContainerName = expectedContainerNamePrefix + expectedDerivedTypeActionName;

            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(8)/";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.ContainsProperty(expectedContainerName, result);
            container = (JObject)result[expectedContainerName];
            JsonAssert.Equal(expectedTargetUrl, "target", container);
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(expectedActionName, "title", container);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("title", container);
            }

            JsonAssert.ContainsProperty(expectedDerivedTypeContainerName, result);
            derivedContainer = (JObject)result[expectedDerivedTypeContainerName];
            JsonAssert.Equal(expectedDerivedTypeTargetUrl, "target", derivedContainer);
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(expectedDerivedTypeActionName, "title", derivedContainer);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("title", derivedContainer);
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
        public void TransientCustomActionsGetAdvertisedInDerivedTypesWithTheTargetWhenAvailable(string acceptHeader)
        {
            //Arrange
            JObject container;
            JObject derivedContainer;
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";

            string expectedTargetUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(8)/TransientActionBaseType";
            string expectedActionName = "TransientActionBaseType";
            string expectedContainerName = expectedContainerNamePrefix + expectedActionName;

            string expectedDerivedTypeTargetUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(8)/WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model.DerivedEntity/TransientActionDerivedType";
            string expectedDerivedTypeActionName = "TransientActionDerivedType";
            string expectedDerivedTypeContainerName = expectedContainerNamePrefix + expectedDerivedTypeActionName;

            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(8)/";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.ContainsProperty(expectedContainerName, result);
            container = (JObject)result[expectedContainerName];
            JsonAssert.Equal(expectedTargetUrl, "target", container);
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(expectedActionName, "title", container);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("title", container);
            }

            JsonAssert.ContainsProperty(expectedDerivedTypeContainerName, result);
            derivedContainer = (JObject)result[expectedDerivedTypeContainerName];
            JsonAssert.Equal(expectedDerivedTypeTargetUrl, "target", derivedContainer);
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(expectedDerivedTypeActionName, "title", derivedContainer);
            }
            else
            {
                JsonAssert.DoesNotContainProperty("title", derivedContainer);
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
        public void TransientCustomActionsDontGetAdvertisedInDerivedTypesWhenNotAvailable(string acceptHeader)
        {
            //Arrange
            string expectedContainerNamePrefix = acceptHeader.Contains("fullmetadata") ? "#Container." : "#";

            string expectedActionName = "TransientActionBaseType";
            string expectedContainerName = expectedContainerNamePrefix + expectedActionName;

            string expectedDerivedTypeActionName = "TransientActionDerivedType";
            string expectedDerivedTypeContainerName = expectedContainerNamePrefix + expectedDerivedTypeActionName;

            string requestUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(9)/";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(request).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.DoesNotContainProperty(expectedContainerName, result);
            JsonAssert.DoesNotContainProperty(expectedDerivedTypeContainerName, result);
        }
    }
}
