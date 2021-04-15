// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Query;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class QueryHelpersTest
    {
        [Fact]
        public void SingleOrDefault_IQueryableOfT_OneElementInSequence_ReturnsElement()
        {
            Customer customer = new Customer();
            IQueryable<Customer> queryable = new[] { customer }.AsQueryable();
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;
            var result = QueryHelpers.SingleOrDefault(queryable, new WebApiActionDescriptor(actionDescriptor));

            Assert.Same(customer, result);
        }

        [Fact]
        public void SingleOrDefault_IQueryableOfT_ZeroElementsInSequence_ReturnsNull()
        {
            IQueryable<Customer> queryable = Enumerable.Empty<Customer>().AsQueryable();
            HttpActionDescriptor actionDescriptor = new Mock<HttpActionDescriptor>().Object;

            var result = QueryHelpers.SingleOrDefault(queryable, new WebApiActionDescriptor(actionDescriptor));

            Assert.Null(result);
        }

        [Fact]
        public void SingleOrDefault_IQueryableOfT_MoreThaneOneElementInSequence_Throws()
        {
            IQueryable<Customer> queryable = new[] { new Customer(), new Customer() }.AsQueryable();
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            ExceptionAssert.Throws<InvalidOperationException>(
                () => QueryHelpers.SingleOrDefault(queryable, new WebApiActionDescriptor(actionDescriptor)),
                "The action 'SomeAction' on controller 'SomeName' returned a SingleResult containing more than one element. " +
                "SingleResult must have zero or one elements.");
        }

        [Fact]
        public void SingleOrDefault_DisposeCalled_EmptySequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.Setup(mock => mock.MoveNext()).Returns(false);

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act
            QueryHelpers.SingleOrDefault(queryable.Object, new WebApiActionDescriptor(actionDescriptor));

            // Assert
            disposable.Verify();
        }

        [Fact]
        public void SingleOrDefault_DisposeCalled_OneElementInSequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.SetupSequence(mock => mock.MoveNext()).Returns(true).Returns(false);
            enumerator.SetupGet(mock => mock.Current).Returns(new Customer());

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act
            QueryHelpers.SingleOrDefault(queryable.Object, new WebApiActionDescriptor(actionDescriptor));

            // Assert
            disposable.Verify();
        }

        [Fact]
        public void SingleOrDefault_DisposeCalled_MultipleElementsInSequence()
        {
            // Arrange
            var enumerator = new Mock<IEnumerator>(MockBehavior.Strict);
            enumerator.Setup(mock => mock.MoveNext()).Returns(true);
            enumerator.SetupGet(mock => mock.Current).Returns(new Customer());

            var disposable = enumerator.As<IDisposable>();
            disposable.Setup(mock => mock.Dispose()).Verifiable();

            var queryable = new Mock<IQueryable>(MockBehavior.Strict);
            queryable.Setup(mock => mock.GetEnumerator()).Returns(enumerator.Object);

            var actionDescriptor = new ReflectedHttpActionDescriptor
            {
                Configuration = new HttpConfiguration(),
                MethodInfo = GetType().GetMethod("SomeAction", BindingFlags.Instance | BindingFlags.NonPublic),
                ControllerDescriptor = new HttpControllerDescriptor { ControllerName = "SomeName" }
            };

            // Act (will throw)
            try
            {
                QueryHelpers.SingleOrDefault(queryable.Object, new WebApiActionDescriptor(actionDescriptor));
            }
            catch
            {
                // Other tests confirm the Exception.
            }

            // Assert
            disposable.Verify();
        }
    }
}
#endif
