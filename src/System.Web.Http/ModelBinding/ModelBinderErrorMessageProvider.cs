// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.ModelBinding
{
    public delegate string ModelBinderErrorMessageProvider(HttpActionContext actionContext, ModelMetadata modelMetadata, object incomingValue);
}
