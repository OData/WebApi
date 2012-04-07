// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public interface IExceptionFilter
    {
        void OnException(ExceptionContext filterContext);
    }
}
