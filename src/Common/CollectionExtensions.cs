// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;

namespace System.Collections.Generic
{
    /// <summary>
    /// Helper extension methods for fast use of collections.
    /// </summary>
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Return the only value from list, the type's default value if empty, or call the errorAction for 2 or more.
        /// </summary>
        public static T SingleDefaultOrError<T, TArg1>(this IList<T> list, Action<TArg1> errorAction, TArg1 errorArg1)
        {
            Contract.Assert(list != null);
            Contract.Assert(errorAction != null);

            switch (list.Count)
            {
                case 0:
                    return default(T);

                case 1:
                    T value = list[0];
                    return value;

                default:
                    errorAction(errorArg1);
                    return default(T);
            }
        }

        /// <summary>
        /// Returns a single value in list matching type TMatch if there is only one, null if there are none of type TMatch or calls the
        /// errorAction with errorArg1 if there is more than one.
        /// </summary>
        public static TMatch SingleOfTypeDefaultOrError<TInput, TMatch, TArg1>(this IList<TInput> list, Action<TArg1> errorAction, TArg1 errorArg1) where TMatch : class
        {
            Contract.Assert(list != null);
            Contract.Assert(errorAction != null);

            TMatch result = null;
            for (int i = 0; i < list.Count; i++)
            {
                TMatch typedValue = list[i] as TMatch;
                if (typedValue != null)
                {
                    if (result == null)
                    {
                        result = typedValue;
                    }
                    else
                    {
                        errorAction(errorArg1);
                        return null;
                    }
                }
            }
            return result;
        }
    }
}
