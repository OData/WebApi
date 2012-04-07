// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web.Http;
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

        public override IActionResultConverter ResultConverter
        {
            get { return _innerDescriptor.ResultConverter; }
        }

        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments)
        {
            return _innerDescriptor.ExecuteAsync(controllerContext, arguments);
        }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            return _innerDescriptor.GetParameters();
        }

        public override Collection<FilterInfo> GetFilterPipeline()
        {
            Collection<FilterInfo> originalFilters = _innerDescriptor.GetFilterPipeline();
            Collection<FilterInfo> newFilters = new Collection<FilterInfo>();

            // for any actions that support query composition, we need to replace it with our
            // query filter.
            foreach (FilterInfo filterInfo in originalFilters)
            {
                FilterInfo newInfo = filterInfo;
                QueryableAttribute queryableFilter = filterInfo.Instance as QueryableAttribute;
                if (queryableFilter != null)
                {
                    newInfo = new FilterInfo(new QueryFilterAttribute() { ResultLimit = queryableFilter.ResultLimit }, filterInfo.Scope);
                }
                newFilters.Add(newInfo);
            }

            return newFilters;
        }
    }
}
