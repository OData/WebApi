//-----------------------------------------------------------------------------
// <copyright file="TestDataModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

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

    public class NewCustomerUnmapped
    {
        [Key]
        public int Id { get; set; }
        [IgnoreDataMember]
        public string Name { get; set; }
        [NotMapped]
        public int Age { get; set; }
    }

    [DataContract]
    public class NewCustomerDataContract
    {
        [Key]
        public int Id { get; set; }
        [IgnoreDataMember]
        [DataMember]
        public string Name { get; set; }
        [IgnoreDataMember]
        public int Age { get; set; }
    }
}
