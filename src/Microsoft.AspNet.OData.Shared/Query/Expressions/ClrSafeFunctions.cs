//-----------------------------------------------------------------------------
// <copyright file="ClrSafeFunctions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// This class contains safe equivalents of CLR functions that
    /// could throw exceptions at runtime.
    /// </summary>
    internal class ClrSafeFunctions
    {
        public static string SubstringStart(string str, int startIndex)
        {
            Contract.Assert(str != null);

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            // String.Substring(int) accepts startIndex==length
            return startIndex <= str.Length
                    ? str.Substring(startIndex)
                    : String.Empty;
        }

        public static string SubstringStartAndLength(string str, int startIndex, int length)
        {
            Contract.Assert(str != null);

            if (startIndex < 0)
            {
                startIndex = 0;
            }
            
            int strLength = str.Length;

            // String.Substring(int, int) accepts startIndex==length
            if (startIndex > strLength)
            {
                return String.Empty;
            }

            length = Math.Min(length, strLength - startIndex);
            return length >= 0
                    ? str.Substring(startIndex, length)
                    : String.Empty;
        }
    }
}
