// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IActionSelector"/> that uses the server's OData routing conventions
    /// to select an action for OData requests.
    /// </summary>
    public class ODataActionSelector : IActionSelector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActionSelector _selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="decisionTreeProvider"></param>
        /// <param name="actionConstraintProviders"></param>
        /// <param name="loggerFactory"></param>
        public ODataActionSelector(IServiceProvider serviceProvider,
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            ActionConstraintCache actionConstraintProviders,
            ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _selector = new ActionSelector(decisionTreeProvider, actionConstraintProviders, loggerFactory);
        }

        public bool HasValidAction(VirtualPathContext context)
        {
            return true;
        }

        /// <inheritdoc />
        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
        {
            if (context.HttpContext.ODataFeature().IsValidODataRequest)
            {
                var options = _serviceProvider.GetRequiredService<IOptions<ODataOptions>>().Value;

                ActionDescriptor actionDescriptor = null;
                foreach (var convention in options.RoutingConventions)
                {
                    actionDescriptor = convention.SelectAction(context);
                    if (actionDescriptor != null)
                    {
                        break;
                    }
                }

                if (actionDescriptor != null)
                {
                    var list = new List<ActionDescriptor>
                    {
                        actionDescriptor
                    };

                    return list.AsReadOnly();
                }
            }

            return _selector.SelectCandidates(context);
        }

        /// <inheritdoc />
        public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
        {
            if (context.HttpContext.ODataFeature().IsValidODataRequest)
            {
                return candidates.First();
            }

            return _selector.SelectBestCandidate(context, candidates);
        }
    }
}
