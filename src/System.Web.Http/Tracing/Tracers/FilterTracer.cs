// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Filters;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Base class and helper for the creation of filter tracers.
    /// </summary>
    internal class FilterTracer : IFilter
    {
        public FilterTracer(IFilter innerFilter, ITraceWriter traceWriter)
        {
            InnerFilter = innerFilter;
            TraceWriter = traceWriter;
        }

        public IFilter InnerFilter { get; set; }

        public ITraceWriter TraceWriter { get; set; }

        public bool AllowMultiple
        {
            get { return InnerFilter.AllowMultiple; }
        }

        public static IEnumerable<IFilter> CreateFilterTracers(IFilter filter, ITraceWriter traceWriter)
        {
            List<IFilter> filters = new List<IFilter>();
            bool addedActionAttributeTracer = false;
            bool addedAuthorizationAttributeTracer = false;
            bool addedExceptionAttributeTracer = false;

            ActionFilterAttribute actionFilterAttribute = filter as ActionFilterAttribute;
            if (actionFilterAttribute != null)
            {
                filters.Add(new ActionFilterAttributeTracer(actionFilterAttribute, traceWriter));
                addedActionAttributeTracer = true;
            }

            AuthorizationFilterAttribute authorizationFilterAttribute = filter as AuthorizationFilterAttribute;
            if (authorizationFilterAttribute != null)
            {
                filters.Add(new AuthorizationFilterAttributeTracer(authorizationFilterAttribute, traceWriter));
                addedAuthorizationAttributeTracer = true;
            }

            ExceptionFilterAttribute exceptionFilterAttribute = filter as ExceptionFilterAttribute;
            if (exceptionFilterAttribute != null)
            {
                filters.Add(new ExceptionFilterAttributeTracer(exceptionFilterAttribute, traceWriter));
                addedExceptionAttributeTracer = true;
            }

            // Do not add an IActionFilter tracer if we already added an ActionFilterAttribute tracer
            IActionFilter actionFilter = filter as IActionFilter;
            if (actionFilter != null && !addedActionAttributeTracer)
            {
                filters.Add(new ActionFilterTracer(actionFilter, traceWriter));
            }

            // Do not add an IAuthorizationFilter tracer if we already added an AuthorizationFilterAttribute tracer
            IAuthorizationFilter authorizationFilter = filter as IAuthorizationFilter;
            if (authorizationFilter != null && !addedAuthorizationAttributeTracer)
            {
                filters.Add(new AuthorizationFilterTracer(authorizationFilter, traceWriter));
            }

            // Do not add an IExceptionFilter tracer if we already added an ExceptoinFilterAttribute tracer
            IExceptionFilter exceptionFilter = filter as IExceptionFilter;
            if (exceptionFilter != null && !addedExceptionAttributeTracer)
            {
                filters.Add(new ExceptionFilterTracer(exceptionFilter, traceWriter));
            }

            if (filters.Count == 0)
            {
                filters.Add(new FilterTracer(filter, traceWriter));
            }
            
            return filters;
        }

        public static IEnumerable<FilterInfo> CreateFilterTracers(FilterInfo filter, ITraceWriter traceWriter)
        {
            IFilter filterInstance = filter.Instance;
            IEnumerable<IFilter> filterTracers = CreateFilterTracers(filterInstance, traceWriter);
            List<FilterInfo> filters = new List<FilterInfo>();
            foreach (IFilter filterTracer in filterTracers)
            {
                filters.Add(new FilterInfo(filterTracer, filter.Scope));
            }

            return filters;
        }

        public static bool IsFilterTracer(IFilter filter)
        {
            return filter is FilterTracer ||
                   filter is ActionFilterAttributeTracer ||
                   filter is AuthorizationFilterAttributeTracer ||
                   filter is ExceptionFilterAttributeTracer;
        }
    }
}
