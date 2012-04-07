// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Security;

namespace Microsoft.Web.Mvc
{
    // Used for mocking out the static MachineKey type

    internal interface IMachineKey
    {
        byte[] Decode(string encodedData, MachineKeyProtection protectionOption);
        string Encode(byte[] data, MachineKeyProtection protectionOption);
    }
}
