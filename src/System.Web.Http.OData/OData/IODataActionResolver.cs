// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter.Deserialization;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Resolves an OData Action
    /// </summary>
    public interface IODataActionResolver
    {
        /// <summary>
        /// Return the matching ODataAction (IEdmFunctionImport) given the request described by the ODataDeserializerContext
        /// </summary>
        /// <param name="context">The ODataDeserializerContext from which the resolver should use to find the Action</param>
        /// <returns>The resolved Action.</returns>
        IEdmFunctionImport Resolve(ODataDeserializerContext context);
    }
}
