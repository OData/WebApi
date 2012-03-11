using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// A wrapper <see cref="HttpActionDescriptor"/> that customizes various aspects of the wrapped
    /// inner descriptor, for example by adding additional action filters.
    /// </summary>
    internal sealed class CustomizingActionDescriptor : HttpActionDescriptor
    {
        private HttpActionDescriptor _innerDescriptor;

        public CustomizingActionDescriptor(HttpActionDescriptor innerDescriptor)
        {
            _innerDescriptor = innerDescriptor;
            Configuration = _innerDescriptor.Configuration;
            ControllerDescriptor = _innerDescriptor.ControllerDescriptor;
        }

        public override string ActionName
        {
            get { return _innerDescriptor.ActionName; }
        }

        public override Type ReturnType
        {
            get { return _innerDescriptor.ReturnType; }
        }

        public override object Execute(HttpControllerContext controllerContext, IDictionary<string, object> arguments)
        {
            return _innerDescriptor.Execute(controllerContext, arguments);
        }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            return _innerDescriptor.GetParameters();
        }

        public override Collection<FilterInfo> GetFilterPipeline()
        {
            Collection<FilterInfo> filters = new Collection<FilterInfo>(_innerDescriptor.GetFilterPipeline());

            // for any actions that support query composition, we need to add our
            // query filter as well. This must be added immediately after the
            // QueryCompositionFilterAttribute.
            // TODO: once filter ordering is supported, there may be a better way
            // than searching on type name like this.
            bool addFilter = false;
            int idx = 0;
            for (idx = 0; idx < filters.Count; idx++)
            {
                if (filters[idx].Instance.GetType().Name == "QueryCompositionFilterAttribute")
                {
                    addFilter = true;
                    break;
                }
            }
            if (addFilter)
            {
                filters.Insert(idx, new FilterInfo(new QueryFilterAttribute(), FilterScope.Action));
            }

            return filters;
        }
    }
}
