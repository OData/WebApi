//-----------------------------------------------------------------------------
// <copyright file="RecursiveComplexTypesE2ETests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Builder.TestModels.Recursive;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class RecursiveComplexTypesE2ETests : WebHostTestBase
    {
        public RecursiveComplexTypesE2ETests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(UsersController));
            configuration.MapODataServiceRoute("recursive", "recursive", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<UserEntity>("Users");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanRetrieveDataUsingRecursiveModel()
        {
            string queryUrl = string.Format("{0}/recursive/Users", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            var client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            const string ExpectedContent =
@"{
    ""value"": [
        {
            ""ID"": 0,
            ""Customer"": {
                ""Name"": ""Customer0"",
                ""Accounts"": []
            },
            ""Address"": {
                ""Street"": ""123 Main Street"",
                ""City"": ""Seattle"",
                ""CountryOrRegion"": ""Country Or Region 1"",
                ""PreviousAddress"": {
                    ""Street"": ""111 West 8th Avenue"",
                    ""City"": ""Vancouver"",
                    ""CountryOrRegion"": ""Country Or Region 2"",
                    ""PreviousAddress"": null
                }
            },
            ""HomeDirectory"": {
                ""Name"": ""/users/customer0"",
                ""Size"": 2048,
                ""Files"": [
                    {
                        ""Name"": ""users/customer0/subdir0"",
                        ""Size"": 0,
                        ""Files"": []
                    },
                    {
                        ""Name"": ""users/customer0/subdir1"",
                        ""Size"": 1024,
                        ""Files"": [
                            {
                                ""Name"": ""users/customers0/subdir1/file0"",
                                ""Size"": 0
                            }
                        ]
                    }
                ]
            },
            ""CustomFields"": [
                {
                    ""Name"": ""LastLoginDate"",
                    ""DataType"": ""DateTime"",
                    ""SubFields"": []
                },
                {
                    ""Name"": ""Referrals"",
                    ""DataType"": ""ComplexType"",
                    ""SubFields"": [
                        {
                            ""Name"": ""Name"",
                            ""DataType"": ""String"",
                            ""SubFields"": []
                        },
                        {
                            ""Name"": ""ReferralBonus"",
                            ""DataType"": ""Double"",
                            ""SubFields"": []
                        }
                    ]
                }
            ],
            ""Base"": {
                ""Derived"": {
                    ""Derived"": null,
                    ""Base"": null
                },
                ""Base"": {
                    ""Derived"": null
                }
            }
        },
        {
            ""ID"": 1,
            ""Customer"": {
                ""Name"": ""Customer1"",
                ""Accounts"": [
                    {
                        ""Number"": 0,
                        ""Owner"": {
                            ""Name"": ""Customer0"",
                            ""Accounts"": []
                        }
                    }
                ]
            },
            ""Address"": {
                ""Street"": ""123 Main Street"",
                ""City"": ""Seattle"",
                ""CountryOrRegion"": ""Country Or Region 1"",
                ""PreviousAddress"": {
                    ""Street"": ""111 West 8th Avenue"",
                    ""City"": ""Vancouver"",
                    ""CountryOrRegion"": ""Country Or Region 2"",
                    ""PreviousAddress"": null
                }
            },
            ""HomeDirectory"": {
                ""Name"": ""/users/customer1"",
                ""Size"": 2048,
                ""Files"": [
                    {
                        ""Name"": ""users/customer1/file0"",
                        ""Size"": 0
                    },
                    {
                        ""Name"": ""users/customer1/subdir0"",
                        ""Size"": 0,
                        ""Files"": []
                    },
                    {
                        ""Name"": ""users/customer1/subdir1"",
                        ""Size"": 1024,
                        ""Files"": [
                            {
                                ""Name"": ""users/customers1/subdir1/file0"",
                                ""Size"": 0
                            }
                        ]
                    }
                ]
            },
            ""CustomFields"": [
                {
                    ""Name"": ""LastLoginDate"",
                    ""DataType"": ""DateTime"",
                    ""SubFields"": []
                },
                {
                    ""Name"": ""Referrals"",
                    ""DataType"": ""ComplexType"",
                    ""SubFields"": [
                        {
                            ""Name"": ""Name"",
                            ""DataType"": ""String"",
                            ""SubFields"": []
                        },
                        {
                            ""Name"": ""ReferralBonus"",
                            ""DataType"": ""Double"",
                            ""SubFields"": []
                        }
                    ]
                }
            ],
            ""Base"": {
                ""Derived"": {
                    ""Derived"": null,
                    ""Base"": null
                },
                ""Base"": {
                    ""Derived"": null
                }
            }
        }
    ]
}";

            string actualContent = await response.Content.ReadAsStringAsync();

            AssertJsonEqual(ExpectedContent, actualContent);
        }

        private static void AssertJsonEqual(string expectedJson, string actualJson)
        {
            Assert.Equal(NormalizeJson(expectedJson), NormalizeJson(actualJson));
        }

        private static string NormalizeJson(string json)
        {
            return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json));
        }
    }

    public class UsersController : TestODataController
    {
        public IList<UserEntity> Users { get; set; }

        public UsersController()
        {
            Users = Enumerable.Range(0, 2).Select(i => new UserEntity
            {
                ID = i,
                Customer = new Customer
                {
                    Name = string.Format("Customer{0}", i),
                    Accounts = Enumerable.Range(0, i).Select(j => new Account
                    {
                        Number = j,
                        Owner = new Customer
                        {
                            Name = string.Format("Customer{0}", j)
                        }
                    }).ToList()
                },
                Address = new Address
                {
                    Street = "123 Main Street",
                    City = "Seattle",
                    CountryOrRegion = "Country Or Region 1",
                    PreviousAddress = new Address
                    {
                        Street = "111 West 8th Avenue",
                        City = "Vancouver",
                        CountryOrRegion = "Country Or Region 2"
                    }
                },
                HomeDirectory = new Directory
                {
                    Name = string.Format("/users/customer{0}", i),
                    Size = 2048,
                    Files = Enumerable.Range(0, i).Select(j => new File
                    {
                        Name = string.Format("users/customer{0}/file{1}", i, j),
                        Size = 1024 * j
                    }).Concat(Enumerable.Range(0, 2).Select(j => new Directory
                    {
                        Name = string.Format("users/customer{0}/subdir{1}", i, j),
                        Size = 1024 * j,
                        Files = Enumerable.Range(0, j).Select(k => new File
                        {
                            Name = string.Format("users/customers{0}/subdir{1}/file{2}", i, j, k),
                            Size = 1024 * k
                        }).ToList()
                    })).ToList()
                },
                CustomFields = new[]
                {
                    new Field
                    {
                        Name = "LastLoginDate",
                        DataType = "DateTime"
                    },
                    new Field
                    {
                        Name = "Referrals",
                        DataType = "ComplexType",
                        SubFields = new[]
                        {
                            new Field
                            {
                                Name = "Name",
                                DataType = "String"
                            },
                            new Field
                            {
                                Name = "ReferralBonus",
                                DataType = "Double"
                            }
                        }.ToList()
                    }
                }.ToList(),
                Base = new Derived()
                {
                    Derived = new Derived(),
                    Base = new Base()
                }
            }).ToList();
        }

        [EnableQuery]
        public IQueryable<UserEntity> Get()
        {
            return Users.AsQueryable();
        }
    }

    public class UserEntity
    {
        public int ID { get; set; }

        public Customer Customer { get; set; }

        public Address Address { get; set; }

        public Directory HomeDirectory { get; set; }

        public List<Field> CustomFields { get; set; }

        public Base Base { get; set; }
    }
}
