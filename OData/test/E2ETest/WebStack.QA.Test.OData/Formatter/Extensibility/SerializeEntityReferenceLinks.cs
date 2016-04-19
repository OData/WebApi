using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace WebStack.QA.Test.OData.Formatter.Extensibility
{
    public class ParentEntity
    {
        public int Id { get; set; }
        public IList<ChildEntity> Children { get; set; }
    }

    public class ChildEntity
    {
        public int Id { get; set; }
    }

    public class ParentEntityController : ODataController
    {
        private static readonly ParentEntity PARENT_ENTITY;

        static ParentEntityController()
        {
            PARENT_ENTITY = new ParentEntity
            {
                Id = 1,
                Children = Enumerable.Range(1, 10).Select(x => new ChildEntity { Id = x }).ToList()
            };
        }

        public HttpResponseMessage GetLinksForChildren(int key)
        {
            IEdmModel model = Request.ODataProperties().Model;
            IEdmEntitySet childEntity = model.EntityContainer.FindEntitySet("ChildEntity");

            return Request.CreateResponse(HttpStatusCode.OK,
                PARENT_ENTITY.Children.Select(x => Url.CreateODataLink(
                    new EntitySetSegment(childEntity),
                    new KeySegment(new[] { new KeyValuePair<string, object>("Id", x.Id)}, childEntity.EntityType(), null)
                )).ToArray());
        }
    }

    public class ODataEntityReferenceLinksSerializer : ODataSerializer
    {
        public ODataEntityReferenceLinksSerializer()
            : base(ODataPayloadKind.EntityReferenceLinks)
        {

        }

        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw new ArgumentNullException("messageWriter");
            }
            if (writeContext == null)
            {
                throw new ArgumentNullException("writeContext");
            }

            if (graph != null)
            {
                Uri[] uris = graph as Uri[];
                if (uris == null)
                {
                    throw new SerializationException("Cannot write the type");
                }

                messageWriter.WriteEntityReferenceLinks(new ODataEntityReferenceLinks
                {
                    Links = uris.Select(uri => new ODataEntityReferenceLink { Url = uri })
                });
            }
        }
    }

    public class CustomODataSerializerProvider : DefaultODataSerializerProvider
    {
        public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type, HttpRequestMessage request)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (type == typeof(Uri[]))
            {
                return new ODataEntityReferenceLinksSerializer();
            }
            return base.GetODataPayloadSerializer(model, type, request);
        }
    }

    public class GetRefRoutingConvention : RefRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath == null)
            {
                throw new ArgumentNullException("odataPath");
            }

            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            if (actionMap == null)
            {
                throw new ArgumentNullException("actionMap");
            }

            HttpMethod requestMethod = controllerContext.Request.Method;
            if (odataPath.PathTemplate == "~/entityset/key/navigation/$ref" && requestMethod == HttpMethod.Get)
            {
                KeySegment keyValueSegment = odataPath.Segments[1] as KeySegment;
                controllerContext.AddKeyValueToRouteData(keyValueSegment);
                NavigationPropertyLinkSegment navigationLinkSegment = odataPath.Segments[2] as NavigationPropertyLinkSegment;
                IEdmNavigationProperty navigationProperty = navigationLinkSegment.NavigationProperty;
                IEdmEntityType declaredType = navigationProperty.DeclaringType as IEdmEntityType;

                string action = requestMethod + "LinksFor" + navigationProperty.Name + "From" + declaredType.Name;
                return actionMap.Contains(action) ? action : requestMethod + "LinksFor" + navigationProperty.Name;
            }
            return base.SelectAction(odataPath, controllerContext, actionMap);
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class SerializeEntityReferenceLinksTest
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
            configuration.Formatters.InsertRange(0, ODataMediaTypeFormatters.Create(new CustomODataSerializerProvider(), new DefaultODataDeserializerProvider()));
            var routingConventions = ODataRoutingConventions.CreateDefault();
            routingConventions.Insert(4, new GetRefRoutingConvention());
            configuration.MapODataServiceRoute(
                "EntityReferenceLinks",
                "EntityReferenceLinks",
                GetEdmModel(configuration), new DefaultODataPathHandler(), routingConventions);
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(configuration);
            var parentSet = builder.EntitySet<ParentEntity>("ParentEntity");
            var childSet = builder.EntitySet<ChildEntity>("ChildEntity");

            return builder.GetEdmModel();
        }

        [Fact]
        public void CanExtendTheFormatterToSerializeEntityReferenceLinks()
        {
            string requestUrl = BaseAddress + "/EntityReferenceLinks/ParentEntity(1)/Children/$ref";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = Client.SendAsync(message).Result;
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            JsonAssert.ArrayLength(10, "value", result);
        }
    }
}
