// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
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

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            var conventions = ODataRoutingConventions.CreateDefault();
            configuration.MapODataServiceRoute("CustomActionConventions", "CustomActionConventions", GetCustomActionConventionsModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.AddODataQueryFilter();
        }

        public static IEdmModel GetCustomActionConventionsModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            var baseEntitySet = builder.EntitySet<BaseEntity>("BaseEntity");

            var alwaysAvailableActionBaseType = baseEntitySet.EntityType.Action("AlwaysAvailableActionBaseType");
            Func<ResourceContext, Uri> alwaysAvailableActionBaseTypeLinkFactory = eic =>
            {
                object id;
                eic.EdmObject.TryGetPropertyValue("Id", out id);
                IList<ODataPathSegment> segments = new List<ODataPathSegment>();
                segments.Add(new EntitySetSegment(eic.NavigationSource as IEdmEntitySet));
                segments.Add(new KeySegment(new[] {new KeyValuePair<string, object>("Id", id) }, eic.StructuredType as IEdmEntityType, null));

                var action = eic.EdmModel.SchemaElements
                                .Where(elem => elem.Name == "AlwaysAvailableActionBaseType")
                                .Cast<IEdmAction>()
                                .FirstOrDefault();
                // bug 1985: Make the internal constructor as public in BoundActionPathSegment
                //segments.Add(new BoundActionPathSegment(action));
                var pathHandler = (IODataPathHandler)eic.Request.RequestContainer().GetService(typeof(IODataPathHandler));
                string link = eic.Url.CreateODataLink("CustomActionConventions", pathHandler, segments);
                link += "/" + action.FullName();
                return new Uri(link);
            };
            alwaysAvailableActionBaseType.HasActionLink(alwaysAvailableActionBaseTypeLinkFactory, false);

            var transientActionBaseType = baseEntitySet.EntityType.Action("TransientActionBaseType");
            Func<ResourceContext, Uri> transientActionBaseTypeLinkFactory = eic =>
            {
                IEdmType baseType = eic.EdmModel.FindType(typeof(BaseEntity).FullName);
                object id;
                eic.EdmObject.TryGetPropertyValue("Id", out id);
                if (!eic.StructuredType.IsOrInheritsFrom(baseType) || (int)id % 2 == 1)
                {
                    return null;
                }
                else
                {
                    IList<ODataPathSegment> segments = new List<ODataPathSegment>();
                    segments.Add(new EntitySetSegment(eic.NavigationSource as IEdmEntitySet));
                    segments.Add(new KeySegment(new[] { new KeyValuePair<string, object>("Id", id) }, eic.StructuredType as IEdmEntityType, null));

                    var action = eic.EdmModel.SchemaElements
                                    .Where(elem => elem.Name == "TransientActionBaseType")
                                    .Cast<IEdmAction>()
                                    .FirstOrDefault();
                    // bug 1985: Make the internal constructor as public in BoundActionPathSegment
                    //segments.Add(new BoundActionPathSegment(action));
                    var pathHandler = (IODataPathHandler)eic.Request.RequestContainer().GetService(typeof(IODataPathHandler));
                    string link = eic.Url.CreateODataLink("CustomActionConventions", pathHandler, segments);
                    link += "/" + action.FullName();
                    return new Uri(link);
                }
            };
            transientActionBaseType.HasActionLink(transientActionBaseTypeLinkFactory, false);

            var derivedEntityType = builder.EntityType<DerivedEntity>().DerivesFrom<BaseEntity>();
            var alwaysAvailableActionDerivedType = derivedEntityType.Action("AlwaysAvailableActionDerivedType");
            Func<ResourceContext, Uri> alwaysAvailableActionDerivedTypeLinkFactory = eic =>
            {
                object id;
                eic.EdmObject.TryGetPropertyValue("Id", out id);
                IList<ODataPathSegment> segments = new List<ODataPathSegment>();
                segments.Add(new EntitySetSegment(eic.NavigationSource as IEdmEntitySet));
                segments.Add(new KeySegment(new[] { new KeyValuePair<string, object>("Id", id) }, eic.StructuredType as IEdmEntityType, null));
                segments.Add(new TypeSegment(eic.StructuredType, null));

                var action = eic.EdmModel.SchemaElements
                                .Where(elem => elem.Name == "AlwaysAvailableActionDerivedType")
                                .Cast<IEdmAction>()
                                .FirstOrDefault();
                // bug 1985: Make the internal constructor as public in BoundActionPathSegment
                //segments.Add(new BoundActionPathSegment(action));
                var pathHandler = (IODataPathHandler)eic.Request.RequestContainer().GetService(typeof(IODataPathHandler));
                string link = eic.Url.CreateODataLink("CustomActionConventions", pathHandler, segments);
                link += "/" + action.FullName();
                return new Uri(link);
            };
            alwaysAvailableActionDerivedType.HasActionLink(alwaysAvailableActionDerivedTypeLinkFactory, false);

            var transientActionDerivedType = derivedEntityType.Action("TransientActionDerivedType");
            Func<ResourceContext, Uri> transientActionDerivedTypeLinkFactory = eic =>
            {
                IEdmType derivedType = eic.EdmModel.FindType(typeof(DerivedEntity).FullName);
                object id;
                eic.EdmObject.TryGetPropertyValue("Id", out id);
                if (!eic.StructuredType.IsOrInheritsFrom(derivedType) || (int)id % 2 == 1)
                {
                    return null;
                }
                else
                {
                    IList<ODataPathSegment> segments = new List<ODataPathSegment>();
                    segments.Add(new EntitySetSegment(eic.NavigationSource as IEdmEntitySet));
                    segments.Add(new KeySegment(new[] {new KeyValuePair<string, object>("Id", id)}, eic.StructuredType as IEdmEntityType, null));
                    segments.Add(new TypeSegment(eic.StructuredType, null));

                    var action = eic.EdmModel.SchemaElements
                                    .Where(elem => elem.Name == "TransientActionDerivedType")
                                    .Cast<IEdmAction>()
                                    .FirstOrDefault();
                    // bug 1985: Make the internal constructor as public in BoundActionPathSegment
                    //segments.Add(new BoundActionPathSegment(action));
                    var pathHandler = (IODataPathHandler)eic.Request.RequestContainer().GetService(typeof(IODataPathHandler));
                    string link = eic.Url.CreateODataLink("CustomActionConventions", pathHandler, segments);
                    link += "/" + action.FullName();
                    return new Uri(link);
                }
            };
            transientActionDerivedType.HasActionLink(transientActionDerivedTypeLinkFactory, false);
            return builder.GetEdmModel();
        }

        public static TheoryDataSet<string> AllAcceptHeaders
        {
            get
            {
                return ODataAcceptHeaderTestSet.GetInstance().GetAllAcceptHeaders();
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public async Task AlwaysAvailableCustomActionsGetAlwaysAdvertised(string acceptHeader)
        {
            // Arrange
            string actionName = "AlwaysAvailableActionBaseType";
            string qualifiedActionName = "Default." + actionName;
            string qualifiedContainerName = "#" + qualifiedActionName;
            string targetUrl = BaseAddress + "/CustomActionConventions/BaseEntity(1)/" + qualifiedActionName;

            // Act
            var requestUri = BaseAddress + "/CustomActionConventions/BaseEntity(1)/";
            var response = await Client.GetWithAcceptAsync(requestUri, acceptHeader);
            var result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty(qualifiedContainerName, result);
            }
            else
            {
                JsonAssert.ContainsProperty(qualifiedContainerName, result);
                JObject container = (JObject)result[qualifiedContainerName];

                var actualTargetUrl = container["target"].ToString();
                ODataUrlAssert.UrlEquals(targetUrl, actualTargetUrl, BaseAddress);
                if (acceptHeader.Contains("odata.metadata=full"))
                {
                    JsonAssert.PropertyEquals(actionName, "title", container);
                }
                else
                {
                    JsonAssert.DoesNotContainProperty(actionName, container);
                }
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public async Task TransientCustomActionsGetAdvertisedWithTheTargetWhenAvailable(string acceptHeader)
        {
            // Arrange
            string actionName = "TransientActionBaseType";
            string qualifiedActionName = "Default." + actionName;
            string containerName = "#" + qualifiedActionName;
            string targetUrl = BaseAddress + "/CustomActionConventions/BaseEntity(2)/" + qualifiedActionName;

            // Act
            var requestUri = BaseAddress + "/CustomActionConventions/BaseEntity(2)/";
            var response = await Client.GetWithAcceptAsync(requestUri, acceptHeader);
            var result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty(containerName, result);
            }
            else
            {
                JsonAssert.ContainsProperty(containerName, result);
                JObject container = (JObject)result[containerName];

                var actualTargetUrl = container["target"].ToString();
                ODataUrlAssert.UrlEquals(targetUrl, actualTargetUrl, BaseAddress);

                //JsonAssert.PropertyEquals(targetUrl, "target", container);
                if (acceptHeader.Contains("odata.metadata=full"))
                {
                    JsonAssert.PropertyEquals(actionName, "title", container);
                }
                else
                {
                    JsonAssert.DoesNotContainProperty("title", container);
                }
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public async Task TransientCustomActionsDontGetAdvertisedWhenNotAvailable(string acceptHeader)
        {
            // Arrange
            string qualifiedActionName = "Default.TransientActionBaseType";
            string containerName = "#" + qualifiedActionName;
            string requestUrl = BaseAddress + "/CustomActionConventions/BaseEntity(1)/";

            // Act
            var response = await Client.GetWithAcceptAsync(requestUrl, acceptHeader);
            JObject result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            if (acceptHeader.Contains("odata.metadata=full"))
            {
                JsonAssert.ContainsProperty(containerName, result);
            }
            else
            {
                JsonAssert.DoesNotContainProperty(containerName, result);
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public async Task AlwaysAvailableCustomActionsInDerivedTypesGetAlwaysAdvertised(string acceptHeader)
        {
            // Arrange
            string baseActionName = "AlwaysAvailableActionBaseType";
            string qualifiedBaseActionName = "Default." + baseActionName;
            string baseContainerName = "#" + qualifiedBaseActionName;
            string baseTargetUrl = BaseAddress + "/CustomActionConventions/BaseEntity(8)/" + qualifiedBaseActionName;

            string derivedActionName = "AlwaysAvailableActionDerivedType";
            string qualifiedDerivedActionName = "Default." + derivedActionName;
            string derivedContainerName = "#" + qualifiedDerivedActionName;
            string deriveTargetUrl = string.Format("{0}/CustomActionConventions/BaseEntity(8)/{1}/{2}",
                BaseAddress, typeof(DerivedEntity).FullName, qualifiedDerivedActionName);

            // Act
            var requestUrl = BaseAddress + "/CustomActionConventions/BaseEntity(8)/";
            var response = await Client.GetWithAcceptAsync(requestUrl, acceptHeader);
            var result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty(baseContainerName, result);
                JsonAssert.DoesNotContainProperty(derivedContainerName, result);
            }
            else
            {
                JsonAssert.ContainsProperty(baseContainerName, result);
                JObject baseContainer = (JObject)result[baseContainerName];
                var actualBaseTargetUrl = baseContainer["target"].ToString();
                ODataUrlAssert.UrlEquals(baseTargetUrl, actualBaseTargetUrl, BaseAddress);

                JsonAssert.ContainsProperty(derivedContainerName, result);
                JObject derivedContainer = (JObject)result[derivedContainerName];
                var actualDerivedTargetUrl = derivedContainer["target"].ToString();
                ODataUrlAssert.UrlEquals(deriveTargetUrl, actualDerivedTargetUrl, BaseAddress);

                if (acceptHeader.Contains("odata.metadata=full"))
                {
                    JsonAssert.PropertyEquals(baseActionName, "title", baseContainer);
                    JsonAssert.PropertyEquals(derivedActionName, "title", derivedContainer);
                }
                else
                {
                    JsonAssert.DoesNotContainProperty("title", baseContainer);
                    JsonAssert.DoesNotContainProperty("title", derivedContainer);
                }
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public async Task TransientCustomActionsGetAdvertisedInDerivedTypesWithTheTargetWhenAvailable(string acceptHeader)
        {
            // Arrange
            string baseActionName = "TransientActionBaseType";
            string qualifiedBaseActionName = "Default." + baseActionName;
            string baseContainerName = "#" + qualifiedBaseActionName;
            string baseTargetUrl = BaseAddress + "/CustomActionConventions/BaseEntity(8)/" + qualifiedBaseActionName;

            string derivedActionName = "TransientActionDerivedType";
            string qualifiedDerivedActionName = "Default." + derivedActionName;
            string derivedContainerName = "#" + qualifiedDerivedActionName;
            string derivedTargetUrl = String.Format("{0}/CustomActionConventions/BaseEntity(8)/{1}/{2}",
                BaseAddress, typeof(DerivedEntity).FullName, qualifiedDerivedActionName);

            // Act
            var requestUrl = BaseAddress.ToLowerInvariant() + "/CustomActionConventions/BaseEntity(8)/";
            var response = await Client.GetWithAcceptAsync(requestUrl, acceptHeader);
            var result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty(baseContainerName, result);
                JsonAssert.DoesNotContainProperty(derivedContainerName, result);
            }
            else
            {
                JsonAssert.ContainsProperty(baseContainerName, result);
                JObject container = (JObject)result[baseContainerName];
                var actualBaseTargetUrl = container["target"].ToString();
                ODataUrlAssert.UrlEquals(baseTargetUrl, actualBaseTargetUrl, BaseAddress);

                JsonAssert.ContainsProperty(derivedContainerName, result);
                JObject derivedContainer = (JObject)result[derivedContainerName];
                var actualDerivedTargetUrl = derivedContainer["target"].ToString();
                ODataUrlAssert.UrlEquals(actualDerivedTargetUrl, derivedTargetUrl, BaseAddress);

                if (acceptHeader.Contains("odata.metadata=full"))
                {
                    JsonAssert.PropertyEquals(derivedActionName, "title", derivedContainer);
                    JsonAssert.PropertyEquals(baseActionName, "title", container);
                }
                else
                {
                    JsonAssert.DoesNotContainProperty("title", derivedContainer);
                    JsonAssert.DoesNotContainProperty("title", container);
                }
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public async Task TransientCustomActionsDontGetAdvertisedInDerivedTypesWhenNotAvailable(string acceptHeader)
        {
            // Arrange
            string baseQualifiedActionName = "Default.TransientActionBaseType";
            string baseContainerName = "#" + baseQualifiedActionName;

            string derviedQualifiedActionName = "Default.TransientActionDerivedType";
            string derviedContainerName = "#" + derviedQualifiedActionName;

            // Act
            var requestUrl = BaseAddress + "/CustomActionConventions/BaseEntity(9)/";
            var response = await Client.GetWithAcceptAsync(requestUrl, acceptHeader);
            var result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            if (acceptHeader.Contains("odata.metadata=full"))
            {
                JsonAssert.ContainsProperty(baseContainerName, result);
                JsonAssert.ContainsProperty(derviedContainerName, result);
            }
            else
            {
                JsonAssert.DoesNotContainProperty(baseContainerName, result);
                JsonAssert.DoesNotContainProperty(derviedContainerName, result);
            }

        }
    }
}
