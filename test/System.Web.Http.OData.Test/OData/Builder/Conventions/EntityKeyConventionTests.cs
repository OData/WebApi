// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class EntityKeyConventionTests
    {
        [Fact]
        public void Apply_Calls_HasKey_OnEdmType()
        {
            // Arrange
            var mockEntityType = new Mock<IEntityTypeConfiguration>(MockBehavior.Strict);
            mockEntityType
                .Setup(edmType => edmType.ClrType)
                .Returns(typeof(EntityKeyConventionTests_EntityType));
            mockEntityType
                .Setup(edmType => edmType.IgnoredProperties)
                .Returns(Enumerable.Empty<PropertyInfo>());
            mockEntityType.Setup(entityType => entityType.HasKey(typeof(EntityKeyConventionTests_EntityType).GetProperty("ID"))).Returns(mockEntityType.Object);

            var mockModelBuilder = new Mock<ODataModelBuilder>(MockBehavior.Strict);

            // Act
            new EntityKeyConvention().Apply(mockEntityType.Object, mockModelBuilder.Object);

            // Assert
            mockEntityType.Verify();
        }

        class EntityKeyConventionTests_EntityType
        {
            public string ID { get; set; }
        }
    }
}
