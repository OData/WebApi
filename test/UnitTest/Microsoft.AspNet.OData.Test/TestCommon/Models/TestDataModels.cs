// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Test.AspNet.OData.Common.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BellevueCustomer : Customer
    {
    }

    public class RedmondCustomer : Customer
    {
    }

    public class KirklandCustomer : Customer
    {
    }

    public class RentonCustomer : Customer
    {
    }

    public class IssaquahCustomer : Customer
    {
    }

    public class SeattleCustomer : Customer
    {
    }

    [AutoExpand]
    public class AutoExpandCustomer : Customer
    {
        public AutoExpandOrder Order { get; set; }
        public AutoExpandCustomer Friend { get; set; }
    }

    public class AutoExpandOrder
    {
        public int Id { get; set; }
        [AutoExpand]
        public AutoExpandChoiceOrder Choice { get; set; }
    }

    public class AutoExpandChoiceOrder
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
