// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETSTANDARD2_0
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing.Matching;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// A service that is responsible for the final OData Endpoint selection decision.
    /// This selector is used to resolve the duplicated Endpoint selection.
    /// </summary>
    internal class ODataEndpointSelector : EndpointSelector
    {
        private EndpointSelector _innerSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEndpointSelector"/> class.
        /// </summary>
        /// <param name="selector">The inner Endpoint selector</param>
        public ODataEndpointSelector(EndpointSelector selector)
        {
            _innerSelector = selector;
        }

        /// <inheritdoc/>
        public override Task SelectAsync(HttpContext httpContext, CandidateSet candidateSet)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (candidateSet == null)
            {
                throw new ArgumentNullException(nameof(candidateSet));
            }

            Select(httpContext, candidateSet);

            if (httpContext.GetEndpoint() == null)
            {
                // If OData Endpoint selector cannot select an Endpoint, delegate it to the inner Endpoint selector.
                return _innerSelector.SelectAsync(httpContext, candidateSet);
            }

            return Task.CompletedTask;
        }

        internal void Select(HttpContext httpContext, CandidateSet candidateSet)
        {
            IODataFeature odataFeature = httpContext.ODataFeature();
            ActionDescriptor actionDescriptor = odataFeature.ActionDescriptor;
            if (actionDescriptor != null)
            {
                int count = candidateSet.Count;
                for (int i = 0; i < count; i++)
                {
                    CandidateState candidate = candidateSet[i];
                    ActionDescriptor action = candidate.Endpoint.Metadata.GetMetadata<ActionDescriptor>();

                    // Noted: we simple use the "ReferenceEquals" to compare the action descriptor.
                    // So far, i don't know the risk, i need .NET team help me to verify it?
                    if (object.ReferenceEquals(action, actionDescriptor))
                    {
                        httpContext.SetEndpoint(candidate.Endpoint);
                        httpContext.Request.RouteValues = candidate.Values;
                        return;
                    }
                }
            }
        }
    }
}
#endif
