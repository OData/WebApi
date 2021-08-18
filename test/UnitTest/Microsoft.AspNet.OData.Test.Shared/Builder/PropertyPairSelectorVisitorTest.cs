//-----------------------------------------------------------------------------
// <copyright file="PropertyPairSelectorVisitorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class PropertyPairSelectorVisitorTest
    {
        [Fact]
        public void CanGetSingleSelectedPropertyPair()
        {
            // Arrange
            Expression<Func<Dependent, Principal, bool>> expr = (d, p) => d.ForeignKey1 == p.PrincipalKey1;

            // Act
            IDictionary<PropertyInfo, PropertyInfo> properties =
                PropertyPairSelectorVisitor.GetSelectedProperty(expr);

            // Assert
            Assert.Single(properties);
            Assert.Equal("ForeignKey1", properties.Keys.First().Name);
            Assert.Equal("PrincipalKey1", properties.Values.First().Name);
        }

        [Fact]
        public void CanGetMultiSelectedPropertyPairs()
        {
            // Arrange
            Expression<Func<Dependent, Principal, bool>> expr =
                (d, p) => d.ForeignKey1 == p.PrincipalKey1 && d.ForeignKey2 == p.PrincipalKey2;

            // Act
            IDictionary<PropertyInfo, PropertyInfo> properties =
                PropertyPairSelectorVisitor.GetSelectedProperty(expr);

            // Assert
            Assert.Equal(2, properties.Count);

            Assert.Equal("PrincipalKey1", properties[typeof(Dependent).GetProperty("ForeignKey1")].Name);
            Assert.Equal("PrincipalKey2", properties[typeof(Dependent).GetProperty("ForeignKey2")].Name);
        }

        [Fact]
        public void CanGetMultiSelectedPropertyPairs_WithSingleAmpersand()
        {
            // Arrange
            Expression<Func<Dependent, Principal, bool>> expr = (d, p) =>
                    d.ForeignKey1 == p.PrincipalKey1 &&
                    d.ForeignKey2 == p.PrincipalKey2 &
                    d.ForeignKey3 == p.PrincipalKey3;

            // Act
            IDictionary<PropertyInfo, PropertyInfo> properties =
                PropertyPairSelectorVisitor.GetSelectedProperty(expr);

            // Assert
            Assert.Equal(3, properties.Count);

            Assert.Equal("PrincipalKey1", properties[typeof(Dependent).GetProperty("ForeignKey1")].Name);
            Assert.Equal("PrincipalKey2", properties[typeof(Dependent).GetProperty("ForeignKey2")].Name);
            Assert.Equal("PrincipalKey3", properties[typeof(Dependent).GetProperty("ForeignKey3")].Name);
        }

        [Fact]
        public void CanWork_WithNullValue()
        {
            // Arrange
            Expression<Func<Dependent, Principal, bool>> expr = null;

            // Act
            IDictionary<PropertyInfo, PropertyInfo> properties =
                PropertyPairSelectorVisitor.GetSelectedProperty(expr);

            // Assert
            Assert.Equal(0, properties.Count);
        }

        [Fact]
        public void LambdaExpressionAccessesNotProperty_ThrowException()
        {
            // Arrange
            Expression<Func<Dependent, Principal, bool>> expr = (d, p) => d.ForeignKey1 == p.Field;

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => PropertyPairSelectorVisitor.GetSelectedProperty(expr),
                string.Format("Member '{0}.Field' is not a property.", typeof(Principal).FullName));
        }

        [Fact]
        public void LambdaExpressionHasNotTwoParameters_ThrowException()
        {
            // Arrange
            Expression<Func<Dependent, int>> expr = d => d.ForeignKey1;

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => PropertyPairSelectorVisitor.GetSelectedProperty(expr),
                SRResources.LambdaExpressionMustHaveExactlyTwoParameters);
        }

        [Fact]
        public void UnsupportedExpressionNodeType_ThrowException()
        {
            // Arrange
            Expression<Func<Dependent, Principal, bool>> expr = (d, p) => d.ForeignKey1 > p.PrincipalKey1;

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => PropertyPairSelectorVisitor.GetSelectedProperty(expr),
                "Unsupported Expression NodeType 'GreaterThan'.");
        }

        [Fact]
        public void MemberExpressionNotBoundToParameter_ThrowException()
        {
            // Arrange
            Expression<Func<Dependent, Principal, bool>> expr =
                (d, p) => new Dependent().ForeignKey1 == new Principal().PrincipalKey1;

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => PropertyPairSelectorVisitor.GetSelectedProperty(expr),
                "MemberExpressions must be bound to the LambdaExpression parameter.");
        }

        [Fact]
        public void MemberExpressionTypeNotMatch_ThrowException()
        {
            // Arrange
            Expression<Func<Dependent, Principal, bool>> expr =
                (d, p) => d.ForeignKey4 == p.PrincipalKey1;

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => PropertyPairSelectorVisitor.GetSelectedProperty(expr),
                String.Format(SRResources.EqualExpressionsMustHaveSameTypes,
                    typeof(Dependent).FullName, "ForeignKey4", "System.Int16",
                    typeof(Principal).FullName, "PrincipalKey1", "System.Int32"));
        }

        class Principal
        {
            public int PrincipalKey1 { get; set; }

            public string PrincipalKey2 { get; set; }

            public Guid PrincipalKey3 { get; set; }

            public int Field = 1;
        }

        class Dependent
        {
            public int ForeignKey1 { get; set; }

            public string ForeignKey2 { get; set; }

            public Guid ForeignKey3 { get; set; }

            public short ForeignKey4 { get; set; }
        }
    }
}
