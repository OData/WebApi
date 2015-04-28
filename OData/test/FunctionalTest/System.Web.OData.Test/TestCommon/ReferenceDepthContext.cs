// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData
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
