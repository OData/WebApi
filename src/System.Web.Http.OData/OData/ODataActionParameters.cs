// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.OData.Formatter;

namespace System.Web.Http.OData
{
    /// <summary>
    /// ActionPayload holds the Parameter names and values provided by a client in a POST request
    /// to invoke a particular Action. The Parameter values are stored in the dictionary keyed using the Parameter name.
    /// </summary>
    [ODataParameterBinding]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Pending, will remove once class has appropriate base type.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Pending, will remove once class has appropriate base type.")]
    public class ODataActionParameters : Dictionary<string, object>
    {
    }
}