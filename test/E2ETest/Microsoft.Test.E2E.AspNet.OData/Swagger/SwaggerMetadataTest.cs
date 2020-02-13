// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.Swagger
{
    public class SwaggerMetadataTest : WebHostTestBase<SwaggerMetadataTest>
    {
        public SwaggerMetadataTest(WebHostTestFixture<SwaggerMetadataTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(SwaggerController), typeof(MetadataController)};
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.InsertFormatter(FormatterFactory.CreateJson(configuration));

            IODataPathHandler handler = new SwaggerPathHandler();
            IList<IODataRoutingConvention> conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new SwaggerRoutingConvention());

            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), handler, conventions);
            configuration.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.Function("UnboundFunction").Returns<string>().Parameter<int>("param");
            builder.Action("UnboundAction").Parameter<double>("param");
            builder.EntityType<Customer>().Function("BoundFunction").Returns<double>().Parameter<string>("name");
            builder.EntitySet<CompositeKeyItem>("CompositeKeyItems");
            builder.EntityType<CompositeKeyItem>().HasKey(x => new { x.FirstKey, x.SecondKey });
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("$swagger")]
        [InlineData("swagger.json")]
        public void SwaggerPathHandlerWorksForSwaggerMetadataUri(string swaggerMetadataUri)
        {
            SwaggerPathHandler handler = new SwaggerPathHandler();
            IEdmModel model = new EdmModel();

            ODataPath path = handler.Parse(model, "http://any", swaggerMetadataUri);

            ODataPathSegment segment = path.Segments.Last();

            Assert.NotNull(path);
            Assert.Null(path.NavigationSource);
            Assert.Null(path.EdmType);
            Assert.Equal("$swagger", segment.ToString());
            Assert.IsType<SwaggerPathSegment>(segment);
        }

        [Fact]
        public async Task EnableSwaggerMetadataTest()
        {
            JObject expectObj = JObject.Parse(@"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""title"": ""OData Service"",
    ""description"": ""The OData Service at http://localhost/"",
    ""version"": ""0.1.0"",
    ""x-odata-version"": ""4.0""
  },
  ""host"": ""default"",
  ""schemes"": [
    ""http""
  ],
  ""basePath"": ""/odata"",
  ""consumes"": [
    ""application/json""
  ],
  ""produces"": [
    ""application/json""
  ],
  ""paths"": {
    ""/Customers"": {
      ""get"": {
        ""summary"": ""Get EntitySet Customers"",
        ""operationId"": ""Customers_Get"",
        ""description"": ""Returns the EntitySet Customers"",
        ""tags"": [
          ""Customers""
        ],
        ""parameters"": [
          {
            ""name"": ""$expand"",
            ""in"": ""query"",
            ""description"": ""Expand navigation property"",
            ""type"": ""string""
          },
          {
            ""name"": ""$select"",
            ""in"": ""query"",
            ""description"": ""select structural property"",
            ""type"": ""string""
          },
          {
            ""name"": ""$orderby"",
            ""in"": ""query"",
            ""description"": ""order by some property"",
            ""type"": ""string""
          },
          {
            ""name"": ""$top"",
            ""in"": ""query"",
            ""description"": ""top elements"",
            ""type"": ""integer""
          },
          {
            ""name"": ""$skip"",
            ""in"": ""query"",
            ""description"": ""skip elements"",
            ""type"": ""integer""
          },
          {
            ""name"": ""$count"",
            ""in"": ""query"",
            ""description"": ""include count in response"",
            ""type"": ""boolean""
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""EntitySet Customers"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Customer""
            }
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      },
      ""post"": {
        ""summary"": ""Post a new entity to EntitySet Customers"",
        ""operationId"": ""Customers_Post"",
        ""description"": ""Post a new entity to EntitySet Customers"",
        ""tags"": [
          ""Customers""
        ],
        ""parameters"": [
          {
            ""name"": ""Customer"",
            ""in"": ""body"",
            ""description"": ""The entity to post"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Customer""
            }
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""EntitySet Customers"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Customer""
            }
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      }
    },
    ""/Customers({CustomerId})"": {
      ""get"": {
        ""summary"": ""Get entity from Customers by key."",
        ""operationId"": ""Customers_GetById"",
        ""description"": ""Returns the entity with the key from Customers"",
        ""tags"": [
          ""Customers""
        ],
        ""parameters"": [
          {
            ""name"": ""CustomerId"",
            ""in"": ""path"",
            ""description"": ""key: CustomerId"",
            ""type"": ""integer"",
            ""format"": ""int32""
          },
          {
            ""name"": ""$select"",
            ""in"": ""query"",
            ""description"": ""description"",
            ""type"": ""string""
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""EntitySet Customers"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Customer""
            }
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      },
      ""patch"": {
        ""summary"": ""Update entity in EntitySet Customers"",
        ""operationId"": ""Customers_PatchById"",
        ""description"": ""Update entity in EntitySet Customers"",
        ""tags"": [
          ""Customers""
        ],
        ""parameters"": [
          {
            ""name"": ""CustomerId"",
            ""in"": ""path"",
            ""description"": ""key: CustomerId"",
            ""type"": ""integer"",
            ""format"": ""int32""
          },
          {
            ""name"": ""Customer"",
            ""in"": ""body"",
            ""description"": ""The entity to patch"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Customer""
            }
          }
        ],
        ""responses"": {
          ""204"": {
            ""description"": ""Empty response""
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      },
      ""delete"": {
        ""summary"": ""Delete entity in EntitySet Customers"",
        ""operationId"": ""Customers_DeleteById"",
        ""description"": ""Delete entity in EntitySet Customers"",
        ""tags"": [
          ""Customers""
        ],
        ""parameters"": [
          {
            ""name"": ""CustomerId"",
            ""in"": ""path"",
            ""description"": ""key: CustomerId"",
            ""type"": ""integer"",
            ""format"": ""int32""
          },
          {
            ""name"": ""If-Match"",
            ""in"": ""header"",
            ""description"": ""If-Match header"",
            ""type"": ""string""
          }
        ],
        ""responses"": {
          ""204"": {
            ""description"": ""Empty response""
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      }
    },
    ""/Orders"": {
      ""get"": {
        ""summary"": ""Get EntitySet Orders"",
        ""operationId"": ""Orders_Get"",
        ""description"": ""Returns the EntitySet Orders"",
        ""tags"": [
          ""Orders""
        ],
        ""parameters"": [
          {
            ""name"": ""$expand"",
            ""in"": ""query"",
            ""description"": ""Expand navigation property"",
            ""type"": ""string""
          },
          {
            ""name"": ""$select"",
            ""in"": ""query"",
            ""description"": ""select structural property"",
            ""type"": ""string""
          },
          {
            ""name"": ""$orderby"",
            ""in"": ""query"",
            ""description"": ""order by some property"",
            ""type"": ""string""
          },
          {
            ""name"": ""$top"",
            ""in"": ""query"",
            ""description"": ""top elements"",
            ""type"": ""integer""
          },
          {
            ""name"": ""$skip"",
            ""in"": ""query"",
            ""description"": ""skip elements"",
            ""type"": ""integer""
          },
          {
            ""name"": ""$count"",
            ""in"": ""query"",
            ""description"": ""include count in response"",
            ""type"": ""boolean""
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""EntitySet Orders"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Order""
            }
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      },
      ""post"": {
        ""summary"": ""Post a new entity to EntitySet Orders"",
        ""operationId"": ""Orders_Post"",
        ""description"": ""Post a new entity to EntitySet Orders"",
        ""tags"": [
          ""Orders""
        ],
        ""parameters"": [
          {
            ""name"": ""Order"",
            ""in"": ""body"",
            ""description"": ""The entity to post"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Order""
            }
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""EntitySet Orders"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Order""
            }
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      }
    },
    ""/Orders({OrderId})"": {
      ""get"": {
        ""summary"": ""Get entity from Orders by key."",
        ""operationId"": ""Orders_GetById"",
        ""description"": ""Returns the entity with the key from Orders"",
        ""tags"": [
          ""Orders""
        ],
        ""parameters"": [
          {
            ""name"": ""OrderId"",
            ""in"": ""path"",
            ""description"": ""key: OrderId"",
            ""type"": ""integer"",
            ""format"": ""int32""
          },
          {
            ""name"": ""$select"",
            ""in"": ""query"",
            ""description"": ""description"",
            ""type"": ""string""
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""EntitySet Orders"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Order""
            }
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      },
      ""patch"": {
        ""summary"": ""Update entity in EntitySet Orders"",
        ""operationId"": ""Orders_PatchById"",
        ""description"": ""Update entity in EntitySet Orders"",
        ""tags"": [
          ""Orders""
        ],
        ""parameters"": [
          {
            ""name"": ""OrderId"",
            ""in"": ""path"",
            ""description"": ""key: OrderId"",
            ""type"": ""integer"",
            ""format"": ""int32""
          },
          {
            ""name"": ""Order"",
            ""in"": ""body"",
            ""description"": ""The entity to patch"",
            ""schema"": {
              ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.Order""
            }
          }
        ],
        ""responses"": {
          ""204"": {
            ""description"": ""Empty response""
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      },
      ""delete"": {
        ""summary"": ""Delete entity in EntitySet Orders"",
        ""operationId"": ""Orders_DeleteById"",
        ""description"": ""Delete entity in EntitySet Orders"",
        ""tags"": [
          ""Orders""
        ],
        ""parameters"": [
          {
            ""name"": ""OrderId"",
            ""in"": ""path"",
            ""description"": ""key: OrderId"",
            ""type"": ""integer"",
            ""format"": ""int32""
          },
          {
            ""name"": ""If-Match"",
            ""in"": ""header"",
            ""description"": ""If-Match header"",
            ""type"": ""string""
          }
        ],
        ""responses"": {
          ""204"": {
            ""description"": ""Empty response""
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      }
    },
    ""/CompositeKeyItems"": {
       ""get"": {
         ""summary"": ""Get EntitySet CompositeKeyItems"",
         ""operationId"": ""CompositeKeyItems_Get"",
         ""description"": ""Returns the EntitySet CompositeKeyItems"",
         ""tags"": [
           ""CompositeKeyItems""
         ],
         ""parameters"": [
           {
             ""name"": ""$expand"",
             ""in"": ""query"",
             ""description"": ""Expand navigation property"",
             ""type"": ""string""
           },
           {
             ""name"": ""$select"",
             ""in"": ""query"",
             ""description"": ""select structural property"",
             ""type"": ""string""
           },
           {
             ""name"": ""$orderby"",
             ""in"": ""query"",
             ""description"": ""order by some property"",
             ""type"": ""string""
           },
           {
             ""name"": ""$top"",
             ""in"": ""query"",
             ""description"": ""top elements"",
             ""type"": ""integer""
           },
           {
             ""name"": ""$skip"",
             ""in"": ""query"",
             ""description"": ""skip elements"",
             ""type"": ""integer""
           },
           {
             ""name"": ""$count"",
             ""in"": ""query"",
             ""description"": ""include count in response"",
             ""type"": ""boolean""
           }
         ],
         ""responses"": {
           ""200"": {
             ""description"": ""EntitySet CompositeKeyItems"",
             ""schema"": {
               ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.CompositeKeyItem""
             }
           },
           ""default"": {
             ""description"": ""Unexpected error"",
             ""schema"": {
               ""$ref"": ""#/definitions/_Error""
             }
           }
         }
       },
       ""post"": {
         ""summary"": ""Post a new entity to EntitySet CompositeKeyItems"",
         ""operationId"": ""CompositeKeyItems_Post"",
         ""description"": ""Post a new entity to EntitySet CompositeKeyItems"",
         ""tags"": [
           ""CompositeKeyItems""
         ],
         ""parameters"": [
           {
             ""name"": ""CompositeKeyItem"",
             ""in"": ""body"",
             ""description"": ""The entity to post"",
             ""schema"": {
               ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.CompositeKeyItem""
             }
           }
         ],
         ""responses"": {
           ""200"": {
             ""description"": ""EntitySet CompositeKeyItems"",
             ""schema"": {
               ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.CompositeKeyItem""
             }
           },
           ""default"": {
             ""description"": ""Unexpected error"",
             ""schema"": {
               ""$ref"": ""#/definitions/_Error""
             }
           }
         }
       }
     },
     ""/CompositeKeyItems({FirstKey}, {SecondKey})"": {
       ""get"": {
         ""summary"": ""Get entity from CompositeKeyItems by key."",
         ""operationId"": ""CompositeKeyItems_GetById"",
         ""description"": ""Returns the entity with the key from CompositeKeyItems"",
         ""tags"": [
           ""CompositeKeyItems""
         ],
         ""parameters"": [
           {
             ""name"": ""FirstKey"",
             ""in"": ""path"",
             ""description"": ""key: FirstKey"",
             ""type"": ""integer"",
             ""format"": ""int32""
           },
           {
             ""name"": ""SecondKey"",
             ""in"": ""path"",
             ""description"": ""key: SecondKey"",
             ""type"": ""integer"",
             ""format"": ""int32""
           },
           {
             ""name"": ""$select"",
             ""in"": ""query"",
             ""description"": ""description"",
             ""type"": ""string""
           }
         ],
         ""responses"": {
           ""200"": {
             ""description"": ""EntitySet CompositeKeyItems"",
             ""schema"": {
               ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.CompositeKeyItem""
             }
           },
           ""default"": {
             ""description"": ""Unexpected error"",
             ""schema"": {
               ""$ref"": ""#/definitions/_Error""
             }
           }
         }
       },
       ""patch"": {
         ""summary"": ""Update entity in EntitySet CompositeKeyItems"",
         ""operationId"": ""CompositeKeyItems_PatchById"",
         ""description"": ""Update entity in EntitySet CompositeKeyItems"",
         ""tags"": [
           ""CompositeKeyItems""
         ],
         ""parameters"": [
           {
             ""name"": ""FirstKey"",
             ""in"": ""path"",
             ""description"": ""key: FirstKey"",
             ""type"": ""integer"",
             ""format"": ""int32""
           },
           {
             ""name"": ""SecondKey"",
             ""in"": ""path"",
             ""description"": ""key: SecondKey"",
             ""type"": ""integer"",
             ""format"": ""int32""
           },
           {
             ""name"": ""CompositeKeyItem"",
             ""in"": ""body"",
             ""description"": ""The entity to patch"",
             ""schema"": {
               ""$ref"": ""#/definitions/Microsoft.Test.E2E.AspNet.OData.Swagger.CompositeKeyItem""
             }
           }
         ],
         ""responses"": {
           ""204"": {
             ""description"": ""Empty response""
           },
           ""default"": {
             ""description"": ""Unexpected error"",
             ""schema"": {
               ""$ref"": ""#/definitions/_Error""
             }
           }
         }
       },
       ""delete"": {
         ""summary"": ""Delete entity in EntitySet CompositeKeyItems"",
         ""operationId"": ""CompositeKeyItems_DeleteById"",
         ""description"": ""Delete entity in EntitySet CompositeKeyItems"",
         ""tags"": [
           ""CompositeKeyItems""
         ],
         ""parameters"": [
           {
             ""name"": ""FirstKey"",
             ""in"": ""path"",
             ""description"": ""key: FirstKey"",
             ""type"": ""integer"",
             ""format"": ""int32""
           },
           {
             ""name"": ""SecondKey"",
             ""in"": ""path"",
             ""description"": ""key: SecondKey"",
             ""type"": ""integer"",
             ""format"": ""int32""
           },
           {
             ""name"": ""If-Match"",
             ""in"": ""header"",
             ""description"": ""If-Match header"",
             ""type"": ""string""
           }
         ],
         ""responses"": {
           ""204"": {
             ""description"": ""Empty response""
           },
           ""default"": {
             ""description"": ""Unexpected error"",
             ""schema"": {
               ""$ref"": ""#/definitions/_Error""
             }
           }
         }
       }
     },
    ""/UnboundFunction(param={param})"": {
      ""get"": {
        ""summary"": ""Call operation import  UnboundFunction"",
        ""operationId"": ""UnboundFunction_FunctionImportGet"",
        ""description"": ""Call operation import  UnboundFunction"",
        ""tags"": [
          ""Function Import""
        ],
        ""parameters"": [
          {
            ""name"": ""param"",
            ""in"": ""path"",
            ""description"": ""parameter: param"",
            ""type"": ""integer"",
            ""format"": ""int32""
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""Response from UnboundFunction"",
            ""schema"": {
              ""type"": ""string""
            }
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      }
    },
    ""/UnboundAction()"": {
      ""post"": {
        ""summary"": ""Call operation import  UnboundAction"",
        ""operationId"": ""UnboundAction_ActionImportPost"",
        ""description"": ""Call operation import  UnboundAction"",
        ""tags"": [
          ""Action Import""
        ],
        ""parameters"": [
          {
            ""name"": ""param"",
            ""in"": ""body"",
            ""description"": ""parameter: param"",
            ""schema"": {
              ""type"": ""number"",
              ""format"": ""double""
            }
          }
        ],
        ""responses"": {
          ""204"": {
            ""description"": ""Empty response""
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      }
    },
    ""/Customers({CustomerId})/Default.BoundFunction(name='{name}')"": {
      ""get"": {
        ""summary"": ""Call operation  BoundFunction"",
        ""operationId"": ""BoundFunction_FunctionGetById"",
        ""description"": ""Call operation  BoundFunction"",
        ""tags"": [
          ""Customers"",
          ""Function""
        ],
        ""parameters"": [
          {
            ""name"": ""CustomerId"",
            ""in"": ""path"",
            ""description"": ""key: CustomerId"",
            ""type"": ""integer"",
            ""format"": ""int32""
          },
          {
            ""name"": ""name"",
            ""in"": ""path"",
            ""description"": ""parameter: name"",
            ""type"": ""string""
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""Response from BoundFunction"",
            ""schema"": {
              ""type"": ""number"",
              ""format"": ""double""
            }
          },
          ""default"": {
            ""description"": ""Unexpected error"",
            ""schema"": {
              ""$ref"": ""#/definitions/_Error""
            }
          }
        }
      }
    }
  },
  ""definitions"": {
    ""Microsoft.Test.E2E.AspNet.OData.Swagger.Customer"": {
      ""properties"": {
        ""CustomerId"": {
          ""description"": ""CustomerId"",
          ""type"": ""integer"",
          ""format"": ""int32""
        }
      }
    },
    ""Microsoft.Test.E2E.AspNet.OData.Swagger.Order"": {
      ""properties"": {
        ""OrderId"": {
          ""description"": ""OrderId"",
          ""type"": ""integer"",
          ""format"": ""int32""
        }
      }
    },
    ""Microsoft.Test.E2E.AspNet.OData.Swagger.CompositeKeyItem"": {
      ""properties"": {
        ""FirstKey"": {
          ""description"": ""FirstKey"",
          ""type"": ""integer"",
          ""format"": ""int32""
        },
        ""SecondKey"": {
          ""description"": ""SecondKey"",
          ""type"": ""integer"",
          ""format"": ""int32""
        }
      }
    },
    ""_Error"": {
      ""properties"": {
        ""error"": {
          ""$ref"": ""#/definitions/_InError""
        }
      }
    },
    ""_InError"": {
      ""properties"": {
        ""code"": {
          ""type"": ""string""
        },
        ""message"": {
          ""type"": ""string""
        }
      }
    }
  }
}");

            var swaggerUri = string.Format("{0}/odata/$swagger", this.BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, swaggerUri);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject actual = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(expectObj, actual);
        }

        public class Customer
        {
            public int CustomerId { get; set; }

            public Order Order { get; set; }
        }

        public class Order
        {
            public int OrderId { get; set; }
        }

        public class CompositeKeyItem
        {
            public int FirstKey { get; set; }
            public int SecondKey { get; set; }
        }
    }
}
