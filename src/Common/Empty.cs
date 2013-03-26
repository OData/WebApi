// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Collections.Generic
{
    /// <summary>
    /// Helper to provide empty instances with minimal allocation.
    /// </summary>
    internal static class Empty<T>
    {
        private static readonly T[] _emptyArray = new T[0];

        /// <summary>
        /// Returns a zero length array of type. Only allocates once per distinct type.
        /// </summary>
        public static T[] Array 
        { 
            get { return _emptyArray; } 
        }
    }
}
