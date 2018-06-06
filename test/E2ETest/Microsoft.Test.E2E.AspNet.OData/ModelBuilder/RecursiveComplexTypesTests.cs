// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Builder.TestModels.Recursive;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class RecursiveComplexTypesTests
    {
        [Fact]
        public void CanBuildModelWithDirectRecursiveReference()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<RecursiveComplexTypesTests_JustAddress>("justaddress");
            IEdmModel model = builder.GetEdmModel();

            var address =
                model.SchemaElements.First(e => e.Name == typeof(RecursiveComplexTypesTests_Address).Name)
                as EdmComplexType;

            var previousAddressProperty =
                address.Properties().Single(p => p.Name == "PreviousAddress") as EdmStructuralProperty;

            Assert.Equal(
                typeof(RecursiveComplexTypesTests_Address).Name,
                previousAddressProperty.Type.AsComplex()?.ComplexDefinition().Name);
        }

        [Fact]
        public void CanBuildModelWithCollectionRecursiveReference()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<RecursiveComplexTypesTests_JustCustomFields>("justcustomfields");
            IEdmModel model = builder.GetEdmModel();

            var field =
                model.SchemaElements.First(e => e.Name == typeof(RecursiveComplexTypesTests_Field).Name)
                as EdmComplexType;

            var subFieldsProperty =
                field.Properties().Single(p => p.Name == "SubFields") as EdmStructuralProperty;

            Assert.Equal(
                typeof(RecursiveComplexTypesTests_Field).Name,
                subFieldsProperty.Type.AsCollection()?.ElementType().AsComplex()?.ComplexDefinition().Name);
        }

        [Fact]
        public void CanBuildModelWithIndirectRecursiveReference()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<RecursiveComplexTypesTests_JustCustomer>("justcustomer");
            IEdmModel model = builder.GetEdmModel();

            string customerTypeName = typeof(RecursiveComplexTypesTests_Customer).Name;

            var customer = model.SchemaElements.First(e => e.Name == customerTypeName) as EdmComplexType;

            var accountsProperty =
                customer.Properties().Single(p => p.Name == "Accounts") as EdmStructuralProperty;

            string accountTypeName = typeof(RecursiveComplexTypesTests_Account).Name;

            Assert.Equal(
                accountTypeName,
                accountsProperty.Type.AsCollection()?.ElementType().AsComplex()?.ComplexDefinition().Name);

            var account = model.SchemaElements.First(e => e.Name == accountTypeName) as EdmComplexType;
            var ownerProperty = account.Properties().Single(p => p.Name == "Owner") as EdmStructuralProperty;

            Assert.Equal(customerTypeName, ownerProperty.Type.AsComplex()?.ComplexDefinition().Name);
        }

        [Fact]
        public void CanBuildModelWithCollectionReferenceViaInheritance()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<RecursiveComplexTypesTests_JustHomeDirectory>("justhomedirectory");
            IEdmModel model = builder.GetEdmModel();

            string fileTypeName = typeof(RecursiveComplexTypesTests_File).Name;
            string directoryTypeName = typeof(RecursiveComplexTypesTests_Directory).Name;

            var file = model.SchemaElements.First(e => e.Name == fileTypeName) as EdmComplexType;
            var directory = model.SchemaElements.First(e => e.Name == directoryTypeName) as EdmComplexType;

            Assert.Equal(fileTypeName, directory.BaseComplexType()?.Name);

            var filesProperty = directory.Properties().Single(p => p.Name == "Files") as EdmStructuralProperty;

            Assert.Equal(
                fileTypeName,
                filesProperty.Type.AsCollection()?.ElementType().AsComplex()?.ComplexDefinition().Name);
        }

        [Fact]
        public void CanBuildModelWithMutuallyRecursiveInheritance()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<RecursiveComplexTypesTests_JustBase>("justbase");
            builder.EntitySet<RecursiveComplexTypesTests_JustDerived>("justderived");
            IEdmModel model = builder.GetEdmModel();

            string baseTypeName = typeof(RecursiveComplexTypesTests_Base).Name;
            string derivedTypeName = typeof(RecursiveComplexTypesTests_Derived).Name;

            var baseType = model.SchemaElements.First(e => e.Name == baseTypeName) as EdmComplexType;
            var derivedType = model.SchemaElements.First(e => e.Name == derivedTypeName) as EdmComplexType;

            Assert.Equal(baseTypeName, derivedType.BaseComplexType()?.Name);

            var baseProperty = derivedType.Properties().Single(p => p.Name == "Base") as EdmStructuralProperty;
            var derivedProperty = baseType.Properties().Single(p => p.Name == "Derived") as EdmStructuralProperty;

            Assert.Equal(baseTypeName, baseProperty.Type.AsComplex()?.ComplexDefinition().Name);
            Assert.Equal(derivedTypeName, derivedProperty.Type.AsComplex()?.ComplexDefinition().Name);
        }
    }

    public class RecursiveComplexTypesE2ETests : WebHostTestBase
    {
        public RecursiveComplexTypesE2ETests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(RecursiveComplexTypes.UsersController));
            configuration.MapODataServiceRoute("recursive", "recursive", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<RecursiveComplexTypesTests_UserEntity>("Users");
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
                ""Country"": ""USA"",
                ""PreviousAddress"": {
                    ""Street"": ""111 West 8th Avenue"",
                    ""City"": ""Vancouver"",
                    ""Country"": ""Canada"",
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
                ""Country"": ""USA"",
                ""PreviousAddress"": {
                    ""Street"": ""111 West 8th Avenue"",
                    ""City"": ""Vancouver"",
                    ""Country"": ""Canada"",
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

    namespace RecursiveComplexTypes
    {
        public class UsersController : TestODataController
        {
            public IList<RecursiveComplexTypesTests_UserEntity> Users { get; set; }

            public UsersController()
            {
                Users = Enumerable.Range(0, 2).Select(i => new RecursiveComplexTypesTests_UserEntity
                {
                    ID = i,
                    Customer = new RecursiveComplexTypesTests_Customer
                    {
                        Name = string.Format("Customer{0}", i),
                        Accounts = Enumerable.Range(0, i).Select(j => new RecursiveComplexTypesTests_Account
                        {
                            Number = j,
                            Owner = new RecursiveComplexTypesTests_Customer
                            {
                                Name = string.Format("Customer{0}", j)
                            }
                        }).ToList()
                    },
                    Address = new RecursiveComplexTypesTests_Address
                    {
                        Street = "123 Main Street",
                        City = "Seattle",
                        Country = "USA",
                        PreviousAddress = new RecursiveComplexTypesTests_Address
                        {
                            Street = "111 West 8th Avenue",
                            City = "Vancouver",
                            Country = "Canada"
                        }
                    },
                    HomeDirectory = new RecursiveComplexTypesTests_Directory
                    {
                        Name = string.Format("/users/customer{0}", i),
                        Size = 2048,
                        Files = Enumerable.Range(0, i).Select(j => new RecursiveComplexTypesTests_File
                        {
                            Name = string.Format("users/customer{0}/file{1}", i, j),
                            Size = 1024 * j
                        }).Concat(Enumerable.Range(0, 2).Select(j => new RecursiveComplexTypesTests_Directory
                        {
                            Name = string.Format("users/customer{0}/subdir{1}", i, j),
                            Size = 1024 * j,
                            Files = Enumerable.Range(0, j).Select(k => new RecursiveComplexTypesTests_File
                            {
                                Name = string.Format("users/customers{0}/subdir{1}/file{2}", i, j, k),
                                Size = 1024 * k
                            }).ToList()
                        })).ToList()
                    },
                    CustomFields = new[]
                    {
                        new RecursiveComplexTypesTests_Field
                        {
                            Name = "LastLoginDate",
                            DataType = "DateTime"
                        },
                        new RecursiveComplexTypesTests_Field
                        {
                            Name = "Referrals",
                            DataType = "ComplexType",
                            SubFields = new[]
                            {
                                new RecursiveComplexTypesTests_Field
                                {
                                    Name = "Name",
                                    DataType = "String"
                                },
                                new RecursiveComplexTypesTests_Field
                                {
                                    Name = "ReferralBonus",
                                    DataType = "Double"
                                }
                            }.ToList()
                        }
                    }.ToList(),
                    Base = new RecursiveComplexTypesTests_Derived()
                    {
                        Derived = new RecursiveComplexTypesTests_Derived(),
                        Base = new RecursiveComplexTypesTests_Base()
                    }
                }).ToList();
            }

            [EnableQuery]
            public IQueryable<RecursiveComplexTypesTests_UserEntity> Get()
            {
                return Users.AsQueryable();
            }
        }
    }
}

