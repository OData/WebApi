// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Class to hold Name and value of a key property
    /// </summary>
    public class KeyProperty
    {
        /// <summary>
        /// Name of the Key Property
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// Value of the Key  Property
        /// </summary>
        public object Value { private set; get; }
    }
}
