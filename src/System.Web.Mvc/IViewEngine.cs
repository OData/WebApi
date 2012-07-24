// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public interface IViewEngine
    {
        ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache);
        ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache);
        void ReleaseView(ControllerContext controllerContext, IView view);
    }
}
