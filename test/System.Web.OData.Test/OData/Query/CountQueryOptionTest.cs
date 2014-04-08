// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Query
{
    public class CountQueryOptionTest
    {
        private static IEdmModel _model = new ODataModelBuilder()
            .Add_Customer_EntityType()
            .Add_Customers_EntitySet().GetEdmModel();
        private static ODataQueryContext _context = new ODataQueryContext(_model, typeof(Customer));
        private static IQueryable _customers = new List<Customer>()
            {
                new Customer { CustomerId = 1, Name = "Andy" },
                new Customer { CustomerId = 2, Name = "Aaron" },
                new Customer { CustomerId = 3, Name = "Alex" }
            }.AsQueryable();

        [Fact]
        public void Constructor_ThrowsException_IfNullContextArgument()
        {
            Assert.ThrowsArgumentNull(() => new CountQueryOption("false", context: null, queryOptionParser: null),
                "context");
        }

        [Fact]
        public void Constructor_ThrowsException_IfNullRawValueArgument()
        {
            Assert.Throws<ArgumentException>(() => new CountQueryOption(null, _context, null),
                "The argument 'rawValue' is null or empty.\r\nParameter name: rawValue");
        }

        [Fact]
        public void Constructor_ThrowsException_IfEmptyRawValue()
        {
            Assert.Throws<ArgumentException>(() => new CountQueryOption(string.Empty, _context, null),
                "The argument 'rawValue' is null or empty.\r\nParameter name: rawValue");
        }

        [Fact]
        public void Constructor_ThrowsException_IfNullQueryOptionParser()
        {
            Assert.ThrowsArgumentNull(() => new CountQueryOption("false", _context, queryOptionParser: null),
                "queryOptionParser");
        }

        [Fact]
        public void Constructor_CanSetContextProperty()
        {
            // Arrange
            var countOption = new CountQueryOption("test", _context);

            // Act & Assert
            Assert.Same(_context, countOption.Context);
        }

        [Fact]
        public void Constructor_CanSetRawValueProperty()
        {
            // Arrange
            var countOption = new CountQueryOption("test", _context);

            // Act & Assert
            Assert.Same("test", countOption.RawValue);
        }

        [Fact]
        public void Value_ReturnsTrue_IfParseTrueRawValue()
        {
            // Assert
            var countOption = new CountQueryOption("true", _context);

            // Act & Assert
            Assert.True(countOption.Value);
        }

        [Fact]
        public void Value_ReturnsFalse_IfParseFalseRawValue()
        {
            // Assert
            var countOption = new CountQueryOption("false", _context);

            // Act & Assert
            Assert.False(countOption.Value);
        }

        [Theory]
        [InlineData("onions")]
        [InlineData(" ")]
        [InlineData("Trrue")]
        [InlineData("TrUe")]
        [InlineData("TRUE")]
        [InlineData("False")]
        [InlineData("FALSE")]
        public void Value_ThrowsODataException_ForInvalidValues(string countValue)
        {
            // Arrange
            var countOption = new CountQueryOption(countValue, _context);

            // Act & Assert
            Assert.Throws<ODataException>(() => countOption.Value,
                "'" + countValue + "' is not a valid count option.");
        }

        [Fact]
        public void GetEntityCount_ReturnsCount_IfValueIsTrue()
        {
            // Arrange
            var countOption = new CountQueryOption("true", _context);

            // Act & Assert
            Assert.Equal(3, countOption.GetEntityCount(_customers));
        }

        [Fact]
        public void GetEntityCount_ReturnsNull_IfValueIsFalse()
        {
            // Arrange
            var countOption = new CountQueryOption("false", _context);

            // Act & Assert
            Assert.Null(countOption.GetEntityCount(_customers));
        }

        [Fact]
        public void Property_Value_WorksWithUnTypedContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            CountQueryOption countOption = new CountQueryOption("true", context);

            // Act & Assert
            Assert.True(countOption.Value);
        }

        [Fact]
        public void GetEntityCount_WithUnTypedContext_Throws_InvalidOperation()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            CountQueryOption countOption = new CountQueryOption("true", context);
            IQueryable queryable = new Mock<IQueryable>().Object;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => countOption.GetEntityCount(queryable),
                "The query option is not bound to any CLR type. 'GetEntityCount' is only supported with a query option bound to a CLR type.");
        }
    }
}
