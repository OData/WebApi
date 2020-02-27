// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETSTANDARD2_0
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// A service that is responsible for the final OData Endpoint selection decision.
    /// This selector is used to resolve the duplicated Endpoint selection.
    /// </summary>
    internal class ODataEndpointSelectorPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        // Run shortly after the DynamicControllerMatcherPolicy
        public override int Order => int.MinValue + 200;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            // If OData is in use, then run this policy everywhere.
            return true;
        }

        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull(nameof(httpContext));
            }

            if (candidates == null)
            {
                throw Error.ArgumentNull(nameof(candidates));
            }

            IODataFeature odataFeature = httpContext.ODataFeature();
            ActionDescriptor actionDescriptor = odataFeature.ActionDescriptor;
            if (actionDescriptor == null)
            {
                // This means the request didn't match an OData endpoint. Just ignore it.
                return Task.CompletedTask;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Endpoint == null)
                {
                    candidates.SetValidity(i, false);
                    continue;
                }

                ActionDescriptor action = candidates[i].Endpoint.Metadata.GetMetadata<ActionDescriptor>();

                if (action != null && !object.ReferenceEquals(action, actionDescriptor))
                {
                    // This candidate is not the one we matched earlier, so disallow it.
                    candidates.SetValidity(i, false);
                }
            }

            return Task.CompletedTask;
        }
    }
}
#endif
