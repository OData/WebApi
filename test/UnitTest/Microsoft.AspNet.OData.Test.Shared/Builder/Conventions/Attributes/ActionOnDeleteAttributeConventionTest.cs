// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions.Attributes;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Builder.Conventions.Attributes
{
    public class ActionOnDeleteAttributeConventionTest
    {
        [Fact]
        public void Empty_Ctor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new ActionOnDeleteAttributeConvention());
        }

        [Fact]
        public void Apply_ActionOnDeleteAttribute_Works()
        {
            // Arrange
            Type orderType = typeof(TestOrder);
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            PropertyInfo propertyInfo = orderType.GetProperty("Customer");
            EntityTypeConfiguration entity = builder.AddEntityType(orderType);
            NavigationPropertyConfiguration navProperty = entity.AddNavigationProperty(propertyInfo, EdmMultiplicity.One);
            navProperty.AddedExplicitly = false;
            navProperty.HasConstraint(orderType.GetProperty("CustomerId"),
                typeof(TestCustomer).GetProperty("Id"));

            // Act
            new ActionOnDeleteAttributeConvention().Apply(navProperty, entity, builder);

            // Assert
            Assert.Equal(EdmOnDeleteAction.Cascade, navProperty.OnDeleteAction);
        }

        [Fact]
        public void Apply_DoesnotModifiy_ExplicitlyAddedAction()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            PropertyInfo propertyInfo = typeof(TestOrder).GetProperty("Customer");
            EntityTypeConfiguration entity = builder.AddEntityType(typeof(TestOrder));
            NavigationPropertyConfiguration navProperty = entity.AddNavigationProperty(propertyInfo, EdmMultiplicity.One);
            navProperty.OnDeleteAction = EdmOnDeleteAction.None;

            // Act
            new ActionOnDeleteAttributeConvention().Apply(navProperty, entity, builder);

            // Assert
            Assert.Equal(EdmOnDeleteAction.None, navProperty.OnDeleteAction);
        }

        class TestCustomer
        {
            public int Id { get; set; }
        }

        class TestOrder
        {
            public int CustomerId { get; set; }

            [ActionOnDelete(EdmOnDeleteAction.Cascade)]
            public TestCustomer Customer { get; set; }
        }
    }
}
