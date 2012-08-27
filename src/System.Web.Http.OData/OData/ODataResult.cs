// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData
{
    public abstract class ODataResult
    {
        private long? _count;

        protected ODataResult(Uri nextPageLink, long? count)
        {
            NextPageLink = nextPageLink;
            Count = count;
        }

        public Uri NextPageLink
        {
            get;
            private set;
        }

        public long? Count
        {
            get 
            {
                return _count;
            }
            private set
            {
                if (value.HasValue && value.Value <= 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value.Value, 1);
                }
                _count = value;
            }
        }
    }
}
