//-----------------------------------------------------------------------------
// <copyright file="OpenTypeDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Test.E2E.AspNet.OData.OpenType
{
    using System;
    using System.Collections.Generic;

    // This is an open entity type.
    public class Account
    {
        public Account()
        {
            DynamicProperties = new Dictionary<string, object>();
        }
        public int Id { get; set; }
        public String Name { get; set; }
        public AccountInfo AccountInfo { get; set; }
        public Address Address { get; set; }
        public Tags Tags { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    // The base type is open, so it is open too.
    public class PremiumAccount : Account
    {
        public DateTimeOffset Since { get; set; }
    }

    // This is not an open entity type.
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Account Account { get; set; }
    }

    // This is an open entity type.
    public class Manager : Employee
    {

        public Manager()
        {
            DynamicProperties = new Dictionary<string, object>();
        }
        public int Heads { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    // This is an open complex type, which may contain dynamic properties: age(int), gender(enum), etc.
    public class AccountInfo
    {
        public AccountInfo()
        {
            DynamicProperties = new Dictionary<string, object>();
        }

        public string NickName { get; set; }

        public Dictionary<string, object> DynamicProperties { get; set; }

    }

    // This is an open complex type，which may contain dynamic properties: countryOrRegion(string)
    public class Address
    {
        public Address()
        {
            DynamicProperties = new Dictionary<string, object>();
        }

        public string City { get; set; }
        public string Street { get; set; }

        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public class GlobalAddress : Address
    {
        public string CountryCode { get; set; }
    }

    // This is an open complex type, which does not contain any declared properties.
    public class Tags
    {
        public Tags()
        {
            DynamicProperties = new Dictionary<string, object>();
        }

        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }
}
