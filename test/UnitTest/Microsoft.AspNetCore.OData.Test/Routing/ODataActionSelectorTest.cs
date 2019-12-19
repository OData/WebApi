// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataActionSelectorTest
    {

        public static TheoryDataSet<Dictionary<string, object>, string, System.Type, string, System.Type[]> Scenarios
        {
            get
            {
                return new TheoryDataSet<Dictionary<string, object>, string, System.Type, string, System.Type[]>
                {
                    // Actions with single key parameter
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        null,
                        typeof(SingleKeyController), "Get",
                        new [] { typeof(int) }
                    },
                    {
                        new Dictionary<string, object>(),
                        null,
                        typeof(SingleKeyController), "Get",
                        new System.Type[0]
                    },
                    // Actions with multiple parameters
                    {
                        new Dictionary<string, object>(),
                        null,
                        typeof(PathAndQueryController), "Get",
                        new [] { typeof(ODataPath), typeof(ODataQueryOptions)}
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 } },
                        null,
                        typeof(KeyAndQueryController), "Get",
                        new [] { typeof(int), typeof(ODataQueryOptions)}
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 }, { "relatedKey", 2 } },
                        null,
                        typeof(KeyAndRelatedKeyController), "Get",
                        new [] { typeof(int), typeof(int) }
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 }, {  "relatedKey", 2 } },
                        null,
                        typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(int), typeof(ODataPath) }
                    },
                    {
                        new Dictionary<string, object>()
                        {
                            { "key", 1 }, { "relatedKey", 2 }, { "navigationProperty", 3 }
                        },
                        null,
                        typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(int), typeof(int) }
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 } },
                        null,
                        typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(ODataPath) }
                    },
                    // actions that expect request body
                    {
                        new Dictionary<string, object>(),
                        "{}",
                        typeof(BodyOnlyController), "Post",
                        new [] { typeof(int) }
                    },
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        "{}",
                        typeof(KeyBodyController), "Put",
                        new [] { typeof(int), typeof(Delta<object>) }
                    },
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        null,
                        typeof(KeyBodyController), "Put",
                        new [] { typeof(int) }
                    }
            };
            }
        }


        [Theory]
        [MemberData(nameof(Scenarios))]
        public void SelectBestCandidate_SelectsCorrectly(Dictionary<string, object> routeDataValues, string bodyContent, System.Type controllerType, string actionName, System.Type[] expectedActionSignature)
        {
            // Arrange
            ODataActionSelectorTestHelper.SetupActionSelector(controllerType, out var routeBuilder, out var actionSelector, out var actionDescriptors);
            var routeContext = ODataActionSelectorTestHelper.SetupRouteContext(routeBuilder, actionName, routeDataValues, bodyContent);

            // Act
            var action = actionSelector.SelectBestCandidate(routeContext, actionDescriptors);

            // Assert
            var method = controllerType.GetMethod(actionName, expectedActionSignature);
            Assert.NotNull(action);
            Assert.True(ODataActionSelectorTestHelper.ActionMatchesMethod(action, method));
        }

    }

    public class SingleKeyController : TestODataController
    {
        public void Get(int key) {}
        public void Get() {}
    }

    public class PathAndQueryController : TestODataController
    {
        public void Get(ODataPath path, ODataQueryOptions queryOptions) { }

        public void Get(ODataPath path) { }
    }

    public class KeyAndQueryController : TestODataController
    {
        public void Get(int key, ODataQueryOptions queryOptions) { }

        public void Get(int key) { }
    }

    public class KeyAndRelatedKeyController : TestODataController
    {
        public void Get(int key, int relatedKey) { }

        public void Get(int key) { }
    }

    public class KeyAndRelatedKeyAndPathController : TestODataController
    {
        public void Get(int key, int relatedKey, ODataPath path) { }

        public void Get(int key, int relatedKey) { }

        public void Get(int key, int relatedKey, int navigationProperty) { }

        public void Get(int key, ODataPath path) { }
    }

    public class KeyBodyController : TestODataController
    {
        public void Put(int key, Delta<object> dt) { }
        public void Put(int key) { }
    }

    public class BodyOnlyController : TestODataController
    {
        public void Post(int body) { }
        public void Post() { }
    }
}