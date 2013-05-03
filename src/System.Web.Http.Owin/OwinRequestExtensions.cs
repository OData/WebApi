// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace System.Web.Http
{
    // It is likely OwinRequest will support these methods directly in the future, in which case this class can be
    // removed.
    internal static class OwinRequestExtensions
    {
        public static async Task<IIdentity> AuthenticateAsync(this OwinRequest request, string authenticationType)
        {
            List<IIdentity> identities = new List<IIdentity>();
            await request.Authenticate(new string[] { authenticationType }, (identity, ignore1, ignore2, ignore3) =>
            {
                identities.Add(identity);
            }, null);

            return identities.SingleOrDefault();
        }
    }
}
