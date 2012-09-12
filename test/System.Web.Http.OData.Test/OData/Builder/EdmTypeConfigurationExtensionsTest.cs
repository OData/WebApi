// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder
{
    public class EdmTypeConfigurationExtensionsTest
    {
        [Fact]
        public void DerivedProperties_ReturnsAllDerivedProperties()
        {
            Mock<IEntityTypeConfiguration> entityA = new Mock<IEntityTypeConfiguration>();
            entityA.Setup(e => e.Properties).Returns(new[] { MockProperty("A1"), MockProperty("A2") });

            Mock<IEntityTypeConfiguration> entityB = new Mock<IEntityTypeConfiguration>();
            entityB.Setup(e => e.Properties).Returns(new[] { MockProperty("B1"), MockProperty("B2") });
            entityB.Setup(e => e.BaseType).Returns(entityA.Object);

            Mock<IEntityTypeConfiguration> entityC = new Mock<IEntityTypeConfiguration>();
            entityC.Setup(e => e.Properties).Returns(new[] { MockProperty("C1"), MockProperty("C2") });
            entityC.Setup(e => e.BaseType).Returns(entityB.Object);

            Assert.Equal(
                new[] { "A1", "A2", "B1", "B2" },
                entityC.Object.DerivedProperties().Select(p => p.Name).OrderBy(s => s));
        }

        private static PropertyConfiguration MockProperty(string name)
        {
            Mock<PropertyInfo> propertyInfo = new Mock<PropertyInfo>();
            propertyInfo.Setup(p => p.Name).Returns(name);

            Mock<PropertyConfiguration> property = new Mock<PropertyConfiguration>(propertyInfo.Object);
            return property.Object;
        }
    }
}
