// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;

namespace System.Web.Mvc.Test
{
    public abstract class MockableUnvalidatedRequestValues : IUnvalidatedRequestValues
    {
        public abstract NameValueCollection Form { get; }
        public abstract NameValueCollection QueryString { get; }
        public abstract string this[string key] { get; }
    }
}
