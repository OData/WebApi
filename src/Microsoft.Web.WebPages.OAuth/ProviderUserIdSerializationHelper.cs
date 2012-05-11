using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web.Security;

namespace Microsoft.Web.WebPages.OAuth
{
    internal static class ProviderUserIdSerializationHelper
    {
        private static byte[] padding = new byte[] { 0x85, 0xC5, 0x65, 0x72 };

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "The instances are disposed correctly.")]
        public static string ProtectData(string providerName, string providerUserId)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(providerName);
                bw.Write(providerUserId);
                bw.Flush();
                byte[] serializedWithPadding = new byte[ms.Length + padding.Length];
                Buffer.BlockCopy(padding, 0, serializedWithPadding, 0, padding.Length);
                Buffer.BlockCopy(ms.GetBuffer(), 0, serializedWithPadding, padding.Length, (int)ms.Length);
                return MachineKey.Encode(serializedWithPadding, MachineKeyProtection.All);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "The instances are disposed correctly.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exception are being caught on purpose.")]
        public static bool UnprotectData(string protectedData, out string providerName, out string providerUserId)
        {
            providerName = null;
            providerUserId = null;
            if (String.IsNullOrEmpty(protectedData))
            {
                return false;
            }

            byte[] decodedWithPadding = MachineKey.Decode(protectedData, MachineKeyProtection.All);

            if (decodedWithPadding.Length < padding.Length)
            {
                return false;
            }

            for (int i = 0; i < padding.Length; i++)
            {
                if (padding[i] != decodedWithPadding[i])
                {
                    return false;
                }
            }

            using (MemoryStream ms = new MemoryStream(decodedWithPadding, padding.Length, decodedWithPadding.Length - padding.Length))
            using (BinaryReader br = new BinaryReader(ms))
            {
                try
                {
                    string a = br.ReadString();
                    string b = br.ReadString();
                    if (ms.ReadByte() == -1)
                    {
                        providerName = a;
                        providerUserId = b;
                        return true;
                    }
                }
                catch
                {
                    // Any exceptions will result in this method returning false.
                }
            }
            return false;
        }
    }
}
