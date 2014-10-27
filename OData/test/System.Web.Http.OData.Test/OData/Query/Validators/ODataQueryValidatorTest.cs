// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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

        public static TheoryDataSet<AllowedQueryOptions, string, string> SupportedQueryOptions
        {
            get
            {
                return new TheoryDataSet<AllowedQueryOptions, string, string>
                {
                    { AllowedQueryOptions.Expand, "$expand=Contacts", "Expand" },
                    { AllowedQueryOptions.Filter, "$filter=Name eq 'Name'", "Filter" },
                    { AllowedQueryOptions.InlineCount, "$inlinecount=allpages", "InlineCount" },
                    { AllowedQueryOptions.OrderBy, "$orderby=Name", "OrderBy" },
                    { AllowedQueryOptions.Select, "$select=Name", "Select" },
                    { AllowedQueryOptions.Skip, "$skip=5", "Skip" },
                    { AllowedQueryOptions.Top, "$top=10", "Top" },
                };
            }
        }

        public static TheoryDataSet<AllowedQueryOptions, string, string> UnsupportedQueryOptions
        {
            get
            {
                return new TheoryDataSet<AllowedQueryOptions, string, string>
                {
                    { AllowedQueryOptions.Format, "$format=json", "Format" },
                    { AllowedQueryOptions.SkipToken, "$skiptoken=__skip__", "SkipToken" },
                };
            }
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

        [Fact]
        public void QueryOptionDataSets_CoverAllValues()
        {
            // Arrange
            // Get all values in the AllowedQueryOptions enum.
            var values = new HashSet<AllowedQueryOptions>(
                Enum.GetValues(typeof(AllowedQueryOptions)).Cast<AllowedQueryOptions>());

            var groupValues = new[]
            {
                AllowedQueryOptions.All,
                AllowedQueryOptions.None,
                AllowedQueryOptions.Supported,
            };
            var dataSets = SupportedQueryOptions.Concat(UnsupportedQueryOptions);

            // Act
            // Remove the group items.
            foreach (var allowed in groupValues)
            {
                values.Remove(allowed);
            }

            // Remove the individual items.
            foreach (var allowed in dataSets.Select(item => (AllowedQueryOptions)(item[0])))
            {
                values.Remove(allowed);
            }

            // Assert
            // Should have nothing left.
            Assert.Empty(values);
        }

        [Theory]
        [PropertyData("SupportedQueryOptions")]
        [PropertyData("UnsupportedQueryOptions")]
        public void AllowedQueryOptions_SucceedIfAllowed(AllowedQueryOptions allow, string query, string unused)
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/?$" + query));
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = allow,
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [PropertyData("SupportedQueryOptions")]
        [PropertyData("UnsupportedQueryOptions")]
        public void AllowedQueryOptions_ThrowIfNotAllowed(AllowedQueryOptions exclude, string query, string optionName)
        {
            // Arrange
            var message = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/?" + query));
            var option = new ODataQueryOptions(_context, message);
            var expectedMessage = string.Format(
                "Query option '{0}' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                optionName);
            var settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.All & ~exclude,
            };

            // Act & Assert
            Assert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [PropertyData("SupportedQueryOptions")]
        [PropertyData("UnsupportedQueryOptions")]
        public void AllowedQueryOptions_ThrowIfNoneAllowed(AllowedQueryOptions unused, string query, string optionName)
        {
            // Arrange
            var message = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/?" + query));
            var option = new ODataQueryOptions(_context, message);
            var expectedMessage = string.Format(
                "Query option '{0}' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                optionName);
            var settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.None,
            };

            // Act & Assert
            Assert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [PropertyData("SupportedQueryOptions")]
        public void SupportedQueryOptions_SucceedIfGroupAllowed(AllowedQueryOptions unused, string query, string unusedName)
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/?$" + query));
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.Supported,
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [PropertyData("SupportedQueryOptions")]
        public void SupportedQueryOptions_ThrowIfGroupNotAllowed(AllowedQueryOptions unused, string query, string optionName)
        {
            // Arrange
            var message = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/?" + query));
            var option = new ODataQueryOptions(_context, message);
            var expectedMessage = string.Format(
                "Query option '{0}' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                optionName);
            var settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Supported,
            };

            // Act & Assert
            Assert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [PropertyData("UnsupportedQueryOptions")]
        public void UnsupportedQueryOptions_SucceedIfGroupAllowed(AllowedQueryOptions unused, string query, string unusedName)
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/?$" + query));
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Supported,
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [PropertyData("UnsupportedQueryOptions")]
        public void UnsupportedQueryOptions_ThrowIfGroupNotAllowed(AllowedQueryOptions unused, string query, string optionName)
        {
            // Arrange
            var message = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/?" + query));
            var option = new ODataQueryOptions(_context, message);
            var expectedMessage = string.Format(
                "Query option '{0}' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                optionName);
            var settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.Supported,
            };

            // Act & Assert
            Assert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
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
