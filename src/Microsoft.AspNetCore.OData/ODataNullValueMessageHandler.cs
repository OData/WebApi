// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see href="HttpMessageHandler" /> that converts null values in OData responses to
    /// HTTP NotFound responses or NoContent responses following the OData specification.
    /// </summary>
    public partial class ODataNullValueMessageHandler
    {
    }
}
