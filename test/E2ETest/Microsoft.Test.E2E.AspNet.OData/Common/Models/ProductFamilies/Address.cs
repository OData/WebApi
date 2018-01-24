// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies
{
    [ComplexType]
    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string CountryOrRegion { get; set; }
    }
}
