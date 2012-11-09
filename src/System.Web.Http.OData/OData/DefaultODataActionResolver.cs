// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// A default implementation of an IODataActionResolver
    /// </summary>
    public class DefaultODataActionResolver : IODataActionResolver
    {
        public IEdmFunctionImport Resolve(ODataDeserializerContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }
            else if (context.Request == null || context.Request.RequestUri == null)
            {
                throw Error.InvalidOperation(SRResources.DefaultODataActionResolverRequirementsNotSatisfied);
            }

            IEdmEntityContainer[] entityContainers = context.Model.EntityContainers().ToArray();
            if (entityContainers.Length != 1)
            {
                throw Error.InvalidOperation(SRResources.ParserModelMustHaveOneContainer, entityContainers.Length);
            }

            IEnumerable<IEdmFunctionImport> matchingActionsQuery = entityContainers[0].FunctionImports();
            string lastSegment = context.Request.RequestUri.Segments.Last();
            matchingActionsQuery = matchingActionsQuery.GetMatchingActions(lastSegment);

            IEdmFunctionImport[] possibleMatches = matchingActionsQuery.ToArray();

            if (possibleMatches.Length == 0)
            {
                throw Error.InvalidOperation(SRResources.ActionNotFound, lastSegment, context.Request.RequestUri.AbsoluteUri);
            }
            else if (possibleMatches.Length > 1)
            {
                throw Error.InvalidOperation(
                    SRResources.ActionResolutionFailed,
                    lastSegment,
                    String.Join(", ", possibleMatches.Select(match => match.Container.FullName() + "." + match.Name)));
            }
            return possibleMatches[0];
        }
    }
}
