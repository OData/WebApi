// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Http.Properties;

namespace System.Web.Http.Internal
{
    // TODO: Once IVT relationship from Formatting DLL has been removed we flip to a link
    // so that we don't have two copies of this code.

    /// <summary>
    /// Helpers for encoding, decoding, and parsing URI query components.
    /// </summary>
    internal static class UriQueryUtility
    {
        public static NameValueCollection ParseQueryString(string query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            if (query.Length > 0 && query[0] == '?')
            {
                query = query.Substring(1);
            }

            return new HttpValueCollection(query);
        }

        [Serializable]
        internal class HttpValueCollection : NameValueCollection
        {
            [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Ported from WCF")]
            internal HttpValueCollection(string str)
                : base(StringComparer.OrdinalIgnoreCase)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    FillFromString(str, true);
                }

                IsReadOnly = false;
            }

            protected HttpValueCollection(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            public override string ToString()
            {
                return ToString(true, null);
            }

            internal void FillFromString(string s, bool urlencoded)
            {
                int l = (s != null) ? s.Length : 0;
                int i = 0;

                while (i < l)
                {
                    ThrowIfMaxHttpCollectionKeysExceeded();

                    // find next & while noting first = on the way (and if there are more)
                    int si = i;
                    int ti = -1;

                    while (i < l)
                    {
                        char ch = s[i];

                        if (ch == '=')
                        {
                            if (ti < 0)
                            {
                                ti = i;
                            }
                        }
                        else if (ch == '&')
                        {
                            break;
                        }

                        i++;
                    }

                    // extract the name / value pair
                    string name = string.Empty;
                    string value = string.Empty;

                    if (ti >= 0)
                    {
                        name = s.Substring(si, ti - si);
                        value = s.Substring(ti + 1, i - ti - 1);
                    }
                    else
                    {
                        value = s.Substring(si, i - si);
                    }

                    // add name / value pair to the collection
                    if (urlencoded)
                    {
                        Add(UriQueryUtility.UrlDecode(name), UriQueryUtility.UrlDecode(value));
                    }
                    else
                    {
                        Add(name, value);
                    }

                    // trailing '&'
                    if (i == l - 1 && s[i] == '&')
                    {
                        Add(string.Empty, string.Empty);
                    }

                    i++;
                }
            }

            string ToString(bool urlencoded, IDictionary excludeKeys)
            {
                int n = Count;
                if (n == 0)
                {
                    return string.Empty;
                }

                StringBuilder s = new StringBuilder();
                string key, keyPrefix, item;

                for (int i = 0; i < n; i++)
                {
                    key = GetKey(i);

                    if (excludeKeys != null && key != null && excludeKeys[key] != null)
                    {
                        continue;
                    }

                    if (urlencoded)
                    {
                        key = UriQueryUtility.UrlEncode(key);
                    }

                    keyPrefix = (!string.IsNullOrEmpty(key)) ? (key + "=") : string.Empty;

                    ArrayList values = (ArrayList)BaseGet(i);
                    int numValues = (values != null) ? values.Count : 0;

                    if (s.Length > 0)
                    {
                        s.Append('&');
                    }

                    if (numValues == 1)
                    {
                        s.Append(keyPrefix);
                        item = (string)values[0];
                        if (urlencoded)
                        {
                            item = UriQueryUtility.UrlEncode(item);
                        }

                        s.Append(item);
                    }
                    else if (numValues == 0)
                    {
                        s.Append(keyPrefix);
                    }
                    else
                    {
                        for (int j = 0; j < numValues; j++)
                        {
                            if (j > 0)
                            {
                                s.Append('&');
                            }

                            s.Append(keyPrefix);
                            item = (string)values[j];
                            if (urlencoded)
                            {
                                item = UriQueryUtility.UrlEncode(item);
                            }

                            s.Append(item);
                        }
                    }
                }

                return s.ToString();
            }

            private void ThrowIfMaxHttpCollectionKeysExceeded()
            {
                if (Count >= MediaTypeFormatter.MaxHttpCollectionKeys)
                {
                    throw Error.InvalidOperation(SRResources.MaxHttpCollectionKeyLimitReached, MediaTypeFormatter.MaxHttpCollectionKeys, typeof(MediaTypeFormatter));
                }
            }
        }

        // The implementation below is ported from WebUtility for use in .Net 4

        #region UrlEncode implementation

        private static byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            byte[] encoded = UrlEncode(bytes, offset, count);

            return (alwaysCreateNewReturnValue && (encoded != null) && (encoded == bytes))
                ? (byte[])encoded.Clone()
                : encoded;
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            int cSpaces = 0;
            int cUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];

