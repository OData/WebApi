using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.OData.Routing
{
    public class ODataActionSelector : IActionSelector
    {
        private readonly IActionSelector _selector;
        private readonly IODataRoutingConvention _convention;

        public ODataActionSelector(IODataRoutingConvention convention,
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            ActionConstraintCache actionConstraintProviders,
            ILoggerFactory loggerFactory)
        {
            _selector = new ActionSelector(decisionTreeProvider, actionConstraintProviders, loggerFactory);
            _convention = convention;
        }

        public bool HasValidAction(VirtualPathContext context)
        {
            return true;
        }

        public ActionDescriptor Select(RouteContext context)
        {
            if (context.HttpContext.ODataProperties().IsValidODataRequest)
            {
                return _convention.SelectAction(context);
            }

            return _selector.Select(context);
        }
    }
}
