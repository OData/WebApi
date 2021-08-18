//-----------------------------------------------------------------------------
// <copyright file="DateAndTimeOfDayModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.DateAndTimeOfDay
{
    public class DCustomer
    {
        public int Id { get; set; }

        // non-nullable
        public DateTime DateTime { get; set; }
        public DateTimeOffset Offset { get; set; }
        public Date Date { get; set; }
        public TimeOfDay TimeOfDay { get; set; }

        // nullable
        public DateTime? NullableDateTime { get; set; }
        public DateTimeOffset? NullableOffset { get; set; }
        public Date? NullableDate { get; set; }
        public TimeOfDay? NullableTimeOfDay { get; set; }

        // Collection
        public IList<DateTime> DateTimes { get; set; }
        public IList<DateTimeOffset> Offsets { get; set; }
        public IList<Date> Dates { get; set; }
        public IList<TimeOfDay> TimeOfDays { get; set; }

        // Collection of nullable
        public IList<DateTime?> NullableDateTimes { get; set; }
        public IList<DateTimeOffset?> NullableOffsets { get; set; }
        public IList<Date?> NullableDates { get; set; }
        public IList<TimeOfDay?> NullableTimeOfDays { get; set; }
    }

    public class EfCustomer
    {
        public int Id { get; set; }

        // non-nullable
        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }
        public DateTimeOffset Offset { get; set; }

        // nullable
        public DateTime? NullableDateTime { get; set; }
        public DateTimeOffset? NullableOffset { get; set; }
    }

    public class EfPerson
    {
        public int Id { get; set; }

        [Column("Birthday", TypeName = "Date")]
        public DateTime? Birthday { get; set; }
    }
}
