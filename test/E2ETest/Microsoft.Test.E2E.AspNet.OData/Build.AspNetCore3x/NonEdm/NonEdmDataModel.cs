//-----------------------------------------------------------------------------
// <copyright file="NonEdmDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Test.E2E.AspNet.OData.NonEdm
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Employee : Person
    {
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Person RelationshipManager { get; set; }
    }

    public class EnterpriseCustomer : Customer
    {
        public Person AccountManager { get; set; }
    }
}
