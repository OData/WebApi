// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.OData
{
    /// <summary>
    /// ActionPayload holds the Parameter names and values provided by a client in a POST request
    /// to invoke a particular Action. The Parameter values are stored in the dictionary keyed using the Parameter name.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "ODataActionParameters is more appropriate here.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "ODataActionParameters is not serializable.")]
    public class ODataActionParameters : Dictionary<string, object>
    {
    }
}