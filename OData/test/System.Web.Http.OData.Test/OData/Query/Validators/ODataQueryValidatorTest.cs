// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Query.Validators
{
    public class ODataQueryValidatorTest
    {
        private ODataQueryValidator _validator;
        private ODataQueryContext _context;

        public ODataQueryValidatorTest()
        {
            _validator = new ODataQueryValidator();
            _context = ValidationTestHelper.CreateCustomerContext();
        }

        [Fact]
        public void ValidateThrowsOnNullOption()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _validator.Validate(null, new ODataValidationSettings()));
        }

        [Fact]
        public void ValidateThrowsOnNullSettings()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _validator.Validate(new ODataQueryOptions(_context, new HttpRequestMessage()), null));
        }

        [Theory]
        [InlineData("filter", "Name eq 'abc'", AllowedQueryOptions.Filter)]
        [InlineData("orderby", "Name", AllowedQueryOptions.OrderBy)]
        [InlineData("skip", "5", AllowedQueryOptions.Skip)]
        [InlineData("top", "5", AllowedQueryOptions.Top)]
        [InlineData("inlinecount", "none", AllowedQueryOptions.InlineCount)]
        [InlineData("select", "Name", AllowedQueryOptions.Select)]
        [InlineData("expand", "Contacts", AllowedQueryOptions.Expand)]
        [InlineData("format", "json", AllowedQueryOptions.Format)]
        [InlineData("skiptoken", "token", AllowedQueryOptions.SkipToken)]
        public void Validate_Throws_ForDisallowedQueryOptions(string queryOptionName, string queryValue, AllowedQueryOptions queryOption)
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://localhost/?$" + queryOptionName + "=" + queryValue)
            );
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.All & ~queryOption
            };

            // Act & Assert
            var exception = Assert.Throws<ODataException>(() => _validator.Validate(option, settings));
            Assert.Equal(
                "Query option '" + queryOptionName + "' is not allowed. To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                exception.Message,
                StringComparer.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("filter", "Name eq 'abc'", AllowedQueryOptions.Filter)]
        [InlineData("orderby", "Name", AllowedQueryOptions.OrderBy)]
        [InlineData("skip", "5", AllowedQueryOptions.Skip)]
        [InlineData("top", "5", AllowedQueryOptions.Top)]
        [InlineData("inlinecount", "none", AllowedQueryOptions.InlineCount)]
        [InlineData("select", "Name", AllowedQueryOptions.Select)]
        [InlineData("expand", "Contacts", AllowedQueryOptions.Expand)]
        [InlineData("format", "json", AllowedQueryOptions.Format)]
        [InlineData("skiptoken", "token", AllowedQueryOptions.SkipToken)]
        public void Validate_DoesNotThrow_ForAllowedQueryOptions(string queryOptionName, string queryValue, AllowedQueryOptions queryOption)
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://localhost/?$" + queryOptionName + "=" + queryValue)
            );
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = queryOption
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Fact]
        public void Validate_ValidatesSelectExpandQueryOption_IfItIsNotNull()
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$expand=Contacts/Contacts");
            ODataQueryOptions option = new ODataQueryOptions(_context, message);

            Mock<SelectExpandQueryValidator> selectExpandValidator = new Mock<SelectExpandQueryValidator>();
            option.SelectExpand.Validator = selectExpandValidator.Object;
            ODataValidationSettings settings = new ODataValidationSettings();

            // Act
            _validator.Validate(option, settings);

            // Assert
            selectExpandValidator.Verify(v => v.Validate(option.SelectExpand, settings), Times.Once());
        }
    }
}
