// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using AspNetCoreODataSample.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreODataSample.Web.Controllers
{
    public class CustomersController : ODataController
    {
        public static IList<Customer> Customers;

        static CustomersController()
        {
            Customers = new List<Customer>
            {
                new Customer
                {
                    CustomerId = 1,
                    Name = "John",
                    HomeAddress = new Address
                    {
                        RelatedCity = new City { Id = 31, Name = "Redmond"},
                        Street = "148TH AVE NE",
                        Region = "Redmond",
                        Emails = new List<string>{ "abc@look.com", "xyz@eye.com" }
                    },
                    Addresses = new List<Address>
                    {
                        new CnAddress { Street = "LianHua Rd", Region = "Shanghai", PostCode = "201501", Emails = new List<string>{ "shangh_abc@look.com", "shangh_xyz@eye.com" }, RelatedCity = new City { Id = 81, Name = "Redm81"}, },
                        new CnAddress { Street = "Tiananmen Rd", Region = "Beijing", PostCode = "101501", Emails = new List<string>{ "beijing_abc@look.com", "beijing_xyz@eye.com" }, RelatedCity = new City { Id = 82, Name = "Redm82"},},
                        new UsAddress { Street = "Klahanie Rd", Region = "Remond", ZipCode = "98029", Emails = new List<string>{ "Remond_abc@look.com", "Remond_xyz@eye.com" }, RelatedCity = null },
                        new UsAddress { Street = "Sammamish Rd", Region = "Sammamish", ZipCode = "98072", Emails = new List<string>{ "Sammamish_abc@look.com", "Sammamish_xyz@eye.com" }, RelatedCity = new City { Id = 83, Name = "Redm83"}, },
                    },
                    HomeOrder = new Order
                    {
                        Id = 11,
                        Title = "John's Order",
                    }
                },
                new VipCustomer
                {
                    CustomerId = 2,
                    Name = "Smith",
                    HomeAddress = new Address
                    {
                        RelatedCity = new City { Id = 41, Name = "Oklawevue"},
                        Street = "18dfTH ST",
                        Region = "Seattle",
                        Emails = new List<string>{ "kfg@alis.com", "could@easdfe.com" }
                    },
                    Addresses = new List<Address>
                    {
                        new CnAddress { Street = "Shafng Rd", Region = "Zhedsdjiang", PostCode = "301501", Emails = new List<string>{ "Zhejiasng_abc@look.com", "Zhejdiang_xyz@eye.com" }, RelatedCity = new City { Id = 61, Name = "Redm21"}, },
                        new CnAddress { Street = "Chondiu Rd", Region = "Jiadsngsu", PostCode = "40d1501", Emails = new List<string>{ "Jiandgsu_abc@look.com", "Jiasngsu_xyz@eye.com" }, RelatedCity = null},
                        new UsAddress { Street = "Issdah Rd", Region = "Issdsaquah", ZipCode = "98d031", Emails = new List<string>{ "Issadquah_abc@look.com", "Issadquah_xyz@eye.com" }, RelatedCity = new City { Id = 62, Name = "Red352"}, },
                        new UsAddress { Street = "Beded Rd", Region = "Redmdond", ZipCode = "98d052", Emails = new List<string>{ "Redd_abc@look.com", "Red_sxyz@eye.com" }, RelatedCity = new City { Id = 63, Name = "Re453"}, },
                    },
                    HomeOrder = new Order
                    {
                        Id = 21,
                        Title = "Smith's Order"
                    },
                    VipPrice = 99,
                    VipTaxes = new [] { 9, 8, 7 },
                    VipAddress = new Address
                    {
                        RelatedCity = new City { Id = 91, Name = "Vipvue"},
                        Street = "1Vip1TH ST",
                        Region = "Seasdfattle",
                        Emails = new List<string>{ "kfadffg@alis.com", "coasdfuld@easdfe.com" }
                    },
                    VipAddresses = new List<Address>
                    {
                        new CnAddress { Street = "VipShafng Rd", Region = "VipZhedsdjiang", PostCode = "30adsf1501", Emails = new List<string>{ "Zjiasng_abc@look.com", "Zhejdiafasd_xyz@eye.com" }, RelatedCity = new City { Id = 71, Name = "Redm21"}, },
                        new UsAddress { Street = "VipBeded Rd", Region = "VipRedmdond", ZipCode = "98dfsda52", Emails = new List<string>{ "Rfd_abc@look.com", "Red_afsyz@eye.com" }, RelatedCity = new City { Id = 73, Name = "Re453"}, },
                    }
                },
                new Customer
                {
                    CustomerId = 3,
                    Name = "Ketti",
                    HomeAddress = new Address
                    {
                        RelatedCity = new City { Id = 31, Name = "Kettievue"},
                        Street = "98TH ST",
                        Region = "Kettlelevue",
                        Emails = new List<string>{ "Kett@alis.com", "Kett123@eye.com" }
                    },
                    Addresses = new List<Address>
                    {
                        new CnAddress { Street = "ShaKetthong Rd", Region = "ZhKettang", PostCode = "3011501", Emails = new List<string>{ "ZheKettg_abc@look.com", "ZhKettg_xyz@eye.com" }, RelatedCity = new City { Id = 71, Name = "ReKett51"}, },
                        new CnAddress { Street = "ChoKettgqiu Rd", Region = "JiaKettgsu", PostCode = "431501", Emails = new List<string>{ "JiKettabc@look.com", "JianKett_xyz@eye.com" }, RelatedCity = null},
                        new UsAddress { Street = "IssKettuah Rd", Region = "IssKettquah", ZipCode = "948031", Emails = new List<string>{ "IsKettah_abc@look.com", "IsKetth_xyz@eye.com" }, RelatedCity = new City { Id = 62, Name = "ReKett2"}, },
                        new UsAddress { Street = "BeKettd Rd", Region = "ReKettdmond", ZipCode = "980552", Emails = new List<string>{ "ReKettabc@look.com", "ReKettz@eye.com" }, RelatedCity = new City { Id = 93, Name = "RedKett3"}, },
                    },
                    HomeOrder = new Order
                    {
                        Id = 31,
                        Title = "Ketti's Order"
                    }
                },
                // null, // It's not allowed in ResourceSet Serializer
                new VipCustomer
                {
                    CustomerId = 5,
                    Name = "Peter",
                    HomeAddress = new Address
                    {
                        RelatedCity = new City { Id = 61, Name = "OPeterevue"},
                        Street = "18PeterH ST",
                        Region = "SePetertle",
                        Emails = new List<string>{ "Peterg@alis.com", "couPeterd@easdfe.com" }
                    },
                    Addresses = new List<Address>
                    {
                        new CnAddress { Street = "ShaPeterng Rd", Region = "ZhedPeterjiang", PostCode = "304501", Emails = new List<string>{ "ZhejPeterng_abc@look.com", "ZhejPeterg_xyz@eye.com" }, RelatedCity = new City { Id = 81, Name = "RePeter21"}, },
                        new CnAddress { Street = "ChPeteru Rd", Region = "JiadPetergsu", PostCode = "40d41501", Emails = new List<string>{ "JiandPeterabc@look.com", "JiasnPeteryz@eye.com" }, RelatedCity = null},
                        new UsAddress { Street = "IssPeter Rd", Region = "IssdsPeterquah", ZipCode = "98d0531", Emails = new List<string>{ "IssadqPeterabc@look.com", "IssaPeterh_xyz@eye.com" }, RelatedCity = new City { Id = 82, Name = "RePeter2"}, },
                        new UsAddress { Street = "BedPeterRd", Region = "RPetermdond", ZipCode = "98d0552", Emails = new List<string>{ "Redd_Peterc@look.com", "RePeterz@eye.com" }, RelatedCity = new City { Id = 83, Name = "Peter3"}, },
                    },
                    HomeOrder = new Order
                    {
                        Id = 41,
                        Title = "Peter's Order"
                    },
                    VipPrice = 99,
                    VipTaxes = new [] { 9, 8, 7 },
                    VipAddress = new Address
                    {
                        RelatedCity = new City { Id = 91, Name = "Vipvue"},
                        Street = "1Vip1TH ST",
                        Region = "Seasdfattle",
                        Emails = new List<string>{ "kfadffg@alis.com", "coasdfuld@easdfe.com" }
                    },
                    VipAddresses = new List<Address>
                    {
                        new CnAddress { Street = "VipShafng Rd", Region = "VipZhedsdjiang", PostCode = "30adsf1501", Emails = new List<string>{ "Zjiasng_abc@look.com", "Zhejdiafasd_xyz@eye.com" }, RelatedCity = new City { Id = 71, Name = "Redm21"}, },
                        new UsAddress { Street = "VipBeded Rd", Region = "VipRedmdond", ZipCode = "98dfsda52", Emails = new List<string>{ "Rfd_abc@look.com", "Red_afsyz@eye.com" }, RelatedCity = new City { Id = 73, Name = "Re453"}, },
                    },
                }

            };
        }

        public int VipPrice { get; set; }

        public IList<int> VipTax { get; set; }

        public Address VipAddress { get; set; }

        public IList<Address> VipAddresses { get; set; }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(Customers);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            Customer c = Customers.FirstOrDefault(k => k.CustomerId == key);
            if (c == null)
            {
                return NotFound();
            }

            return Ok(c);
        }

        [EnableQuery]
        public IActionResult GetFromVipCustomer()
        {
            return Ok(Customers.Where(c => c is VipCustomer));
        }
    }
}
