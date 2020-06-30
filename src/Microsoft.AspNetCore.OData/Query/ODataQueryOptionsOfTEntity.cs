// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    public partial class ODataQueryOptions<TEntity> 
        : ODataQueryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from
        /// the <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="request">The incoming request message</param>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public ODataQueryOptions(ODataQueryContext context, HttpRequest request)
            : base(context, request)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.Argument("context", SRResources.ElementClrTypeNull, typeof(ODataQueryContext).Name);
            }

            if (context.ElementClrType != typeof(TEntity))
            {
                throw Error.Argument("context", SRResources.EntityTypeMismatch, context.ElementClrType.FullName, typeof(TEntity).FullName);
            }
        }
    }
}
