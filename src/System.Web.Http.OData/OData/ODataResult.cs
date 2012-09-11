// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace System.Web.Http.OData
{
    [DataContract]
    public abstract class ODataResult
    {
        private long? _count;

        protected ODataResult(Uri nextPageLink, long? count)
        {
            NextPageLink = nextPageLink;
            Count = count;
        }

        [DataMember]
        public Uri NextPageLink
        {
            get;
            private set;
        }

        [DataMember]
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
