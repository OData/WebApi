// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public interface IExtensibleModelBinder
    {
        bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext);
    }
}
