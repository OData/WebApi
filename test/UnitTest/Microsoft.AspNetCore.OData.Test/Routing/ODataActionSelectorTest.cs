//-----------------------------------------------------------------------------
// <copyright file="ODataActionSelectorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataActionSelectorTest
    {

        public static TheoryDataSet<Dictionary<string, object>, (string Method, string Body), (Type ControllerType, string ActionName, Type[] Signature)> ScenariosWithCorrectAction
        {
            get
            {
                return new TheoryDataSet<Dictionary<string, object>, (string, string), (Type, string, Type[])>
                {
                    // Actions with single key parameter
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        ("GET", null),
                        (typeof(SingleKeyController), "Get", new [] { typeof(int) })
                    },
                    {
                        new Dictionary<string, object>(),
                        ("GET", null),
                        (typeof(SingleKeyController), "Get", new Type[0])
                    },
                    // Actions with multiple parameters
                    {
                        new Dictionary<string, object>(),
                        ("GET", null),
                        (typeof(PathAndQueryController), "Get",
                        new [] { typeof(ODataPath), typeof(ODataQueryOptions)})
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 } },
                        ("GET", null),
                        (typeof(KeyAndQueryController), "Get",
                        new [] { typeof(int), typeof(ODataQueryOptions)})
                    },
                    {
                        new Dictionary<string, object>(),
                        ("GET", null),
                        (typeof(FromQueryController), "Get",
                        new [] { typeof(string), typeof(ODataQueryOptions)})
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 } },
                        ("GET", null),
                        (typeof(FromQueryController), "Get",
                        new [] { typeof(int), typeof(string) })
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 }, { "relatedKey", 2 } },
                        ("GET", null),
                        (typeof(KeyAndRelatedKeyController), "Get",
                        new [] { typeof(int), typeof(int) })
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 }, {  "relatedKey", 2 } },
                        ("GET", null),
                        (typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(int), typeof(ODataPath) })
                    },
                    {
                        new Dictionary<string, object>()
                        {
                            { "key", 1 }, { "relatedKey", 2 }, { "navigationProperty", 3 }
                        },
                        ("GET", null),
                        (typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(int), typeof(int) })
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 } },
                        ("GET", null),
                        (typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new [] { typeof(int), typeof(ODataPath) })
                    },
                    // actions that expect request body
                    {
                        new Dictionary<string, object>(),
                        ("POST", "{}"),
                        (typeof(BodyOnlyController), "Post",
                        new [] { typeof(int) })
                    },
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        ("PATCH", "{}"),
                        (typeof(KeyBodyController), "Patch",
                        new [] { typeof(int), typeof(Delta<object>) })
                    },
                    // actions that declare extra parameters with registered model binders
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        ("GET", null),
                        (typeof(ExtraParametersWithOwnModelBindersController), "Get",
                        new [] { typeof(int), typeof(System.Threading.CancellationToken) })
                    },
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        ("PATCH", "{}"),
                        (typeof(ExtraParametersWithOwnModelBindersController), "Patch",
                        new [] { typeof(int), typeof(System.Threading.CancellationToken), typeof(Delta<object>) })
                    },
                    {
                        new Dictionary<string, object>() { { "key", 1 } },
                        ("GET", null),
                        (typeof(FromQueryController), "Get",
                        new [] { typeof(int), typeof(string) })
                    },
                    {
                    new Dictionary<string, object>() { { "key", 1 }, { "relatedKey", 2 } },
                        ("GET", null),
                        (typeof(KeyAndRelatedKeyController), "Get",
                        new[] { typeof(int), typeof(int) })
                    },
                    {
                    new Dictionary<string, object>() { { "key", 1 }, { "relatedKey", 2 } },
                        ("GET", null),
                        (typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new[] { typeof(int), typeof(int), typeof(ODataPath) })
                    },
                    {
                    new Dictionary<string, object>()
                        {
                            { "key", 1 }, { "relatedKey", 2 }, { "navigationProperty", 3 }
                        },
                        ("GET", null),
                        (typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new[] { typeof(int), typeof(int), typeof(int) })
                    },
                    {
                    new Dictionary<string, object>() { { "key", 1 } },
                        ("GET", null),
                        (typeof(KeyAndRelatedKeyAndPathController), "Get",
                        new[] { typeof(int), typeof(ODataPath) })
                    },
                    // actions that expect request body
                    {
                    new Dictionary<string, object>(),
                        ("POST", "{}"),
                        (typeof(BodyOnlyController), "Post",
                        new[] { typeof(int) })
                    },
                    {
                    new Dictionary<string, object> { { "key", 1 } },
                        ("PATCH", "{}"),
                        (typeof(KeyBodyController), "Patch",
                        new[] { typeof(int), typeof(Delta<object>) })
                    },
                    // actions that declare extra parameters with registered model binders
                    {
                    new Dictionary<string, object> { { "key", 1 } },
                        ("GET", null),
                        (typeof(ExtraParametersWithOwnModelBindersController), "Get",
                        new[] { typeof(int), typeof(System.Threading.CancellationToken) })
                    },
                    {
                    new Dictionary<string, object> { { "key", 1 } },
                        ("PATCH", "{}"),
                        (typeof(ExtraParametersWithOwnModelBindersController), "Patch",
                        new[] { typeof(int), typeof(System.Threading.CancellationToken), typeof(Delta<object>) })
                    }
                };
            }
        }

        public static TheoryDataSet<Dictionary<string, object>, (string Method, string Body), (Type ControllerType, string Action)> ScenariosWithNoCorrectAction
        {
            get
            {
                return new TheoryDataSet<Dictionary<string, object>, (string, string), (Type, string)>
                {
                    // Action has no param but route data has values
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        ("GET", null),
                        (typeof(NoParamController), "Get")
                    },
                    // Action has parameters not in route values
                    {
                        new Dictionary<string, object>() { { "someValue", 1 } },
                        ("PATCH", null),
                        (typeof(NoParamController), "Patch")
                    },
                    // Action has extra parameters that don't have associated model binders
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        ("GET", null),
                        (typeof(ExtraParametersWithoutModelBindersController), "Get")
                    },
                    {
                        new Dictionary<string, object> { { "key", 1 } },
                        ("PATCH", ""),
                        (typeof(ExtraParametersWithoutModelBindersController), "Patch")
                    }
                };
            }
        }


        [Theory]
        [MemberData(nameof(ScenariosWithCorrectAction))]
        public void SelectBestCandidate_SelectsCorrectly(Dictionary<string, object> routeDataValues,
            (string Method, string Body) requestData,
            (Type ControllerType, string ActionName, Type[] Signature) expectedAction)
        {
            // Arrange
            ODataActionSelectorTestHelper.SetupActionSelector(expectedAction.ControllerType, expectedAction.ActionName,
                out var routeBuilder, out var actionSelector, out var actionDescriptors);
            var routeContext = ODataActionSelectorTestHelper.SetupRouteContext(routeBuilder, expectedAction.ActionName, routeDataValues,
                requestData.Method, requestData.Body);

            // Act
            var action = actionSelector.SelectBestCandidate(routeContext, actionDescriptors);

            // Assert
            var method = expectedAction.ControllerType.GetMethod(expectedAction.ActionName, expectedAction.Signature);
            Assert.NotNull(action);
            Assert.True(ODataActionSelectorTestHelper.ActionMatchesMethod(action, method));
        }

        [Theory]
        [MemberData(nameof(ScenariosWithNoCorrectAction))]
        public void SelectBestCandidate_WhenNoActionWithMatchingParameters_ReturnsNull(
            Dictionary<string, object> routeDataValues,
            (string Method, string Body) requestData,
            (Type ControllerType, string ActionName) actionData)
        {
            // Arrange
            ODataActionSelectorTestHelper.SetupActionSelector(actionData.ControllerType, actionData.ActionName, out var routeBuilder,
                out var actionSelector, out var actionDescriptors);
            var routeContext = ODataActionSelectorTestHelper.SetupRouteContext(routeBuilder, actionData.ActionName, routeDataValues,
                requestData.Method, requestData.Body);

            // Act
            var action = actionSelector.SelectBestCandidate(routeContext, actionDescriptors);

            // Assert
            Assert.True(action == null);
        }
    }

    public class SingleKeyController : TestODataController
    {
        public void Get(int key) { }
        public void Get() { }
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

    public class FromQueryController : TestODataController
    {
        public void Get([FromQuery] string option, ODataQueryOptions queryOptions) { }
        public void Get(int key, [FromQuery] string option) { }
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
        public void Patch(int key, Delta<object> dt) { }
    }

    public class BodyOnlyController : TestODataController
    {
        public void Post(int body) { }
    }

    public class NoParamController : TestODataController
    {
        public void Get() { }
    }

    public class ExtraParametersWithOwnModelBindersController
    {
        public void Get(int key, System.Threading.CancellationToken cancellationToken) { }

        public void Patch(int key, System.Threading.CancellationToken cancellationToken, Delta<object> delta) { }
    }

    public class ExtraParametersWithoutModelBindersController
    {
        public void Get(int key, UnknownModel other) { }
        public void Patch(int key, UnknownModel other, Delta<object> delta) { }
    }

    public class UnknownModel { }
}
