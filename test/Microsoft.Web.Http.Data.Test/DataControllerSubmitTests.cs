// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using Microsoft.Web.Http.Data.Test.Models;
using Newtonsoft.Json;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Http.Data.Test
{
    public class DataControllerSubmitTests
    {
        // Verify that POSTs directly to CUD actions still go through the submit pipeline
        [Fact]
        public void Submit_Proxy_Insert()
        {
            Order order = new Order { OrderID = 1, OrderDate = DateTime.Now };

            HttpResponseMessage response = this.ExecuteSelfHostRequest(TestConstants.CatalogUrl + "InsertOrder", "Catalog", order);
            Order resultOrder = response.Content.ReadAsAsync<Order>().Result;
            Assert.NotNull(resultOrder);
        }

        // Submit a changeset with multiple entries
        [Fact]
        public void Submit_Multiple_Success()
        {
            Order order = new Order { OrderID = 1, OrderDate = DateTime.Now };
            Product product = new Product { ProductID = 1, ProductName = "Choco Wafers" };
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = order, Operation = ChangeOperation.Insert },
                new ChangeSetEntry { Id = 2, Entity = product, Operation = ChangeOperation.Update }
            };

            ChangeSetEntry[] resultChangeSet = this.ExecuteSubmit(TestConstants.CatalogUrl + "Submit", "Catalog", changeSet);
            Assert.Equal(2, resultChangeSet.Length);
            Assert.True(resultChangeSet.All(p => !p.HasError));
        }

        // Submit a changeset with one parent object and multiple dependent children
        [Fact]
        public void Submit_Tree_Success()
        {
            Order order = new Order { OrderID = 1, OrderDate = DateTime.Now };
            Order_Detail d1 = new Order_Detail { ProductID = 1 };
            Order_Detail d2 = new Order_Detail { ProductID = 2 };
            Dictionary<string, int[]> detailsAssociation = new Dictionary<string, int[]>();
            detailsAssociation.Add("Order_Details", new int[] { 2, 3 });
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = order, Operation = ChangeOperation.Insert, Associations = detailsAssociation },
                new ChangeSetEntry { Id = 2, Entity = d1, Operation = ChangeOperation.Insert },
                new ChangeSetEntry { Id = 3, Entity = d2, Operation = ChangeOperation.Insert }
            };

            ChangeSetEntry[] resultChangeSet = this.ExecuteSubmit(TestConstants.CatalogUrl + "Submit", "Catalog", changeSet);
            Assert.Equal(3, resultChangeSet.Length);
            Assert.True(resultChangeSet.All(p => !p.HasError));
        }

        /// <summary>
        /// End to end validation scenario showing changeset validation. DataAnnotations validation attributes are applied to
        /// the model by DataController metadata providers (metadata coming all the way from the EF model, as well as "buddy
        /// class" metadata), and these are validated during changeset validation. The validation results per entity/member are
        /// returned via the changeset and verified.
        /// </summary>
        [Fact]
        public void Submit_Validation_Failure()
        {
            Microsoft.Web.Http.Data.Test.Models.EF.Product newProduct = new Microsoft.Web.Http.Data.Test.Models.EF.Product { ProductID = 1, ProductName = String.Empty, UnitPrice = -1 };
            Microsoft.Web.Http.Data.Test.Models.EF.Product updateProduct = new Microsoft.Web.Http.Data.Test.Models.EF.Product { ProductID = 1, ProductName = new string('x', 50), UnitPrice = 55.77M };
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = newProduct, Operation = ChangeOperation.Insert },
                new ChangeSetEntry { Id = 2, Entity = updateProduct, Operation = ChangeOperation.Update }
            };

            HttpResponseMessage response = this.ExecuteSelfHostRequest("http://testhost/NorthwindEFTest/Submit", "NorthwindEFTest", changeSet);
            changeSet = response.Content.ReadAsAsync<ChangeSetEntry[]>().Result;

            // errors for the new product
            ValidationResultInfo[] errors = changeSet[0].ValidationErrors.ToArray();
            Assert.Equal(2, errors.Length);
            Assert.True(changeSet[0].HasError);

            // validation rule inferred from EF model
            Assert.Equal("ProductName", errors[0].SourceMemberNames.Single());
            Assert.Equal("The ProductName field is required.", errors[0].Message);

            // validation rule coming from buddy class
            Assert.Equal("UnitPrice", errors[1].SourceMemberNames.Single());
            Assert.Equal("The field UnitPrice must be between 0 and 1000000.", errors[1].Message);

            // errors for the updated product
            errors = changeSet[1].ValidationErrors.ToArray();
            Assert.Equal(1, errors.Length);
            Assert.True(changeSet[1].HasError);

            // validation rule inferred from EF model
            Assert.Equal("ProductName", errors[0].SourceMemberNames.Single());
            Assert.Equal("The field ProductName must be a string with a maximum length of 40.", errors[0].Message);
        }

        [Fact]
        public void Submit_Authorization_Success()
        {
            TestAuthAttribute.Reset();

            Product product = new Product { ProductID = 1, ProductName = "Choco Wafers" };
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = product, Operation = ChangeOperation.Update }
            };

            ChangeSetEntry[] resultChangeSet = this.ExecuteSubmit("http://testhost/TestAuth/Submit", "TestAuth", changeSet);
            Assert.Equal(1, resultChangeSet.Length);
            Assert.True(TestAuthAttribute.Log.SequenceEqual(new string[] { "Global", "Class", "SubmitMethod", "UserMethod" }));
        }

        [Fact]
        public void Submit_Authorization_Fail_UserMethod()
        {
            TestAuthAttribute.Reset();

            Product product = new Product { ProductID = 1, ProductName = "Choco Wafers" };
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = product, Operation = ChangeOperation.Update }
            };

            TestAuthAttribute.FailLevel = "UserMethod";
            HttpResponseMessage response = this.ExecuteSelfHostRequest("http://testhost/TestAuth/Submit", "TestAuth", changeSet);

            Assert.True(TestAuthAttribute.Log.SequenceEqual(new string[] { "Global", "Class", "SubmitMethod", "UserMethod" }));
            Assert.Equal("Not Authorized", response.ReasonPhrase);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void Submit_Authorization_Fail_SubmitMethod()
        {
            TestAuthAttribute.Reset();

            Product product = new Product { ProductID = 1, ProductName = "Choco Wafers" };
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = product, Operation = ChangeOperation.Update }
            };

            TestAuthAttribute.FailLevel = "SubmitMethod";
            HttpResponseMessage response = this.ExecuteSelfHostRequest("http://testhost/TestAuth/Submit", "TestAuth", changeSet);

            Assert.True(TestAuthAttribute.Log.SequenceEqual(new string[] { "Global", "Class", "SubmitMethod" }));
            Assert.Equal("Not Authorized", response.ReasonPhrase);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void Submit_Authorization_Fail_Class()
        {
            TestAuthAttribute.Reset();

            Product product = new Product { ProductID = 1, ProductName = "Choco Wafers" };
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = product, Operation = ChangeOperation.Update }
            };

            TestAuthAttribute.FailLevel = "Class";
            HttpResponseMessage response = this.ExecuteSelfHostRequest("http://testhost/TestAuth/Submit", "TestAuth", changeSet);

            Assert.True(TestAuthAttribute.Log.SequenceEqual(new string[] { "Global", "Class" }));
            Assert.Equal("Not Authorized", response.ReasonPhrase);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void Submit_Authorization_Fail_Global()
        {
            TestAuthAttribute.Reset();

            Product product = new Product { ProductID = 1, ProductName = "Choco Wafers" };
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = product, Operation = ChangeOperation.Update }
            };

            TestAuthAttribute.FailLevel = "Global";
            HttpResponseMessage response = this.ExecuteSelfHostRequest("http://testhost/TestAuth/Submit", "TestAuth", changeSet);

            Assert.True(TestAuthAttribute.Log.SequenceEqual(new string[] { "Global" }));
            Assert.Equal("Not Authorized", response.ReasonPhrase);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Verify that a CUD operation that isn't supported for a given entity type
        // results in a server error
        [Fact]
        public void Submit_ResolveActions_UnsupportedAction()
        {
            Product product = new Product { ProductID = 1, ProductName = "Choco Wafers" };
            ChangeSetEntry[] changeSet = new ChangeSetEntry[] { 
                new ChangeSetEntry { Id = 1, Entity = product, Operation = ChangeOperation.Delete }
            };

            HttpConfiguration configuration = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(configuration, "NorthwindEFTestController", typeof(NorthwindEFTestController));
            DataControllerDescription description = DataControllerDescription.GetDescription(controllerDescriptor);
            Assert.Throws<InvalidOperationException>(
                () => DataController.ResolveActions(description, changeSet),
                String.Format(Resource.DataController_InvalidAction, "Delete", "Product"));
        }

        /// <summary>
        /// Execute a full roundtrip Submit request for the specified changeset, going through
        /// the full serialization pipeline.
        /// </summary>
        private ChangeSetEntry[] ExecuteSubmit(string url, string controllerName, ChangeSetEntry[] changeSet)
        {
            HttpResponseMessage response = this.ExecuteSelfHostRequest(url, controllerName, changeSet);
            ChangeSetEntry[] resultChangeSet = GetChangesetResponse(response);
            return changeSet;
        }

        private HttpResponseMessage ExecuteSelfHostRequest(string url, string controller, object data)
        {
            return ExecuteSelfHostRequest(url, controller, data, "application/json");
        }

        private HttpResponseMessage ExecuteSelfHostRequest(string url, string controller, object data, string mediaType)
        {
            HttpConfiguration config = new HttpConfiguration();
            IHttpRoute routeData;
            if (!config.Routes.TryGetValue(controller, out routeData))
            {
                HttpRoute route = new HttpRoute("{controller}/{action}", new HttpRouteValueDictionary(controller));
                config.Routes.Add(controller, route);
            }

            HttpControllerDispatcher dispatcher = new HttpControllerDispatcher(config);
            HttpServer server = new HttpServer(config, dispatcher);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            string serializedChangeSet = String.Empty;
            if (mediaType == "application/json")
            {
                JsonSerializer serializer = new JsonSerializer() { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.All };            
                MemoryStream ms = new MemoryStream();
                JsonWriter writer = new JsonTextWriter(new StreamWriter(ms));
                serializer.Serialize(writer, data);
                writer.Flush();
                ms.Seek(0, 0);
                serializedChangeSet = Encoding.UTF8.GetString(ms.GetBuffer()).TrimEnd('\0');
            }
            else
            {
                DataContractSerializer ser = new DataContractSerializer(data.GetType(), GetTestKnownTypes());
                MemoryStream ms = new MemoryStream();
                ser.WriteObject(ms, data);
                ms.Flush();
                ms.Seek(0, 0);
                serializedChangeSet = Encoding.UTF8.GetString(ms.GetBuffer()).TrimEnd('\0');
            }

            HttpRequestMessage request = TestHelpers.CreateTestMessage(url, HttpMethod.Post, config);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            request.Content = new StringContent(serializedChangeSet, Encoding.UTF8, mediaType);

            return invoker.SendAsync(request, CancellationToken.None).Result;
        }

        /// <summary>
        /// For the given Submit response, serialize and deserialize the content. This forces the
        /// formatter pipeline to run so we can verify that registered serializers are being used
        /// properly.
        /// </summary>
        private ChangeSetEntry[] GetChangesetResponse(HttpResponseMessage responseMessage)
        {
            // serialize the content to a stream
            ObjectContent content = (ObjectContent)responseMessage.Content;
            MemoryStream ms = new MemoryStream();
            content.CopyToAsync(ms).Wait();
            ms.Flush();
            ms.Seek(0, 0);

            // deserialize based on content type
            ChangeSetEntry[] changeSet = null;
            string mediaType = responseMessage.RequestMessage.Content.Headers.ContentType.MediaType;
            if (mediaType == "application/json")
            {
                JsonSerializer ser = new JsonSerializer() { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.All };
                changeSet = (ChangeSetEntry[])ser.Deserialize(new JsonTextReader(new StreamReader(ms)), content.ObjectType);
            }
            else
            {
                DataContractSerializer ser = new DataContractSerializer(content.ObjectType, GetTestKnownTypes());
                changeSet = (ChangeSetEntry[])ser.ReadObject(ms);
            }

            return changeSet;
        }

        private IEnumerable<Type> GetTestKnownTypes()
        {
            List<Type> knownTypes = new List<Type>(new Type[] { typeof(Order), typeof(Product), typeof(Order_Detail) });
            knownTypes.AddRange(new Type[] { typeof(Microsoft.Web.Http.Data.Test.Models.EF.Order), typeof(Microsoft.Web.Http.Data.Test.Models.EF.Product), typeof(Microsoft.Web.Http.Data.Test.Models.EF.Order_Detail) });
            return knownTypes;
        }
    }

    /// <summary>
    /// Test controller used for multi-level authorization testing
    /// </summary>
    [TestAuth(Level = "Class")]
    public class TestAuthController : DataController
    {
        [TestAuth(Level = "UserMethod")]
        public void UpdateProduct(Product product)
        {
        }

        [TestAuth(Level = "SubmitMethod")]
        public override bool Submit(ChangeSet changeSet)
        {
            return base.Submit(changeSet);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            controllerContext.Configuration.Filters.Add(new TestAuthAttribute() { Level = "Global" });

            base.Initialize(controllerContext);
        }
    }

    /// <summary>
    /// Test authorization attribute used to verify authorization behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class TestAuthAttribute : AuthorizationFilterAttribute
    {
        public string Level;

        public static string FailLevel;

        public static List<string> Log = new List<string>();

        public override void OnAuthorization(HttpActionContext context)
        {
            TestAuthAttribute.Log.Add(Level);

            if (FailLevel != null && FailLevel == Level)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                response.ReasonPhrase = "Not Authorized";
                context.Response = response;
            }

            base.OnAuthorization(context);
        }

        public static void Reset()
        {
            FailLevel = null;
            Log.Clear();
        }
    }
}
