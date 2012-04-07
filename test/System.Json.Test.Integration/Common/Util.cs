// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Json
{
    internal static class Util
    {
        public static bool CompareObjects<T>(T o1, T o2) where T : class
        {
            if ((o1 == null) != (o2 == null))
            {
                return false;
            }

            return (o1 == null) || o1.Equals(o2);
        }

        public static bool CompareNullable<T>(Nullable<T> n1, Nullable<T> n2) where T : struct
        {
            if (n1.HasValue != n2.HasValue)
            {
                return false;
            }

            return (!n1.HasValue) || n1.Value.Equals(n2.Value);
        }

        public static bool CompareLists<T>(List<T> list1, List<T> list2)
        {
            if (list1 == null)
            {
                return list2 == null;
            }

            if (list2 == null)
            {
                return false;
            }

            return CompareArrays(list1.ToArray(), list2.ToArray());
        }

        public static bool CompareDictionaries<K, V>(Dictionary<K, V> dict1, Dictionary<K, V> dict2)
            where K : IComparable
            where V : class
        {
            if (dict1 == null)
            {
                return dict2 == null;
            }

            if (dict2 == null)
            {
                return false;
            }

            List<K> keys1 = new List<K>(dict1.Keys);
            List<K> keys2 = new List<K>(dict2.Keys);
            keys1.Sort();
            keys2.Sort();
            if (!CompareLists<K>(keys1, keys2))
            {
                return false;
            }

            foreach (K key in keys1)
            {
                V value1 = dict1[key];
                V value2 = dict2[key];
                if (!CompareObjects<V>(value1, value2))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CompareArrays(Array array1, Array array2)
        {
            if (array1 == null)
            {
                return array2 == null;
            }

            if (array2 == null || array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                object o1 = array1.GetValue(i);
                object o2 = array2.GetValue(i);
                if ((o1 == null) != (o2 == null))
                {
                    return false;
                }

                if (o1 != null)
                {
                    if ((o1 is Array) && (o2 is Array))
                    {
                        if (!CompareArrays((Array)o1, (Array)o2))
                        {
                            return false;
                        }
                    }
                    else if (o1 is IEnumerable && o2 is IEnumerable)
                    {
                        if (!CompareArrays(ToObjectArray((IEnumerable)o1), ToObjectArray((IEnumerable)o2)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!o1.Equals(o2))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static int ComputeArrayHashCode(Array array)
        {
            if (array == null)
            {
                return 0;
            }

            int result = 0;
            result += array.Length;
            for (int i = 0; i < array.Length; i++)
            {
                object o = array.GetValue(i);
                if (o != null)
                {
                    if (o is Array)
                    {
                        result ^= ComputeArrayHashCode((Array)o);
                    }
                    else if (o is Enumerable)
                    {
                        result ^= ComputeArrayHashCode(ToObjectArray((IEnumerable)o));
                    }
                    else
                    {
                        result ^= o.GetHashCode();
                    }
                }
            }

            return result;
        }

        public static string EscapeString(object obj)
        {
            StringBuilder sb = new StringBuilder();
            if (obj == null)
            {
                return "<<null>>";
            }
            else
            {
                string str = obj.ToString();
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    if (c < ' ' || c > '~')
                    {
                        sb.AppendFormat("\\u{0:X4}", (int)c);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        static object[] ToObjectArray(IEnumerable enumerable)
        {
            List<object> result = new List<object>();
            foreach (var item in enumerable)
            {
                result.Add(item);
            }

            return result.ToArray();
        }
    }
}