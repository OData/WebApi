// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.OData.Builder.TestModels
{
    public class DateTimeModel
    {
        public int Id { get; set; }

        public DateTime BirthdayA { get; set; }

        public DateTime? BirthdayB { get; set; }

        public IList<DateTime> BirthdayC { get; set; }

        public IList<DateTime?> BirthdayD { get; set; }
    }
}
