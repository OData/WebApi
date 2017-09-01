// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// The base class for all expression binders.
    /// </summary>
    public abstract partial class ExpressionBinderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBinderBase"/> class.
        /// </summary>
        /// <param name="requestContainer">The request container.</param>
        protected ExpressionBinderBase(IServiceProvider requestContainer)
        {
            Contract.Assert(requestContainer != null);

            QuerySettings = requestContainer.GetRequiredService<ODataQuerySettings>();
            Model = requestContainer.GetRequiredService<IEdmModel>();

            IAssembliesResolver resolver = requestContainer.GetRequiredService<IAssembliesResolver>();
            AssembliesResolver = new WebApiAssembliesResolver(resolver);
        }
    }
}