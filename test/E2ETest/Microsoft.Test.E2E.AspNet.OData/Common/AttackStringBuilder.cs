//-----------------------------------------------------------------------------
// <copyright file="AttackStringBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Text;

namespace Microsoft.Test.E2E.AspNet.OData
{
    public class AttackStringBuilder
    {
        private StringBuilder sb = new StringBuilder();

        public AttackStringBuilder Repeat(string s, int times)
        {
            for (int i = 0; i < times; i++)
            {
                sb.AppendFormat(s, i);
            }

            return this;
        }

        public AttackStringBuilder Append(string s)
        {
            sb.Append(s);
            return this;
        }

        public AttackStringBuilder Remove(int length)
        {
            sb.Remove(sb.Length - length, length);
            return this;
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
