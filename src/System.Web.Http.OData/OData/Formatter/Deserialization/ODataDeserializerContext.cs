// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataDeserializerContext
    {
        private int _maxReferenceDepth = 200;
        private int _currentReferenceDepth;

        public ODataDeserializerContext()
        {
            _currentReferenceDepth = -2;
        }

        public int MaxReferenceDepth
        {
            get { return _maxReferenceDepth; }
        }

        public bool IsPatchMode { get; set; }

        public bool IncrementCurrentReferenceDepth()
        {
            if (++_currentReferenceDepth > _maxReferenceDepth)
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
