//-----------------------------------------------------------------------------
// <copyright file="DateTimeModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
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
