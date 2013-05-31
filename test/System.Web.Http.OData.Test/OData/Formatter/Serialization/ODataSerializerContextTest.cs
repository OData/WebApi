// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System.Net.Http;
using System.Reflection;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.Http.OData.Routing.ODataPath;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataSerializerContextTest
    {
        [Fact]
        public void EmptyCtor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new ODataSerializerContext());
        }

        [Fact]
        public void CopyCtor_ThrowsArgumentNull_Context()
        {
            Assert.ThrowsArgumentNull(() => new ODataSerializerContext(context: null), "context");
        }

        [Fact]
        public void CopyCtor_CopiesAllTheProperties()
        {
            // Arrange
            ODataSerializerContext context = new ODataSerializerContext
            {
                EntitySet = new Mock<IEdmEntitySet>().Object,
                IsNested = true,
                MetadataLevel = ODataMetadataLevel.FullMetadata,
                Model = new Mock<IEdmModel>().Object,
                Path = new ODataPath(),
                Request = new HttpRequestMessage(),
                RootElementName = "somename",
                SelectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true),
                SkipExpensiveAvailabilityChecks = true,
                Url = new UrlHelper()
            };

            // Act
            ODataSerializerContext result = new ODataSerializerContext(context);

            // Assert
            // Check each and every property. This test should fail if someone accidentally adds a new property and 
            // forgets to update the copy constructor.
            foreach (PropertyInfo property in typeof(ODataSerializerContext).GetProperties())
            {
                if (property.PropertyType.IsValueType)
                {
                    Assert.Equal(property.GetValue(context), property.GetValue(result));
                }
                else
                {
                    Assert.Same(property.GetValue(context), property.GetValue(result));
                }
            }
        }
    }
}
