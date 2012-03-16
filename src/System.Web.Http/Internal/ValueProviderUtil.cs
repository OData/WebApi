// Combination of code from System.Web.Mvc and Microsoft.Web.Mvc

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http.Internal
{
    internal static class ValueProviderUtil
    {
        internal static bool CollectionContainsPrefix(IEnumerable<string> collection, string prefix)
        {
            foreach (string key in collection)
            {
                if (key != null)
                {
                    if (prefix.Length == 0)
                    {
                        return true; // shortcut - non-null key matches empty prefix
                    }

                    if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        if (key.Length == prefix.Length)
                        {
                            return true; // exact match
                        }
                        else
                        {
                            switch (key[prefix.Length])
                            {
                                case '.': // known separator characters
                                case '[':
                                    return true;
                            }
                        }
                    }
                }
            }

            return false; // nothing found
        }

        // Given "foo.bar", "foo.hello", "something.other", foo[abc].baz and asking for prefix "foo" will return:
        // - "bar"/"foo.bar"
        // - "hello"/"foo.hello"
        // - "abc"/"foo[abc]"
        internal static IDictionary<string, string> GetKeysFromPrefix(IEnumerable<string> collection, string prefix)
        {
            if (String.IsNullOrWhiteSpace(prefix))
            {
                return collection.ToDictionary(value => value, StringComparer.OrdinalIgnoreCase);
            }

            IDictionary<string, string> keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in collection)
            {
                if (entry != null)
                {
                    string key = null;
                    string fullName = null;

                    if (entry.Length == prefix.Length)
                    {
                        // No key in this entry
                        continue;
                    }

                    if (entry.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        int keyPosition = prefix.Length + 1;
                        switch (entry[prefix.Length])
                        {
                            case '.':
                                int dotPosition = entry.IndexOf('.', keyPosition);
                                if (dotPosition == -1)
                                {
                                    dotPosition = entry.Length;
                                }

                                key = entry.Substring(keyPosition, dotPosition - keyPosition);
                                fullName = entry.Substring(0, dotPosition);
                                break;

                            case '[':
                                int bracketPosition = entry.IndexOf(']', keyPosition);
                                if (bracketPosition == -1)
                                {
                                    // Malformed for dictionary
                                    continue;
                                }

                                key = entry.Substring(keyPosition, bracketPosition - keyPosition);
                                fullName = entry.Substring(0, bracketPosition + 1);
                                break;

                            default:
                                continue;
                        }

                        if (!keys.ContainsKey(key))
                        {
                            keys.Add(key, fullName);
                        }
                    }
                }
            }

            return keys;
        }

        // Given "foo.bar[baz].quux", this method will return:
        // - "foo.bar[baz].quux"
        // - "foo.bar[baz]"
        // - "foo.bar"
        // - "foo"
        internal static IEnumerable<string> GetPrefixes(string key)
        {
            yield return key;
            for (int i = key.Length - 1; i >= 0; i--)
            {
                switch (key[i])
                {
                    case '.':
                    case '[':
                        yield return key.Substring(0, i);
                        break;
                }
            }
        }

        internal static bool IsPrefixMatch(string prefix, string testString)
        {
            if (testString == null)
            {
                return false;
            }

            if (prefix.Length == 0)
            {
                return true; // shortcut - non-null testString matches empty prefix
            }

            if (!testString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false; // prefix doesn't match
            }

            if (testString.Length == prefix.Length)
            {
                return true; // exact match
            }

            // invariant: testString.Length > prefix.Length
            switch (testString[prefix.Length])
            {
                case '.':
                case '[':
                    return true; // known delimiters

                default:
                    return false; // not known delimiter
            }
        }
    }
}
