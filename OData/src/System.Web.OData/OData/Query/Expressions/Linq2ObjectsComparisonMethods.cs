// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.OData.Query.Expressions
{
    internal static class Linq2ObjectsComparisonMethods
    {
        /// <summary>Method info for byte array comparison.</summary>
        public static readonly MethodInfo AreByteArraysEqualMethodInfo =
            typeof(Linq2ObjectsComparisonMethods).GetMethod("AreByteArraysEqual");

        /// <summary>Method info for byte array comparison.</summary>
        public static readonly MethodInfo AreByteArraysNotEqualMethodInfo =
            typeof(Linq2ObjectsComparisonMethods).GetMethod("AreByteArraysNotEqual");

        /// <summary>Compares two byte arrays for equality.</summary>
        /// <param name="left">First byte array.</param>
        /// <param name="right">Second byte array.</param>
        /// <returns>true if the arrays are equal; false otherwise.</returns>
        public static bool AreByteArraysEqual(byte[] left, byte[] right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares two byte arrays for equality.</summary>
        /// <param name="left">First byte array.</param>
        /// <param name="right">Second byte array.</param>
        /// <returns>true if the arrays are not equal; false otherwise.</returns>
        public static bool AreByteArraysNotEqual(byte[] left, byte[] right)
        {
            return !AreByteArraysEqual(left, right);
        }
    }
}
