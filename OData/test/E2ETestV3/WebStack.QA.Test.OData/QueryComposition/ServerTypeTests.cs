using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.OData;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class ServerTypeTests
    {
        private static ODataModelTypeCreator creator = null;
        public static ODataModelTypeCreator Creator
        {
            get
            {
                if (creator == null)
                {
                    creator = new ODataModelTypeCreator();
                    creator.CreateTypes(100, new Random(RandomSeedGenerator.GetRandomSeed()));
                }
                return creator;
            }
        }

        public static TheoryDataSet<Type, string> TypeData
        {
            get
            {
                var data = new TheoryDataSet<Type, string>();
                foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Union(Creator.Assembly.GetTypes()))
                {
                    if (type.IsAbstract
                        //|| (type.BaseType != null && type.BaseType.BaseType != null)
                        || type.GetConstructor(Type.EmptyTypes) == null
                        || type.IsGenericTypeDefinition
                        || type.FullName == "System.__Canon"
                        || type.Name == "IdentityReferenceCollection"
                        || type == typeof(EventArgs)
                        //|| string.IsNullOrEmpty(type.Namespace)
                        || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        || type == typeof(object))
                    {
                        continue;
                    }

                    var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                    if (props.Any(p => 
                        p.GetGetMethod() == null 
                        || p.GetSetMethod() == null
                        || !p.GetGetMethod().IsPublic
                        || !p.GetSetMethod().IsPublic))
                    {
                        continue;
                    }

                    var names = new List<string>();
                    bool valid = true;
                    foreach (var p in props)
                    {
                        if (names.Contains(p.Name))
                        {
                            valid = false;
                            break;
                        }
                        names.Add(p.Name);
                    }

                    if (!valid)
                    {
                        continue;
                    }

                    data.Add(type, BuildFilterString(type));
                }

                return data;
            }
        }

        private static string BuildFilterString(Type type)
        {
            var sb = new StringBuilder();
            sb.Append("$top=1&$skip=1");

            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty).Where(p =>
                (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType.IsEnum)
                && p.GetSetMethod() != null
                && p.PropertyType != typeof(UIntPtr)
                && p.PropertyType != typeof(IntPtr)
                && p.GetGetMethod() != null
                && !p.PropertyType.IsNested
                && p.GetGetMethod().IsPublic
                && (p.GetIndexParameters() == null || p.GetIndexParameters().Length == 0));
            var attrs = type.GetCustomAttributes(typeof(DataContractAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                props = props.Where(p => p.GetCustomAttributes(typeof(DataMemberAttribute), false) != null
                    && p.GetCustomAttributes(typeof(DataMemberAttribute), false).Length > 0);
            }
            props = props.Where(p => p.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), false) == null);

            if (props.Count() > 0)
            {
                props = props.Take(10);
                sb.Append("&$orderby=");
                sb.Append(string.Join(",", props.Select(p => p.Name)));

                sb.Append("&$filter=");
                sb.Append(string.Join(" and ", props.Select(p => p.Name + " eq " + p.Name)));
            }

            return sb.ToString();
        }

        [Theory(Skip = "Not stable")]
        //[Theory]
        [PropertyData("TypeData")]
        public void RunQueryableOnAllPossibleTypes(Type type, string queryString)
        {
            int seed = RandomSeedGenerator.GetRandomSeed();
            Random r = new Random(seed);
            Type generic = typeof(IEnumerable<>);
            var collectionType = generic.MakeGenericType(type);

            Type listType = typeof(List<>).MakeGenericType(type);
            var array = Activator.CreateInstance(listType);

            EnableQueryAttribute q = new EnableQueryAttribute();
            var configuration = new HttpConfiguration();
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.MapHttpRoute("ApiDefault", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test/api/Objects?" + queryString);
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, configuration);
            var controllerContext = new HttpControllerContext(
                configuration,
                configuration.Routes.GetRouteData(request),
                request);
            var actionContext = new HttpActionContext(controllerContext, new ReflectedHttpActionDescriptor()
            {
                Configuration = configuration
            });
            var context = new HttpActionExecutedContext(actionContext, null);
            context.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            context.Response.Content = new ObjectContent(collectionType, array, new JsonMediaTypeFormatter());

            q.OnActionExecuted(context);

            Console.WriteLine(context.Response.Content.ReadAsStringAsync().Result);
            Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
        }
    }
}
