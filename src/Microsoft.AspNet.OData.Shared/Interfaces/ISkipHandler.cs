// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// Allows for custom implementations of SkipToken with a custom format and application specific filtering.
    /// </summary>
    public interface ISkipTokenHandler
    {
        /// <summary>
        /// Returns a function that converts an object to a skiptoken value string
        /// </summary>
        /// <param name="lastMember">Object based on which the value of the skiptoken is generated.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="orderByNodes">QueryOption</param>
        /// <returns></returns>
        string GenerateSkipTokenValue(object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes);

        /// <summary>
        /// Process skiptoken value to create a dictionary that contains objects converted from the string values with property names as the keys.
        /// </summary>
        /// <param name="rawValue"></param>
        IDictionary<string, object> ProcessSkipTokenValue(string rawValue);

        /// <summary>
        /// Gets and sets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        ODataQueryContext Context { get; set; }
    }
}
