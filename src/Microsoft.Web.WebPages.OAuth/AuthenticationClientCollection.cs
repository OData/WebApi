// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using DotNetOpenAuth.AspNet;

namespace Microsoft.Web.WebPages.OAuth
{
    /// <summary>
    /// A collection to store instances of IAuthenticationClient by keying off ProviderName.
    /// </summary>
    internal sealed class AuthenticationClientCollection : KeyedCollection<string, IAuthenticationClient>
    {
        public AuthenticationClientCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        protected override string GetKeyForItem(IAuthenticationClient item)
        {
            return item.ProviderName;
        }
    }
}
