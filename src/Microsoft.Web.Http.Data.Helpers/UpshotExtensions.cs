// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace Microsoft.Web.Http.Data.Helpers
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UpshotExtensions
    {
        public static UpshotConfigBuilder UpshotContext(this HtmlHelper htmlHelper)
        {
            return UpshotContext(htmlHelper, false);
        }

        public static UpshotConfigBuilder UpshotContext(this HtmlHelper htmlHelper, bool bufferChanges)
        {
            return new UpshotConfigBuilder(htmlHelper, bufferChanges);
        }
    }

    public class UpshotConfigBuilder : IHtmlString
    {
        private readonly HtmlHelper htmlHelper;
        private readonly bool bufferChanges;
        private readonly IDictionary<string, IDataSourceConfig> dataSources = new Dictionary<string, IDataSourceConfig>();
        private readonly IDictionary<Type, string> clientMappings = new Dictionary<Type, string>();

        public UpshotConfigBuilder(HtmlHelper htmlHelper, bool bufferChanges)
        {
            this.htmlHelper = htmlHelper;
            this.bufferChanges = bufferChanges;
        }

        private interface IDataSourceConfig
        {
            string ClientName { get; }
            Type DataControllerType { get; }
            string SharedDataContextExpression { get; }
            string DataContextExpression { set; }
            string ClientMappingsJson { set; }
            string GetInitializationScript();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Following established design pattern for HTML helpers.")]
        public UpshotConfigBuilder DataSource<TDataController>(Expression<Func<TDataController, object>> queryOperation) where TDataController : DataController
        {
            return this.DataSource<TDataController>(queryOperation, null, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Following established design pattern for HTML helpers."),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Following established design pattern for HTML helpers.")]
        public UpshotConfigBuilder DataSource<TDataController>(Expression<Func<TDataController, object>> queryOperation, string serviceUrl, string clientName) where TDataController : DataController
        {
            IDataSourceConfig dataSourceConfig = new DataSourceConfig<TDataController>(htmlHelper, bufferChanges, queryOperation, serviceUrl, clientName);
            if (dataSources.ContainsKey(dataSourceConfig.ClientName))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot have multiple data sources with the same clientName. Found multiple data sources with the name '{0}'", dataSourceConfig.ClientName));
            }
            dataSources.Add(dataSourceConfig.ClientName, dataSourceConfig);
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Following established design pattern for HTML helpers.")]
        public UpshotConfigBuilder ClientMapping<TEntity>(string clientConstructor)
        {
            if (string.IsNullOrEmpty(clientConstructor))
            {
                throw new ArgumentException("clientConstructor cannot be null or empty", "clientConstructor");
            }
            if (clientMappings.ContainsKey(typeof(TEntity)))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot have multiple client mappings for the same entity type. Found multiple client mappings for '{0}'", typeof(TEntity).FullName));
            }
            clientMappings.Add(typeof(TEntity), clientConstructor);
            return this;
        }

        public string ToHtmlString()
        {
            StringBuilder js = new StringBuilder("upshot.dataSources = upshot.dataSources || {};\n");

            // First emit metadata for each referenced DataController
            IEnumerable<Type> dataControllerTypes = dataSources.Select(x => x.Value.DataControllerType).Distinct();
            foreach (Type dataControllerType in dataControllerTypes)
            {
                js.AppendFormat("upshot.metadata({0});\n", GetMetadata(dataControllerType));
            }

            // Let the first dataSource construct a dataContext, and all subsequent ones share it
            IEnumerable<IDataSourceConfig> allDataSources = dataSources.Values;
            IDataSourceConfig firstDataSource = allDataSources.FirstOrDefault();
            if (firstDataSource != null)
            {
                // All but the first data source share the DataContext implicitly instantiated by the first.
                foreach (IDataSourceConfig dataSource in allDataSources.Skip(1))
                {
                    dataSource.DataContextExpression = firstDataSource.SharedDataContextExpression;
                }

                // Let the first dataSource define the client mappings
                firstDataSource.ClientMappingsJson = GetClientMappingsObjectLiteral();
            }

            // Now emit initialization code for each dataSource
            foreach (IDataSourceConfig dataSource in allDataSources)
            {
                js.AppendLine("\n" + dataSource.GetInitializationScript());
            }

            // Also record the mapping functions in use
            foreach (var mapping in clientMappings)
            {
                js.AppendFormat("upshot.registerType(\"{0}\", function() {{ return {1} }});\n", EncodeServerTypeName(mapping.Key), mapping.Value);
            }

            return string.Format(CultureInfo.InvariantCulture, "<script type='text/javascript'>\n{0}</script>", js);
        }

        private string GetMetadata(Type dataControllerType)
        {
            var methodInfo = typeof(MetadataExtensions).GetMethod("Metadata");
            var result = (IHtmlString)methodInfo.MakeGenericMethod(dataControllerType).Invoke(null, new[] { htmlHelper });
            return result.ToHtmlString();
        }

        private string GetClientMappingsObjectLiteral()
        {
            IEnumerable<string> clientMappingStrings =
                clientMappings.Select(
                    clientMapping => string.Format(CultureInfo.InvariantCulture, "\"{0}\": function(data) {{ return new {1}(data) }}", EncodeServerTypeName(clientMapping.Key), clientMapping.Value));
            return string.Format(CultureInfo.InvariantCulture, "{{{0}}}", string.Join(",", clientMappingStrings));
        }

        // TODO: Duplicated from DataControllerMetadataGenerator.cs.  Refactor when combining this into the main System.Web.Http.Data.Helper assembly.
        private static string EncodeServerTypeName(Type type)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", type.Name, ":#", type.Namespace);
        }

        private class DataSourceConfig<TDataController> : IDataSourceConfig where TDataController : DataController
        {
            private readonly HtmlHelper htmlHelper;
            private readonly bool bufferChanges;
            private readonly Expression<Func<TDataController, object>> queryOperation;
            private readonly string serviceUrlOverride;
            private readonly string clientName;

            public DataSourceConfig(HtmlHelper htmlHelper, bool bufferChanges, Expression<Func<TDataController, object>> queryOperation, string serviceUrlOverride, string clientName)
            {
                this.htmlHelper = htmlHelper;
                this.bufferChanges = bufferChanges;
                this.queryOperation = queryOperation;
                this.serviceUrlOverride = serviceUrlOverride;
                this.clientName = string.IsNullOrEmpty(clientName) ? DefaultClientName : clientName;
            }

            public string ClientName
            {
                get
                {
                    return clientName;
                }
            }

            public Type DataControllerType
            {
                get
                {
                    return typeof(TDataController);
                }
            }

            public string DataContextExpression { private get; set; }

            public string ClientMappingsJson { private get; set; }

            public string SharedDataContextExpression
            {
                get
                {
                    return ClientExpression + ".getDataContext()";
                }
            }

            private string ClientExpression
            {
                get
                {
                    return "upshot.dataSources." + ClientName;
                }
            }

            private Type EntityType
            {
                get
                {
                    Type operationReturnType = OperationMethod.ReturnType;
                    Type genericTypeDefinition = operationReturnType.IsGenericType ? operationReturnType.GetGenericTypeDefinition() : null;
                    Type entityType;
                    if (genericTypeDefinition != null && (genericTypeDefinition == typeof(IQueryable<>) || genericTypeDefinition == typeof(IEnumerable<>)))
                    {
                        // Permits IQueryable<TEntity> and IEnumerable<TEntity>.
                        entityType = operationReturnType.GetGenericArguments().Single();
                    }
                    else
                    {
                        entityType = operationReturnType;
                    }

                    if (!Description.EntityTypes.Any(type => type == entityType))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "queryOperation '{0}' must return an entity type or an IEnumerable/IQueryable of an entity type", OperationMethod.Name));
                    }

                    return entityType;
                }
            }

            private string ServiceUrl
            {
                get
                {
                    if (!string.IsNullOrEmpty(serviceUrlOverride))
                    {
                        return serviceUrlOverride;
                    }

                    UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
                    string dataControllerName = typeof(TDataController).Name;
                    if (!dataControllerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("DataController type name must end with 'Controller'");
                    }
                    string controllerRouteName = dataControllerName.Substring(0, dataControllerName.Length - "Controller".Length);
                    return urlHelper.RouteUrl(new { controller = controllerRouteName, action = UrlParameter.Optional, httproute = true });
                }
            }

            private string DefaultClientName
            {
                get
                {
                    string operationName = OperationMethod.Name;
                    // By convention, strip away any "Get" verb on the method.  Clients can override by explictly specifying client name.
                    return operationName.StartsWith("Get", StringComparison.OrdinalIgnoreCase) && operationName.Length > 3 && char.IsLetter(operationName[3]) ? operationName.Substring(3) : operationName;
                }
            }

            private MethodInfo OperationMethod
            {
                get
                {
                    Expression body = queryOperation.Body;

                    // The VB compiler will inject a convert to object here.
                    if (body.NodeType == ExpressionType.Convert)
                    {
                        UnaryExpression convert = (UnaryExpression)body;
                        if (convert.Type == typeof(object))
                        {
                            body = convert.Operand;
                        }
                    }

                    MethodCallExpression methodCall = body as MethodCallExpression;
                    if (methodCall == null)
                    {
                        throw new ArgumentException("queryOperation must be a method call");
                    }

                    if (!methodCall.Method.DeclaringType.IsAssignableFrom(typeof(TDataController)))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "queryOperation must be a method on '{0}' or a base type", typeof(TDataController).Name));
                    }

                    return methodCall.Method;
                }
            }

            private static DataControllerDescription Description
            {
                get
                {
                    HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor
                    {
                        Configuration = GlobalConfiguration.Configuration, // This helper can't be run until after global app init.
                        ControllerType = typeof(TDataController)
                    };

                    DataControllerDescription description = DataControllerDescription.GetDescription(controllerDescriptor);
                    return description;
                }
            }

            public string GetInitializationScript()
            {
                return string.Format(CultureInfo.InvariantCulture, @"{0} = upshot.RemoteDataSource({{
    providerParameters: {{ url: ""{1}"", operationName: ""{2}"" }},
    entityType: ""{3}"",
    bufferChanges: {4},
    dataContext: {5},
    mapping: {6}
}});",
                    ClientExpression, ServiceUrl, OperationMethod.Name, EncodeServerTypeName(EntityType),
                    bufferChanges ? "true" : "false", DataContextExpression ?? "undefined", ClientMappingsJson ?? "undefined");
            }
        }
    }
}
