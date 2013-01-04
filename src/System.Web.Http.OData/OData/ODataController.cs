// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Description;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Defines a base class for OData controllers that support writing and reading data using the OData formats.
    /// </summary>
    [ODataFormatting]
    [ODataRouting]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class ODataController : ApiController
    {
    }
}
