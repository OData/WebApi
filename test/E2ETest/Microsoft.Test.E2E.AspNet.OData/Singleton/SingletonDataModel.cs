//-----------------------------------------------------------------------------
// <copyright file="SingletonDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.Test.E2E.AspNet.OData.Singleton
{
    /// <summary>
    /// Present the EntityType "Partner"
    /// </summary>
    public class Partner
    {
        public int ID { get; set; }
        public string Name { get; set; }

        [Singleton]
        public Company Company { get; set;}
    }

    /// <summary>
    /// Present company category, which is an enum type
    /// </summary>
    public enum CompanyCategory
    {
        IT = 0,
        Communication = 1,
        Electronics = 2,
        Others = 3
    }

    /// <summary>
    /// Present the EntityType "Company"
    /// </summary>
    public class Company
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Int64 Revenue { get; set; }
        public CompanyCategory Category { get; set; }
        [NotCountable]
        public IList<Partner> Partners { get; set; }
        public IList<Office> Branches { get; set; }
    }

    /// <summary>
    /// Present a complex type
    /// </summary>
    public class Office
    {
        public string City { get; set; }
        public string Address { get; set; }
    }

    /// <summary>
    /// EntityType derives from "Company"
    /// </summary>
    public class SubCompany : Company
    {
        public string Location { get; set; }
        public string Description { get; set; }
        public Office Office { get; set; }
    }
}
