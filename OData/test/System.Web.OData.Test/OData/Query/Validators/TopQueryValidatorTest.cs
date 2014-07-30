// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core;
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
    }
}