// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Formatter
{
    internal static partial class EdmLibHelpers
    {
        private static readonly IWebApiAssembliesResolver _defaultAssemblyResolver = new WebApiAssembliesResolver();
    }
}