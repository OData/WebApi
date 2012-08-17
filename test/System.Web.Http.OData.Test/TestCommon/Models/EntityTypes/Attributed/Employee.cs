// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace System.Web.Http.OData.TestCommon.Models
{
    [DataContract]
    public class Employee : Person
    {
        [Key]
        [DataMember]
        public long EmployeeId;

        [DataMember]
        public Person Manager;

        [DataMember]
        public List<Employee> DirectReports;

        [DataMember(Name = "CurrentWorkItem")]
        public WorkItem WorkItem;

        public Employee(int index, ReferenceDepthContext context)
            : base(index, context)
        {
            this.EmployeeId = index;
            this.WorkItem = new WorkItem() { EmployeeID = index, IsCompleted = false, NumberOfHours = ((index + 100) / 6), WorkItemID = index + 25 };
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
