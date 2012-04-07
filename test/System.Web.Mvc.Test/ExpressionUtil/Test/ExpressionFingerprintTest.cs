// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Xunit;

namespace System.Web.Mvc.ExpressionUtil.Test
{
    public class ExpressionFingerprintTest
    {
        [Fact]
        public void Comparison_Equality()
        {
            // Act
            DummyExpressionFingerprint fingerprint1 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(object));
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(object));

            // Assert
            Assert.Equal(fingerprint1, fingerprint2);
            Assert.Equal(fingerprint1.GetHashCode(), fingerprint2.GetHashCode());
        }

        [Fact]
        public void Comparison_Inequality_NodeType()
        {
            // Act
            DummyExpressionFingerprint fingerprint1 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(object));
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(ExpressionType.Parameter, typeof(object));

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }

        [Fact]
        public void Comparison_Inequality_Type()
        {
            // Act
            DummyExpressionFingerprint fingerprint1 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(object));
            DummyExpressionFingerprint fingerprint2 = new DummyExpressionFingerprint(ExpressionType.Default, typeof(string));

            // Assert
            Assert.NotEqual(fingerprint1, fingerprint2);
        }
    }
}
