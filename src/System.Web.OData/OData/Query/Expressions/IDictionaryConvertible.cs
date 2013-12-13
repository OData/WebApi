// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.OData.Query.Expressions
{
    // SelectExpandWrapper<T> implements this interface for custom json serialization.
    internal interface IDictionaryConvertible
    {
        Dictionary<string, object> ToDictionary();
    }
}
