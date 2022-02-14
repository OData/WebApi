//-----------------------------------------------------------------------------
// <copyright file="ReferenceDepthContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Test.Common
{
    public class ReferenceDepthContext
    {
        int maxRefDepth;
        int currentRefDepth = -1;

        public ReferenceDepthContext(int maxRefDepth)
        {
            this.maxRefDepth = maxRefDepth;
        }

        public bool IncreamentCounter()
        {
            if (++currentRefDepth > this.maxRefDepth)
            {
                return false;
            }

            return true;
        }

        public void DecrementCounter()
        {
            --currentRefDepth;
        }
    }
}
