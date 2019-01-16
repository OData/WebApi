// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Aggregation;

namespace Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int Age { get; set; }
        public Address Location { get; set; }

        public Address Home { get; set; }

        public GeoLocation PreciseLocation { get; set; }

        public Orders Order { get; set; }
    }

    public class Orders
    {
        public Address Zip { get; set; }
        public Orders Order { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public ZipCode ZipCode { get; set; }
    }

    public class ZipCode
    {
        [Key]
        public int Zip { get; set; }
        public string City { get; set; }
        public string State { get; set; }

    }

    public class GeoLocation : Address
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public ZipCode Area { get; set; }
    }

    public class ModelGenerator
    {
        // Builds the EDM model for the OData service.
        public static IEdmModel GetConventionalEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            var peopleEntitySet = modelBuilder.EntitySet<Person>("People");
            var zipcodes = modelBuilder.EntitySet<ZipCode>("ZipCodes");

            modelBuilder.Namespace = typeof(Person).Namespace;
            return modelBuilder.GetEdmModel();
        }

    }
}