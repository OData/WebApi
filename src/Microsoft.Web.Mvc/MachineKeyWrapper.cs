// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Security;

namespace Microsoft.Web.Mvc
{
    // Concrete implementation of IMachineKey that talks to the static MachineKey type

    internal sealed class MachineKeyWrapper : IMachineKey
    {
        public byte[] Decode(string encodedData, MachineKeyProtection protectionOption)
        {
#pragma warning disable 0618 // Decode is [Obsolete] in 4.5
            return MachineKey.Decode(encodedData, protectionOption);
#pragma warning restore 0618
        }

        public string Encode(byte[] data, MachineKeyProtection protectionOption)
        {
#pragma warning disable 0618 // Encode is [Obsolete] in 4.5
            return MachineKey.Encode(data, protectionOption);
#pragma warning restore 0618
        }
    }
}
