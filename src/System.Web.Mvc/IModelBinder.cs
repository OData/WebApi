// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public interface IModelBinder
    {
        object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext);
    }
}
