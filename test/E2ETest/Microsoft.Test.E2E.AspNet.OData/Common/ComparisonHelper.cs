//-----------------------------------------------------------------------------
// <copyright file="ComparisonHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Test.E2E.AspNet.OData
{
    public static class ComparisonHelper
    {
        public static bool CompareObjects<T>(T o1, T o2) where T : class
        {
            if ((o1 == null) != (o2 == null))
            {
                return false;
            }

            return (o1 == null) || o1.Equals(o2);
        }

        public static bool CompareNullable<T>(T? n1, T? n2) where T : struct
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

        public static bool CompareDictionaries<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
            where TKey : IComparable
            where TValue : class
        {
            if (dict1 == null)
            {
                return dict2 == null;
            }

            if (dict2 == null)
            {
                return false;
            }

            List<TKey> keys1 = new List<TKey>(dict1.Keys);
            List<TKey> keys2 = new List<TKey>(dict2.Keys);
            keys1.Sort();
            keys2.Sort();
            if (!CompareLists<TKey>(keys1, keys2))
            {
                return false;
            }

            foreach (TKey key in keys1)
            {
                TValue value1 = dict1[key];
                TValue value2 = dict2[key];
                if (!CompareObjects<TValue>(value1, value2))
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

        public static bool CompareTicks(object o1, object o2)
        {
            if (o1 == null)
            {
                return o2 == null;
            }

            Type objectType;
            if ((objectType = o1.GetType()) != o2.GetType())
            {
                throw new Exception(String.Format("Objects o1 {0} and o2 {1} are of different types", objectType.Name, o2.GetType().Name));
            }

            if (objectType == typeof(DateTime))
            {
                return ((DateTime)o1).Ticks == ((DateTime)o2).Ticks;
            }
            else if (objectType == typeof(DateTimeOffset))
            {
                return ((DateTimeOffset)o1).UtcTicks == ((DateTimeOffset)o2).UtcTicks;
            }
            else if (objectType == typeof(DateTime?))
            {
                if (((DateTime?)o1).HasValue != ((DateTime?)o2).HasValue)
                {
                    return false;
                }

                if (((DateTime?)o1).HasValue)
                {
                    return ((DateTime?)o1).Value.Ticks == ((DateTime?)o2).Value.Ticks;
                }

                return true;
            }
            else if (objectType == typeof(DateTimeOffset?))
            {
                if (((DateTimeOffset?)o1).HasValue != ((DateTimeOffset?)o2).HasValue)
                {
                    return false;
                }

                if (((DateTimeOffset?)o1).HasValue)
                {
                    return ((DateTimeOffset?)o1).Value.UtcTicks == ((DateTimeOffset?)o2).Value.UtcTicks;
                }

                return true;
            }
            else
            {
                throw new Exception("Cannot compare Ticks with this type {0}" + objectType);
            }
        }

        public static int ComputeArrayHashCode(Array array)
        {
            if (array == null)
            {
                return 0;
            }

            int result = 0;
            result += array.Length;
            for (var i = 0; i < array.Length; i++)
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

                foreach (var c in str)
                {
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

        private static object[] ToObjectArray(IEnumerable enumerable)
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
