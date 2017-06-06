using System.Collections.Generic;
using System.Linq;

namespace WebStack.QA.Instancing
{
    public class CharsToUseBuilder
    {
        private HashSet<char> charList = new HashSet<char>();
        public CharsToUseBuilder Append(char c)
        {
            if (!charList.Contains(c))
            {
                charList.Add(c);
            }
            return this;
        }

        public CharsToUseBuilder Append(string s)
        {
            foreach (char c in s)
            {
                Append(c);
            }
            return this;
        }

        public CharsToUseBuilder Append(char start, char end)
        {
            for (char c = start; c <= end; c++)
            {
                Append(c);
            }
            return this;
        }

        public CharsToUseBuilder Exclude(char c)
        {
            if (charList.Contains(c))
            {
                charList.Remove(c);
            }
            return this;
        }

        public CharsToUseBuilder Exclude(string s)
        {
            foreach (char c in s)
            {
                Exclude(c);
            }
            return this;
        }

        public override string ToString()
        {
            return new string(charList.ToArray());
        }
    }
}
