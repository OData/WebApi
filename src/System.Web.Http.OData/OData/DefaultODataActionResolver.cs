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
            Contract.Assert(context.Request != null);
            Contract.Assert(context.Model != null);

            string actionName = null;
            string containerName = null;
            string nspace = null;

            string lastSegment = context.Request.RequestUri.Segments.Last();
            string[] nameParts = lastSegment.Split('.');

            IEnumerable<IEdmFunctionImport> matchingActionsQuery = context.Model.EntityContainers().Single().FunctionImports();

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
                throw Error.InvalidOperation(SRResources.ActionNotFound, actionName);
            }
            if (possibleMatches.Length > 1)
            {
                throw Error.InvalidOperation(SRResources.ActionResolutionFailed, actionName);
            }
            return possibleMatches[0];
        }
    }
}
