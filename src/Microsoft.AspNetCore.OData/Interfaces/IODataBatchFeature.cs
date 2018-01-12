// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// Provide the interface for the details of a given OData batch request.
    /// </summary>
    public interface IODataBatchFeature
    {
        /// <summary>
        /// Gets or sets the batch id.
        /// </summary>
        Guid? BatchId { get; set; }

        /// <summary>
        /// Gets or sets the change set id.
        /// </summary>
        Guid? ChangeSetId { get; set; }

        /// <summary>
        /// Gets or sets the content id.
        /// </summary>
        string ContentId { get; set; }

        /// <summary>
        /// Gets or sets the content id mapping.
        /// </summary>
        IDictionary<string, string> ContentIdMapping { get; set; }
    }
}
