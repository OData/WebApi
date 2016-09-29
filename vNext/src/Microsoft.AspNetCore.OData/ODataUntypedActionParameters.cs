// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// ActionPayload holds the Parameter names and values provided by a client in a POST request
    /// to invoke a particular Action. The Parameter values are stored in the dictionary keyed using the Parameter name.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "ODataUntypedActionParameters is more appropriate here.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "ODataUntypedActionParameters is not serializable.")]
    [NonValidatingParameterBinding]
    public class ODataUntypedActionParameters : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataUntypedActionParameters"/> class.
        /// </summary>
        /// <param name="action">The OData action of this parameters.</param>
        public ODataUntypedActionParameters(IEdmAction action)
        {
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            Action = action;
        }

        /// <summary>
        /// Gets the OData action of this parameters.
        /// </summary>
        public IEdmAction Action { get; private set; }
    }
}
