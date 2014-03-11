// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder.TestModels
{
    public class MySpecialOrder : MyOrder
    {
        public bool IsGift { get; set; }

        [Contained]
        public virtual Gift Gift { get; set; }
    }
}
