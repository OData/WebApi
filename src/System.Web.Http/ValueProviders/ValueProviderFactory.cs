// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders
{
    public abstract class ValueProviderFactory
    {
        public abstract IValueProvider GetValueProvider(HttpActionContext actionContext);
    }
}
