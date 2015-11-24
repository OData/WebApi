// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData
{
    public class ODataSwaggerConverterTest
    {
        private IEdmModel _model;

        public ODataSwaggerConverterTest()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.Function("UnboundFunction").Returns<string>().Parameter<int>("param");
            builder.Action("UnboundAction").Parameter<double>("param");
            builder.EntityType<Customer>().Function("BoundFunction").Returns<double>().Parameter<string>("name");
            _model = builder.GetEdmModel();
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Action()
        {
            Assert.ThrowsArgumentNull(() => new ODataSwaggerConverter(model: null), "model");
        }

        [Fact]
        public void ODataSwaggerConverterTest_Works()
        {
            // Arrange
            string expect = @"{
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
              ""$ref"": ""#/definitions/System.Web.OData.Customer""
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
              ""$ref"": ""#/definitions/System.Web.OData.Customer""
            }
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""EntitySet Customers"",
            ""schema"": {
              ""$ref"": ""#/definitions/System.Web.OData.Customer""
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
              ""$ref"": ""#/definitions/System.Web.OData.Customer""
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
              ""$ref"": ""#/definitions/System.Web.OData.Customer""
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
              ""$ref"": ""#/definitions/System.Web.OData.Order""
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
              ""$ref"": ""#/definitions/System.Web.OData.Order""
            }
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""EntitySet Orders"",
            ""schema"": {
              ""$ref"": ""#/definitions/System.Web.OData.Order""
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
              ""$ref"": ""#/definitions/System.Web.OData.Order""
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
              ""$ref"": ""#/definitions/System.Web.OData.Order""
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
    ""/UnboundFunction(param={param})"": {
      ""get"": {
        ""summary"": ""Call operation import  UnboundFunction"",
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
    ""System.Web.OData.Customer"": {
      ""properties"": {
        ""CustomerId"": {
          ""description"": ""CustomerId"",
          ""type"": ""integer"",
          ""format"": ""int32""
        }
      }
    },
    ""System.Web.OData.Order"": {
      ""properties"": {
        ""OrderId"": {
          ""description"": ""OrderId"",
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
}";
            ODataSwaggerConverter converter = new ODataSwaggerConverter(_model);

            // Act
            JObject obj = converter.ConvertToSwaggerModel();

            // Assert
            Assert.NotNull(obj);
            Assert.Equal(expect, obj.ToString());
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
    }
}
