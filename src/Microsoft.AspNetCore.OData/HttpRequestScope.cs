// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Provides access to the <see cref="HttpRequest"/>
    /// to which the OData service container instance is scoped.
    /// </summary>
    public class HttpRequestScope
    {
        /// <summary>
        /// Provides access to the <see cref="HttpRequest"/>
        /// to which the OData service container instance is scoped.
        /// </summary>
        public HttpRequest HttpRequest { get; set; }
    }
}
