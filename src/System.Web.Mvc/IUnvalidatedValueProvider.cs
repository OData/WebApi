// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    // Represents a special IValueProvider that has the ability to skip request validation.
    public interface IUnvalidatedValueProvider : IValueProvider
    {
        ValueProviderResult GetValue(string key, bool skipValidation);
    }
}
