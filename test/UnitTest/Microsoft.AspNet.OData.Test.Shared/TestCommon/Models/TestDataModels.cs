//-----------------------------------------------------------------------------
// <copyright file="TestDataModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData.Test.Common.Models
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
