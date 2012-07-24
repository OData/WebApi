// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc
{
    public interface ITempDataProvider
    {
        IDictionary<string, object> LoadTempData(ControllerContext controllerContext);
        void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values);
    }
}
