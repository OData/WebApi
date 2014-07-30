// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.TestCommon.Models
{
    public class WorkItem
    {
        //Automatically is made Key
        public int ID { get; set; }

        public int EmployeeID { get; set; }

        public bool IsCompleted { get; set; }

        public float NumberOfHours { get; set; }

        public int Field;
    }

    // Used as a type on which keys can be explicitly set
    public class DerivedWorkItem : WorkItem
    {
    }
}
