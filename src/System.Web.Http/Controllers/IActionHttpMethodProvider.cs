// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;

namespace System.Web.Http.Controllers
{
    internal interface IActionHttpMethodProvider
    {
        Collection<HttpMethod> HttpMethods { get; }
    }
}
