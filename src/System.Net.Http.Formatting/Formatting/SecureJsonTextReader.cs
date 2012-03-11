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
            StringBuilder sb = new StringBuilder(s);
            for (int i = 0; i < sb.Length; i++)
            {
                char ch = sb[i];
                if (Char.IsLowSurrogate(ch))
                {
                    // Low surrogate with no preceding high surrogate; this char is replaced
                    sb[i] = UnicodeReplacementChar;
                }
                else if (Char.IsHighSurrogate(ch))
                {
                    // Potential start of a surrogate pair
                    if (i + 1 == sb.Length)
                    {
                        // last character is an unmatched surrogate - replace
                        sb[i] = UnicodeReplacementChar;
                    }
                    else
                    {
                        char nextChar = sb[i + 1];
                        if (Char.IsLowSurrogate(nextChar))
                        {
                            // the surrogate pair is valid
                            // skip the low surrogate char
                            i++;
                        }
                        else
                        {
                            // High surrogate not followed by low surrogate; original char is replaced
                            sb[i] = UnicodeReplacementChar;
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }
}