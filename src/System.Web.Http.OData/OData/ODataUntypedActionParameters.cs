// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// ActionPayload holds the Parameter names and values provided by a client in a POST request
    /// to invoke a particular Action. The Parameter values are stored in the dictionary keyed using the Parameter name.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "ODataUntypedActionParameters is more appropriate here.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "ODataUntypedActionParameters is not serializable.")]
    public class ODataUntypedActionParameters : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataUntypedActionParameters"/> class.
        /// </summary>
        /// <param name="action">The OData action of this parameters.</param>
        public ODataUntypedActionParameters(IEdmFunctionImport action)
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
        public IEdmFunctionImport Action { get; private set; }
    }
}
