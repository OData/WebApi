using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Common
{
    public static class ODataTestExtension
    {
        public static void ClearRepository(this IODataTestBase test, string entityName)
        {
            test.Client.DeleteAsync(test.BaseAddress + "/api/" + entityName + "/Delete").Wait();
        }

        public static void EnableODataSupport(this HttpConfiguration configuration, IEdmModel model, string routePrefix)
        {
            //TODO: how to customize OData path handler?
            //configuration.SetODataPathHandler(new AzureODataPathHandler());

            var conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new PropertyRoutingConvention());
            conventions.Insert(0, new NavigationRoutingConvention2());
            //conventions.Insert(0, new LinkRoutingConvention2());

            configuration.MapODataServiceRoute(
                ODataTestConstants.DefaultRouteName, 
                routePrefix, model, 
                new DefaultODataPathHandler(), 
                conventions);

            configuration.AddODataQueryFilter();
        }

        public static void EnableODataSupport(this HttpConfiguration configuration, IEdmModel model)
        {
            configuration.EnableODataSupport(model, routePrefix: null);
        }

        public static string EntitySetLink(this IODataPathHandler parser, string entitySet, object id)
        {
            ODataPath path = new ODataPath(
                new EntitySetPathSegment(entitySet),
                new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(id, Microsoft.OData.Core.ODataVersion.V4)));
            return parser.Link(path);
        }

        public static string NavigationLink(this IODataPathHandler parser, string entitySet, object key, IEdmNavigationProperty navigationProperty)
        {
            ODataPath path = new ODataPath(
                new EntitySetPathSegment(entitySet),
                new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(key, Microsoft.OData.Core.ODataVersion.V4)),
                new NavigationPathSegment(navigationProperty));

            return parser.Link(path);
        }

        /// <summary>
        /// Helper method to get the odata path for an arbitrary odata uri.
        /// </summary>
        /// <param name="request">The request instance in current context</param>
        /// <param name="uri">OData uri</param>
        /// <returns>The parsed odata path</returns>
        public static ODataPath CreateODataPath(this HttpRequestMessage request, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var newRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            var route = request.GetRouteData().Route;

            var newRoute = new HttpRoute(
                route.RouteTemplate,
                new HttpRouteValueDictionary(route.Defaults),
                new HttpRouteValueDictionary(route.Constraints),
                new HttpRouteValueDictionary(route.DataTokens),
                route.Handler);
            var routeData = newRoute.GetRouteData(request.GetConfiguration().VirtualPathRoot, newRequest);
            if (routeData == null)
            {
                throw new InvalidOperationException("The link is not a valid odata link.");
            }

            return newRequest.ODataProperties().Path;
        }

        /// <summary>
        /// Helper method to get the key value from a uri.
        /// Usually used by $link action to extract the key value from the url in body.
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <param name="request">The request instance in current context</param>
        /// <param name="uri">OData uri that contains the key value</param>
        /// <returns>The key value</returns>
        public static TKey GetKeyValue<TKey>(this HttpRequestMessage request, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            //get the odata path Ex: ~/entityset/key/$links/navigation
            var odataPath = request.CreateODataPath(uri);
            var keySegment = odataPath.Segments.OfType<KeyValuePathSegment>().FirstOrDefault();
            if (keySegment == null)
            {
                throw new InvalidOperationException("The link does not contain a key.");
            }

            var value = ODataUriUtils.ConvertFromUriLiteral(keySegment.Value, Microsoft.OData.Core.ODataVersion.V4);
            return (TKey)value;
        }

        /// <summary>
        /// Convert model state errors into string value.
        /// </summary>
        /// <param name="modelState">Model state</param>
        /// <returns>String value which contains all model errors</returns>
        public static string GetModelStateErrorInformation(ModelStateDictionary modelState)
        {
            StringBuilder errorMessageBuilder = new StringBuilder();
            errorMessageBuilder.AppendLine("Invalid request received.");

            if (modelState != null)
            {
                foreach (var key in modelState.Keys)
                {
                    if (modelState[key].Errors.Count > 0)
                    {
                        errorMessageBuilder.AppendLine(key + ":" + ((modelState[key].Value != null) ? modelState[key].Value.RawValue : "null"));
                    }
                }
            }

            return errorMessageBuilder.ToString();
        }
    }

    public class EntityTypeConstraint : IHttpRouteConstraint
    {
        public bool Match(System.Net.Http.HttpRequestMessage request, IHttpRoute route, string parameterName, System.Collections.Generic.IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (routeDirection == HttpRouteDirection.UriGeneration)
            {
                return true;
            }

            return ((string)values[parameterName]).Contains(".");
        }
    }
}
