// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Interface for model binding.
    /// </summary>
    public interface IModelBinder
    {
        bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext);
    }
}
