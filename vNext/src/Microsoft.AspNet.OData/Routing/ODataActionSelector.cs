using Microsoft.AspNet.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Mvc.ActionConstraints;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Abstractions;

namespace Microsoft.AspNet.OData.Routing
{
    public class ODataActionSelector : IActionSelector
    {
        private readonly IActionSelector _selector;
        private readonly IODataRoutingConvention _convention;

        public ODataActionSelector(IODataRoutingConvention convention,
            IActionDescriptorsCollectionProvider actionDescriptorsCollectionProvider,
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            IEnumerable<IActionConstraintProvider> actionConstraintProviders,
            ILoggerFactory loggerFactory)
        {
            _selector = new DefaultActionSelector(
                decisionTreeProvider, actionConstraintProviders, loggerFactory);
            _convention = convention;
        }

        public bool HasValidAction(VirtualPathContext context)
        {
            return true;
        }

        public async Task<ActionDescriptor> SelectAsync(RouteContext context)
        {
            if (context.HttpContext.ODataProperties().IsValidODataRequest)
            {
                return await Task.FromResult(_convention.SelectAction(context));
            }

            return await _selector.SelectAsync(context);
        }
    }
}
