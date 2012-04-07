// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;

namespace System.Web.Mvc
{
    // Used for mocking the UnvalidatedRequestValues type in System.Web.WebPages

    internal interface IUnvalidatedRequestValues
    {
        NameValueCollection Form { get; }
        NameValueCollection QueryString { get; }
        string this[string key] { get; }
    }
}
