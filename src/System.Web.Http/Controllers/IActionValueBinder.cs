// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Controllers
{
    public interface IActionValueBinder
    {
        HttpActionBinding GetBinding(HttpActionDescriptor actionDescriptor);
    }
}
