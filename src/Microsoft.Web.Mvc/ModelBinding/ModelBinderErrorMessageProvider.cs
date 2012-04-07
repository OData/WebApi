// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public delegate string ModelBinderErrorMessageProvider(ControllerContext controllerContext, ModelMetadata modelMetadata, object incomingValue);
}
