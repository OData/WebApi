// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Test.E2E.AspNet.OData.DollarLevels
{
    public class DLManager
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DLManager Manager { get; set; }

        public IList<DLManager> DirectReports { get; set; }
    }

    public class DLEmployee
    {
        public int ID { get; set; }

        public DLEmployee Friend { get; set; }
    }

    public class Test
    {
        [Key]
        public string Id { get; set; }
        public string name { get; set; }
    }
}
