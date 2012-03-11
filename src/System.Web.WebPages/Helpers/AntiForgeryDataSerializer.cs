using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.WebPages.Resources;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    internal class AntiForgeryDataSerializer
    {
        // Testing hooks

        internal Func<string, byte[]> Decoder =
            (value) => MachineKey.Decode(Base64ToHex(value), MachineKeyProtection.All);

        internal Func<byte[], string> Encoder =
            (bytes) => HexToBase64(MachineKey.Encode(bytes, MachineKeyProtection.All).ToUpperInvariant());

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream is resilient to double-Dispose")]
        public virtual AntiForgeryData Deserialize(string serializedToken)
        {
            if (String.IsNullOrEmpty(serializedToken))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "serializedToken");
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(Decoder(serializedToken)))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        return new AntiForgeryData
                        {
                            Salt = reader.ReadString(),
                            Value = reader.ReadString(),
                            CreationDate = new DateTime(reader.ReadInt64()),
                            Username = reader.ReadString()
                        };
                    }
                }
            }
            catch
            {
                throw new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_ValidationFailed);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream is resilient to double-Dispose")]
        public virtual string Serialize(AntiForgeryData token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(token.Salt);
                    writer.Write(token.Value);
                    writer.Write(token.CreationDate.Ticks);
                    writer.Write(token.Username);

                    return Encoder(stream.ToArray());
                }
            }
        }

        // String transformation helpers

        private static string Base64ToHex(string base64)
        {
            StringBuilder builder = new StringBuilder(base64.Length * 4);
            foreach (byte b in Convert.FromBase64String(base64))
            {
                builder.Append(HexDigit(b >> 4));
                builder.Append(HexDigit(b & 0x0F));
            }
            string result = builder.ToString();
            return result;
        }

        internal static char HexDigit(int value)
        {
            return (char)(value > 9 ? value + '7' : value + '0');
        }

        internal static int HexValue(char digit)
        {
            return digit > '9' ? digit - '7' : digit - '0';
        }

        private static string HexToBase64(string hex)
        {
            int size = hex.Length / 2;
            byte[] bytes = new byte[size];
            for (int idx = 0; idx < size; idx++)
            {
                bytes[idx] = (byte)((HexValue(hex[idx * 2]) << 4) + HexValue(hex[(idx * 2) + 1]));
            }
            string result = Convert.ToBase64String(bytes);
            return result;
        }
    }
}
