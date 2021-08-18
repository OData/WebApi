//-----------------------------------------------------------------------------
// <copyright file="ServerTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Microsoft.Test.E2E.AspNet.OData.Common.TypeCreator;
#else
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Microsoft.Test.E2E.AspNet.OData.Common.TypeCreator;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
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
                IEnumerable<Type> types = Creator.Assembly.GetTypes();
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.Contains("LTAF.Infrastructure"))
                    {
                        // If this assembly is not ignored, then below errors will occur on CI on IIS host:
                        // Method 'AddImport' in type 'LTAF.Infrastructure.NuGetWebProjectSystem' from assembly 'LTAF.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' does not have an implementation.
                        // Method 'ResolveFileConflict' in type 'ErrorLogger' from assembly 'LTAF.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' does not have an implementation.
                        continue;
                    }
                    if (assembly.FullName.Contains("xunit.runner"))
                    {
                        // If this assembly is not ignored, then below errors will occur:
                        // System.Reflection.ReflectionTypeLoadException : Unable to load one or more of the requested types. 
                        // Retrieve the LoaderExceptions property for more information.
                        // when loading: microsoft.visualstudio.testplatform.objectmodel
                        continue;
                    }

                    try
                    {
                        types = types.Union(assembly.GetTypes());
                    }
                    catch (Exception ex)
                    {
                        EventLog appLog = new EventLog();
                        appLog.Source = "WebApi OData Test: ServerTypeTests.TypeData";
                        appLog.WriteEntry(
                            string.Format("Exception: {0}\n Assembly: {1}\n Message: {2}\n Stack trace: {3}\n", ex.GetType().FullName, assembly.FullName, ex.Message, ex.StackTrace),
                            EventLogEntryType.Error);

                        ReflectionTypeLoadException reflectionTypeLoadException = ex as ReflectionTypeLoadException;
                        if (reflectionTypeLoadException != null)
                        {
                            StringBuilder sb = new StringBuilder();

                            foreach (Exception exSub in reflectionTypeLoadException.LoaderExceptions)
                            {
                                sb.AppendLine(string.Format("{0}: {1}.", exSub.GetType().FullName, exSub.Message));
                                FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                                if (exFileNotFound != null)
                                {
                                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                                    {
                                        sb.AppendLine("Fusion Log:");
                                        sb.AppendLine(exFileNotFound.FusionLog);
                                    }
                                }
                                sb.AppendLine();
                            }
                            string errorMessage = sb.ToString();
                            appLog.WriteEntry(errorMessage, EventLogEntryType.Error);
                        }

#pragma warning disable CA2200 // Rethrow to preserve stack details.
                        throw ex;
#pragma warning restore CA2200 // Rethrow to preserve stack details.
                    }
                }
                foreach (var type in types)
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

                    if (props.Any(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?)))
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

#if !NETCORE // TODO #939: Enable this test for AspNetCore
        [Theory]
        [MemberData(nameof(TypeData))]
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
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.Routes.MapHttpRoute("ApiDefault", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            configuration.EnableDependencyInjection();
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

            try
            {
                q.OnActionExecuted(context);
                Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
            }
            catch (ArgumentException ae)
            {
                // For example: 
                // The type 'System.DateTime' of property 'NotAfter' in the 
                // 'System.Security.Cryptography.X509Certificates.X509Certificate2' type is not a supported type. 
                // Change to use 'System.DateTimeOffset' or ignore this type by calling 
                // Ignore<System.Security.Cryptography.X509Certificates.X509Certificate2>() 
                // on 'System.Web.OData.Builder.ODataModelBuilder'.
                Assert.True(ae.Message.Contains("The type 'System.DateTime' of property")
                    || ae.Message.Contains("System.Windows.Forms.AxHost")
                    || ae.Message.Contains("Found more than one dynamic property container in type"),

                    "The exception should contains \"The type 'System.DateTime' of property\", or " + 
                    "\"System.Windows.Forms.AxHost\" or" +
                    "\"Found more than one dynamic property container in type\", but actually, it is:" +
                    ae.Message);
            }
        }
#endif
    }
}
