// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using System.Threading.Tasks;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ODataFunctionTests
    {
        private const string BaseAddress = @"http://localhost/";

        private const string PrimitiveValues = "(intValues=@p)?@p=[1, 2, null, 7, 8]";

        private const string ComplexValue1 = "{\"@odata.type\":\"%23NS.Address\",\"Street\":\"NE 24th St.\",\"City\":\"Redmond\"}";
        private const string ComplexValue2 = "{\"@odata.type\":\"%23NS.SubAddress\",\"Street\":\"LianHua Rd.\",\"City\":\"Shanghai\", \"Code\":9.9}";

        private const string ComplexValue = "(address=@p)?@p=" + ComplexValue1;
        private const string CollectionComplex = "(addresses=@p)?@p=[" + ComplexValue1 + "," + ComplexValue2 + "]";

        private const string EnumValue = "(color=NS.Color'Red')";
        private const string CollectionEnum = "(colors=@p)?@p=['Red', 'Green']";

        private const string EntityValue1 = "{\"@odata.type\":\"%23NS.Customer\",\"Id\":91,\"Name\":\"John\",\"Location\":" + ComplexValue1 + "}";
        private const string EntityValue2 = "{\"@odata.type\":\"%23NS.SpecialCustomer\",\"Id\":92,\"Name\":\"Mike\",\"Location\":" + ComplexValue2 + ",\"Title\":\"883F50C5-F554-4C49-98EA-F7CACB41658C\"}";

        private const string EntityValue = "(customer=@p)?@p=" + EntityValue1;
        private const string CollectionEntity = "(customers=@p)?@p=[" + EntityValue1 + "," + EntityValue2 + "]";

        private const string EntityReference = "(customer=@p)?@p={\"@odata.id\":\"http://localhost/odata/FCustomers(8)\"}";

        private const string EntityReferences =
            "(customers=@p)?@p=[{\"@odata.id\":\"http://localhost/odata/FCustomers(81)\"},{\"@odata.id\":\"http://localhost/odata/FCustomers(82)/NS.SpecialCustomer\"}]";

        private readonly HttpClient _client;

        public ODataFunctionTests()
        {
            DefaultODataPathHandler pathHandler = new DefaultODataPathHandler();
            var controllers = new[] { typeof(MetadataController), typeof(FCustomersController) };
            var model = GetUnTypedEdmModel();
            var server = TestServerFactory.Create(controllers, (configuration) =>
            {
                // without attribute routing
                configuration.MapODataServiceRoute("odata1", "odata", model, pathHandler, ODataRoutingConventions.CreateDefault());

                // only with attribute routing
                IList<IODataRoutingConvention> routingConventions = new List<IODataRoutingConvention>
                {
#if NETCORE
                    new AttributeRoutingConvention("odata2", configuration.ServiceProvider, pathHandler)
#else
                    new AttributeRoutingConvention("odata2", configuration)
#endif
                };

                configuration.MapODataServiceRoute("odata2", "attribute", model, pathHandler, routingConventions);
            });

            _client = TestServerFactory.CreateClient(server);
        }

        public static TheoryDataSet<string> BoundFunctionRouteData
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { GetBoundFunction("IntCollectionFunction", PrimitiveValues) },

                    { GetBoundFunction("ComplexFunction", ComplexValue) },

                    { GetBoundFunction("ComplexCollectionFunction", CollectionComplex) },

                    { GetBoundFunction("EnumFunction", EnumValue) },

                    { GetBoundFunction("EnumCollectionFunction", CollectionEnum) },

                    { GetBoundFunction("EntityFunction", EntityValue) },
                    { GetBoundFunction("EntityFunction", EntityReference) },// reference

                    { GetBoundFunction("CollectionEntityFunction", CollectionEntity) },
                    { GetBoundFunction("CollectionEntityFunction", EntityReferences) },// references
                };
            }
        }

        public static TheoryDataSet<string> UnboundFunctionRouteData
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { GetUnboundFunction("UnboundIntCollectionFunction", PrimitiveValues) },

                    { GetUnboundFunction("UnboundComplexFunction", ComplexValue) },

                    { GetUnboundFunction("UnboundComplexCollectionFunction", CollectionComplex) },

                    { GetUnboundFunction("UnboundEnumFunction", EnumValue) },

                    { GetUnboundFunction("UnboundEnumCollectionFunction", CollectionEnum) },

                    { GetUnboundFunction("UnboundEntityFunction", EntityValue) },
                    { GetUnboundFunction("UnboundEntityFunction", EntityReference) },// reference

                    { GetUnboundFunction("UnboundCollectionEntityFunction", CollectionEntity) },
                    { GetUnboundFunction("UnboundCollectionEntityFunction", EntityReferences) }, // references
                };
            }
        }

        private static string GetUnboundFunction(string functionName, string parameter)
        {
            int key = 9;
            if (parameter.Contains("@odata.id"))
            {
                key = 8; // used to check the result
            }

            parameter = parameter.Insert(1, "key=" + key + ",");
            return functionName + parameter;
        }

        private static string GetBoundFunction(string functionName, string parameter)
        {
            int key = 9;
            if (parameter.Contains("@odata.id"))
            {
                key = 8; // used to check the result
            }

            return "FCustomers(" + key + ")/NS." + functionName + parameter;
        }

        [Theory]
        [MemberData(nameof(BoundFunctionRouteData))]
        public async Task FunctionWorks_WithParameters_ForUnTyped(string odataPath)
        {
            // Arrange
            string requestUri = BaseAddress + "odata/" + odataPath;

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.True((bool)result["value"]);
        }

        [Theory]
        [MemberData(nameof(BoundFunctionRouteData))]
        [MemberData(nameof(UnboundFunctionRouteData))]
        public async Task FunctionWorks_WithParameters_OnlyWithAttributeRouting_ForUnTyped(string odataPath)
        {
            // Arrange
            string requestUri = BaseAddress + "attribute/" + odataPath;
            if (requestUri.Contains("@odata.id"))
            {
                requestUri = requestUri.Replace("http://localhost/odata", "http://localhost/attribute");
            }

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.True((bool)result["value"]);
        }

        [Fact]
        public void FunctionCallingFails_WithParametersValueMissing()
        {
            // Arrange
            string complexFunction = "ComplexFunction(address=@p)";
            string requestUri = BaseAddress + "odata/FCustomers(2)/NS." + complexFunction;

#if NETCORE
            // Act
            AggregateException exception = Assert.Throws<AggregateException>(() => _client.GetAsync(requestUri).Result);

            // Assert
            Assert.Contains("Missing the value of the parameter 'address' in the function 'ComplexFunction' calling", exception.Message);
#else
            // Act
            var response = _client.GetAsync(requestUri).Result;

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

            string result = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("Missing the value of the parameter 'address' in the function 'ComplexFunction' calling", result);
#endif
        }

        [Fact]
        public async Task FunctionCallingSuccess_WithParametersValueAsNull()
        {
            // Arrange
            string complexFunction = "ComplexFunction(address=null)";
            string requestUri = BaseAddress + "odata/FCustomers(99)/NS." + complexFunction;

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.False((bool)result["value"]);
        }

        [Fact]
        public async Task Response_Includes_FunctionLinkForFeed_WithAcceptHeader()
        {
            // Arrange
            string editLink = BaseAddress + "odata/FCustomers";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, editLink);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));

            // Act
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();
            dynamic result = JObject.Parse(responseString);
            dynamic function = result["#NS.BoundToCollectionFunction"];

            // Assert
            Assert.NotNull(function);
            Assert.Equal("http://localhost/odata/FCustomers/NS.BoundToCollectionFunction(p=@p)", (string)function.target);
            Assert.Equal("BoundToCollectionFunction", (string)function.title);
        }

        private static IEdmModel GetUnTypedEdmModel()
        {
            EdmModel model = new EdmModel();

            // Enum type "Color"
            EdmEnumType colorEnum = new EdmEnumType("NS", "Color");
            colorEnum.AddMember(new EdmEnumMember(colorEnum, "Red", new EdmEnumMemberValue(0)));
            colorEnum.AddMember(new EdmEnumMember(colorEnum, "Blue", new EdmEnumMemberValue(1)));
            colorEnum.AddMember(new EdmEnumMember(colorEnum, "Green", new EdmEnumMemberValue(2)));
            model.AddElement(colorEnum);

            // complex type "Address"
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // derived complex type "SubAddress"
            EdmComplexType subAddress = new EdmComplexType("NS", "SubAddress", address);
            subAddress.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Double);
            model.AddElement(subAddress);

            // entity type "Customer"
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            customer.AddStructuralProperty("Location", new EdmComplexTypeReference(address, isNullable: true));
            model.AddElement(customer);

            // derived entity type special customer
            EdmEntityType specialCustomer = new EdmEntityType("NS", "SpecialCustomer", customer);
            specialCustomer.AddStructuralProperty("Title", EdmPrimitiveTypeKind.Guid);
            model.AddElement(specialCustomer);

            // entity sets
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            container.AddEntitySet("FCustomers", customer);

            EdmComplexTypeReference complexType = new EdmComplexTypeReference(address, isNullable: true);
            EdmCollectionTypeReference complexCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(complexType));

            EdmEnumTypeReference enumType = new EdmEnumTypeReference(colorEnum, isNullable: false);
            EdmCollectionTypeReference enumCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(enumType));

            EdmEntityTypeReference entityType = new EdmEntityTypeReference(customer, isNullable: false);
            EdmCollectionTypeReference entityCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(entityType));

            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: true);
            EdmCollectionTypeReference primitiveCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(intType));

            // bound functions
            BoundFunction(model, "IntCollectionFunction", "intValues", primitiveCollectionType, entityType);

            BoundFunction(model, "ComplexFunction", "address", complexType, entityType);

            BoundFunction(model, "ComplexCollectionFunction", "addresses", complexCollectionType, entityType);

            BoundFunction(model, "EnumFunction", "color", enumType, entityType);

            BoundFunction(model, "EnumCollectionFunction", "colors", enumCollectionType, entityType);

            BoundFunction(model, "EntityFunction", "customer", entityType, entityType);

            BoundFunction(model, "CollectionEntityFunction", "customers", entityCollectionType, entityType);

            // unbound functions
            UnboundFunction(container, "UnboundIntCollectionFunction", "intValues", primitiveCollectionType);

            UnboundFunction(container, "UnboundComplexFunction", "address", complexType);

            UnboundFunction(container, "UnboundComplexCollectionFunction", "addresses", complexCollectionType);

            UnboundFunction(container, "UnboundEnumFunction", "color", enumType);

            UnboundFunction(container, "UnboundEnumCollectionFunction", "colors", enumCollectionType);

            UnboundFunction(container, "UnboundEntityFunction", "customer", entityType);

            UnboundFunction(container, "UnboundCollectionEntityFunction", "customers", entityCollectionType);

            // bound to collection
            BoundToCollectionFunction(model, "BoundToCollectionFunction", "p", intType, entityType);

            model.SetAnnotationValue<BindableOperationFinder>(model, new BindableOperationFinder(model));
            return model;
        }

        private static void BoundFunction(EdmModel model, string funcName, string paramName, IEdmTypeReference edmType, IEdmEntityTypeReference bindingType)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

            EdmFunction boundFunction = new EdmFunction("NS", funcName, returnType, isBound: true, entitySetPathExpression: null, isComposable: false);
            boundFunction.AddParameter("entity", bindingType);
            boundFunction.AddParameter(paramName, edmType);
            model.AddElement(boundFunction);
        }

        private static void BoundToCollectionFunction(EdmModel model, string funcName, string paramName, IEdmTypeReference edmType, IEdmEntityTypeReference bindingType)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmCollectionTypeReference collectonType = new EdmCollectionTypeReference(new EdmCollectionType(bindingType));
            EdmFunction boundFunction = new EdmFunction("NS", funcName, returnType, isBound: true, entitySetPathExpression: null, isComposable: false);
            boundFunction.AddParameter("entityset", collectonType);
            boundFunction.AddParameter(paramName, edmType);
            model.AddElement(boundFunction);
        }
        private static void UnboundFunction(EdmEntityContainer container, string funcName, string paramName, IEdmTypeReference edmType)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

            var unboundFunction = new EdmFunction("NS", funcName, returnType, isBound: false, entitySetPathExpression: null, isComposable: true);
            unboundFunction.AddParameter("key", EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false));
            unboundFunction.AddParameter(paramName, edmType);
            container.AddFunctionImport(funcName, unboundFunction, entitySet: null);
        }
    }

    public class FCustomersController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            IEdmModel model = Request.GetModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Customer");

            EdmEntityObject customer = new EdmEntityObject(customerType);

            customer.TrySetPropertyValue("Id", 1);
            customer.TrySetPropertyValue("Tony", 1);

            EdmEntityObjectCollection customers =
                new EdmEntityObjectCollection(
                    new EdmCollectionTypeReference(new EdmCollectionType(customerType.ToEdmTypeReference(false))));
            customers.Add(customer);
            return Ok(customers);

        }

        [HttpGet]
        [ODataRoute("FCustomers({key})/NS.IntCollectionFunction(intValues={intValues})")]
        [ODataRoute("UnboundIntCollectionFunction(key={key},intValues={intValues})")]
        public bool IntCollectionFunction(int key, [FromODataUri] IEnumerable<int?> intValues)
        {
            Assert.NotNull(intValues);

            IList<int?> values = intValues.ToList();
            Assert.Equal(1, values[0]);
            Assert.Equal(2, values[1]);
            Assert.Null(values[2]);
            Assert.Equal(7, values[3]);
            Assert.Equal(8, values[4]);

            return true;
        }

        [HttpGet]
        [ODataRoute("FCustomers({key})/NS.EnumFunction(color={color})")]
        [ODataRoute("UnboundEnumFunction(key={key},color={color})")]
        public bool EnumFunction(int key, [FromODataUri] EdmEnumObject color)
        {
            Assert.NotNull(color);
            Assert.Equal("NS.Color", color.GetEdmType().FullName());
            Assert.Equal("0", color.Value);
            return true;
        }

        [HttpGet]
        [ODataRoute("FCustomers({key})/NS.EnumCollectionFunction(colors={colors})")]
        [ODataRoute("UnboundEnumCollectionFunction(key={key},colors={colors})")]
        public bool EnumCollectionFunction(int key, [FromODataUri] EdmEnumObjectCollection colors)
        {
            Assert.NotNull(colors);
            IList<IEdmEnumObject> results = colors.ToList();

            Assert.Equal(2, results.Count);

            // #1
            EdmEnumObject color = results[0] as EdmEnumObject;
            Assert.NotNull(color);
            Assert.Equal("NS.Color", color.GetEdmType().FullName());
            Assert.Equal("Red", color.Value);

            // #2
            EdmEnumObject color2 = results[1] as EdmEnumObject;
            Assert.NotNull(color2);
            Assert.Equal("NS.Color", color2.GetEdmType().FullName());
            Assert.Equal("Green", color2.Value);
            return true;
        }

        [HttpGet]
        [ODataRoute("FCustomers({key})/NS.ComplexFunction(address={address})")]
        [ODataRoute("UnboundComplexFunction(key={key},address={address})")]
        public bool ComplexFunction(int key, [FromODataUri] EdmComplexObject address)
        {
            if (key == 99)
            {
                Assert.Null(address);
                return false;
            }

            Assert.NotNull(address);
            dynamic result = address;
            Assert.Equal("NS.Address", address.GetEdmType().FullName());
            Assert.Equal("NE 24th St.", result.Street);
            Assert.Equal("Redmond", result.City);
            return true;
        }

        [HttpGet]
        [ODataRoute("FCustomers({key})/NS.ComplexCollectionFunction(addresses={addresses})")]
        [ODataRoute("UnboundComplexCollectionFunction(key={key},addresses={addresses})")]
        public bool ComplexCollectionFunction(int key, [FromODataUri] EdmComplexObjectCollection addresses)
        {
            Assert.NotNull(addresses);
            IList<IEdmComplexObject> results = addresses.ToList();

            Assert.Equal(2, results.Count);

            // #1
            EdmComplexObject complex = results[0] as EdmComplexObject;
            Assert.Equal("NS.Address", complex.GetEdmType().FullName());

            dynamic address = results[0];
            Assert.NotNull(address);
            Assert.Equal("NE 24th St.", address.Street);
            Assert.Equal("Redmond", address.City);

            // #2
            complex = results[1] as EdmComplexObject;
            Assert.Equal("NS.SubAddress", complex.GetEdmType().FullName());

            address = results[1];
            Assert.NotNull(address);
            Assert.Equal("LianHua Rd.", address.Street);
            Assert.Equal("Shanghai", address.City);
            Assert.Equal(9.9, address.Code);
            return true;
        }

        [HttpGet]
        [ODataRoute("FCustomers({key})/NS.EntityFunction(customer={customer})")]
        [ODataRoute("UnboundEntityFunction(key={key},customer={customer})")]
        public bool EntityFunction(int key, [FromODataUri] EdmEntityObject customer)
        {
            Assert.NotNull(customer);
            dynamic result = customer;
            Assert.Equal("NS.Customer", customer.GetEdmType().FullName());

            // entity call
            if (key == 9)
            {
                Assert.Equal(91, result.Id);
                Assert.Equal("John", result.Name);

                dynamic address = result.Location;
                EdmComplexObject addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.Address", addressObj.GetEdmType().FullName());
                Assert.Equal("NE 24th St.", address.Street);
                Assert.Equal("Redmond", address.City);
            }
            else
            {
                // entity reference call
                Assert.Equal(8, result.Id);
                Assert.Equal("Id", String.Join(",", customer.GetChangedPropertyNames()));

                Assert.Equal("Name,Location", String.Join(",", customer.GetUnchangedPropertyNames()));
            }

            return true;
        }

        [HttpGet]
        [ODataRoute("FCustomers({key})/NS.CollectionEntityFunction(customers={customers})")]
        [ODataRoute("UnboundCollectionEntityFunction(key={key},customers={customers})")]
        public bool CollectionEntityFunction(int key, [FromODataUri] EdmEntityObjectCollection customers)
        {
            Assert.NotNull(customers);
            IList<IEdmEntityObject> results = customers.ToList();
            Assert.Equal(2, results.Count);

            // entities call
            if (key == 9)
            {
                // #1
                EdmEntityObject entity = results[0] as EdmEntityObject;
                Assert.NotNull(entity);
                Assert.Equal("NS.Customer", entity.GetEdmType().FullName());

                dynamic customer = results[0];
                Assert.Equal(91, customer.Id);
                Assert.Equal("John", customer.Name);

                dynamic address = customer.Location;
                EdmComplexObject addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.Address", addressObj.GetEdmType().FullName());
                Assert.Equal("NE 24th St.", address.Street);
                Assert.Equal("Redmond", address.City);

                // #2
                entity = results[1] as EdmEntityObject;
                Assert.Equal("NS.SpecialCustomer", entity.GetEdmType().FullName());

                customer = results[1];
                Assert.Equal(92, customer.Id);
                Assert.Equal("Mike", customer.Name);

                address = customer.Location;
                addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.SubAddress", addressObj.GetEdmType().FullName());
                Assert.Equal("LianHua Rd.", address.Street);
                Assert.Equal("Shanghai", address.City);
                Assert.Equal(9.9, address.Code);

                Assert.Equal(new Guid("883F50C5-F554-4C49-98EA-F7CACB41658C"), customer.Title);
            }
            else
            {
                // entity references call
                int id = 81;
                foreach (IEdmEntityObject edmObj in results)
                {
                    EdmEntityObject entity = edmObj as EdmEntityObject;
                    Assert.NotNull(entity);
                    Assert.Equal("NS.Customer", entity.GetEdmType().FullName());

                    dynamic customer = entity;
                    Assert.Equal(id++, customer.Id);
                    Assert.Equal("Id", String.Join(",", customer.GetChangedPropertyNames()));
                    Assert.Equal("Name,Location", String.Join(",", customer.GetUnchangedPropertyNames()));
                }
            }

            return true;
        }
    }
}
