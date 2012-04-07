// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace System.Net.Http.Formatting
{
    internal class SecureJsonTextReader : JsonTextReader
    {
        private const char UnicodeReplacementChar = '\uFFFD';
        private readonly int _maxDepth;

        public SecureJsonTextReader(TextReader reader, int maxDepth)
            : base(reader)
        {
            _maxDepth = maxDepth;
        }

        public override object Value
        {
            get
            {
                if (ValueType == typeof(string))
                {
                    return FixUpInvalidUnicodeString(base.Value as string);
                }
                return base.Value;
            }
        }

        public override bool Read()
        {
            int initialDepth = Depth;
            bool didRead = base.Read();
            if (Depth > _maxDepth)
            {
                // Advance the reader past the initial depth to avoid more exceptions from this violation
                while (Depth > initialDepth)
                {
                    base.Read();
                }
                throw new JsonReaderQuotaException(RS.Format(Properties.Resources.JsonTooDeep, _maxDepth));
            }
            return didRead;
        }

        private static string FixUpInvalidUnicodeString(string s)
        {
            StringBuilder sb = null;

            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (Char.IsLowSurrogate(ch))
                {
                    // Low surrogate with no preceding high surrogate; this char is replaced
                    if (sb == null)
                    {
                        sb = new StringBuilder(s);
                    }
                    sb[i] = UnicodeReplacementChar;
                }
                else if (Char.IsHighSurrogate(ch))
                {
                    // Potential start of a surrogate pair
                    if (i + 1 == s.Length)
                    {
                        // last character is an unmatched surrogate - replace
                        if (sb == null)
                        {
                            sb = new StringBuilder(s);
                        }
                        sb[i] = UnicodeReplacementChar;
                    }
                    else
                    {
                        char nextChar = s[i + 1];
                        if (Char.IsLowSurrogate(nextChar))
                        {
                            // the surrogate pair is valid
                            // skip the low surrogate char
                            i++;
                        }
                        else
                        {
                            // High surrogate not followed by low surrogate; original char is replaced
                            if (sb == null)
                            {
                                sb = new StringBuilder(s);
                            }
                            sb[i] = UnicodeReplacementChar;
                        }
                    }
                }
            }
            return sb == null ? s : sb.ToString();
        }
    }
}