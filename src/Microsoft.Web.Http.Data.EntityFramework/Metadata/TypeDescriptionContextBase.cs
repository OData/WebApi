// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Web.Http.Data.EntityFramework.Metadata
{
    /// <summary>
    /// Base class for LTS and EF type description contexts
    /// </summary>
    internal abstract class TypeDescriptionContextBase
    {
        /// <summary>
        /// Given a suggested name and a collection of existing names, this method
        /// creates a unique name by appending a numerix suffix as required.
        /// </summary>
        /// <param name="suggested">The desired name</param>
        /// <param name="existing">Collection of existing names</param>
        /// <returns>The unique name</returns>
        protected static string MakeUniqueName(string suggested, IEnumerable<string> existing)
        {
            int i = 1;
            string currSuggestion = suggested;
            while (existing.Contains(currSuggestion))
            {
                currSuggestion = suggested + (i++).ToString(CultureInfo.InvariantCulture);
            }

            return currSuggestion;
        }
    }
}
