// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Query
{
    internal class QueryHelpers
    {
        /// <summary>
        /// Get a single or default value from a collection.
        /// </summary>
        /// <param name="queryable">The response value as <see cref="IQueryable"/>.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <returns></returns>
        internal static object SingleOrDefault(
            IQueryable queryable,
            IWebApiActionDescriptor actionDescriptor)
        {
            var enumerator = queryable.GetEnumerator();
            try
            {
                var result = enumerator.MoveNext() ? enumerator.Current : null;

                if (enumerator.MoveNext())
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.SingleResultHasMoreThanOneEntity,
                        actionDescriptor.ActionName,
                        actionDescriptor.ControllerName,
                        "SingleResult"));
                }

                return result;
            }
            finally
            {
                // Ensure any active/open database objects that were created
                // iterating over the IQueryable object are properly closed.
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
