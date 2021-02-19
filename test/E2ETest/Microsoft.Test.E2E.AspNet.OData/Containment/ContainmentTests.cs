// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Containment
{
    public class ContainmentTests : WebHostTestBase
    {
        public ContainmentTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<string, string> MediaTypes
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] mimes = new string[]{
                    "json",
                    "application/json",
                    "application/json;odata.metadata=none",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full"};
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string mode in modes)
                {
                    foreach (string mime in mimes)
                    {
                        data.Add(mode, mime);
                    }
                }
                return data;
            }
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(AccountsController), typeof(AnonymousAccountController), typeof(MetadataController), typeof(PaginatedAccountsController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration
                .MapODataServiceRoute(routeName: "convention",
                    routePrefix: "convention",
                    model: ContainmentEdmModels.GetConventionModel(configuration),
                    batchHandler: configuration.CreateDefaultODataBatchHandler());

            configuration
                .MapODataServiceRoute(routeName: "explicit",
                    routePrefix: "explicit",
                    model: ContainmentEdmModels.GetExplicitModel(),
                    batchHandler: configuration.CreateDefaultODataBatchHandler());

            configuration.EnsureInitialized();
        }

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

            var accountType = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "Account");
            Assert.Equal(5, accountType.Properties().Count());
            var payinPIs = accountType.NavigationProperties().Single(np => np.Name == "PayinPIs");
            Assert.True(payinPIs.ContainsTarget, "The navigation property 'PayinPIs' should be a containment navigation property.");
            Assert.Equal(string.Format("Collection({0})", typeof(PaymentInstrument).FullName), payinPIs.Type.Definition.FullTypeName());
            var payoutPI = accountType.DeclaredNavigationProperties().Single(np => np.Name == "PayoutPI");
            Assert.True(payoutPI.ContainsTarget, "PreminumAccountType.GiftCard");
            Assert.Equal(EdmMultiplicity.ZeroOrOne, payoutPI.TargetMultiplicity());

            var paymentInstrumentType = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "PaymentInstrument");
            Assert.Equal(4, paymentInstrumentType.Properties().Count());
            var statement = paymentInstrumentType.NavigationProperties().Single(np => np.Name == "Statement");
            Assert.True(statement.ContainsTarget, "PaymentInstrumentType.ContainsTarget");
            Assert.Equal(EdmMultiplicity.ZeroOrOne, statement.TargetMultiplicity());
            var signatories = paymentInstrumentType.NavigationProperties().Single(np => np.Name == "Signatories");
            Assert.True(signatories.ContainsTarget);
            Assert.Equal(EdmMultiplicity.Many, signatories.TargetMultiplicity());

            var premiumAccountType = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "PremiumAccount");
            Assert.Single(premiumAccountType.DeclaredProperties);
            var giftCard = premiumAccountType.DeclaredProperties.Single() as IEdmNavigationProperty;
            Assert.True(giftCard.ContainsTarget, "PreminumAccountType.GiftCard");
            Assert.Equal(EdmMultiplicity.One, giftCard.TargetMultiplicity());

            var edmNamespace = typeof(PaymentInstrument).Namespace;
            var actionClear = edmModel.FindOperations(edmNamespace + ".Clear").Single() as IEdmAction;
            var nameContains = actionClear.Parameters.Single(p => p.Name == "nameContains");
            Assert.Equal("Edm.String", nameContains.Type.Definition.ToString());
            Assert.Equal("Edm.Int32", actionClear.ReturnType.Definition.ToString());

            var functionGetCount = edmModel.FindDeclaredOperations(edmNamespace + ".GetCount").Single() as IEdmFunction;
            nameContains = functionGetCount.Parameters.Single(p => p.Name == "nameContains");
            Assert.Equal("Edm.String", nameContains.Type.Definition.ToString());
            Assert.Equal("Edm.Int32", functionGetCount.ReturnType.Definition.ToString());

            var container = edmModel.EntityContainer;
            Assert.Equal("Container", container.Name);
        }

        #region CRUD on containing entity
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test whether it is able to add a containing Entity.
        // Post ~/Accounts
        public async Task CreateAccount(string mode)
        {
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts", BaseAddress, mode);

            string content = @"{'AccountID':0,'Name':'Name300'}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            /*
            {
              "@odata.context":"http://jinfutan03:9123/explicit/$metadata#Accounts/$entity","AccountID":300,"Name":"Name300"
            }
            */
            var json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(300, (int)json["AccountID"]);
            Assert.Equal(serviceRootUri + "/Accounts(300)", response.Headers.Location.OriginalString);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test it is able to expand the containment navigation properties(mutiplicity is optional and many) from the containing entity
        // GET ~/Accounts?$expand=PayinPIs,PayoutPI
        public async Task ExpandPayinPIsAndPayoutPIFromAcccounts(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts?$select=AccountID&$expand=PayinPIs,PayoutPI&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            string expectedValue, actualValue;
            var results = json.GetValue("value") as JArray;
            Assert.Equal<int>(2, results.Count);
            if (mime == "json" || mime.Contains("odata.metadata=minimal") || mime.Contains("odata.metadata=full"))
            {
                var odataContext = (string)json["@odata.context"];
                Assert.Equal(serviceRootUri + "/$metadata#Accounts(AccountID,PayinPIs(),PayoutPI())", odataContext);
                var odataType = (string)results[1]["@odata.type"];
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount", odataType);
            }

            if (mime.Contains("odata.metadata=full"))
            {
                var account = results[0];
                Assert.Equal(serviceRootUri + "/Accounts(100)/PayinPIs/$ref", (string)account["PayinPIs@odata.associationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(100)/PayinPIs", (string)account["PayinPIs@odata.navigationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(100)/PayoutPI/$ref", (string)account["PayoutPI@odata.associationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(100)/PayoutPI", (string)account["PayoutPI@odata.navigationLink"]);

                var payoutPIOfAccount = account["PayoutPI"];
                Assert.Equal("Accounts(100)/PayoutPI", (string)payoutPIOfAccount["@odata.editLink"]);
                Assert.Equal("Accounts(100)/PayoutPI", (string)payoutPIOfAccount["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payoutPIOfAccount["@odata.type"]);
                Assert.Equal(serviceRootUri + "/Accounts(100)/PayoutPI/Statement", (string)payoutPIOfAccount["Statement@odata.navigationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(100)/PayoutPI/Statement/$ref", (string)payoutPIOfAccount["Statement@odata.associationLink"]);

                var payinPIsOfAccont = account["PayinPIs"];

                // Bug 1862: Functions/Actions bound to a collection of entity should be advertised.
                /*Functions that are bound to a collection of entities are advertised in representations of that collection.*/

                Assert.Equal("Accounts(100)/PayinPIs(101)", (string)payinPIsOfAccont[0]["@odata.editLink"]);
                Assert.Equal("Accounts(100)/PayinPIs(101)", (string)payinPIsOfAccont[0]["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payinPIsOfAccont[0]["@odata.type"]);
                Assert.Equal(serviceRootUri + "/Accounts(100)/PayinPIs(101)/Statement", (string)payinPIsOfAccont[0]["Statement@odata.navigationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(100)/PayinPIs(101)/Statement/$ref", (string)payinPIsOfAccont[0]["Statement@odata.associationLink"]);

                var premiumAccount = results[1];
                actualValue = (string)(premiumAccount["PayinPIs@odata.navigationLink"]);
                expectedValue = serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs";
                // Bug 1861: The navigation link of a containment navigation property should contain cast segment if the containing entity is actually a derived type.
                // Assert.Equal(expectedValue, actualValue); // Actual: http://jinfutanwebapi1:9123/convention/Accounts(200)/PayinPIs

                expectedValue = serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs/$ref";
                actualValue = (string)(premiumAccount["PayinPIs@odata.associationLink"]);
                Assert.Equal(expectedValue, actualValue);

                expectedValue = serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI";
                actualValue = (string)(premiumAccount["PayoutPI@odata.navigationLink"]);
                // Bug 1861: The navigation link of a containment navigation property should contain cast segment if the containing entity is actually a derived type.
                // Assert.Equal(expectedValue, actualValue); // Actual: http://jinfutanwebapi1:9123/convention/Accounts(200)/PayoutPI

                expectedValue = serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI/$ref";
                actualValue = (string)(premiumAccount["PayoutPI@odata.associationLink"]);
                Assert.Equal(expectedValue, actualValue);


                var payoutPIOfPremiumAccount = premiumAccount["PayoutPI"];

                Assert.Equal("Accounts(200)/PayoutPI", (string)payoutPIOfPremiumAccount["@odata.editLink"]);
                Assert.Equal("Accounts(200)/PayoutPI", (string)payoutPIOfPremiumAccount["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payoutPIOfPremiumAccount["@odata.type"]);

                var payinPIsOfPremiumAccont = premiumAccount["PayinPIs"];

                // Bug 1862: Functions/Actions bound to a collection of entity should be advertised.
                /*Functions that are bound to a collection of entities are advertised in representations of that collection.*/

                actualValue = (string)(payinPIsOfPremiumAccont[0]["@odata.editLink"]);
                Assert.Equal("Accounts(200)/PayinPIs(201)", actualValue);
                Assert.Equal("Accounts(200)/PayinPIs(201)", (string)payinPIsOfPremiumAccont[0]["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payinPIsOfPremiumAccont[0]["@odata.type"]);
            }
        }

        [Theory]
        [InlineData("/convention/Accounts(100)/PayinPIs/$count?", 2)]
        [InlineData("/explicit/Accounts(100)/PayinPIs/$count?$filter=PaymentInstrumentID gt 101", 1)]
        public async Task QueryPayinPIsCount(string url, int expectedCount)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = BaseAddress + url;

            // Act
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string count = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedCount, int.Parse(count));
        }

        [Theory]
        [InlineData("/convention/Accounts/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/$count", 1)]
        [InlineData("/explicit/Accounts/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/$count?$filter=AccountID gt 1000", 0)]
        public async Task QueryPremiumAccountCount(string url, int expectedCount)
        {
            // Arrange
            await ResetDatasource();
            string requestUri = BaseAddress + url;

            // Act
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string count = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedCount, int.Parse(count));
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test 
        //      1. it is able to expand containment navigation properties from an entity that derived from the containing entity.
        //      2. it is able to expand containment navigation peroperty that is defined on an derived entity.
        // GET ~/Accounts/Namespace.PremiumAccount?$expand=PayinPIs,PayoutPI,GiftCard.
        public async Task ExpandContainmentNavigationOnDerivedEntity(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount?$expand=PayinPIs,PayoutPI,GiftCard&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            /*
             * Sample response payload:
            {
              "@odata.context":"http://jinfutanwebapi1:9123/explicit/$metadata#Accounts/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount","value":[
                {
                  "@odata.type":"#Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount",
             * "@odata.id":"http://jinfutanwebapi1:9123/explicit/Accounts(200)",
             * "@odata.editLink":"http://jinfutanwebapi1:9123/explicit/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount",
             * "AccountID":200,"Name":"Name200",
             * "PayinPIs@odata.context":"http://jinfutanwebapi1:9123/explicit/$metadata#Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs",
             * "PayinPIs@odata.associationLink":"http://jinfutanwebapi1:9123/explicit/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs/$ref",
             * "PayinPIs@odata.navigationLink":"http://jinfutanwebapi1:9123/explicit/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs",
             * "PayinPIs":[
                    {
                      "@odata.type":"#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument","@odata.id":"Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs(201)","@odata.editLink":"Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs(201)","PaymentInstrumentID":201,"FriendlyName":"201 first PI","Statement@odata.associationLink":"http://jinfutanwebapi1:9123/explicit/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs(201)/Statement/$ref","Statement@odata.navigationLink":"http://jinfutanwebapi1:9123/explicit/PayinPIs(201)/Statement","#Microsoft.Test.E2E.AspNet.OData.Containment.Delete":{
                        "title":"Microsoft.Test.E2E.AspNet.OData.Containment.Delete","target":"http://jinfutanwebapi1:9123/explicit/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs(201)/Microsoft.Test.E2E.AspNet.OData.Containment.Delete"
                      }
                    }
                  ],
               "PayoutPI@odata.context":"http://jinfutanwebapi1:9123/explicit/$metadata#Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI/$entity",
             * "PayoutPI@odata.associationLink":"http://jinfutanwebapi1:9123/explicit/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI/$ref",
             * "PayoutPI@odata.navigationLink":"http://jinfutanwebapi1:9123/explicit/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI","PayoutPI":{
                    "@odata.type":"#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument","@odata.id":"Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI",
             * "@odata.editLink":"Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI","PaymentInstrumentID":200,"FriendlyName":"Payout PI: Direct Debit","Statement@odata.associationLink":"http://jinfutanwebapi1:9123/explicit/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI/Statement/$ref","Statement@odata.navigationLink":"http://jinfutanwebapi1:9123/explicit/PayoutPI(200)/Statement","#Microsoft.Test.E2E.AspNet.OData.Containment.Delete":{
                      "title":"Microsoft.Test.E2E.AspNet.OData.Containment.Delete","target":"http://jinfutanwebapi1:9123/explicit/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI/Microsoft.Test.E2E.AspNet.OData.Containment.Delete"
                    }
                  },
                 "GiftCard@odata.context":"http://jinfutanwebapi1:9123/explicit/$metadata#Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard/$entity",
             * "GiftCard@odata.associationLink":"http://jinfutanwebapi1:9123/explicit/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard/$ref",
             * "GiftCard@odata.navigationLink":"http://jinfutanwebapi1:9123/explicit/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard",
             * "GiftCard":{
                    "@odata.type":"#Microsoft.Test.E2E.AspNet.OData.Containment.GiftCard","@odata.id":"Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard","@odata.editLink":"Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard","GiftCardID":200,"GiftCardNO":"BBA1-2BBC","Amount":2000.0
                  }
                }
              ]
            }
             */
            var json = await response.Content.ReadAsObject<JObject>();

            var results = json.GetValue("value") as JArray;
            Assert.Single(results);
            var premiumAccount = results[0];
            if (mime == "json" || mime.Contains("odata.metadata=minimal") || mime.Contains("odata.metadata=full"))
            {
                var odataContext = (string)json["@odata.context"]; // PreminumAccount
                Assert.Equal(serviceRootUri + "/$metadata#Accounts/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount(PayinPIs(),PayoutPI(),GiftCard())", odataContext);
            }
            if (mime.Contains("odata.metadata=full"))
            {
                var odataType = (string)premiumAccount["@odata.type"];
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount", odataType);

                Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs", (string)premiumAccount["PayinPIs@odata.navigationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayinPIs/$ref", (string)premiumAccount["PayinPIs@odata.associationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI", (string)premiumAccount["PayoutPI@odata.navigationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/PayoutPI/$ref", (string)premiumAccount["PayoutPI@odata.associationLink"]);

                Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard",
                    (string)premiumAccount["GiftCard@odata.navigationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard/$ref",
                    (string)premiumAccount["GiftCard@odata.associationLink"]);

                var payoutPIOfPremiumAccount = premiumAccount["PayoutPI"];

                Assert.Equal(serviceRootUri + "/Accounts(200)/PayoutPI/Microsoft.Test.E2E.AspNet.OData.Containment.Delete",
                    (string)payoutPIOfPremiumAccount["#Microsoft.Test.E2E.AspNet.OData.Containment.Delete"]["target"]);

                Assert.Equal("Accounts(200)/PayoutPI", (string)payoutPIOfPremiumAccount["@odata.editLink"]);
                Assert.Equal("Accounts(200)/PayoutPI", (string)payoutPIOfPremiumAccount["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payoutPIOfPremiumAccount["@odata.type"]);

                var payinPIsOfPremiumAccont = premiumAccount["PayinPIs"];

                Assert.Equal(serviceRootUri + "/Accounts(200)/PayinPIs(201)/Microsoft.Test.E2E.AspNet.OData.Containment.Delete",
                    (string)payinPIsOfPremiumAccont[0]["#Microsoft.Test.E2E.AspNet.OData.Containment.Delete"]["target"]);

                Assert.Equal("Accounts(200)/PayinPIs(201)", (string)payinPIsOfPremiumAccont[0]["@odata.editLink"]);
                Assert.Equal("Accounts(200)/PayinPIs(201)", (string)payinPIsOfPremiumAccont[0]["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payinPIsOfPremiumAccont[0]["@odata.type"]);

                var giftCard = premiumAccount["GiftCard"];
                string expected = "Accounts(200)/GiftCard";
                string actual = (string)giftCard["@odata.editLink"];
                Assert.True(expected == actual, string.Format("odata.editLink of GiftCard, exptected: {0}, actual: {1}, request url: {2}", expected, actual, requestUri));

                expected = "Accounts(200)/GiftCard";
                actual = (string)giftCard["@odata.id"];
                Assert.True(expected == actual, string.Format("odata.id link of GiftCard, exptected: {0}, actual: {1}, request url: {2}", expected, actual, requestUri));

                expected = "#Microsoft.Test.E2E.AspNet.OData.Containment.GiftCard";
                actual = (string)giftCard["@odata.type"];
                Assert.True(expected == actual, string.Format("odata.type of GiftCard, exptected: {0}, actual: {1}, request url: {2}", expected, actual, requestUri));

                Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard/$ref",
                    (string)premiumAccount["GiftCard@odata.associationLink"]);
                Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard",
                    (string)premiumAccount["GiftCard@odata.navigationLink"]);
            }
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to update a containing entity.
        public async Task PutAccount(string modelMode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Accounts(100)";

            var content = new StringContent(content:
@"{
  'AccountID':100,
  'Name':'newName1000',
  'PayinPIs':
  [
    {
      'PaymentInstrumentID':1010,
      'FriendlyName':'1010 first PI'
    },
    {
      'PaymentInstrumentID':1020,
      'FriendlyName':'1020 second PI'
    }
  ]
}", encoding: Encoding.UTF8, mediaType: "application/json");
            HttpResponseMessage response = await Client.PutAsync(requestUri, content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to update a drived containing entity.
        public async Task PatchPremiumAccount(string modelMode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount";

            var content = @"{'AccountID':-1, 'Name':'newName1000'}";
            HttpResponseMessage response = await Client.PatchAsync(requestUri, content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(200, (int)json["AccountID"]);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to delete a containing entity.
        public async Task DeleteAccount(string modelMode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, modelMode).ToLower();
            string requestUri = serviceRootUri + "/Accounts(100)";

            HttpResponseMessage response = await Client.DeleteAsync(requestUri);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        #endregion

        #region CRUD on containment navigation properties
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to add a contained entity to a navigation property(multiplicity is many)
        // POST ~/Account(100)/PayinPIs
        public async Task AddAPayinPI(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs", BaseAddress, mode);

            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentInstrumentID = 0,
                FriendlyName = "Pi103",
                Signatories = new List<Signatory>()
                {
                    new Signatory()
                    {
                        SignatoryID=1,
                        SignatoryName="Sig 1"
                    }
                }
            };

            var response = await Client.PostAsJsonAsync<PaymentInstrument>(requestUri, pi);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(string.Format("{0}/$metadata#Accounts(100)/PayinPIs/$entity", serviceRootUri), (string)json["@odata.context"]);
            Assert.Equal(103, (int)json["PaymentInstrumentID"]);

            Assert.Equal(serviceRootUri + "/Accounts(100)/PayinPIs(103)", response.Headers.Location.OriginalString);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test it is able to query a containing navigation property of collection type.
        public async Task QueryPayinPIsFromAccount(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs?$filter=PaymentInstrumentID lt 102&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            if (!mime.Contains("odata.metadata=none"))
            {
                var expectedContextUrl = serviceRootUri + "/$metadata#Accounts(100)/PayinPIs";
                var actualContextUrl = (string)json["@odata.context"];
                Assert.Equal(expectedContextUrl, actualContextUrl);
            }

            var results = json.GetValue("value") as JArray;
            Assert.Single(results);

            if (mime.Contains("odata.metadata=full"))
            {
                // Bug 1862: Functions/Actions bound to a collection of entity should be advertised.

                var payinPI = results[0];

                Assert.Equal(serviceRootUri + "/Accounts(100)/PayinPIs(101)/Microsoft.Test.E2E.AspNet.OData.Containment.Delete",
                    (string)payinPI["#Microsoft.Test.E2E.AspNet.OData.Containment.Delete"]["target"]);
                Assert.Equal("Accounts(100)/PayinPIs(101)", (string)payinPI["@odata.editLink"]);
                Assert.Equal("Accounts(100)/PayinPIs(101)", (string)payinPI["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payinPI["@odata.type"]);
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test if we can get the odata.nextLink in a contained navigation property of collection type.
        public async Task QueryPaginatedPayinPIsFromAccount(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/PaginatedAccounts(100)/PayinPIs?$filter=PaymentInstrumentID gt 1&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var nextlink = (string)json["@odata.nextLink"];
            Assert.NotNull(nextlink);
            nextlink = nextlink.Replace("%28", "(").Replace("%29", ")");
            Assert.Contains("PaginatedAccounts(100)/PayinPIs?$filter=PaymentInstrumentID%20gt%201".ToLower(), nextlink.ToLower());
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test if we can get the odata.nextLink in a contained navigation property of collection type when using $expand.
        public async Task QueryExpandPaginatedPayinPIsFromAccount(string mode, string mime)
        {
            await ResetDatasource();
            string requestUri = string.Format("{0}/{1}/PaginatedAccounts?$expand=PayinPIs($filter=PaymentInstrumentID gt 1)&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadAsStringAsync();
            payload = payload.Replace("%28", "(").Replace("%29", ")").ToLower();
            Assert.Contains(string.Format("\"PayinPIs@odata.nextLink\":\"{0}/{1}/PaginatedAccounts(100)/PayinPIs?$filter=PaymentInstrumentID%20gt%201", BaseAddress, mode).ToLower(), payload);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test if we can get the odata.nextLink in a contained navigation property of collection type when using $expand.
        public async Task QueryMultiExpandPaginatedPayinPIsFromAccount(string mode, string mime)
        {
            await ResetDatasource();
            string requestUri = string.Format("{0}/{1}/PaginatedAccounts?$expand=PayinPIs($expand=Signatories)&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadAsStringAsync();
            payload = payload.Replace("%28", "(").Replace("%29", ")").ToLower();
            Assert.Contains(string.Format("\"Signatories@odata.nextLink\":\"{0}/{1}/PaginatedAccounts(100)/PayinPIs(101)/Signatories?$skip=1", BaseAddress, mode).ToLower(), payload);
            Assert.Contains(string.Format("\"PayinPIs@odata.nextLink\":\"{0}/{1}/PaginatedAccounts(100)/PayinPIs?$expand=Signatories&$skip=1", BaseAddress, mode).ToLower(), payload);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test if we can get the odata.nextLink in a contained navigation property of collection type when using $expand.
        public async Task QueryExpandPaginatedSignatoriesFromMostRecentPIInAccount(string mode, string mime)
        {
            await ResetDatasource();
            string requestUri = string.Format("{0}/{1}/PaginatedAccounts(100)/MostRecentPI?$expand=Signatories&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadAsStringAsync();
            payload = payload.Replace("%28", "(").Replace("%29", ")").ToLower();
            Assert.Contains(string.Format("\"Signatories@odata.nextLink\":\"{0}/{1}/PaginatedAccounts(100)/MostRecentPI/Signatories?$skip=1", BaseAddress, mode).ToLower(), payload);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test if we can get the odata.nextLink in a contained navigation property of a singleton when using $expand.
        public async Task QueryExpandPaginatedPayinPIsFromAnonymousAccount(string mode, string mime)
        {
            await ResetDatasource();
            string requestUri = string.Format("{0}/{1}/AnonymousAccount?$expand=PayinPIs($filter=PaymentInstrumentID gt 0)&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadAsStringAsync();
            payload = payload.Replace("%28", "(").Replace("%29", ")").ToLower();
            Assert.Contains(string.Format("\"PayinPIs@odata.nextLink\":\"{0}/{1}/AnonymousAccount/PayinPIs?$filter=PaymentInstrumentID%20gt%200", BaseAddress, mode).ToLower(), payload);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test it is able to query ONE entity of a collectiona containment navigation property
        // GET ~/Accounts(1)/PayinPIs(1)
        public async Task QueryOnePayinPI(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs(101)?$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            if (!mime.Contains("odata.metadata=none"))
            {
                var expectedContextUrl = serviceRootUri + "/$metadata#Accounts(100)/PayinPIs/$entity";
                var actualContextUrl = (string)json["@odata.context"];
                Assert.Equal(expectedContextUrl, actualContextUrl);
            }

            if (mime.Contains("odata.metadata=full"))
            {
                var payinPI = json;

                Assert.Equal(serviceRootUri + "/Accounts(100)/PayinPIs(101)/Microsoft.Test.E2E.AspNet.OData.Containment.Delete",
                    (string)payinPI["#Microsoft.Test.E2E.AspNet.OData.Containment.Delete"]["target"]);
                Assert.Equal("Accounts(100)/PayinPIs(101)", (string)payinPI["@odata.editLink"]);
                Assert.Equal("Accounts(100)/PayinPIs(101)", (string)payinPI["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payinPI["@odata.type"]);
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test is is able to query a nullable containment navigation property.
        // GET ~/Accounts(1)/PayoutPI
        public async Task QueryPayoutPI(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayoutPI?$select=PaymentInstrumentID&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            string expectedValue, actualValue;
            if (!mime.Contains("odata.metadata=none"))
            {
                expectedValue = serviceRootUri + "/$metadata#Accounts(100)/PayoutPI(PaymentInstrumentID)/$entity";
                actualValue = (string)json["@odata.context"];
                Assert.True(expectedValue == actualValue, string.Format("@odata.context, expected: {0}, actual: {1}, request url: {2}", expectedValue, actualValue, requestUri));//Actual:   http://jinfutan03:9123/explicit/$metadata#Accounts(100)/PayoutPI/$entity
            }
            if (mime.Contains("odata.metadata=full"))
            {
                Assert.Equal("Accounts(100)/PayoutPI", (string)json["@odata.editLink"]);
                Assert.Equal("Accounts(100)/PayoutPI", (string)json["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)json["@odata.type"]);
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test it is able to query a non-nullable containment navigation property defined on a derived entity.
        // GET ~/Accounts(1)/Namespace.PremiumAccount/GiftCard
        public async Task QueryContainmentNavigationOnDerivedEntity(string mode, string mime)
        {
            await ResetDatasource();

            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard?$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            if (!mime.Contains("odata.metadata=none"))
            {
                var expectedContextUrl = serviceRootUri + "/$metadata#Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard/$entity";
                var actualContextUrl = (string)json["@odata.context"];
                Assert.Equal(expectedContextUrl, actualContextUrl);
            }
            if (mime.Contains("odata.metadata=full"))
            {
                Assert.Equal("Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard", (string)json["@odata.editLink"]);
                Assert.Equal("Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard", (string)json["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.GiftCard", (string)json["@odata.type"]);
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test it is able to query the association link of a non-nullable navigation property defined on a derived entity
        // GET ~/Accounts(1)/Namespace.PremiumAccount/GiftCard/$ref
        public async Task QueryAssociationLinkOfGiftCard(string mode, string mime)
        {
            await ResetDatasource();

            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard/$ref?$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            /*
            {
              "@odata.context": "http://host/service/$metadata#Collection($ref)",
              "@odata.id": "Orders(10643)"
            }
            */
            var json = await response.Content.ReadAsObject<JObject>();
            if (!mime.Contains("odata.metadata=none"))
            {
                var expectedContextUrl = serviceRootUri + "/$metadata#$ref";
                var actualContextUrl = (string)json["@odata.context"];
                Assert.Equal(expectedContextUrl, actualContextUrl);
            }
            Assert.Equal(serviceRootUri + "/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard", (string)json["@odata.id"]);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test it is able to query the association link of a collection navigation property
        // GET ~/Accounts(1)/PayinPIs/$ref
        public async Task QueryAssociationLinkOfPayinPIs(string mode, string mime)
        {
            await ResetDatasource();

            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(200)/PayinPIs/$ref?$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            /*
            {
              "@odata.context": "http://host/service/$metadata#Collection($ref)",
              "value": 
              [
                { "@odata.id": "Orders(10643)" },
                { "@odata.id": "Orders(10759)" }
              ]
            }
            */

            var json = await response.Content.ReadAsObject<JObject>();
            if (!mime.Contains("odata.metadata=none"))
            {
                var expectedContextUrl = serviceRootUri + "/$metadata#Collection($ref)";
                var actualContextUrl = (string)json["@odata.context"];
                Assert.Equal(expectedContextUrl, actualContextUrl);
            }

            Assert.Equal(serviceRootUri + "/Accounts(200)/PayinPIs(201)", (string)json["value"][0]["@odata.id"]);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test it is able to query the association link of a single-valued navigation property
        // GET ~/Accounts(1)/PayoutPI/$ref
        public async Task QueryAssociationLinkOfPayoutPI(string mode, string mime)
        {
            await ResetDatasource();

            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(200)/PayoutPI/$ref?$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            /*
            {
              "@odata.context":"http://jinfutanwebapi1/70f320778d1e4645bf5325c00d2cc963/convention/$metadata#$ref",
              "@odata.id":"http://jinfutanwebapi1/70f320778d1e4645bf5325c00d2cc963/convention/Accounts(200)/PayoutPI"
            }
            */

            var json = await response.Content.ReadAsObject<JObject>();
            if (!mime.Contains("odata.metadata=none"))
            {
                var expectedContextUrl = serviceRootUri + "/$metadata#$ref";
                var actualContextUrl = (string)json["@odata.context"];
                Assert.Equal(expectedContextUrl, actualContextUrl);
            }

            Assert.Equal(serviceRootUri + "/Accounts(200)/PayoutPI", (string)json["@odata.id"]);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to PUT to ONE entity of a collection containment navigation property
        // PUT ~/Accounts(1)/PayinPIs(1)
        public async Task PutToOneOfPayinPIs(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs(101)", BaseAddress, mode);
            string newFriendlyName = "NewPiName";
            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentInstrumentID = 101,
                FriendlyName = newFriendlyName,
                Signatories = new List<Signatory>()
                {
                    new Signatory()
                    {
                        SignatoryID=1,
                        SignatoryName="Sig 1"
                    }
                }
            };

            var response = await Client.PutAsJsonAsync<PaymentInstrument>(requestUri, pi);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(string.Format("{0}/$metadata#Accounts(100)/PayinPIs/$entity", serviceRootUri), (string)json["@odata.context"]);
            Assert.Equal(newFriendlyName, (string)json["FriendlyName"]);
            Assert.Equal(101, (int)json["PaymentInstrumentID"]);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to put to a nullable containment navigation property
        // PUT ~/Accounts(1)/PayoutPI
        public async Task PutPayoutPI(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayoutPI", BaseAddress, mode);
            string newFriendlyName = "NewPiName";
            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentInstrumentID = 1000,
                FriendlyName = newFriendlyName,
                Signatories = new List<Signatory>()
                {
                    new Signatory()
                    {
                        SignatoryID=1,
                        SignatoryName="Sig 1"
                    }
                }
            };

            var response = await Client.PutAsJsonAsync<PaymentInstrument>(requestUri, pi);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(string.Format("{0}/$metadata#Accounts(100)/PayoutPI/$entity", serviceRootUri), (string)json["@odata.context"]);
            Assert.Equal(newFriendlyName, (string)json["FriendlyName"]);
            Assert.Equal(1000, (int)json["PaymentInstrumentID"]);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to patch to a non-nullable containment navigation property defined on a derived entity
        // PATCH ~/Accounts(1)/Namespace.PremiumAccount/GiftCard
        public async Task PatchGiftCard(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard", BaseAddress, mode);
            string cardNo = "ABCD-1234";
            double amount = 2000;
            GiftCard giftCard = new GiftCard()
            {
                GiftCardID = 200,
                GiftCardNO = cardNo,
                Amount = amount,
            };

            var response = await Client.PatchAsJsonAsync<GiftCard>(requestUri, giftCard);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(string.Format("{0}/$metadata#Accounts(200)/Microsoft.Test.E2E.AspNet.OData.Containment.PremiumAccount/GiftCard/$entity", serviceRootUri), (string)json["@odata.context"]);
            Assert.Equal(200, (int)json["GiftCardID"]);
            Assert.Equal(cardNo, (string)json["GiftCardNO"]);
            Assert.Equal(amount, (double)json["Amount"]);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to delete ONE entity of a collection containment navigation property
        // DELETE ~/Accounts(1)/PayinPIs(1)
        public async Task DeleteOneOfPayinPIs(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs(101)", BaseAddress, mode);

            var response = await Client.DeleteAsync(requestUri);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to delete ONE entity through collection association link
        // DELETE ~/Accounts(1)/PayoutPIs/$ref?$id=...
        public async Task DeleteOneOfPayoutPIsThroughAssociationLink(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs/$ref?$id=../../Accounts(100)/PayinPIs(101)", BaseAddress, mode);

            var response = await Client.DeleteAsync(requestUri);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to delete a nullable containment navigation property
        // DELETE ~/Accounts(1)/PayoutPI
        public async Task DeletePayoutPI(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayoutPI", BaseAddress, mode);

            var response = await Client.DeleteAsync(requestUri);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await Client.GetAsync(serviceRootUri + "/Accounts(100)?$expand=PayoutPI");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var account = await response.Content.ReadAsObject<JObject>();
            Assert.Null(account["PayoutPI"]);
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        // To test it is able to query a contained entity which is navigated from a containment navigation property
        // GET ~/Accounts(1)/PayinPIs(101)/Statement
        public async Task GetStatement(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs(101)/Statement?$format={2}", BaseAddress, mode, mime);

            var response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            /*
            {
              "@odata.context":"http://jinfutan03:9123/explicit/$metadata#Accounts(100)/PayinPIs(101)/Statement/$entity",
              "@odata.type":"#Microsoft.Test.E2E.AspNet.OData.Containment.Statement",
              "@odata.id":"Accounts(100)/PayinPIs(101)/Statement",
              "@odata.editLink":"Accounts(100)/PayinPIs(101)/Statement",
              "StatementID":1,
              "TransactionDescription":"Physical Goods.",
              "Amount":0.0
            }
            */
            var json = await response.Content.ReadAsObject<JObject>();
            if (!mime.Contains("odata.metadata=none"))
            {
                var expectedContextUrl = serviceRootUri + "/$metadata#Accounts(100)/PayinPIs(101)/Statement/$entity";
                var actualContextUrl = (string)json["@odata.context"];
                Assert.Equal(expectedContextUrl, actualContextUrl);
            }
            if (mime.Contains("odata.metadata=full"))
            {
                Assert.Equal("Accounts(100)/PayinPIs(101)/Statement", (string)json["@odata.editLink"]);
                Assert.Equal("Accounts(100)/PayinPIs(101)/Statement", (string)json["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.Statement", (string)json["@odata.type"]);
            }
            Assert.Equal(1, (int)json["StatementID"]);

        }
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to put a contained entity which is navigated from a containment navigation property
        // PUT ~/Accounts(1)/PayinPIs(1)/Statement
        public async Task PutStatement(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs(101)/Statement", BaseAddress, mode);
            string newDescription = "newDescription";
            Statement statement = new Statement()
            {
                StatementID = 1010,
                TransactionDescription = newDescription,
            };

            /* {"StatementID":1010,"TransactionDescription":"newDescription","Amount":0.0}*/
            var response = await Client.PutAsJsonAsync<Statement>(requestUri, statement);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            /*
            {
              "@odata.context":"http://jinfutan03:9123/explicit/$metadata#Accounts(100)/PayinPIs(101)/Statement/$entity",
              "StatementID":1010,
              "TransactionDescription":"newDescription",
              "Amount":0.0
            }
            */
            var json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(string.Format("{0}/$metadata#Accounts(100)/PayinPIs(101)/Statement/$entity", serviceRootUri), (string)json["@odata.context"]);
            Assert.Equal(newDescription, (string)json["TransactionDescription"]);
            Assert.Equal(1010, (int)json["StatementID"]);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // To test it is able to delete a contained entity which is navigated from a containment navigation property
        // DELETE ~/Accounts(1)/PayinPIs(1)/Statement
        public async Task DeleteStatement(string mode)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs(101)/Statement", BaseAddress, mode);

            var response = await Client.DeleteAsync(requestUri);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await Client.GetAsync(serviceRootUri + "/Accounts(100)/PayinPIs(101)?$expand=Statement");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var account = await response.Content.ReadAsObject<JObject>();
            Assert.Null(account["Steatement"]);
        }
        #endregion

        #region Actions and Functions
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Action bound to a collection of contained entity.
        public async Task ClearPayinPIsWhoseNameContainsGivenString(string mode)
        {
            await ResetDatasource();
            string requestUriBase = string.Format("{0}/{1}/Accounts(100)/PayinPIs", BaseAddress, mode);
            var response = await Client.GetAsync(requestUriBase);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsObject<JObject>();
            var originCount = ((JArray)json["value"]).Count;

            string requestUri = string.Format("{0}/Microsoft.Test.E2E.AspNet.OData.Containment.Clear", requestUriBase);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent("{'nameContains':'10'}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsObject<JObject>();
            var deletedCount = (int)json["value"];

            response = await Client.GetAsync(requestUriBase);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsObject<JObject>();
            var currentCount = ((JArray)json["value"]).Count;

            Assert.Equal(originCount - deletedCount, currentCount);
        }

        [Theory]
        [InlineData("convention/Accounts(100)/PayinPIs(101)/Microsoft.Test.E2E.AspNet.OData.Containment.Delete")]
        [InlineData("convention/Accounts(100)/PayoutPI/Microsoft.Test.E2E.AspNet.OData.Containment.Delete")]
        [InlineData("explicit/Accounts(100)/PayinPIs(101)/Microsoft.Test.E2E.AspNet.OData.Containment.Delete")]
        [InlineData("explicit/Accounts(100)/PayoutPI/Microsoft.Test.E2E.AspNet.OData.Containment.Delete")]
        // Action bound to a single contained entity.
        public async Task ActionBoundToASingleContainmedEntity(string requestUri)
        {
            await ResetDatasource();
            requestUri = BaseAddress + "/" + requestUri;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var response = await Client.PostAsync(requestUri, null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        // Function bound to a single contained entity.
        public async Task GetPayinPIsCountWhoseNameContainsGivenString(string mode)
        {
            await ResetDatasource();
            string requestUri = string.Format("{0}/{1}/Accounts(100)/PayinPIs/Microsoft.Test.E2E.AspNet.OData.Containment.GetCount(nameContains='10')", BaseAddress, mode);
            var response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsObject<JObject>();
            var count = (int)json["value"];

            Assert.Equal(2, count);
        }

        #endregion

        #region singleton
        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task ExpandContainmentNavigationPropertyOnSingleton(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/AnonymousAccount?$select=AccountID&$expand=PayinPIs,PayoutPI&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            if (mime == "json" || mime.Contains("odata.metadata=minimal") || mime.Contains("odata.metadata=full"))
            {
                var odataContext = (string)json["@odata.context"]; // PreminumAccount
                Assert.Equal(serviceRootUri + "/$metadata#AnonymousAccount(AccountID,PayinPIs(),PayoutPI())", odataContext);
            }

            if (mime.Contains("odata.metadata=full"))
            {
                var account = json;
                Assert.Equal(serviceRootUri + "/AnonymousAccount/PayinPIs/$ref", (string)account["PayinPIs@odata.associationLink"]);
                Assert.Equal(serviceRootUri + "/AnonymousAccount/PayinPIs", (string)account["PayinPIs@odata.navigationLink"]);
                Assert.Equal(serviceRootUri + "/AnonymousAccount/PayoutPI/$ref", (string)account["PayoutPI@odata.associationLink"]);
                Assert.Equal(serviceRootUri + "/AnonymousAccount/PayoutPI", (string)account["PayoutPI@odata.navigationLink"]);

                var payoutPIOfAccount = account["PayoutPI"];
                Assert.Equal("AnonymousAccount/PayoutPI", (string)payoutPIOfAccount["@odata.editLink"]);
                Assert.Equal("AnonymousAccount/PayoutPI", (string)payoutPIOfAccount["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payoutPIOfAccount["@odata.type"]);

                var payinPIsOfAccont = account["PayinPIs"];

                // Bug 1862: Functions/Actions bound to a collection of entity should be advertised.
                /*Functions that are bound to a collection of entities are advertised in representations of that collection.*/

                Assert.Equal("AnonymousAccount/PayinPIs(0)", (string)payinPIsOfAccont[0]["@odata.editLink"]);
                Assert.Equal("AnonymousAccount/PayinPIs(0)", (string)payinPIsOfAccont[0]["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)payinPIsOfAccont[0]["@odata.type"]);
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryNullableContainmentNavigationPropertyFromSingleton(string mode, string mime)
        {
            await ResetDatasource();
            string serviceRootUri = string.Format("{0}/{1}", BaseAddress, mode).ToLower();
            string requestUri = string.Format("{0}/{1}/AnonymousAccount/PayoutPI?$select=PaymentInstrumentID&$format={2}", BaseAddress, mode, mime);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            if (!mime.Contains("odata.metadata=none"))
            {
                var expectedContextUrl = serviceRootUri + "/$metadata#AnonymousAccount/PayoutPI(PaymentInstrumentID)/$entity";
                var actualContextUrl = (string)json["@odata.context"];
                Assert.Equal(expectedContextUrl, actualContextUrl);
            }
            if (mime.Contains("odata.metadata=full"))
            {
                Assert.Equal("AnonymousAccount/PayoutPI", (string)json["@odata.editLink"]);
                Assert.Equal("AnonymousAccount/PayoutPI", (string)json["@odata.id"]);
                Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Containment.PaymentInstrument", (string)json["@odata.type"]);
            }
        }

        #endregion

        #region batch
        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task PerformCUDInBatch(string modelBuilderMode)
        {
            await ResetDatasource();

            Uri serviceUrl = new Uri(BaseAddress + "/" + modelBuilderMode);
            var client = new Proxy.Container(serviceUrl);
            client.MergeOption = MergeOption.OverwriteChanges;
            client.Format.UseJson();

            var accounts = await client.Accounts.Expand("PayinPIs").ExecuteAsync();
            List<Proxy.Account> accountList = accounts.ToList();

            Proxy.Account accountToDelete = accountList.Single(a => a.AccountID == 200);
            client.DeleteObject(accountToDelete);

            Proxy.Account accountToUpdate = accountList.Single(a => a.AccountID == 100);

            Proxy.PaymentInstrument piToAdd = new Proxy.PaymentInstrument()
            {
                PaymentInstrumentID = 0,
                FriendlyName = "newName",
            };
            client.AddRelatedObject(accountToUpdate, "PayinPIs", piToAdd);

            Proxy.PaymentInstrument piToUpdate = accountToUpdate.PayinPIs.Single(pi => pi.PaymentInstrumentID == 101);
            piToUpdate.FriendlyName = "updatedName";
            client.UpdateObject(piToUpdate);

            Proxy.PaymentInstrument piToDelete = accountToUpdate.PayinPIs.Single(pi => pi.PaymentInstrumentID == 102);
            client.DeleteObject(piToDelete);

            var response = await client.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);

            var newClient = new Proxy.Container(serviceUrl);
            var changedAccounts = await newClient.Accounts.ExecuteAsync();
            List<Proxy.Account> changedAccountsList = changedAccounts.ToList();

            Assert.DoesNotContain(changedAccountsList, (x) => x.AccountID == accountToDelete.AccountID);

            Proxy.Account account = await Task.Factory.FromAsync(newClient.Accounts.BeginExecute(null, null), (asyncResult) =>
            {
                return newClient.Accounts.EndExecute(asyncResult).Where(a => a.AccountID == 100).Single();
            });

            await newClient.LoadPropertyAsync(account, "PayinPIs");
            var payinPIList = account.PayinPIs.ToList();

            Assert.DoesNotContain(payinPIList, (pi) => pi.PaymentInstrumentID == 102);
            Assert.Contains(payinPIList, (pi) => pi.PaymentInstrumentID == 103);

            Assert.Equal("updatedName", payinPIList.Single(pi => pi.PaymentInstrumentID == 101).FriendlyName);
        }

        #endregion
        private async Task<HttpResponseMessage> ResetDatasource()
        {
            var uriReset = this.BaseAddress + "/convention/ResetDataSource";
            var response = await this.Client.PostAsync(uriReset, null);
            return response;
        }
    }
}
