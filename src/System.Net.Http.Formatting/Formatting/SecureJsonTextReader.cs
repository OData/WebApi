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
                if (this.ValueType == typeof(string))
                {
                    return FixUpInvalidUnicodeString(base.Value as string);
                }
                return base.Value;
            }
        }

        public override bool Read()
        {
            if (this.Depth > _maxDepth)
            {
                throw new JsonSerializationException(RS.Format(Properties.Resources.JsonTooDeep, _maxDepth));
            }
            return base.Read();
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