                if (ch == ' ')
                    cSpaces++;
                else if (!IsUrlSafeChar(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if (cSpaces == 0 && cUnsafe == 0)
                return bytes;

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (IsUrlSafeChar(ch))
                {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ')
                {
                    expandedBytes[pos++] = (byte)'+';
                }
                else
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        #endregion

        #region UrlEncode public methods

        public static string UrlEncode(string str)
        {
            if (str == null)
                return null;

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return Encoding.ASCII.GetString(UrlEncode(bytes, 0, bytes.Length, false /* alwaysCreateNewReturnValue */));
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            return UrlEncode(bytes, offset, count, true /* alwaysCreateNewReturnValue */);
        }

        #endregion

        #region UrlDecode implementation

        private static string UrlDecodeInternal(string value, Encoding encoding)
        {
            if (value == null)
            {
                return null;
            }

            int count = value.Length;
            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the string's chars collapsing %XX and %uXXXX and
            // appending each char as char, with exception of %XX constructs
            // that are appended as bytes

            for (int pos = 0; pos < count; pos++)
            {
                char ch = value[pos];

                if (ch == '+')
                {
                    ch = ' ';
                }
                else if (ch == '%' && pos < count - 2)
                {
                    if (value[pos + 1] == 'u' && pos < count - 5)
                    {
                        int h1 = HexToInt(value[pos + 2]);
                        int h2 = HexToInt(value[pos + 3]);
                        int h3 = HexToInt(value[pos + 4]);
                        int h4 = HexToInt(value[pos + 5]);

                        if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0)
                        {   // valid 4 hex chars
                            ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                            pos += 5;

                            // only add as char
                            helper.AddChar(ch);
                            continue;
                        }
                    }
                    else
                    {
                        int h1 = HexToInt(value[pos + 1]);
                        int h2 = HexToInt(value[pos + 2]);

                        if (h1 >= 0 && h2 >= 0)
                        {     // valid 2 hex chars
                            byte b = (byte)((h1 << 4) | h2);
                            pos += 2;

                            // don't add as char
                            helper.AddByte(b);
                            continue;
                        }
                    }
                }

                if ((ch & 0xFF80) == 0)
                    helper.AddByte((byte)ch); // 7 bit have to go as bytes because of Unicode
                else
                    helper.AddChar(ch);
            }

            return helper.GetString();
        }

        private static byte[] UrlDecodeInternal(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            int decodedBytesCount = 0;
            byte[] decodedBytes = new byte[count];

            for (int i = 0; i < count; i++)
            {
                int pos = offset + i;
                byte b = bytes[pos];

                if (b == '+')
                {
                    b = (byte)' ';
                }
                else if (b == '%' && i < count - 2)
                {
                    int h1 = HexToInt((char)bytes[pos + 1]);
                    int h2 = HexToInt((char)bytes[pos + 2]);

                    if (h1 >= 0 && h2 >= 0)
                    {     // valid 2 hex chars
                        b = (byte)((h1 << 4) | h2);
                        i += 2;
                    }
                }

                decodedBytes[decodedBytesCount++] = b;
            }

            if (decodedBytesCount < decodedBytes.Length)
            {
                byte[] newDecodedBytes = new byte[decodedBytesCount];
                Array.Copy(decodedBytes, newDecodedBytes, decodedBytesCount);
                decodedBytes = newDecodedBytes;
            }

            return decodedBytes;
        }

        #endregion

        #region UrlDecode public methods

        public static string UrlDecode(string str)
        {
            if (str == null)
                return null;

            return UrlDecodeInternal(str, Encoding.UTF8);
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count)
        {
            return UrlDecodeInternal(bytes, offset, count);
        }

        #endregion

        #region Helper methods

        private static int HexToInt(char h)
        {
            return (h >= '0' && h <= '9') ? h - '0' :
            (h >= 'a' && h <= 'f') ? h - 'a' + 10 :
            (h >= 'A' && h <= 'F') ? h - 'A' + 10 :
            -1;
        }

        private static char IntToHex(int n)
        {
            Contract.Assert(n < 0x10);

            if (n <= 9)
                return (char)(n + (int)'0');
            else
                return (char)(n - 10 + (int)'a');
        }

        // Set of safe chars, from RFC 1738.4 minus '+'
        private static bool IsUrlSafeChar(char ch)
        {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;

            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if (bytes == null && count == 0)
                return false;
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            return true;
        }

        #endregion

        #region UrlDecoder nested class

        // Internal class to facilitate URL decoding -- keeps char buffer and byte buffer, allows appending of either chars or bytes
        private class UrlDecoder
        {
            private int _bufferSize;

            // Accumulate characters in a special array
            private int _numChars;
            private char[] _charBuffer;

            // Accumulate bytes for decoding into characters in a special array
            private int _numBytes;
            private byte[] _byteBuffer;

            // Encoding to convert chars to bytes
            private Encoding _encoding;

            private void FlushBytes()
            {
                if (_numBytes > 0)
                {
                    _numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
                    _numBytes = 0;
                }
            }

            internal UrlDecoder(int bufferSize, Encoding encoding)
            {
                _bufferSize = bufferSize;
                _encoding = encoding;

                _charBuffer = new char[bufferSize];
                // byte buffer created on demand
            }

            internal void AddChar(char ch)
            {
                if (_numBytes > 0)
                    FlushBytes();

                _charBuffer[_numChars++] = ch;
            }

            internal void AddByte(byte b)
            {
                if (_byteBuffer == null)
                    _byteBuffer = new byte[_bufferSize];

                _byteBuffer[_numBytes++] = b;
            }

            internal String GetString()
            {
                if (_numBytes > 0)
                    FlushBytes();

                if (_numChars > 0)
                    return new String(_charBuffer, 0, _numChars);
                else
                    return String.Empty;
            }
        }

        #endregion
    }
}