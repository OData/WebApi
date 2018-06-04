﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Test.Common.Models
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
