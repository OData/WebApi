//-----------------------------------------------------------------------------
// <copyright file="BoundOperationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BoundOperation
{
    public class BoundOperationTest : WebHostTestBase
    {
        private const string CollectionOfEmployee = "Collection(NS.Employee)";
        private const string CollectionOfManager = "Collection(NS.Manager)";
        private const string Employee = "NS.Employee";
        private const string Manager = "NS.Manager";

        public BoundOperationTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var requestUriForPost = this.BaseAddress + "/AttributeRouting/ResetDataSource";
            var responseForPost = await this.Client.PostAsync(requestUriForPost, new StringContent(""));
            Assert.True(responseForPost.IsSuccessStatusCode);
            return responseForPost;
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();

            IEdmModel edmModel = UnBoundFunctionEdmModel.GetEdmModel(configuration);
            DefaultODataPathHandler pathHandler = new DefaultODataPathHandler();

            // only with attribute routing & metadata routing convention
            IList<IODataRoutingConvention> routingConventions = new List<IODataRoutingConvention>
            {
                configuration.CreateAttributeRoutingConvention(),
                new MetadataRoutingConvention()
            };
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("AttributeRouting", "AttributeRouting", edmModel, pathHandler, routingConventions);

            // only with convention routing
            configuration.MapODataServiceRoute("ConventionRouting", "ConventionRouting", edmModel, pathHandler, ODataRoutingConventions.CreateDefault());
            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData("AttributeRouting")]
        [InlineData("ConventionRouting")]
        [Trait("Pioneer", "true")]
        public async Task ModelBuilderTest(string routing)
        {
            // Arrange
            string requestUri = string.Format("{0}/{1}/$metadata", this.BaseAddress, routing);
            var typeOfEmployee = typeof(Employee);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            // Assert
            #region functions
            // Function GetCount
            var iEdmOperationsOfGetCount = edmModel.FindDeclaredOperations("Default.GetCount");
            Assert.Equal(3, iEdmOperationsOfGetCount.Count());

            //     (Collection(Employee)).GetCount(String Name)
            var getCount = iEdmOperationsOfGetCount.Where(f => f.Parameters.Count() == 2);
            Assert.Single(getCount);

            //     (Collection(Manager)).GetCount()
            foreach (var t in iEdmOperationsOfGetCount)
            {
                var t1 = t.Parameters.First().Type.Definition.ToString();
                var t2 = t.Parameters.First().Type;
            }
            getCount = iEdmOperationsOfGetCount.Where(f => f.Parameters.Count() == 1
                && f.Parameters.First().Type.Definition.ToString()
                .Equals(string.Format("Collection([{0} Nullable=True])", Manager)));
            Assert.Single(getCount);

            //     (Collection(Employee)).GetCount()
            getCount = iEdmOperationsOfGetCount.Where(f => f.Parameters.Count() == 1
                && f.Parameters.First().Type.Definition.ToString()
                .Equals(string.Format("Collection([{0} Nullable=True])", Employee)));
            Assert.Single(getCount);

            // Function GetEmailsCount()
            var iEdmOperationsOfGetEmailsCount = edmModel.FindDeclaredOperations("Default.GetEmailsCount");
            Assert.Equal(2, iEdmOperationsOfGetEmailsCount.Count());

            //     Empllyee.GetEmailCount()
            var getEmailsCount = iEdmOperationsOfGetEmailsCount.Where(f => f.Parameters.Count() == 1
                && f.Parameters.First().Type.Definition.ToString().Equals(Employee));
            Assert.Single(getEmailsCount);

            //     Manager.GetEmailCount()
            getEmailsCount = iEdmOperationsOfGetEmailsCount.Where(f => f.Parameters.Count() == 1
                && f.Parameters.First().Type.Definition.ToString().Equals(Manager));
            Assert.Single(getEmailsCount);

            // primitive & collection of primitive
            AssertPrimitiveOperation(edmModel, "Default.PrimitiveFunction");

            // Enum & collection of Enum
            AssertEnumOperation(edmModel, "Default.EnumFunction");

            // Complex & collection of Complex
            AssertComplexOperation(edmModel, "Default.ComplexFunction");

            // Entity & collection of Entity
            AssertEntityOperation(edmModel, "Default.EntityFunction");

            // Function with optional parameters
            AssertOperationWithOptionalParameter(edmModel, "Default.GetWholeSalary");

            #endregion

            #region actions

            // Action IncreaseSalary
            var iEdmOperationOfIncreaseSalary = edmModel.FindDeclaredOperations("Default.IncreaseSalary");
            Assert.Equal(4, iEdmOperationOfIncreaseSalary.Count());

            //    (Collection(Employee)).IncreaseSalary(String Name)
            var increaseSalary = iEdmOperationOfIncreaseSalary.Where(a => a.Parameters.Count() == 2
                && a.Parameters.First().Type.Definition.ToString().Equals(CollectionOfEmployee));
            Assert.NotNull(increaseSalary);

            //    (Collection(Manager)).IncreaseSalary()
            increaseSalary = iEdmOperationOfIncreaseSalary.Where(a => a.Parameters.Count() == 1
                && a.Parameters.First().Type.Definition.ToString().Equals(CollectionOfManager));
            Assert.NotNull(increaseSalary);

            //    Employee.IncreaseSalary()
            increaseSalary = iEdmOperationOfIncreaseSalary.Where(a => a.Parameters.Count() == 1
                && a.Parameters.First().Type.Definition.ToString().Equals(Employee));
            Assert.NotNull(increaseSalary);

            //    Manager.IncreaseSalary()
            increaseSalary = iEdmOperationOfIncreaseSalary.Where(a => a.Parameters.Count() == 1
                && a.Parameters.First().Type.Definition.ToString().Equals(Manager));
            Assert.NotNull(increaseSalary);

            // primitive & collection of primitive
            AssertPrimitiveOperation(edmModel, "Default.PrimitiveAction");

            // Enum & collection of Enum
            AssertEnumOperation(edmModel, "Default.EnumAction");

            // Complex & collection of Complex
            AssertComplexOperation(edmModel, "Default.ComplexAction");

            // Entity & collection of Entity
            AssertEntityOperation(edmModel, "Default.EntityAction");

            // Action with optional parameters
            AssertOperationWithOptionalParameter(edmModel, "Default.IncreaseWholeSalary");

            #endregion

            // ActionImport: ResetDataSource
            Assert.Single(edmModel.EntityContainer.OperationImports());
        }

        private static void AssertOperationWithOptionalParameter(IEdmModel edmModel, string opertionName)
        {
            IEdmOperation primitiveFunc = Assert.Single(edmModel.FindDeclaredOperations(opertionName));
            Assert.Equal(4, primitiveFunc.Parameters.Count());

            // non-optional parameter
            IEdmOperationParameter parameter = Assert.Single(primitiveFunc.Parameters.Where(e => e.Name == "minSalary"));
            Assert.Equal("Edm.Double", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            // optional parameter without default value
            parameter = Assert.Single(primitiveFunc.Parameters.Where(e => e.Name == "maxSalary"));
            Assert.NotNull(parameter);
            Assert.Equal("Edm.Double", parameter.Type.FullName());
            IEdmOptionalParameter optionalParameterInfo = Assert.IsAssignableFrom<IEdmOptionalParameter>(parameter);
            Assert.NotNull(optionalParameterInfo);
            Assert.Null(optionalParameterInfo.DefaultValueString);

            // optional parameter with default value
            parameter = Assert.Single(primitiveFunc.Parameters.Where(e => e.Name == "aveSalary"));
            Assert.NotNull(parameter);
            Assert.Equal("Edm.Double", parameter.Type.FullName());
            optionalParameterInfo = Assert.IsAssignableFrom<IEdmOptionalParameter>(parameter);
            Assert.NotNull(optionalParameterInfo);
            Assert.Equal("8.9", optionalParameterInfo.DefaultValueString);
        }

        private static void AssertPrimitiveOperation(IEdmModel edmModel, string opertionName)
        {
            IEdmOperation primitiveFunc = Assert.Single(edmModel.FindDeclaredOperations(opertionName));
            Assert.Equal(5, primitiveFunc.Parameters.Count());

            IEdmOperationParameter parameter = Assert.Single(primitiveFunc.Parameters.Where(e => e.Name == "param"));
            Assert.Equal("Edm.Int32", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            parameter = Assert.Single(primitiveFunc.Parameters.Where(e => e.Name == "price"));
            Assert.Equal("Edm.Double", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);

            parameter = Assert.Single(primitiveFunc.Parameters.Where(e => e.Name == "name"));
            Assert.Equal("Edm.String", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);

            parameter = Assert.Single(primitiveFunc.Parameters.Where(e => e.Name == "names"));
            Assert.Equal("Collection(Edm.String)", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);
        }

        private static void AssertEnumOperation(IEdmModel edmModel, string operationName)
        {
            IEdmOperation enumFunc = Assert.Single(edmModel.FindDeclaredOperations(operationName));
            Assert.Equal(4, enumFunc.Parameters.Count());

            IEdmOperationParameter parameter = Assert.Single(enumFunc.Parameters.Where(e => e.Name == "bkColor"));
            Assert.Equal("NS.Color", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            parameter = Assert.Single(enumFunc.Parameters.Where(e => e.Name == "frColor"));
            Assert.Equal("NS.Color", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);

            parameter = Assert.Single(enumFunc.Parameters.Where(e => e.Name == "colors"));
            Assert.Equal("Collection(NS.Color)", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);
        }

        private static void AssertComplexOperation(IEdmModel edmModel, string operationName)
        {
            IEdmOperation complexFunc = Assert.Single(edmModel.FindDeclaredOperations(operationName));
            Assert.Equal(4, complexFunc.Parameters.Count());

            IEdmOperationParameter parameter = Assert.Single(complexFunc.Parameters.Where(e => e.Name == "address"));
            Assert.Equal("NS.Address", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            parameter = Assert.Single(complexFunc.Parameters.Where(e => e.Name == "location"));
            Assert.Equal("NS.Address", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);

            parameter = Assert.Single(complexFunc.Parameters.Where(e => e.Name == "addresses"));
            Assert.Equal("Collection(NS.Address)", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);
        }

        private static void AssertEntityOperation(IEdmModel edmModel, string operationName)
        {
            IEdmOperation entityFunc = Assert.Single(edmModel.FindDeclaredOperations(operationName));
            Assert.Equal(4, entityFunc.Parameters.Count());

            IEdmOperationParameter parameter = Assert.Single(entityFunc.Parameters.Where(e => e.Name == "person"));
            Assert.Equal("NS.Employee", parameter.Type.FullName());
            Assert.False(parameter.Type.IsNullable);

            parameter = Assert.Single(entityFunc.Parameters.Where(e => e.Name == "guard"));
            Assert.Equal("NS.Employee", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);

            parameter = Assert.Single(entityFunc.Parameters.Where(e => e.Name == "staff"));
            Assert.Equal("Collection(NS.Employee)", parameter.Type.FullName());
            Assert.True(parameter.Type.IsNullable);
        }
        #region functions

        [Theory]
        [InlineData("ConventionRouting/Employees/Default.GetCount()")]//Convention routing
        [InlineData("AttributeRouting/Employees/Default.GetCount()")]//Attribute routing
        public async Task FunctionBoundToEntitySet(string url)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("/$metadata#Edm.Int32", responseString);
            if (url.StartsWith("ConventionRouting"))
            {
                Assert.Contains(@"""value"":11", responseString);
            }
            else
            {
                Assert.Contains(@"""value"":22", responseString);
            }
        }

        [Theory]
        [InlineData("ConventionRouting/Employees/Default.GetCount(Name='Name1%252F')", 1)]// Slash
        [InlineData("AttributeRouting/Employees/Default.GetCount(Name='Name6%3F')", 2)]// QuestionMark
        [InlineData("AttributeRouting/Employees/Default.GetCount(Name='Name20%23')", 2)]// Pound
        public async Task FunctionBoundToEntitySetOverload(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("/$metadata#Edm.Int32", responseString);
            Assert.Contains(string.Format(@"""value"":{0}", expectedCount), responseString);
        }

        [Theory]
        [InlineData("ConventionRouting/Employees/NS.Manager/Default.GetCount()", 5)]//Convention routing
        [InlineData("AttributeRouting/Employees/NS.Manager/Default.GetCount()", 10)]//Attribute routing
        public async Task FunctionBoundToEntitySetForDerivedBindingType(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("/$metadata#Edm.Int32", responseString);
            Assert.Contains(string.Format(@"""value"":{0}", expectedCount), responseString);
        }

        [Theory]
        [InlineData("ConventionRouting/Employees(1)/Default.GetEmailsCount()", 1)]//Convention routing
        [InlineData("AttributeRouting/Employees(1)/Default.GetEmailsCount()", 2)]//Attribute routing
        public async Task FunctionBoundToEntityType(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("/$metadata#Edm.Int32", responseString);
            Assert.Contains(string.Format(@"""value"":{0}", expectedCount), responseString);
        }

        [Theory]
        [InlineData("ConventionRouting/Employees/Default.GetWholeSalary(minSalary=6.8)", "(6.8, 0, 8.9)")]
        [InlineData("AttributeRouting/Employees/Default.GetWholeSalary(minSalary=6.8)", "(6.8, 0, 8.9)")]
        [InlineData("ConventionRouting/Employees/Default.GetWholeSalary(minSalary=1.09,maxSalary=7.3)", "(1.09, 7.3, 8.9)")]
        [InlineData("AttributeRouting/Employees/Default.GetWholeSalary(minSalary=1.09,maxSalary=7.3)", "(1.09, 7.3, 8.9)")]
        [InlineData("ConventionRouting/Employees/Default.GetWholeSalary(minSalary=8.1,maxSalary=1.1,aveSalary=3.3)", "(8.1, 1.1, 3.3)")]
        [InlineData("AttributeRouting/Employees/Default.GetWholeSalary(minSalary=8.1,maxSalary=1.1,aveSalary=3.3)", "(8.1, 1.1, 3.3)")]
        public async Task FunctionWithOptionalParamsBoundToEntityType(string url, string expected)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("/$metadata#Edm.String", responseString);
            Assert.Contains(string.Format(@"""value"":""GetWholeSalary{0}""", expected), responseString);
        }

        [Theory]
        [InlineData("ConventionRouting/Employees(1)/NS.Manager/Default.GetEmailsCount()", 1)]//Convention routing
        [InlineData("AttributeRouting/Employees(1)/NS.Manager/Default.GetEmailsCount()", 2)]//Attribute routing
        public async Task FunctionBoundToDerivedEntityType(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("/$metadata#Edm.Int32", responseString);
            Assert.Contains(string.Format(@"""value"":{0}", expectedCount), responseString);
        }

        [Theory]
        [InlineData("ConventionRouting/Employees?$filter=Default.GetEmailsCount() lt 10")]
        [InlineData("ConventionRouting/Employees?$filter=$it/Default.GetEmailsCount() lt 10")]
        [InlineData("ConventionRouting/Employees/NS.Manager?$filter=Default.GetEmailsCount() lt 10")]
        [InlineData("ConventionRouting/Employees/NS.Manager?$filter=$it/Default.GetEmailsCount() lt 10")]
        public async Task BoundFunctionInDollarFilter(string url)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // 400
            Assert.Contains(@"Unknown function 'Default.GetEmailsCount'", responseString);
            Assert.Contains(@"System.NotImplementedException", responseString);
        }

        [Theory]
        [InlineData("ConventionRouting/Employees(1)/Emails/$count", 1)]
        [InlineData("AttributeRouting/Employees(1)/Emails/$count", 2)]
        public async Task DollarCount(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedCount, int.Parse(responseString));
        }

        [Theory]
        [InlineData("ConventionRouting/Employees(1)/Default.GetOptionalAddresses()/$count", 1)]
        [InlineData("AttributeRouting/Employees(1)/Default.GetOptionalAddresses()/$count", 2)]
        [InlineData("ConventionRouting/Employees(1)/Default.GetOptionalAddresses()/$count?$filter=City eq 'Beijing'", 0)]
        [InlineData("AttributeRouting/Employees(1)/Default.GetOptionalAddresses()/$count?$filter=City eq 'Beijing'", 1)]
        [InlineData("ConventionRouting/Employees(1)/OptionalAddresses/$count", 1)]
        [InlineData("AttributeRouting/Employees(1)/OptionalAddresses/$count", 2)]
        [InlineData("ConventionRouting/Employees(1)/OptionalAddresses/$count?$filter=City eq 'Beijing'", 0)]
        [InlineData("AttributeRouting/Employees(1)/OptionalAddresses/$count?$filter=City eq 'Beijing'", 1)]
        public async Task DollarCountFollowingComplexCollection(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(expectedCount == int.Parse(responseString),
                string.Format("Expected: {0}; Actual: {1}; Request URL: {2}", expectedCount, responseString, requestUri));
        }

        [Theory]
        [InlineData("ConventionRouting", "(param=7,price=9.9,name='Tony',names=['Mike','John'])")]
        [InlineData("ConventionRouting", "(param=8,price=null,name=null,names=['Sun',null,'Mike'])")]
        [InlineData("ConventionRouting", "(param=9,price=null,name=null,names=@p)?@p=['Mike',null,'John']")] // parameter alias
        [InlineData("AttributeRouting", "(param=1,price=0.9,name='Tony',names=['Mike','John'])")]
        [InlineData("AttributeRouting", "(param=2,price=null,name=null,names=['Sun',null,'Mike'])")]
        [InlineData("AttributeRouting", "(param=3,price=null,name=@p,names=@q)?@p=null&@q=['Mike',null,'John']")] // parameter alias
        public async Task BoundFunction_Works_WithPrimitive_And_CollectionOfPrimitiveParameters(string route, string parameter)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.PrimitiveFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains(ReplaceParameterAlias(parameter), responseString);
        }

        [Theory]
        [InlineData("ConventionRouting", "(param=null,price=9.9,name='Tony',names=['Mike','John'])")]
        [InlineData("AttributeRouting", "(param=null,price=9.9,name='Tony',names=['Mike','John'])")]
        public async Task BoundFunction_DoesnotWork_WithNullValue_ForNonNullablePrimitiveParameter(string route, string parameter)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.PrimitiveFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("Type verification failed. Expected non-nullable type 'Edm.Int32' but received a null value.", responseString);
        }

        [Theory]
        [InlineData("ConventionRouting", "(bkColor=NS.Color'Red',frColor=NS.Color'Blue',colors=['Red','Green'])")]
        [InlineData("ConventionRouting", "(bkColor=NS.Color'Blue',frColor=null,colors=['Red','Green'])")]
        [InlineData("ConventionRouting", "(bkColor=NS.Color'Green',frColor=@x,colors=@y)?@x=null&@y=['Red','Blue']")] // parameter alias
        [InlineData("AttributeRouting", "(bkColor=NS.Color'Red',frColor=NS.Color'Blue',colors=['Red','Green'])")]
        [InlineData("AttributeRouting", "(bkColor=NS.Color'Blue',frColor=null,colors=['Red','Green'])")]
        [InlineData("AttributeRouting", "(bkColor=NS.Color'Green',frColor=@x,colors=@y)?@x=null&@y=['Red','Blue']")] // parameter alias
        public async Task BoundFunction_Works_WithEnum_And_CollectionOfEnumParameters(string route, string parameter)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.EnumFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains(ReplaceParameterAlias(parameter.Replace("NS.Color", "")), responseString);
        }

        [Theory]
        [InlineData("ConventionRouting", "(bkColor=null,frColor=NS.Color'Red',colors=['Red','Green'])")]
        [InlineData("AttributeRouting", "(bkColor=null,frColor=NS.Color'Blue',colors=['Red','Green'])")]
        public async Task BoundFunction_DoesnotWork_WithNullValue_ForNonNullableEnumParameter(string route, string parameter)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.EnumFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("Type verification failed. Expected non-nullable type 'NS.Color' but received a null value.", responseString);
        }

        [Theory]
        [InlineData("ConventionRouting", "(bkColor=NS.Color'Red',frColor=NS.Color'Red',colors=[null,'Red'])")]
        [InlineData("AttributeRouting", "(bkColor=NS.Color'Green',frColor=NS.Color'Green',colors=[null,'Green'])")]
        public async Task BoundFunction_DoesnotWork_WithNullValue_ForNonNullableCollectionEnumParameter(string route, string parameter)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.EnumFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // change '(names=@p)?@p=xxx' as '(names=xxx)'
        private static string ReplaceParameterAlias(string parameter)
        {
            string[] segments = parameter.Split('?');
            if (segments.Length == 1)
            {
                return parameter;
            }

            string result = segments[0];
            string[] alias = segments[1].Trim().Split('&');
            foreach (var q in alias)
            {
                string[] keyValue = q.Trim().Split('=');
                Assert.Equal(2, keyValue.Length);

                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();
                result = result.Replace(key, value);
            }

            return result;
        }

        public static TheoryDataSet<string, string, string> ComplexTestData
        {
            get
            {
                const string address = "{\"Street\":\"NE 24th St.\",\"City\":\"Redmond\"}";
                const string subAddress = "{\"@odata.type\":\"%23NS.SubAddress\",\"Street\":\"LianHua Rd.\",\"City\":\"Shanghai\", \"Code\":9.9}";
                string[] results =
                {
                   @"{""@odata.context"":CONTEXT,""value"":[ADDRESS,SUBADDRESS,ADDRESS,SUBADDRESS]}",
                   @"{""@odata.context"":CONTEXT,""value"":[ADDRESS,null,ADDRESS,null,SUBADDRESS]}",
                };

                for (int i = 0; i< results.Length; i++)
                {
                    results[i] = results[i].Replace("SUBADDRESS", subAddress); // first replace SUBADDRESS
                    results[i] = results[i].Replace("ADDRESS", address); // then replace ADDRESS
                    results[i] = results[i].Replace("%23", "#");
                }

                IDictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {"(address=@x,location=@y,addresses=@z)?@x=" + address + "&@y=" + subAddress + "&@z=[" + address + "," + subAddress + "]", results[0]},
                    {"(address=@x,location=@y,addresses=@z)?@x=" + address + "&@y=null&@z=[" + address + ",null," + subAddress + "]", results[1] },
                };

                string[] modes = { "ConventionRouting", "AttributeRouting" };
                TheoryDataSet<string, string, string> data = new TheoryDataSet<string, string, string>();
                foreach (string mode in modes)
                {
                    foreach (KeyValuePair<string, string> f in parameters)
                    {
                        data.Add(mode, f.Key, f.Value);
                    }
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(ComplexTestData))]
        public async Task BoundFunction_Works_WithComplex_And_CollectionOfComplexParameters(string route, string parameter, string expect)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.ComplexFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            expect = expect.Replace("CONTEXT", String.Format("\"{0}/{1}/$metadata#Collection(NS.Address)\"", BaseAddress.ToLower(), route));

            Assert.Equal(JObject.Parse(expect), JObject.Parse(responseString));
        }

        [Theory]
        [InlineData("ConventionRouting", "(address=null,location=null,addresses=[])")]
        [InlineData("AttributeRouting", "(address=null,location=null,addresses=[])")]
        public async Task BoundFunction_DoesnotWork_WithNullValue_ForNonNullableComplexParameter(string route, string parameter)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.ComplexFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("Type verification failed. Expected non-nullable type 'NS.Address' but received a null value.", responseString);
        }

        [Theory]
        [InlineData("ConventionRouting", "(address=@p,location=null,addresses=null)?@p={\"Street\":\"NE 24th St.\",\"City\":\"Redmond\"}")]
        [InlineData("AttributeRouting", "(address=@p,location=null,addresses=null)?@p={\"Street\":\"NE 24th St.\",\"City\":\"Redmond\"}")]
        public async Task BoundFunction_DoesnotWork_WithNullValue_ForCollectionComplexParameter(string route, string parameter)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.ComplexFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("Value cannot be null.", responseString);
        }

        public static TheoryDataSet<string, string> EntityTestData
        {
            get
            {
                const string person = "{\"@odata.type\":\"%23NS.Manager\",\"ID\":901,\"Name\":\"John\",\"Heads\":9}";
                const string guard = "{\"@odata.type\":\"%23NS.Employee\",\"ID\":801,\"Name\":\"Mike\"}";
                const string odataId = "{\"@odata.id\":\"BASEADDRESS/Employees(8)\"}";

                string[] parameters =
                {
                    "(person=@x,guard=@y,staff=@z)?@x=" + person + "&@y=" + guard + "&@z=[" + person + "," + guard + "]",
                    "(person=@x,guard=@y,staff=@z)?@x=" + guard + "&@y=null&@z=[" + guard + "," + person + "]",

                    // ODL doesn't work for 'null' in collection of entity. https://github.com/OData/odata.net/issues/100
                    // "(person=@x,guard=@y,staff=@z)?@x=" + guard + "&@y=null&@z={\"value\":[" + guard + ",null," + person + "]}",

                    // Entity Reference
                    "(person=@x,guard=@y,staff=@z)?@x=" + odataId + "&@y=null&@z=[" + odataId + "," + odataId + "]",
                };

                string[] modes = { "ConventionRouting", "AttributeRouting" };
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string mode in modes)
                {
                    foreach (var f in parameters)
                    {
                        data.Add(mode, f);
                    }
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(EntityTestData))]
        public async Task BoundFunction_Works_WithEntity_And_CollectionOfEntityParameters(string route, string parameter)
        {
            // Arrange
            parameter = parameter.Replace("BASEADDRESS", string.Format("{0}/{1}", BaseAddress, route));
            var requestUri = string.Format("{0}/{1}/Employees/Default.EntityFunction{2}", BaseAddress, route, parameter);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();
        }
        #endregion

        #region actions

        [Theory]
        [InlineData("ConventionRouting/Employees/Default.IncreaseSalary", 2)]//Convention routing
        [InlineData("AttributeRouting/Employees/Default.IncreaseSalary", 1)]//Attribute routing
        public async Task ActionBountToEntitySet(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestForPost.Content = new StringContent(@"{""Name"":""Name1""}");
            requestForPost.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            //Act
            HttpResponseMessage responseForPost = await this.Client.SendAsync(requestForPost);
            await this.ResetDatasource();

            //Assert
            Assert.True(responseForPost.IsSuccessStatusCode);
            var response = (await responseForPost.Content.ReadAsObject<JObject>())["value"] as JArray;
            Assert.Equal(expectedCount, response.Count());
        }

        [Theory]
        [InlineData("ConventionRouting/Employees/NS.Manager/Default.IncreaseSalary", 5)]//Convention routing
        [InlineData("AttributeRouting/Employees/NS.Manager/Default.IncreaseSalary", 2)]//Attribute routing
        public async Task ActionBountToEntitySetForDerivedBindingType(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestForPost.Content = new StringContent(@"{""Name"":""Name""}");
            requestForPost.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            //Act
            HttpResponseMessage responseForPost = await this.Client.SendAsync(requestForPost);
            await this.ResetDatasource();

            //Assert
            Assert.True(responseForPost.IsSuccessStatusCode);
            var response = (await responseForPost.Content.ReadAsObject<JObject>())["value"] as JArray;
            Assert.Equal(expectedCount, response.Count());
        }

        [Theory]
        [InlineData("ConventionRouting/Employees/Default.IncreaseSalary", 3)]//Convention routing
        [InlineData("AttributeRouting/Employees/Default.IncreaseSalary", 1)]//Attribute routing
        public async Task ActionFollowedByQueryOption(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}?$filter=ID mod 4 eq 0", this.BaseAddress, url);
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestForPost.Content = new StringContent(@"{""Name"":""Name""}");
            requestForPost.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            //Act
            HttpResponseMessage responseForPost = await this.Client.SendAsync(requestForPost);
            await this.ResetDatasource();

            //Assert
            Assert.True(responseForPost.IsSuccessStatusCode);
            var response = (await responseForPost.Content.ReadAsObject<JObject>())["value"] as JArray;
            Assert.Equal(expectedCount, response.Count());
        }

        [Theory]
        [InlineData("ConventionRouting/Employees(1)/Default.IncreaseSalary", 20)]//Convention routing
        [InlineData("AttributeRouting/Employees(1)/Default.IncreaseSalary", 40)]//Attribute routing
        public async Task ActionBountToBaseEntityType(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Act
            HttpResponseMessage responseForPost = await this.Client.SendAsync(requestForPost);
            await this.ResetDatasource();

            string responseString = await responseForPost.Content.ReadAsStringAsync();

            // Assert
            Assert.True(responseForPost.IsSuccessStatusCode);
            Assert.Contains("/$metadata#Edm.Int32", responseString);
            Assert.Contains(@"""value"":" + expectedCount, responseString);
        }


        [Theory]
        [InlineData("ConventionRouting/Employees(1)/NS.Manager/Default.IncreaseSalary", 20)]//Convention routing
        [InlineData("AttributeRouting/Employees(1)/NS.Manager/Default.IncreaseSalary", 40)]//Attribute routing
        public async Task ActionBountToDerivedEntityType(string url, int expectedCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}", this.BaseAddress, url);
            var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Act
            HttpResponseMessage responseForPost = await this.Client.SendAsync(requestForPost);
            await this.ResetDatasource();
            string responseString = await responseForPost.Content.ReadAsStringAsync();

            // Assert
            Assert.True(responseForPost.IsSuccessStatusCode);
            Assert.Contains("/$metadata#Edm.Int32", responseString);
            Assert.Contains(@"""value"":" + expectedCount, responseString);
        }

        [Theory]
        [InlineData("ConventionRouting")]
        [InlineData("AttributeRouting")]
        public async Task BoundAction_Works_WithPrimitive_And_CollectionOfPrimitiveParameters(string route)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.PrimitiveAction", BaseAddress, route);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            string payload = @"{
                ""param"": 7,
                ""price"": 9.9,
                ""name"": ""Tony"",
                ""names"": [ ""Mike"", null, ""John""]
            }";
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains(String.Format("\"@odata.context\":\"{0}/{1}/$metadata#Edm.Boolean\",\"value\":true", BaseAddress.ToLower(), route),
                responseString);
        }

        [Theory]
        [InlineData("ConventionRouting")]
        [InlineData("AttributeRouting")]
        public async Task BoundAction_Works_WithEnum_And_CollectionOfEnumParameters(string route)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.EnumAction", BaseAddress, route);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            string payload = @"{
                ""bkColor"": ""Red"",
                ""frColor"": ""Green"",
                ""colors"": [""Red"", ""Blue""]
            }";
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains(String.Format("\"@odata.context\":\"{0}/{1}/$metadata#Edm.Boolean\",\"value\":true", BaseAddress.ToLower(), route),
                responseString);
        }

        [Theory]
        [InlineData("ConventionRouting")]
        [InlineData("AttributeRouting")]
        public async Task BoundAction_Works_WithComplex_And_CollectionOfComplexParameters(string route)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.ComplexAction", BaseAddress, route);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            string payload = @"{
                ""address"": {""Street"":""NE 24th St."",""City"":""Redmond""},
                ""location"": {""@odata.type"":""#NS.SubAddress"",""Street"":""LianHua Rd."",""City"":""Shanghai"", ""Code"":9.9},
                ""addresses"": [{""Street"":""NE 24th St."",""City"":""Redmond""}, {""@odata.type"":""#NS.SubAddress"",""Street"":""LianHua Rd."",""City"":""Shanghai"", ""Code"":9.9}]
            }";

            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains(String.Format("\"@odata.context\":\"{0}/{1}/$metadata#Edm.Boolean\",\"value\":true", BaseAddress.ToLower(), route),
                responseString);
        }

        [Theory]
        [InlineData("ConventionRouting")]
        [InlineData("AttributeRouting")]
        public async Task BoundAction_Works_WithEntity_And_CollectionOfEntityParameters(string route)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Employees/Default.EntityAction", BaseAddress, route);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            string payload = @"{
                ""person"": {""@odata.type"":""#NS.Employee"",""ID"":801,""Name"":""Mike""},
                ""guard"": {""@odata.type"":""#NS.Manager"",""ID"":901,""Name"":""John"", ""Heads"":9},
                ""staff"": [{""@odata.type"":""#NS.Employee"",""ID"":801,""Name"":""Mike""}, {""@odata.type"":""#NS.Manager"",""ID"":901,""Name"":""John"", ""Heads"":9}]
            }";

            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Contains(String.Format("\"@odata.context\":\"{0}/{1}/$metadata#Edm.Boolean\",\"value\":true", BaseAddress.ToLower(), route),
                responseString);
        }
        #endregion
    }
}
