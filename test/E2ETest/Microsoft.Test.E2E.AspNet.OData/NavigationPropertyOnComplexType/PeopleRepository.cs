// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType
{
    public class PeopleRepository
    {
        public List<Person> people { get; set; }
        public List<ZipCode> zipCodes;
        public IDictionary<string, object> propertyBag = new Dictionary<string, object>();

        public PeopleRepository()
        {
            zipCodes = new List<ZipCode>
            {
                new ZipCode { Zip = 98052, City = "Redmond", State="Washington"},
                new ZipCode {Zip = 98030, City = "Kent", State = "Washington"},
                new ZipCode {Zip = 98004, City = "Bellevue", State = "Washington"}
            };

            propertyBag.Add("key", zipCodes[1]);

            people = new List<Person> { 
                new Person { Id=1, FirstName = "Kate", LastName = "Jones", Age = 5, Location = new Address{ ZipCode = zipCodes[1], Street = "110th" }, Home = new Address{ ZipCode = zipCodes[0], Street = "110th" }, Order = new Orders{ Zip = new Address{ ZipCode = zipCodes[0], Street = "110th" }}},
                new Person { Id =2, FirstName = "Lewis", LastName = "James", Age = 6 , Location = new GeoLocation{ ZipCode = zipCodes[1], Street = "110th", Latitude = "12.211", Longitude ="231.131" }, Home = new Address{ ZipCode = zipCodes[0], Street = "110th" }, Order = new Orders{ Zip = new Address{ ZipCode = zipCodes[0], Street = "110th" }}},
                new Person { Id = 3, FirstName = "Carlos", LastName = "Park", Age = 7, Location = new Address{ ZipCode = zipCodes[2], Street = "110th" }, Home = new Address{ ZipCode = zipCodes[0], Street = "110th" }, Order = new Orders{ Zip = new Address{ ZipCode = zipCodes[0], Street = "110th" }}, PreciseLocation = new GeoLocation{Area = zipCodes[2], Latitude = "12", Longitude = "22", Street = "50th", ZipCode = zipCodes[1]}},
                new Person { Id = 4, FirstName = "Carlos", LastName = "Park", Age = 7, Location = new Address{ ZipCode = zipCodes[2], Street = "110th" }, Home = new Address{ ZipCode = zipCodes[0], Street = "110th" }, Order = new Orders{ Zip = new Address{ ZipCode = zipCodes[0], Street = "110th" }, Order = new Orders{ Zip = new Address{ ZipCode = zipCodes[1], Street = "110th" }}}, PreciseLocation = new GeoLocation{Area = zipCodes[2], Latitude = "12", Longitude = "22", Street = "50th", ZipCode = zipCodes[1]}},
                new Person { Id = 5, FirstName = "Carlos", LastName = "Park", Age = 7, Order = new Orders() {propertybag = propertyBag} }
            };
        }

        public IEnumerable<Person> Get()
        {
            return people;
        }

        public Person Get(string firstName, string lastName)
        {
            return people.Where(p => p.FirstName == firstName && p.LastName == lastName).FirstOrDefault();
        }

        public Person Remove(string firstName, string lastName)
        {
            var p = Get(firstName, lastName);
            if (p == null)
            {
                return null;
            }

            people.Remove(p);
            return p;
        }
    }
}