//-----------------------------------------------------------------------------
// <copyright file="DollarLevelsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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


    public class TestQueryOptions
    {
        [Key]
        public string Id { get; set; }
        public string name { get; set; }
    }
}
