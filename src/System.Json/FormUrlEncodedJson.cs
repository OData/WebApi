using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Json
{
    /// <summary>
    /// This class provides a low-level API for parsing HTML form URL-encoded data, also known as <c>application/x-www-form-urlencoded</c> 
    /// data. The output of the parser is a <see cref="JsonObject"/> instance. 
    /// <remarks>This is a low-level API intended for use by other APIs. It has been optimized for performance and 
    /// is not intended to be called directly from user code.</remarks>
    /// </summary>
    public static class FormUrlEncodedJson
    {
        private const string ApplicationFormUrlEncoded = @"application/x-www-form-urlencoded";
        private const int MinDepth = 0;

        private static readonly string[] _emptyPath = new string[]
        {
            String.Empty
        };

        /// <summary>
        /// Parses a collection of query string values as a <see cref="System.Json.JsonObject"/>.
        /// </summary>
        /// <remarks>This is a low-level API intended for use by other APIs. It has been optimized for performance and 
        /// is not intended to be called directly from user code.</remarks>
        /// <param name="nameValuePairs">The collection of query string name-value pairs parsed in lexical order. Both names
        /// and values must be un-escaped so that they don't contain any <see cref="Uri"/> encoding.</param>
        /// <returns>The <see cref="System.Json.JsonObject"/> corresponding to the given query string values.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a low-level API used by other APIs to provide end-user functionality.")]
        public static JsonObject Parse(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
        {
            return ParseInternal(nameValuePairs, Int32.MaxValue, true);
        }

        /// <summary>
        /// Parses a collection of query string values as a <see cref="System.Json.JsonObject"/>.
        /// </summary>
        /// <remarks>This is a low-level API intended for use by other APIs. It has been optimized for performance and 
        /// is not intended to be called directly from user code.</remarks>
        /// <param name="nameValuePairs">The collection of query string name-value pairs parsed in lexical order. Both names
        /// and values must be un-escaped so that they don't contain any <see cref="Uri"/> encoding.</param>
        /// <param name="maxDepth">The maximum depth of object graph encoded as <c>x-www-form-urlencoded</c>.</param>
        /// <returns>The <see cref="System.Json.JsonObject"/> corresponding to the given query string values.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a low-level API used by other APIs to provide end-user functionality.")]
        public static JsonObject Parse(IEnumerable<KeyValuePair<string, string>> nameValuePairs, int maxDepth)
        {
            return ParseInternal(nameValuePairs, maxDepth, true);
        }

        /// <summary>
        /// Parses a collection of query string values as a <see cref="System.Json.JsonObject"/>.
        /// </summary>
        /// <remarks>This is a low-level API intended for use by other APIs. It has been optimized for performance and 
        /// is not intended to be called directly from user code.</remarks>
        /// <param name="nameValuePairs">The collection of query string name-value pairs parsed in lexical order. Both names
        /// and values must be un-escaped so that they don't contain any <see cref="Uri"/> encoding.</param>
        /// <param name="value">The parsed result or null if parsing failed.</param>
        /// <returns><c>true</c> if <paramref name="nameValuePairs"/> was parsed successfully; otherwise false.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a low-level API used by other APIs to provide end-user functionality.")]
        public static bool TryParse(IEnumerable<KeyValuePair<string, string>> nameValuePairs, out JsonObject value)
        {
            return (value = ParseInternal(nameValuePairs, Int32.MaxValue, false)) != null;
        }

        /// <summary>
        /// Parses a collection of query string values as a <see cref="System.Json.JsonObject"/>.
        /// </summary>
        /// <remarks>This is a low-level API intended for use by other APIs. It has been optimized for performance and 
        /// is not intended to be called directly from user code.</remarks>
        /// <param name="nameValuePairs">The collection of query string name-value pairs parsed in lexical order. Both names
        /// and values must be un-escaped so that they don't contain any <see cref="Uri"/> encoding.</param>
        /// <param name="maxDepth">The maximum depth of object graph encoded as <c>x-www-form-urlencoded</c>.</param>
        /// <param name="value">The parsed result or null if parsing failed.</param>
        /// <returns><c>true</c> if <paramref name="nameValuePairs"/> was parsed successfully; otherwise false.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a low-level API used by other APIs to provide end-user functionality.")]
        public static bool TryParse(IEnumerable<KeyValuePair<string, string>> nameValuePairs, int maxDepth, out JsonObject value)
        {
            return (value = ParseInternal(nameValuePairs, maxDepth, false)) != null;
        }

        /// <summary>
        /// Parses a collection of query string values as a <see cref="System.Json.JsonObject"/>.
        /// </summary>
        /// <remarks>This is a low-level API intended for use by other APIs. It has been optimized for performance and 
        /// is not intended to be called directly from user code.</remarks>
        /// <param name="nameValuePairs">The collection of query string name-value pairs parsed in lexical order. Both names
        /// and values must be un-escaped so that they don't contain any <see cref="Uri"/> encoding.</param>
        /// <param name="maxDepth">The maximum depth of object graph encoded as <c>x-www-form-urlencoded</c>.</param>
        /// <param name="throwOnError">Indicates whether to throw an exception on error or return false</param>
        /// <returns>The <see cref="System.Json.JsonObject"/> corresponding to the given query string values.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a low-level API used by other APIs to provide end-user functionality.")]
        private static JsonObject ParseInternal(IEnumerable<KeyValuePair<string, string>> nameValuePairs, int maxDepth, bool throwOnError)
        {
            if (nameValuePairs == null)
            {
                throw new ArgumentNullException("nameValuePairs");
            }

            if (maxDepth <= MinDepth)
            {
                throw new ArgumentException(RS.Format(Properties.Resources.MinParameterSize, MinDepth), "maxDepth");
            }

            JsonObject result = new JsonObject();
            foreach (var nameValuePair in nameValuePairs)
            {
                if (nameValuePair.Key == null)
                {
                    if (String.IsNullOrEmpty(nameValuePair.Value))
                    {
                        if (throwOnError)
                        {
                            throw new ArgumentException(Properties.Resources.QueryStringNameShouldNotNull, "nameValuePairs");
                        }

                        return null;
                    }

                    string[] path = new string[] { nameValuePair.Value };
                    if (!Insert(result, path, null, throwOnError))
                    {
                        return null;
                    }
                }
                else
                {
                    string[] path = GetPath(nameValuePair.Key, maxDepth, throwOnError);
                    if (path == null || !Insert(result, path, nameValuePair.Value, throwOnError))
                    {
                        return null;
                    }
                }
            }

            FixContiguousArrays(result);
            return result;
        }

        private static string[] GetPath(string key, int maxDepth, bool throwOnError)
        {
            Contract.Assert(key != null, "Key cannot be null (this function is only called by Parse if key != null)");

            if (String.IsNullOrWhiteSpace(key))
            {
                return _emptyPath;
            }

            if (!ValidateQueryString(key, throwOnError))
            {
                return null;
            }

            string[] path = key.Split('[');
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i].EndsWith("]", StringComparison.Ordinal))
                {
                    path[i] = path[i].Substring(0, path[i].Length - 1);
                }
            }

            // For consistency with JSON, the depth of a[b]=1 is 3 (which is the depth of {a:{b:1}}, given
            // that in the JSON-XML mapping there's a <root> element wrapping the JSON object:
            // <root><a><b>1</b></a></root>. So if the length of the path is greater than *or equal* to
            // maxDepth, then we throw.
            if (path.Length >= maxDepth)
            {
                if (throwOnError)
                {
                    throw new ArgumentException(RS.Format(Properties.Resources.MaxDepthExceeded, maxDepth));
                }

                return null;
            }

            return path;
        }

        private static bool ValidateQueryString(string key, bool throwOnError)
        {
            bool hasUnMatchedLeftBraket = false;
            for (int i = 0; i < key.Length; i++)
            {
                switch (key[i])
                {
                    case '[':
                        if (!hasUnMatchedLeftBraket)
                        {
                            hasUnMatchedLeftBraket = true;
                        }
                        else
                        {
                            if (throwOnError)
                            {
                                throw new ArgumentException(RS.Format(Properties.Resources.NestedBracketNotValid, ApplicationFormUrlEncoded, i));
                            }

                            return false;
                        }

                        break;
                    case ']':
                        if (hasUnMatchedLeftBraket)
                        {
                            hasUnMatchedLeftBraket = false;
                        }
                        else
                        {
                            if (throwOnError)
                            {
                                throw new ArgumentException(RS.Format(Properties.Resources.UnMatchedBracketNotValid, ApplicationFormUrlEncoded, i));
                            }

                            return false;
                        }

                        break;
                }
            }

            if (hasUnMatchedLeftBraket)
            {
                if (throwOnError)
                {
                    throw new ArgumentException(RS.Format(Properties.Resources.NestedBracketNotValid, ApplicationFormUrlEncoded, key.LastIndexOf('[')));
                }

                return false;
            }

            return true;
        }

        private static bool Insert(JsonObject root, string[] path, string value, bool throwOnError)
        {
            // to-do: verify consistent with new parsing, whether single value is in path or value
            Contract.Assert(root != null, "Root object can't be null");

            JsonObject current = root;
            JsonObject parent = null;

            for (int i = 0; i < path.Length - 1; i++)
            {
                if (String.IsNullOrEmpty(path[i]))
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException(RS.Format(Properties.Resources.InvalidArrayInsert, BuildPathString(path, i)));
                    }

                    return false;
                }

                if (!current.ContainsKey(path[i]))
                {
                    current[path[i]] = new JsonObject();
                }
                else
                {
                    // Since the loop goes up to the next-to-last item in the path, if we hit a null
                    // or a primitive, then we have a mismatching node.
                    if (current[path[i]] == null || current[path[i]] is JsonPrimitive)
                    {
                        if (throwOnError)
                        {
                            throw new ArgumentException(RS.Format(Properties.Resources.FormUrlEncodedMismatchingTypes, BuildPathString(path, i)));
                        }

                        return false;
                    }
                }

                parent = current;
                current = current[path[i]] as JsonObject;
            }

            string lastKey = path[path.Length - 1];
            if (String.IsNullOrEmpty(lastKey) && path.Length > 1)
            {
                if (!AddToArray(parent, path, value, throwOnError))
                {
                    return false;
                }
            }
            else
            {
                if (current == null)
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException(RS.Format(Properties.Resources.FormUrlEncodedMismatchingTypes, BuildPathString(path, path.Length - 1)));
                    }

                    return false;
                }

                if (!AddToObject(current, path, value, throwOnError))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AddToObject(JsonObject obj, string[] path, string value, bool throwOnError)
        {
            Contract.Assert(obj != null, "JsonObject cannot be null");

            int pathIndex = path.Length - 1;
            string key = path[pathIndex];

            if (obj.ContainsKey(key))
            {
                if (obj[key] == null)
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException(RS.Format(Properties.Resources.FormUrlEncodedMismatchingTypes, BuildPathString(path, pathIndex)));
                    }

                    return false;
                }

                bool isRoot = path.Length == 1;
                if (isRoot)
                {
                    // jQuery 1.3 behavior, make it into an array(object) if primitive
                    if (obj[key].JsonType == JsonType.String)
                    {
                        string oldValue = obj[key].ReadAs<string>();
                        JsonObject jo = new JsonObject();
                        jo.Add("0", oldValue);
                        jo.Add("1", value);
                        obj[key] = jo;
                    }
                    else if (obj[key] is JsonObject)
                    {
                        // if it was already an object, simply add the value
                        JsonObject jo = obj[key] as JsonObject;
                        string index = GetIndex(jo.Keys, throwOnError);
                        if (index == null)
                        {
                            return false;
                        }

                        jo.Add(index, value);
                    }
                }
                else
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException(RS.Format(Properties.Resources.JQuery13CompatModeNotSupportNestedJson, BuildPathString(path, pathIndex)));
                    }

                    return false;
                }
            }
            else
            {
                // if the object didn't contain the key, simply add it now
                obj[key] = value;
            }

            return true;
        }

        // JsonObject passed in is semantically an array
        private static bool AddToArray(JsonObject parent, string[] path, string value, bool throwOnError)
        {
            Contract.Assert(parent != null, "Parent cannot be null");
            Contract.Assert(path.Length >= 2, "The path must be at least 2, one for the ending [], and one for before the '[' (which can be empty)");

            string parentPath = path[path.Length - 2];

            Contract.Assert(parent.ContainsKey(parentPath), "It was added on insert to get to this point");
            JsonObject jo = parent[parentPath] as JsonObject;

            if (jo == null)
            {
                // a[b][c]=1&a[b][]=2 => invalid
                if (throwOnError)
                {
                    throw new ArgumentException(RS.Format(Properties.Resources.FormUrlEncodedMismatchingTypes, BuildPathString(path, path.Length - 1)));
                }

                return false;
            }
            else
            {
                string index = GetIndex(jo.Keys, throwOnError);
                if (index == null)
                {
                    return false;
                }

                jo.Add(index, value);
            }

            return true;
        }

        // TODO: consider optimize it by only look at the last one
        private static string GetIndex(IEnumerable<string> keys, bool throwOnError)
        {
            int max = -1;
            foreach (var key in keys)
            {
                int tempInt;
                if (Int32.TryParse(key, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out tempInt) && tempInt > max)
                {
                    max = tempInt;
                }
                else
                {
                    if (throwOnError)
                    {
                        throw new ArgumentException(RS.Format(Properties.Resources.FormUrlEncodedMismatchingTypes, key));
                    }

                    return null;
                }
            }

            max++;
            return max.ToString(CultureInfo.InvariantCulture);
        }

        private static void FixContiguousArrays(JsonValue jv)
        {
            JsonArray ja = jv as JsonArray;

            if (ja != null)
            {
                for (int i = 0; i < ja.Count; i++)
                {
                    if (ja[i] != null)
                    {
                        ja[i] = FixSingleContiguousArray(ja[i]);
                        FixContiguousArrays(ja[i]);
                    }
                }
            }
            else
            {
                JsonObject jo = jv as JsonObject;

                if (jo != null)
                {
                    List<string> keys = new List<string>(jo.Keys);
                    foreach (string key in keys)
                    {
                        if (jo[key] != null)
                        {
                            jo[key] = FixSingleContiguousArray(jo[key]);
                            FixContiguousArrays(jo[key]);
                        }
                    }
                }
            }

            //// do nothing for primitives
        }

        private static JsonValue FixSingleContiguousArray(JsonValue original)
        {
            JsonObject jo = original as JsonObject;
            if (jo != null && jo.Count > 0)
            {
                List<string> childKeys = new List<string>(jo.Keys);
                List<string> sortedKeys;
                if (CanBecomeArray(childKeys, out sortedKeys))
                {
                    JsonArray newResult = new JsonArray();
                    foreach (string sortedKey in sortedKeys)
                    {
                        newResult.Add(jo[sortedKey]);
                    }

                    return newResult;
                }
            }

            return original;
        }

        private static bool CanBecomeArray(List<string> keys, out List<string> sortedKeys)
        {
            List<ArrayCandidate> intKeys = new List<ArrayCandidate>();
            sortedKeys = null;
            bool areContiguousIndices = true;
            foreach (string key in keys)
            {
                int intKey;
                if (!Int32.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out intKey))
                {
                    // if not a non-negative number, it cannot become an array
                    areContiguousIndices = false;
                    break;
                }

                string strKey = intKey.ToString(CultureInfo.InvariantCulture);
                if (!strKey.Equals(key, StringComparison.Ordinal))
                {
                    // int.Parse returned true, but it's not really the same number.
                    // It's the case for strings such as "1\0".
                    areContiguousIndices = false;
                    break;
                }

                intKeys.Add(new ArrayCandidate(intKey, strKey));
            }

            if (areContiguousIndices)
            {
                intKeys.Sort((x, y) => x.Key - y.Key);

                for (int i = 0; i < intKeys.Count; i++)
                {
                    if (intKeys[i].Key != i)
                    {
                        areContiguousIndices = false;
                        break;
                    }
                }
            }

            if (areContiguousIndices)
            {
                sortedKeys = new List<string>(intKeys.Select(x => x.Value));
            }

            return areContiguousIndices;
        }

        private static string BuildPathString(string[] path, int i)
        {
            StringBuilder errorPath = new StringBuilder(path[0]);
            for (int p = 1; p <= i; p++)
            {
                errorPath.AppendFormat("[{0}]", path[p]);
            }

            return errorPath.ToString();
        }

        /// <summary>
        /// Class that wraps key-value pairs.
        /// </summary>
        /// <remarks>
        /// This use of this class avoids a FxCop warning CA908 which happens if using various generic types.
        /// </remarks>
        private class ArrayCandidate
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ArrayCandidate"/> class.
            /// </summary>
            /// <param name="key">The key of this <see cref="ArrayCandidate"/> instance.</param>
            /// <param name="value">The value of this <see cref="ArrayCandidate"/> instance.</param>
            public ArrayCandidate(int key, string value)
            {
                Key = key;
                Value = value;
            }

            /// <summary>
            /// Gets or sets the key of this <see cref="ArrayCandidate"/> instance.
            /// </summary>
            /// <value>
            /// The key of this <see cref="ArrayCandidate"/> instance.
            /// </value>
            public int Key { get; set; }

            /// <summary>
            /// Gets or sets the value of this <see cref="ArrayCandidate"/> instance.
            /// </summary>
            /// <value>
            /// The value of this <see cref="ArrayCandidate"/> instance.
            /// </value>
            public string Value { get; set; }
        }
    }
}
