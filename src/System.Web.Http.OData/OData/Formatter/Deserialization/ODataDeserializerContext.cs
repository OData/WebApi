// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataDeserializerContext
    {
        private const int MaxReferenceDepth = 200;
        private int _currentReferenceDepth = 0;

        public bool IsPatchMode { get; set; }

        public bool IncrementCurrentReferenceDepth()
        {
            if (++_currentReferenceDepth > MaxReferenceDepth)
            {
                return false;
            }

            return true;
        }

        public void DecrementCurrentReferenceDepth()
        {
            _currentReferenceDepth--;
        }
    }
}
