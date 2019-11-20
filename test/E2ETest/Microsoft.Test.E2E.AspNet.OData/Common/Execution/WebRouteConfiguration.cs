// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.TestCommon;
using Newtonsoft.Json;
#else
using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    /// <summary>
    /// And abstracted version of web configuration allow callers to configure AspNet or AspNetCore.
    /// </summary>
#if NETCORE
    public sealed class WebRouteConfiguration : IRouteBuilder
    {
        private IRouteBuilder routeBuilder;
        private ApplicationPartManager _scopedPartManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRouteConfiguration"/> class
        /// for pass 1 initialization against IRouteBuilder.
        /// </summary>
        public WebRouteConfiguration(IRouteBuilder routeBuilder)
        {
            this.routeBuilder = routeBuilder;
        }

        /// <summary>
        /// Implement IRouteBuilder and pass to the base class.
        /// </summary>
        public IApplicationBuilder ApplicationBuilder => routeBuilder.ApplicationBuilder;
        IRouter IRouteBuilder.DefaultHandler { get => routeBuilder.DefaultHandler; set { routeBuilder.DefaultHandler = value; } }
        public IServiceProvider ServiceProvider => routeBuilder.ServiceProvider;
        public IList<IRouter> Routes => routeBuilder.Routes;
        public IRouter Build() => routeBuilder.Build();

        /// <summary>
        /// A list of action to apply to MvcOptions
        /// </summary>
        public List<Action<MvcOptions>> MvcOptionsActions { get; } = new List<Action<MvcOptions>>();

        /// <summary>
        /// Create an <see cref="DefaultODataBatchHandler"/>.
        /// </summary>
        /// <returns>An <see cref="DefaultODataBatchHandler"/></returns>
        public DefaultODataBatchHandler CreateDefaultODataBatchHandler()
        {
            return new DefaultODataBatchHandler();
        }

        /// <summary>
        /// Create an <see cref="UnbufferedODataBatchHandler"/>.
        /// </summary>
        /// <returns>An <see cref="UnbufferedODataBatchHandler"/></returns>
        public UnbufferedODataBatchHandler CreateUnbufferedODataBatchHandler()
        {
            return new UnbufferedODataBatchHandler();
        }

        /// <summary>
        /// Gets or sets a value indicating whether error details should be included.
        /// </summary>
        public bool IncludeErrorDetail { get; set; } = true;

        /// <summary>
        /// Gets or sets the ReferenceLoopHandling property on the Json formatter.
        /// </summary>
        public ReferenceLoopHandling? JsonReferenceLoopHandling { get; set; }

        /// <summary>
        /// Gets or sets the Indent property on the Json formatter.
        /// </summary>
        public bool? JsonFormatterIndent { get; set; }

        /// <summary>
        /// Gets or sets the maximum received message size.
        /// </summary>
        public int? MaxReceivedMessageSize { get; set; }

        /// <summary>
        /// An instance of EnableQueryAttribute.
        /// </summary>
        public EnableQueryAttribute EnableQueryAttributeFilter { get; private set; }

        /// An instance of IActionFilter.
        /// </summary>
        public IActionFilter IActionFilterFilter { get; private set; }

        /// An instance of ETagMessageHandler.
        /// </summary>
        public ETagMessageHandler ETagMessageHandlerFilter { get; private set; }

        /// <summary>
        /// Ensure the configuration is initialized.
        /// </summary>
        public void EnsureInitialized()
        {
            // This is a no-op on AspNetCore.
        }

        /// <summary>
        /// Enable dependency injection for non-OData routes.
        /// </summary>
        public void EnableDependencyInjection()
        {
            routeBuilder.EnableDependencyInjection();
        }

        /// <summary>
        /// Enable HTTP route.
        /// </summary>
        public void MapHttpRoute(string name, string template)
        {
            this.MapRoute(name, template);
        }

        /// <summary>
        /// Enable HTTP route.
        /// </summary>
        public void MapHttpRoute(string name, string template, object defaults)
        {
            this.MapRoute(name, template, defaults);
        }

        /// <summary>
        /// Clear the formatters from the configuration.
        /// </summary>
        public void RemoveNonODataFormatters()
        {
            MvcOptionsActions.Add(options =>
            {
                IEnumerable<IOutputFormatter> formattersToRemove = options.OutputFormatters
                    .Where(f => f.GetType() != typeof(ODataOutputFormatter))
                    .ToList();

                foreach (var formatter in formattersToRemove)
                {
                    options.OutputFormatters.Remove(formatter);
                }
            });
        }

        /// <summary>
        /// Add a formatters to the configuration.
        /// </summary>
        /// <param name="formatters">The formatter.</param>
        public void InsertFormatter(OutputFormatter formatter)
        {
            MvcOptionsActions.Add(options =>
            {
                options.OutputFormatters.Add(formatter);
            });
        }

        /// <summary>
        /// Add a formatters to the configuration.
        /// </summary>
        /// <param name="formatters">One or more formatters.</param>
        public void InsertFormatters(IList<OutputFormatter> formatters)
        {
            // Insert in reverse order to preserve original order.
            foreach (var formatter in formatters.Reverse())
            {
                this.InsertFormatter(formatter);
            }
        }

        /// <summary>
        /// Create an <see cref="ODataConventionModelBuilder"/>.
        /// </summary>
        /// <returns>An <see cref="ODataConventionModelBuilder"/></returns>
        public ODataConventionModelBuilder CreateConventionModelBuilder()
        {
            if (_scopedPartManager != null)
            {
                // If there is a scoped part manager, controllers have been added but the
                // model building needs to take place over the fill set of classes.
                return new ODataConventionModelBuilder();
            }

            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider);
        }

        /// <summary>
        /// Create an <see cref="AttributeRoutingConvention"/>.
        /// </summary>
        /// <returns>An <see cref="AttributeRoutingConvention"/></returns>
        public AttributeRoutingConvention CreateAttributeRoutingConvention(string name = "AttributeRouting")
        {
            // Since we could be building the container, we must supply the path handler.
            return new AttributeRoutingConvention(name, routeBuilder.ServiceProvider, new DefaultODataPathHandler());
        }

        /// <summary>
        /// Add a list of controllers to be discovered by the application.
        /// </summary>
        /// <param name="controllers"></param>
        public void AddControllers(params Type[] controllers)
        {
            // Strip out all the IApplicationPartTypeProvider parts.
            _scopedPartManager = routeBuilder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            IList<ApplicationPart> parts = _scopedPartManager.ApplicationParts;
            IList<ApplicationPart> nonAssemblyParts = parts.Where(p => p.GetType() != typeof(IApplicationPartTypeProvider)).ToList();
            _scopedPartManager.ApplicationParts.Clear();
            _scopedPartManager.ApplicationParts.Concat(nonAssemblyParts);

            // Add a new AssemblyPart with the controllers.
            AssemblyPart part = new AssemblyPart(new TestAssembly(controllers));
            _scopedPartManager.ApplicationParts.Add(part);
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        public void AddODataQueryFilter()
        {
            this.EnableQueryAttributeFilter = new EnableQueryAttribute();
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public void AddODataQueryFilter(IActionFilter queryFilter)
        {
            if (this.IActionFilterFilter != null)
            {
                throw new InvalidOperationException("IActionFilterFilter has been set");
            }

            this.IActionFilterFilter = queryFilter;
        }

        /// <summary>
        /// Add an <see cref="ETagMessageHandler"/> to the configuration.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public void AddETagMessageHandler(ETagMessageHandler handler)
        {
            if (this.ETagMessageHandlerFilter != null)
            {
                throw new InvalidOperationException("ETagMessageHandlerFilter has been set");
            }

            this.ETagMessageHandlerFilter = handler;
        }
    }
#else
    public class WebRouteConfiguration : HttpConfiguration
    {
        public WebRouteConfiguration AddControllers(params Type[] controllers)
        {
            this.Services.Replace(
                typeof(IAssembliesResolver),
                new TestAssemblyResolver(controllers));

            return this;
        }

        /// <summary>
        /// Enable http route.
        /// </summary>
        public void MapHttpRoute(string name, string template)
        {
            this.Routes.MapHttpRoute(name, template);
        }

        /// <summary>
        /// Enable http route.
        /// </summary>
        public void MapHttpRoute(string name, string template, object defaults)
        {
            this.Routes.MapHttpRoute(name, template, defaults);
        }

        /// <summary>
        /// Clear the formatters from the configuration.
        /// </summary>
        public void RemoveNonODataFormatters()
        {
            this.Formatters.Clear();
        }

        /// <summary>
        /// Add a formatters to the configuration.
        /// </summary>
        /// <param name="formatters">A single formatters.</param>
        public void InsertFormatter(MediaTypeFormatter formatters)
        {
            this.Formatters.Insert(0, formatters);
        }

        /// <summary>
        /// Add a formatters to the configuration.
        /// </summary>
        /// <param name="formatters">One or more formatters.</param>
        public void InsertFormatters(params MediaTypeFormatter[] formatters)
        {
            this.Formatters.InsertRange(0, formatters);
        }

        /// <summary>
        /// Add an <see cref="ETagMessageHandler"/> to the configuration.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public void AddETagMessageHandler(ETagMessageHandler handler)
        {
            this.MessageHandlers.Add(new ETagMessageHandler());
        }

        /// <summary>
        /// Create an <see cref="ODataConventionModelBuilder"/>.
        /// </summary>
        /// <returns>An <see cref="ODataConventionModelBuilder"/></returns>
        public ODataConventionModelBuilder CreateConventionModelBuilder()
        {
            return new ODataConventionModelBuilder();
        }

        /// <summary>
        /// Create an <see cref="AttributeRoutingConvention"/>.
        /// </summary>
        /// <returns>An <see cref="AttributeRoutingConvention"/></returns>
        public AttributeRoutingConvention CreateAttributeRoutingConvention(string name = "AttributeRouting")
        {
            return new AttributeRoutingConvention(name, this);
        }

        /// <summary>
        /// Create an <see cref="DefaultODataBatchHandler"/>.
        /// </summary>
        /// <returns>An <see cref="DefaultODataBatchHandler"/></returns>
        public DefaultODataBatchHandler CreateDefaultODataBatchHandler()
        {
            return new DefaultODataBatchHandler(this.GetHttpServer());
        }

        /// <summary>
        /// Create an <see cref="UnbufferedODataBatchHandler"/>.
        /// </summary>
        /// <returns>An <see cref="UnbufferedODataBatchHandler"/></returns>
        public UnbufferedODataBatchHandler CreateUnbufferedODataBatchHandler()
        {
            return new UnbufferedODataBatchHandler(this.GetHttpServer());
        }

        /// <summary>
        /// Gets or sets a value indicating whether error details should be included.
        /// </summary>
        public bool IncludeErrorDetail
        {
            get
            {
                return this.IncludeErrorDetailPolicy == IncludeErrorDetailPolicy.Always;
            }

            set
            {
                this.IncludeErrorDetailPolicy = value
                    ? IncludeErrorDetailPolicy.Always
                    : IncludeErrorDetailPolicy.Never;
            }
        }

        /// <summary>
        /// Gets or sets the ReferenceLoopHandling property on the Json formatter.
        /// </summary>
        public ReferenceLoopHandling JsonReferenceLoopHandling
        {
            get { return this.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling; }
            set { this.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = value; }
        }

        /// <summary>
        /// Gets or sets the Indent property on the Json formatter.
        /// </summary>
        public bool JsonFormatterIndent
        {
            get { return this.Formatters.JsonFormatter.Indent; }
            set { this.Formatters.JsonFormatter.Indent = value; }

        }

        /// <summary>
        /// Gets or sets the ReferenceLoopHandling property on the Json formatter.
        /// </summary>
        public int MaxReceivedMessageSize
        {
            get { return 0; }
            set { }
            //var selfHostConfig = configuration as HttpSelfHostConfiguration;
            //if (selfHostConfig != null)
            //{
            //    selfHostConfig.MaxReceivedMessageSize = selfHostConfig.MaxBufferSize = int.MaxValue;
            //}
        }
    }
#endif
}
