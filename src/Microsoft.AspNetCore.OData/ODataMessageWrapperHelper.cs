//-----------------------------------------------------------------------------
// <copyright file="ODataMessageWrapperHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData
{
    internal static class ODataMessageWrapperHelper
    {
        internal static ODataMessageWrapper Create(Stream stream, IHeaderDictionary headers)
        {
            return ODataMessageWrapperHelper.Create(stream, headers, contentIdMapping: null);
        }

        internal static ODataMessageWrapper Create(Stream stream, IHeaderDictionary headers, IServiceProvider container)
        {
            return ODataMessageWrapperHelper.Create(stream, headers, null, container);
        }

        internal static ODataMessageWrapper Create(Stream stream, IHeaderDictionary headers, IDictionary<string, string> contentIdMapping, IServiceProvider container)
        {
            ODataMessageWrapper responseMessageWrapper = ODataMessageWrapperHelper.Create(stream, headers, contentIdMapping);
            responseMessageWrapper.Container = container;

            return responseMessageWrapper;
        }

        internal static ODataMessageWrapper Create(Stream stream, IHeaderDictionary headers, IDictionary<string, string> contentIdMapping)
        {
            return new ODataMessageWrapper(
                stream,
                headers.ToDictionary(kvp => kvp.Key, kvp => String.Join(";", kvp.Value)),
                contentIdMapping);
        }
    }
}
