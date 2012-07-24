// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.ValueProviders
{
    public interface IEnumerableValueProvider : IValueProvider
    {
        IDictionary<string, string> GetKeysFromPrefix(string prefix);
    }
}
