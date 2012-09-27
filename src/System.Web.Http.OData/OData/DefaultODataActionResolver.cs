// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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

            string actionName = null;
            string containerName = null;
            string nspace = null;

            string lastSegment = context.Request.RequestUri.Segments.Last();
            string[] nameParts = lastSegment.Split('.');

            IEdmEntityContainer[] entityContainers = context.Model.EntityContainers().ToArray();
            Contract.Assert(entityContainers.Length == 1);
            IEnumerable<IEdmFunctionImport> matchingActionsQuery = entityContainers[0].FunctionImports();

            if (nameParts.Length == 1)
            {
                actionName = nameParts[0];
                matchingActionsQuery = matchingActionsQuery.Where(f => f.Name == actionName && f.IsSideEffecting == true);
            }
            else if (nameParts.Length == 2)
            {
                actionName = nameParts[nameParts.Length - 1];
                containerName = nameParts[nameParts.Length - 2];
                matchingActionsQuery = matchingActionsQuery.Where(f => f.Name == actionName && f.IsSideEffecting == true && f.Container.Name == containerName);
            }
            else if (nameParts.Length > 2)
            {
                actionName = nameParts[nameParts.Length - 1];
                containerName = nameParts[nameParts.Length - 2];
                nspace = String.Join(".", nameParts.Take(nameParts.Length - 2));
                matchingActionsQuery = matchingActionsQuery.Where(f => f.Name == actionName && f.IsSideEffecting == true && f.Container.Name == containerName && f.Container.Namespace == nspace);
            }

            IEdmFunctionImport[] possibleMatches = matchingActionsQuery.ToArray();

            if (possibleMatches.Length == 0)
            {
                throw Error.InvalidOperation(SRResources.ActionNotFound, actionName, context.Request.RequestUri.AbsoluteUri);
            }
            else if (possibleMatches.Length > 1)
            {
                throw Error.InvalidOperation(SRResources.ActionResolutionFailed, actionName, context.Request.RequestUri.AbsoluteUri);
            }
            return possibleMatches[0];
        }
    }
}
