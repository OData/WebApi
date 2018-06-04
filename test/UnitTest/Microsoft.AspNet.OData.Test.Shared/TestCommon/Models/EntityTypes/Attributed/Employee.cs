﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Microsoft.AspNet.OData.Test.Common.Models
{
    [DataContract]
    public class Employee : Person
    {
        [Key]
        [DataMember]
        public long EmployeeId { get; set; }

        [DataMember]
        public Employee Manager { get; set; }

        [DataMember]
        public List<Employee> DirectReports { get; set; }

        [DataMember]
        public WorkItem WorkItem { get; set; }

        public Employee(int index, ReferenceDepthContext context)
            : base(index, context)
        {
            this.EmployeeId = index;
            this.WorkItem = new WorkItem() { EmployeeID = index, IsCompleted = false, NumberOfHours = ((index + 100) / 6), ID = index + 25 };
            this.Manager = (Employee)TypeInitializer.InternalGetInstance(SupportedTypes.Employee, (index + 1) % (DataSource.MaxIndex + 1), context);

            this.DirectReports = new System.Collections.Generic.List<Employee>();
            Employee directEmployee = (Employee)TypeInitializer.InternalGetInstance(SupportedTypes.Employee, (index + 2) % (DataSource.MaxIndex + 1), context);
            if (directEmployee != null)
            {
                this.DirectReports.Add(directEmployee);
            }
        }
    }
}
