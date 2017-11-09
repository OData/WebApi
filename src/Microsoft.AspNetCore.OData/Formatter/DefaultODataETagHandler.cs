// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNet.OData.Builder.Conventions;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    internal class DefaultODataETagHandler : IETagHandler
    {
        public EntityTagHeaderValue CreateETag(IDictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> ParseETag(EntityTagHeaderValue etagHeaderValue)
        {
            throw new NotImplementedException();
        }
    }
}
