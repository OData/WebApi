﻿using System;
using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.DateTimeSupport
{
    public class File
    {
        public int FileId { get; set; }

        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? DeleteDate { get; set; }

        public IList<DateTime> ModifiedDates { get; set; }
    }
}
