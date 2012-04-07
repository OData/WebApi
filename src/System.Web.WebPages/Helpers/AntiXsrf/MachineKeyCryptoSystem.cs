// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Text;
using System.Web.Security;

namespace System.Web.Helpers.AntiXsrf
{
    // Interfaces with the System.Web.MachineKey static class
    internal sealed class MachineKeyCryptoSystem : ICryptoSystem
    {
        // This is the magic header that identifies an AntiForgeryToken payload.
        // It helps differentiate this from other encrypted payloads.
        private const uint MagicHeader = 0x8587f266;

        private readonly Func<string, MachineKeyProtection, byte[]> _decoder;
        private readonly Func<byte[], MachineKeyProtection, string> _encoder;

        public MachineKeyCryptoSystem()
            : this(MachineKey.Encode, MachineKey.Decode)
        {
        }

        // for unit testing
        internal MachineKeyCryptoSystem(Func<byte[], MachineKeyProtection, string> encoder, Func<string, MachineKeyProtection, byte[]> decoder)
        {
            _encoder = encoder;
            _decoder = decoder;
        }

        public string Protect(byte[] data)
        {
            byte[] dataWithHeader = new byte[data.Length + 4];
            Buffer.BlockCopy(data, 0, dataWithHeader, 4, data.Length);
            unchecked
            {
                dataWithHeader[0] = (byte)(MagicHeader >> 24);
                dataWithHeader[1] = (byte)(MagicHeader >> 16);
                dataWithHeader[2] = (byte)(MagicHeader >> 8);
                dataWithHeader[3] = (byte)(MagicHeader);
            }

            string hex = _encoder(dataWithHeader, MachineKeyProtection.All);
            return HexToBase64(hex);
        }

        public byte[] Unprotect(string protectedData)
        {
            string hex = Base64ToHex(protectedData);
            byte[] dataWithHeader = _decoder(hex, MachineKeyProtection.All);

            if (dataWithHeader == null || dataWithHeader.Length < 4 || (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(dataWithHeader, 0)) != MagicHeader)
            {
                // the decoded data is blank or doesn't begin with the magic header
                return null;
            }

            byte[] retVal = new byte[dataWithHeader.Length - 4];
            Buffer.BlockCopy(dataWithHeader, 4, retVal, 0, retVal.Length);
            return retVal;
        }

        // String transformation helpers

        internal static string Base64ToHex(string base64)
        {
            StringBuilder builder = new StringBuilder((int)(base64.Length * 1.5));
            foreach (byte b in HttpServerUtility.UrlTokenDecode(base64))
            {
                builder.Append(HexDigit(b >> 4));
                builder.Append(HexDigit(b & 0x0F));
            }
            string result = builder.ToString();
            return result;
        }

        private static char HexDigit(int value)
        {
            return (char)(value > 9 ? value + '7' : value + '0');
        }

        private static int HexValue(char digit)
        {
            return digit > '9' ? digit - '7' : digit - '0';
        }

        internal static string HexToBase64(string hex)
        {
            int size = hex.Length / 2;
            byte[] bytes = new byte[size];
            for (int idx = 0; idx < size; idx++)
            {
                bytes[idx] = (byte)((HexValue(hex[idx * 2]) << 4) + HexValue(hex[(idx * 2) + 1]));
            }
            string result = HttpServerUtility.UrlTokenEncode(bytes);
            return result;
        }
    }
}
