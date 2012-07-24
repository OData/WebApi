// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf.Test
{
    // An ICryptoSystem that can be passed to MoQ
    public abstract class MockableCryptoSystem : ICryptoSystem
    {
        public abstract string Protect(byte[] data);
        public abstract byte[] Unprotect(string protectedData);
    }
}
