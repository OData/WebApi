// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Xunit;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataActionSelectorTest
    {

        public static TheoryDataSet<Dictionary<string, object>, System.Type, string, System.Type[]> Scenarios
        {
            get
            {
                return new TheoryDataSet<Dictionary<string, object>, System.Type, string, System.Type[]>
                {
                    // Actions with single key parameter
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        typeof(SingleKeyController), "Get",
                        new [] { typeof(int) }
                    },
                    {
                        new Dictionary<string, object>(),
                        typeof(SingleKeyController), "Get",
                        new System.Type[0]
                    },
                    // Actions with multiple parameters
                    {
                        new Dictionary<string, object>(),
                        typeof(PathAndQueryController), "Get",
                        new [] { typeof(ODataPath), typeof(ODataQueryOptions)}
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 } },
                        typeof(KeyAndQueryController), "Get",
                        new [] { typeof(int), typeof(ODataQueryOptions)}
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 }, { "relatedKey", 2 } },
                        typeof(KeyAndRelatedKeyController), "Get",
                        new [] { typeof(int), typeof(int) }
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 }, {  "relatedKey", 2 } },
                        typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(int), typeof(ODataPath) }
                    },
                    {
                        new Dictionary<string, object>()
                        {
                            { "key", 1 }, { "relatedKey", 2 }, { "navigationProperty", 3 }
                        },
                        typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(int), typeof(int) }
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 } },
                        typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(ODataPath) }
                    }
                };
            }
        }


        [Theory]
        [MemberData(nameof(Scenarios))]
        public void SelectBestCandidate_SelectsCorrectly(Dictionary<string, object> routeDataValues, System.Type controllerType, string actionName, System.Type[] expectedActionSignature)
        {
            // Arrange
            ODataActionSelectorTestHelper.SetupActionSelector(controllerType, out var routeBuilder, out var actionSelector, out var actionDescriptors);
            var routeContext = ODataActionSelectorTestHelper.SetupRouteContext(routeBuilder, actionName, routeDataValues);

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
        public string Get(int key) => $"Get({key})";
        public string Get() => "Get()";
    }

    public class PathAndQueryController : TestODataController
    {
        public string Get(ODataPath path, ODataQueryOptions queryOptions) => "Get(path, queryOptions)";

        public string Get(ODataPath path) => "Get(path)";
    }

    public class KeyAndQueryController : TestODataController
    {
        public string Get(int key, ODataQueryOptions queryOptions) => "Get(key, queryOptions)";

        public string Get(int key) => "Get(key)";
    }

    public class KeyAndRelatedKeyController : TestODataController
    {
        public string Get(int key, int relatedKey) => "Get(key, relatedKey)";

        public string Get(int key) => "Get(key)";
    }

    public class KeyAndRelatedKeyAndPathController : TestODataController
    {
        public string Get(int key, int relatedKey, ODataPath path) => "Get(key, relatedKey, path)";

        public string Get(int key, int relatedKey) => "Get(key, relatedKey)";

        public string Get(int key, int relatedKey, int navigationProperty)
            => "Get(key, relatedKey, navigationProperty)";

        public string Get(int key, ODataPath path) => "Get(key, path)";
    }

    public class BodyController : TestODataController
    {
        public string Put(int key, Delta<object> dt) => "Put(key, dt)";
        public string Put(int key) => "Put(key)";
        public string Post(int body) => "Post(body)";
    }
}