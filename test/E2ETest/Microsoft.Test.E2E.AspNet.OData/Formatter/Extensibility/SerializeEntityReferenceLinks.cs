//-----------------------------------------------------------------------------
// <copyright file="SerializeEntityReferenceLinks.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.Extensibility
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

    public class ParentEntityController : TestODataController
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

        public ITestActionResult GetLinksForChildren(int key)
        {
            IEdmModel model = Request.GetModel();
            IEdmEntitySet childEntity = model.EntityContainer.FindEntitySet("ChildEntity");

            return Ok(PARENT_ENTITY.Children.Select(x => Url.CreateODataLink(
                    new EntitySetSegment(childEntity),
                    new KeySegment(new[] { new KeyValuePair<string, object>("Id", x.Id) }, childEntity.EntityType(), null)
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

            if (graph != null)
            {
                Uri[] uris = GetUris(graph);

                messageWriter.WriteEntityReferenceLinks(new ODataEntityReferenceLinks
                {
                    Links = uris.Select(uri => new ODataEntityReferenceLink { Url = uri })
                });
            }
        }

        public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw new ArgumentNullException("messageWriter");
            }

            if (graph != null)
            {
                Uri[] uris = GetUris(graph);
                await messageWriter.WriteEntityReferenceLinksAsync(new ODataEntityReferenceLinks
                {
                    Links = uris.Select(uri => new ODataEntityReferenceLink { Url = uri })
                });
            }
        }

        private static Uri[] GetUris(object graph)
        {
            Uri[] uris = graph as Uri[];
            if (uris == null)
            {
                throw new SerializationException("Cannot write the type");
            }

            return uris;
        }
    }

    public class CustomODataSerializerProvider : DefaultODataSerializerProvider
    {
        public CustomODataSerializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
        }

#if NETCORE
        public override ODataSerializer GetODataPayloadSerializer(Type type, Microsoft.AspNetCore.Http.HttpRequest request)
#else
        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequestMessage request)
#endif
        {
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
            return base.GetODataPayloadSerializer(type, request);
        }
    }

    public class GetRefRoutingConvention : RefRoutingConvention
    {
#if NETCORE
        /// <inheritdoc/>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public override string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            string requestMethod = routeContext.HttpContext.Request.Method;
            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            TestControllerContext context = new TestControllerContext(routeContext);
            IList<string> actionMap = actionDescriptors.Select(a => a.ActionName).Distinct().ToList();

#else
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            string requestMethod = controllerContext.Request.Method.ToString().ToUpperInvariant();
            TestControllerContext context = new TestControllerContext(controllerContext);
#endif
            if (odataPath.PathTemplate == "~/entityset/key/navigation/$ref" && requestMethod == "GET")
            {
                KeySegment keyValueSegment = odataPath.Segments[1] as KeySegment;
                context.AddKeyValueToRouteData(keyValueSegment);
                NavigationPropertyLinkSegment navigationLinkSegment = odataPath.Segments[2] as NavigationPropertyLinkSegment;
                IEdmNavigationProperty navigationProperty = navigationLinkSegment.NavigationProperty;
                IEdmEntityType declaredType = navigationProperty.DeclaringType as IEdmEntityType;

                string action = requestMethod + "LinksFor" + navigationProperty.Name + "From" + declaredType.Name;
                return actionMap.Contains(action) ? action : requestMethod + "LinksFor" + navigationProperty.Name;
            }

#if NETCORE
            return base.SelectAction(routeContext, controllerResult, actionDescriptors);
#else
            return base.SelectAction(odataPath, controllerContext, actionMap);
#endif
        }
    }

    public class SerializeEntityReferenceLinksTest : WebHostTestBase
    {
        public SerializeEntityReferenceLinksTest(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var routingConventions = ODataRoutingConventions.CreateDefault();
            routingConventions.Insert(4, new GetRefRoutingConvention());
            configuration.MapODataServiceRoute("EntityReferenceLinks", "EntityReferenceLinks", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => GetEdmModel(configuration))
                       .AddService(ServiceLifetime.Singleton, sp => new DefaultODataPathHandler())
                       .AddService(ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable())
                       .AddService(ServiceLifetime.Singleton, sp => new CustomODataSerializerProvider(new MockContainer())));
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            var parentSet = builder.EntitySet<ParentEntity>("ParentEntity");
            var childSet = builder.EntitySet<ChildEntity>("ChildEntity");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanExtendTheFormatterToSerializeEntityReferenceLinks()
        {
            string requestUrl = BaseAddress + "/EntityReferenceLinks/ParentEntity(1)/Children/$ref";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = await Client.SendAsync(message);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            JsonAssert.ArrayLength(10, "value", result);
        }

        private class MockContainer : IServiceProvider
        {
            private readonly ODataSerializerProvider serializerProvider;
            private readonly ODataCollectionSerializer collectionSerializer;
            private readonly ODataPrimitiveSerializer primitiveSerializer;

            public MockContainer()
            {
                serializerProvider = new DefaultODataSerializerProvider(this);
                collectionSerializer = new ODataCollectionSerializer(serializerProvider);
                primitiveSerializer = new ODataPrimitiveSerializer();
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ODataSerializerProvider))
                {
                    return serializerProvider;
                }

                if (serviceType == typeof(ODataCollectionSerializer))
                {
                    return collectionSerializer;
                }

                if (serviceType == typeof(ODataPrimitiveSerializer))
                {
                    return primitiveSerializer;
                }

                throw new NotImplementedException();
            }
        }
    }
}
