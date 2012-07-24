// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;

namespace System.Web.Http.Controllers
{
    public interface IActionHttpMethodProvider
    {
        Collection<HttpMethod> HttpMethods { get; }
    }
}
