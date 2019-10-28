// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType
{
    public class PeopleRepository
    {
        public List<Person> People { get; private set; }

        public PeopleRepository()
        {
            var zipCodes = new List<ZipCode>
            {
                new ZipCode { Zip = 98052, City = "Redmond", State="Washington"},
                new ZipCode { Zip = 35816, City = "Huntsville", State = "Alabama"},
                new ZipCode { Zip = 10048, City = "New York", State = "New York"}
            };

            IDictionary<string, object> propertyBag = new Dictionary<string, object>
            {
                { "DynamicInt", 9 },
                {
                    "DynamicAddress",
                    new Address
                    {
                        Street = "",
                        Emails = new List<string>
                        {
                            "abc@1.com",
                            "xyz@2.com"
                        }
                    }
                }
            };

            var repoLocations = new Address[]
            {
                new Address
                {
                    Street = "110th",
                    TaxNo = 19,
                    Emails = new [] { "E1", "E3", "E2" },
                    RelatedInfo = new AddressInfo { AreaSize = 101, CountyName = "King" },
                    AdditionInfos = Enumerable.Range(1, 3).Select(e => new AddressInfo
                    {
                        AreaSize = 101 + e,
                        CountyName = "King" + e
                    }).ToList(),
                    ZipCode = zipCodes[0],
                    DetailCodes = zipCodes
                },
                new GeoLocation
                {
                    Street = "120th",
                    TaxNo = 17,
                    Emails = new [] { "E7", "E4", "E5" },
                    RelatedInfo = null,
                    AdditionInfos = Enumerable.Range(1, 3).Select(e => new AddressInfo
                    {
                        AreaSize = 101 + e,
                        CountyName = "King" + e
                    }).ToList(),
                    Latitude = "12.8",
                    Longitude = "22.9",
                    ZipCode = zipCodes[1],
                    DetailCodes = zipCodes,
                    Area = zipCodes[2]
                },
                new Address
                {
                    Street = "130th",
                    TaxNo = 18,
                    Emails = new [] { "E9", "E6", "E8" },
                    RelatedInfo = new AddressInfo { AreaSize = 201, CountyName = "Queue" },
                    AdditionInfos = new AddressInfo[0],
                    ZipCode = zipCodes[2],
                    DetailCodes = zipCodes
                },

            };

            People = new List<Person>
            {
                new Person
                {
                    Id = 1,
                    Name = "Kate",
                    Age = 5,
                    Taxes = new [] { 7, 5, 9 },
                    HomeLocation = repoLocations[0],
                    RepoLocations = repoLocations,
                    PreciseLocation = null, // by design
                    OrderInfo = new OrderInfo
                    {
                        BillLocation = repoLocations[0],
                        SubInfo = null
                    }
                },
                new Person
                {
                    Id = 2,
                    Name = "Lewis",
                    Age = 6 ,
                    Taxes = new [] { 1, 5, 2 },
                    HomeLocation = new GeoLocation{ ZipCode = zipCodes[1], Street = "110th", Latitude = "12.211", Longitude ="231.131" },
                    RepoLocations = repoLocations,
                    PreciseLocation = null, // by design
                    OrderInfo = new OrderInfo
                    {
                        BillLocation = new Address{ ZipCode = zipCodes[0], Street = "110th" }
                    }
                },
                new Person
                {
                    Id = 3,
                    Name = "Carlos",
                    Age = 7,
                    HomeLocation = null, // by design
                    RepoLocations = repoLocations,
                    OrderInfo = new OrderInfo
                    {
                        BillLocation = new Address{ ZipCode = zipCodes[0], Street = "110th" }
                    },
                    PreciseLocation = new GeoLocation{Area = zipCodes[2], Latitude = "12", Longitude = "22", Street = "50th", ZipCode = zipCodes[1]}
                },
                new Person
                {
                    Id = 4,
                    Name = "Jones",
                    Age = 9,
                    HomeLocation = repoLocations[1],
                    RepoLocations = repoLocations.Take(2).ToList(),
                    PreciseLocation = new GeoLocation{Area = zipCodes[2], Latitude = "12", Longitude = "22", Street = "50th", ZipCode = zipCodes[1]},
                    OrderInfo = new OrderInfo
                    {
                        BillLocation = new Address{ ZipCode = zipCodes[0], Street = "110th" },
                        SubInfo = new OrderInfo{ BillLocation = new Address{ ZipCode = zipCodes[1], Street = "110th" }}
                    }
                },
                new Person
                {
                    Id = 5,
                    Name = "Park",
                    Age = 17,
                    HomeLocation = repoLocations[2],
                    RepoLocations = repoLocations.Take(1).ToList(),
                    OrderInfo = new OrderInfo()
                    {
                        propertybag = propertyBag
                    }
                },
                new VipPerson
                {
                    Id = 6,
                    Name = "Sam",
                    Age = 40,
                    HomeLocation = new Address
                    {
                        Street = "130th",
                        TaxNo = 18,
                        Emails = new [] { "A9", "A6", "A8" },
                        RelatedInfo = new AddressInfo { AreaSize = 101, CountyName = "King" },
                        AdditionInfos = Enumerable.Range(1, 3).Select(e => new AddressInfo
                        {
                            AreaSize = 101 + e,
                            CountyName = "King" + e
                        }).ToList(),
                        ZipCode = zipCodes[2],
                        DetailCodes = zipCodes
                    },
                    Bonus = 99
                }
            };
        }
    }
}