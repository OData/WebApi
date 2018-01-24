// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Test.E2E.AspNet.OData.SxS.ODataV4.Models
{
    public class Product
    {
        public int Id
        {
            get;
            set;
        }

        public string Title
        {
            get; set;
        }

        public DateTimeOffset ManufactureDateTime
        {
            get;
            set;
        }
    }
}