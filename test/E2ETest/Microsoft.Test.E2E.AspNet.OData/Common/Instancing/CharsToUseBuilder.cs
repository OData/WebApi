//-----------------------------------------------------------------------------
// <copyright file="CharsToUseBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    public class CharactorSet
    {
        private HashSet<char> _charList = new HashSet<char>();

        public CharactorSet Append(char c)
        {
            if (!_charList.Contains(c))
            {
                _charList.Add(c);
            }

            return this;
        }

        public CharactorSet Append(string s)
        {
            foreach (char c in s)
            {
                Append(c);
            }

            return this;
        }

        public CharactorSet Append(char start, char end)
        {
            for (char c = start; c <= end; c++)
            {
                Append(c);
            }

            return this;
        }

        public CharactorSet Exclude(char c)
        {
            if (_charList.Contains(c))
            {
                _charList.Remove(c);
            }

            return this;
        }

        public CharactorSet Exclude(string s)
        {
            foreach (char c in s)
            {
                Exclude(c);
            }
            
            return this;
        }

        public override string ToString()
        {
            return new string(_charList.ToArray());
        }
    }
}
