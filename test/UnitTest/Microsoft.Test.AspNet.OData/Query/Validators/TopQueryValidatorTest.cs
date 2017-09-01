// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Query.Validators
{
    public class TopQueryValidatorTest
    {
        private TopQueryValidator _validator;
        private ODataQueryContext _context;

        public TopQueryValidatorTest()
        {
            _validator = new TopQueryValidator();
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
                _validator.Validate(new TopQueryOption("2", _context), null));
        }

        [Fact]
        public void ValidateThrowsWhenLimitIsExceeded()
        {
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxTop = 10
            };

            Assert.Throws<ODataException>(() =>
                _validator.Validate(new TopQueryOption("11", _context), settings),
                "The limit of '10' for Top query has been exceeded. The value from the incoming request is '11'.");
        }

        [Fact]
        public void ValidatePassWhenLimitIsReached()
        {
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxTop = 10
            };

            Assert.DoesNotThrow(() => _validator.Validate(new TopQueryOption("10", _context), settings));
        }

        [Fact]
        public void ValidatePassWhenLimitIsNotReached()
        {
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxTop = 10
            };

            Assert.DoesNotThrow(() => _validator.Validate(new TopQueryOption("9", _context), settings));
        }

        [Fact]
        public void ValidatePassWhenQuerySettingsLimitIsNotReached()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxTop = 20
            };
            ModelBoundQuerySettings modelBoundQuerySettings = new ModelBoundQuerySettings();
            modelBoundQuerySettings.MaxTop = 20;
            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
            context.Model.SetAnnotationValue(context.ElementType as IEdmStructuredType, modelBoundQuerySettings);

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(new TopQueryOption("20", context), settings));
        }

        [Fact]
        public void ValidateThrowsWhenQuerySettingsLimitIsExceeded()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxTop = 20
            };
            ModelBoundQuerySettings modelBoundQuerySettings = new ModelBoundQuerySettings();
            modelBoundQuerySettings.MaxTop = 10;
            ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
            context.Model.SetAnnotationValue(context.ElementType as IEdmStructuredType, modelBoundQuerySettings);

            // Act & Assert
            Assert.Throws<ODataException>(() =>
                _validator.Validate(new TopQueryOption("11", context), settings),
                "The limit of '10' for Top query has been exceeded. The value from the incoming request is '11'.");
        }
    }
}