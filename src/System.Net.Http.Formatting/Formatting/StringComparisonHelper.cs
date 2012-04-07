// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Helper class for validating <see cref="StringComparison"/> values.
    /// </summary>
    internal static class StringComparisonHelper
    {
        private static readonly Type _stringComparisonType = typeof(StringComparison);

        /// <summary>
        /// Determines whether the specified <paramref name="value"/> is defined by the <see cref="StringComparison"/>
        /// enumeration.
        /// </summary>
        /// <param name="value">The value to verify.</param>
        /// <returns>
        /// <c>true</c> if the specified options is defined; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDefined(StringComparison value)
        {
            return value == StringComparison.CurrentCulture ||
                   value == StringComparison.CurrentCultureIgnoreCase ||
                   value == StringComparison.InvariantCulture ||
                   value == StringComparison.InvariantCultureIgnoreCase ||
                   value == StringComparison.Ordinal ||
                   value == StringComparison.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Validates the specified <paramref name="value"/> and throws an <see cref="InvalidEnumArgumentException"/>
        /// exception if not valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="parameterName">Name of the parameter to use if throwing exception.</param>
        public static void Validate(StringComparison value, string parameterName)
        {
            if (!IsDefined(value))
            {
                throw new InvalidEnumArgumentException(parameterName, (int)value, _stringComparisonType);
            }
        }
    }
}
