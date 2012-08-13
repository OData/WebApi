// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace System.Web.Http.OData.TestCommon.Models
{
    [DataContract]
    public class MultipleKeyEmployee : Employee
    {
        [Key]
        [DataMember]
        public Guid EmployeeGuid;

        [Key]
        [DataMember]
        public string UniqueStringId
        {
            get;
            set;
        }

        public MultipleKeyEmployee(int index, ReferenceDepthContext context)
            : base(index, context)
        {
            this.EmployeeGuid = new Guid("844080c0-2f9e-472e-8c72-1a8ecd9f9037");
            this.UniqueStringId = DataSource.Names[index] + index;
        }
    }
}
