// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
