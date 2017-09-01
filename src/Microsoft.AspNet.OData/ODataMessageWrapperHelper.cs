// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Formatter;

namespace Microsoft.AspNet.OData
{
    internal static class ODataMessageWrapperHelper
    {
        internal static ODataMessageWrapper Create(Stream stream, HttpContentHeaders headers)
        {
            return ODataMessageWrapperHelper.Create(stream, headers, contentIdMapping: null);
        }

        internal static ODataMessageWrapper Create(Stream stream, HttpContentHeaders headers, IServiceProvider container)
        {
            return ODataMessageWrapperHelper.Create(stream, headers, null, container);
        }

        internal static ODataMessageWrapper Create(Stream stream, HttpContentHeaders headers, IDictionary<string, string> contentIdMapping, IServiceProvider container)
        {
            ODataMessageWrapper responseMessageWrapper = ODataMessageWrapperHelper.Create(stream, headers, contentIdMapping);
            responseMessageWrapper.Container = container;

            return responseMessageWrapper;
        }

        internal static ODataMessageWrapper Create(Stream stream, HttpContentHeaders headers, IDictionary<string, string> contentIdMapping)
        {
            return new ODataMessageWrapper(
                stream,
                headers.ToDictionary(kvp => kvp.Key, kvp => String.Join(";", kvp.Value)),
                contentIdMapping);
        }
    }
}
