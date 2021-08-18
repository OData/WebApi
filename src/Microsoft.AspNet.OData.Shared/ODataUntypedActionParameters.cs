//-----------------------------------------------------------------------------
// <copyright file="ODataUntypedActionParameters.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
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
