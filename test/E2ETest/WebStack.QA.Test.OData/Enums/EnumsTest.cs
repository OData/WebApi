﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Enums
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class EnumsTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(EmployeesController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("convention", "convention", EnumsEdmModel.GetConventionModel());
            configuration.MapODataServiceRoute("explicit", "explicit", EnumsEdmModel.GetExplicitModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.EnsureInitialized();
        }

        #region ModelBuilder

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task ModelBuilderTest(string modelMode)
        {
            string requestUri = string.Format("{0}/{1}/$metadata", this.BaseAddress, modelMode);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            var stream = await response.Content.ReadAsStreamAsync();

            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            var container = edmModel.EntityContainer;
            Assert.Equal("Container", container.Name);

            var favoriteSports = edmModel.SchemaElements.OfType<IEdmComplexType>().First();
            Assert.Equal("FavoriteSports", favoriteSports.Name);
            Assert.Equal(2, favoriteSports.Properties().Count());

            //ComplexType Enum Property
            var likeMost = favoriteSports.Properties().SingleOrDefault(p => p.Name == "LikeMost");
            Assert.True(likeMost.Type.IsEnum());

            //ComplexType Enum Property
            var like = favoriteSports.Properties().SingleOrDefault(p => p.Name == "Like");
            Assert.True(like.Type.IsCollection());

            var employee = edmModel.SchemaElements.SingleOrDefault(e => e.Name == "Employee") as IEdmEntityType;
            Assert.Equal(1, employee.Key().Count());
            Assert.Equal("ID", employee.Key().First().Name);
            Assert.Equal(6, employee.Properties().Count());

            //Entity Enum Collection Property
            var skillSet = employee.Properties().SingleOrDefault(p => p.Name == "SkillSet");
            Assert.True(skillSet.Type.IsCollection());

            //Entity Enum Property
            var gender = employee.Properties().SingleOrDefault(p => p.Name == "Gender");
            Assert.True(gender.Type.IsEnum());
            var edmEnumType = gender.Type.Definition as IEdmEnumType;
            Assert.False(edmEnumType.IsFlags);

            var accessLevel = employee.Properties().SingleOrDefault(p => p.Name == "AccessLevel") as IEdmStructuralProperty;
            edmEnumType = accessLevel.Type.Definition as IEdmEnumType;
            Assert.Equal(3, edmEnumType.Members.Count());
            Assert.True(edmEnumType.IsFlags);

            //Action AddSkill
            var iEdmOperation = edmModel.FindOperations(typeof(Employee).Namespace + ".AddSkill").FirstOrDefault();
            var iEdmOperationParameter = iEdmOperation.Parameters.SingleOrDefault(p => p.Name == "skill");
            var definition = iEdmOperationParameter.Type.Definition;
            Assert.Equal(EdmTypeKind.Enum, definition.TypeKind);

            var iEdmCollectionTpeReference = iEdmOperation.ReturnType as IEdmCollectionTypeReference;
            var iEdmTypeReference = iEdmCollectionTpeReference.ElementType();
            Assert.Equal(EdmTypeKind.Enum, iEdmTypeReference.Definition.TypeKind);

            // Action SetAccessLevel
            var iEdmOperationOfSetAccessLevel = edmModel.FindOperations(typeof(Employee).Namespace + ".SetAccessLevel").FirstOrDefault();
            var iEdmOperationParameterOfAccessLevel = iEdmOperationOfSetAccessLevel.Parameters.SingleOrDefault(p => p.Name == "accessLevel");
            var definitionOfAccessLevel = iEdmOperationParameterOfAccessLevel.Type.Definition;
            Assert.Equal(EdmTypeKind.Enum, definitionOfAccessLevel.TypeKind);

            var iEdmTpeReferenceOfAccessLevel = iEdmOperationOfSetAccessLevel.ReturnType;
            Assert.Equal(EdmTypeKind.Enum, iEdmTpeReferenceOfAccessLevel.Definition.TypeKind);

            // Function GetAccessLevel
            var iEdmOperationOfGetAccessLevel = edmModel.FindDeclaredOperations(typeof(Employee).Namespace + ".GetAccessLevel").FirstOrDefault();
            var iEdmTypeReferenceOfGetAccessLevel = iEdmOperationOfGetAccessLevel.ReturnType;
            Assert.Equal(EdmTypeKind.Enum, iEdmTypeReferenceOfGetAccessLevel.Definition.TypeKind);

            // Function HasAccessLevel
            var iEdmOperationOfHasAccessLevel = edmModel.FindDeclaredOperations(typeof(Employee).Namespace + ".HasAccessLevel").FirstOrDefault();
            var iEdmOperationParameterOfHasAccessLevel = iEdmOperationOfHasAccessLevel.Parameters.SingleOrDefault(p => p.Name == "AccessLevel");
            Assert.Equal(EdmTypeKind.Enum, iEdmOperationParameterOfHasAccessLevel.Type.Definition.TypeKind);
        }

        #endregion

        #region Query

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEntitySet(string format)
        {
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsAsync<JObject>();
            var results = json.GetValue("value") as JArray;
            Assert.Equal<int>(3, results.Count);
            if (format == "application/json;odata.metadata=full")
            {
                var typeOfAccessLevel = results[0]["AccessLevel@odata.type"].ToString();
                Assert.Equal("#WebStack.QA.Test.OData.Enums.AccessLevel", typeOfAccessLevel);

                var typeOfSkillSet = results[0]["SkillSet@odata.type"].ToString();
                Assert.Equal("#Collection(WebStack.QA.Test.OData.Enums.Skill)", typeOfSkillSet);
            }
        }

        [Theory]
        [InlineData("/convention/Employees/$count", 3)]
        [InlineData("/convention/Employees/$count?$filter=Name eq 'Name1'", 1)]
        public async Task QueryEntitySetCount(string url, int expectedCount)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + url;

            // Act
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            string count = await response.Content.ReadAsStringAsync();
            Assert.Equal<int>(expectedCount, int.Parse(count));
        }

        [Theory]
        [InlineData("/convention/Employees(1)/SkillSet/$count", 2)]
        [InlineData("/convention/Employees(1)/SkillSet/$count?$filter=$it eq WebStack.QA.Test.OData.Enums.Skill'Sql'", 1)]
        public async Task QuerySkillSetCount(string url, int expectedCount)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = this.BaseAddress + url;

            // Act
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            string count = await response.Content.ReadAsStringAsync();
            Assert.Equal<int>(expectedCount, int.Parse(count));
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEnumPropertyInEntityType(string format)
        {
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)/AccessLevel?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsAsync<JObject>();
            var value = json.GetValue("value").ToString();
            Assert.Equal("Execute", value);
            if (format != "application/json;odata.metadata=none")
            {
                var context = json.GetValue("@odata.context").ToString();
                Assert.True(context.IndexOf("/$metadata#Employees(1)/AccessLevel") > 0);
            }

            requestUri = this.BaseAddress + "/convention/Employees(1)/SkillSet?$format=" + format;
            response = await this.Client.GetAsync(requestUri);
            json = await response.Content.ReadAsAsync<JObject>();
            JArray skillSet = json["value"] as JArray;
            Assert.Equal(2, skillSet.Count);

            Assert.Equal("CSharp", (string)skillSet[0]);
            Assert.Equal("Sql", (string)skillSet[1]);
            if (format != "application/json;odata.metadata=none")
            {
                var context = json["@odata.context"].ToString();
                Assert.True(context.IndexOf("/$metadata#Collection(WebStack.QA.Test.OData.Enums.Skill)") >= 0);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEnumPropertyValueInEntityType(string format)
        {
            await ResetDatasource();
            var requestUri = this.BaseAddress + "/convention/Employees(1)/AccessLevel/$value?$format=" + format;
            var response = await this.Client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            AccessLevel actual;
            Assert.True(Enum.TryParse<AccessLevel>(content, out actual));
            Assert.Equal(AccessLevel.Execute, actual);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEnumPropertyInComplexType(string format)
        {
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)/FavoriteSports?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsAsync<JObject>();
            var value = result.GetValue("LikeMost").ToString();
            Assert.Equal("Pingpong", value);
            value = result.GetValue("Like").ToString();
            Assert.Equal(@"[""Pingpong"",""Basketball""]", value.Replace("\r\n", "").Replace(" ", ""));
            if (format == "application/json;odata.metadata=full")
            {
                var context = result.GetValue("@odata.context").ToString();
                Assert.True(context.IndexOf("/$metadata#Employees(1)/FavoriteSports") > 0);
            }

            requestUri = this.BaseAddress + "/convention/Employees(1)/FavoriteSports/LikeMost?$format=" + format;
            response = await this.Client.GetAsync(requestUri);
            result = await response.Content.ReadAsAsync<JObject>();
            value = result.GetValue("value").ToString();
            Assert.Equal("Pingpong", value);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEntity(string format)
        {
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees(1)?$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsAsync<JObject>();
            if (format == "application/json;odata.metadata=full")
            {
                var typeOfAccessLevel = result["AccessLevel@odata.type"].ToString();
                Assert.Equal("#WebStack.QA.Test.OData.Enums.AccessLevel", typeOfAccessLevel);

                var typeOfSkillSet = result["SkillSet@odata.type"].ToString();
                Assert.Equal("#Collection(WebStack.QA.Test.OData.Enums.Skill)", typeOfSkillSet);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryEntitiesFilterByEnum(string format)
        {
            await ResetDatasource();
            // in the template {0}: operation, {1}: typename, {2}: enum value, {3}: format
            string uriTemplate = this.BaseAddress + "/convention/Employees?$filter=AccessLevel {0} {1}'{2}'&$format={3}";
            string uriEq = string.Format(uriTemplate, "eq", typeof(AccessLevel).FullName, AccessLevel.Read.ToString(), format);
            string uriHas = string.Format(uriTemplate, "has", typeof(AccessLevel).FullName, AccessLevel.Read.ToString(), format);

            using (var response = await this.Client.GetAsync(uriEq))
            {
                // http://<siteurl>/convention/Employees?$filter=AccessLevel eq WebStack.QA.Test.OData.Enums.AccessLevel'Read'&$format=<Format>
                Assert.True(response.IsSuccessStatusCode);

                var result = await response.Content.ReadAsAsync<JObject>();
                var value = result.GetValue("value") as JArray;
                Assert.NotNull(value);
                Assert.Equal(1, value.Count);
            }

            using (var response = await this.Client.GetAsync(uriHas))
            {
                // http://<siteurl>/convention/Employees?$filter=AccessLevel has WebStack.QA.Test.OData.Enums.AccessLevel'Read'&$format=<Format>
                Assert.True(response.IsSuccessStatusCode);

                var result = await response.Content.ReadAsAsync<JObject>();
                var value = result.GetValue("value") as JArray;
                Assert.NotNull(value);
                Assert.Equal(2, value.Count);
            }
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task EnumInSelect(string format)
        {
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees?$select=AccessLevel,SkillSet,FavoriteSports&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsAsync<JObject>();

            var value = result.GetValue("value") as JArray;
            Assert.Equal(3, value.Count);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task EnumInOrderBy(string format)
        {
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees?$orderby=AccessLevel,FavoriteSports/LikeMost&$format=" + format;

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.True(response.IsSuccessStatusCode);

            var result = await response.Content.ReadAsAsync<JObject>();

            var value = result.GetValue("value") as JArray;
            Assert.Equal(3, value.Count);

            var firstEmployee = value[0];
            Assert.Equal(2, firstEmployee["ID"]);

            var secondEmployee = value[1];
            Assert.Equal(3, secondEmployee["ID"]);
        }

        #endregion

        #region Update

        [Fact]
        public async Task AddEntity()
        {
            await ResetDatasource();
            string requestUri = this.BaseAddress + "/convention/Employees?$format=application/json;odata.metadata=none";

            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.Equal<int>(3, result.Count);
            }

            var postUri = this.BaseAddress + "/convention/Employees";

            var postContent = JObject.Parse(@"{""ID"":1,
                    ""Name"":""Name2"",
                    ""SkillSet"":[""Sql""],
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Read,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Pingpong"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    }}");
            using (HttpResponseMessage response = await this.Client.PostAsJsonAsync(postUri, postContent))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            }

            using (HttpResponseMessage response = await this.Client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsAsync<JObject>();
                var result = json.GetValue("value") as JArray;
                Assert.Equal<int>(4, result.Count);
            }
        }

        [Fact]
        public async Task UpdateEntity()
        {
            await ResetDatasource();
            string getUri = this.BaseAddress + "/convention/Employees(2)?$format=application/json;odata.metadata=none";

            using (HttpResponseMessage response = await Client.GetAsync(getUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsAsync<JObject>();
                var accessLevel = json.GetValue("AccessLevel").ToString();
                Assert.Equal("Read", accessLevel);

                var skillSet = json.GetValue("SkillSet").ToString();
                Assert.Equal("[]", skillSet);

                var favoriteSport = json["FavoriteSports"]["LikeMost"].ToString();
                Assert.Equal("Pingpong", favoriteSport);

                var sports = json["FavoriteSports"]["Like"].ToString();
                Assert.Equal(@"[""Pingpong"",""Basketball""]", sports.Replace("\r\n", "").Replace(" ", ""));
            }

            var putUri = this.BaseAddress + "/convention/Employees(2)";
            var putContent = JObject.Parse(@"{""ID"":2,
                    ""Name"":""Name2"",
                    ""SkillSet"":[""Sql""],
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Execute,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Basketball"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    }}");
            using (HttpResponseMessage response = await Client.PutAsJsonAsync(putUri, putContent))
            {
                response.EnsureSuccessStatusCode();
            }

            using (HttpResponseMessage response = await Client.GetAsync(getUri))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsAsync<JObject>();

                var accessLevel = json.GetValue("AccessLevel");
                Assert.Equal("Write, Execute", accessLevel);

                var skillSet = json.GetValue("SkillSet").ToString();
                Assert.Equal(@"[""Sql""]", skillSet.Replace("\r\n", "").Replace(" ", ""));

                var favoriteSport = json["FavoriteSports"]["LikeMost"].ToString();
                Assert.Equal("Basketball", favoriteSport);

                var sports = json["FavoriteSports"]["Like"].ToString();
                Assert.Equal(@"[""Pingpong"",""Basketball""]", sports.Replace("\r\n", "").Replace(" ", ""));
            }
        }

        [Theory]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        public async Task UpsertEntity(string method)
        {
            await ResetDatasource();

            var requestUri = this.BaseAddress + "/convention/Employees(20)";
            var requestContent = JObject.Parse(@"{""ID"":20,
                    ""Name"":""Name2"",
                    ""SkillSet"":[""Sql""],
                    ""Gender"":""Female"",
                    ""AccessLevel"":""Execute,Write"",
                    ""FavoriteSports"":{
                            ""LikeMost"":""Basketball"",
                            ""Like"":[""Pingpong"",""Basketball""]
                    }}");
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);
            request.Content = new StringContent(requestContent.ToString());
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Headers.Add("Prefer", "return=minimal");
            using (HttpResponseMessage response = await Client.SendAsync(request))
            {
                Assert.True(HttpStatusCode.NoContent == response.StatusCode,
                    string.Format("Response code is not right, expected: {0}, actual: {1}", HttpStatusCode.NoContent, response.StatusCode));
                Assert.True(response.Headers.Contains("OData-EntityId"), "The response should contain Header 'OData-EntityId'");
                Assert.True(response.Headers.Contains("Location"), "The response should contain Header 'Location'");
            }
        }

        #endregion

        #region Delete

        [Fact]
        public async Task DeleteEntity()
        {
            await ResetDatasource();
            string uriGet = this.BaseAddress + "/convention/Employees?$format=application/json;odata.metadata=none";

            using (HttpResponseMessage response = await Client.GetAsync(uriGet))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsAsync<JObject>();
                var values = json.GetValue("value") as JArray;
                Assert.Equal<int>(3, values.Count);
            }

            var uriDelete = this.BaseAddress + "/convention/Employees(1)";
            using (HttpResponseMessage response = await Client.DeleteAsync(uriDelete))
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            using (HttpResponseMessage response = await Client.GetAsync(uriGet))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsAsync<JObject>();
                var values = json.GetValue("value") as JArray;
                Assert.Equal<int>(2, values.Count);
            }
        }

        #endregion

        #region Enum with action

        [Fact]
        public async Task EnumInActionParameter()
        {
            await ResetDatasource();
            var postUri = this.BaseAddress + "/convention/Employees(1)/WebStack.QA.Test.OData.Enums.AddSkill";
            var postContent = new StringContent(@"{""skill"":""Sql""}");
            postContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var response = await Client.PostAsync(postUri, postContent);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task EnumInActionOutput()
        {
            await ResetDatasource();
            var postUri = this.BaseAddress + "/convention/SetAccessLevel";
            var postContent = JObject.Parse(@"{""accessLevel"":""Read,Execute"",""ID"":1}");

            var response = await Client.PostAsJsonAsync(postUri, postContent);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsAsync<JObject>();
            var value = json["value"].ToString();

            Assert.Equal("Read, Execute", value);
        }

        #endregion

        #region Enum with function

        [Fact]
        public async Task EnumInFunctionOutput()
        {
            await ResetDatasource();
            var getUri = this.BaseAddress + "/convention/Employees(1)/WebStack.QA.Test.OData.Enums.GetAccessLevel";
            var response = await this.Client.GetAsync(getUri);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsAsync<JObject>();
            var value = json["value"].ToString();
            Assert.Equal("Execute", value);
        }

        [Theory]
        [InlineData("/convention/HasAccessLevel(ID=1,AccessLevel=WebStack.QA.Test.OData.Enums.AccessLevel'Read')", false)]
        [InlineData("/convention/HasAccessLevel(ID=2,AccessLevel=WebStack.QA.Test.OData.Enums.AccessLevel'1')", true)]
        public async Task EnumInFunctionParameter(string url, bool expectedValue)
        {
            await ResetDatasource();
            var requestUri = this.BaseAddress + url;
            var response = await this.Client.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsAsync<JObject>();
            var actualValue = json["value"].Value<bool>();
            Assert.Equal(expectedValue, actualValue);
        }

        #endregion

        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var uriReset = this.BaseAddress + "/convention/ResetDataSource";
            var response = await this.Client.PostAsync(uriReset, null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            return response;
        }
    }
}