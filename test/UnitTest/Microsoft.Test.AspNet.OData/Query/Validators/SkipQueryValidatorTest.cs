// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.TestCommon;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Query.Validators
{
    public class SkipQueryValidatorTest
    {
        private SkipQueryValidator _validator;
        private ODataQueryContext _context;

        public SkipQueryValidatorTest()
        {
            _validator = new SkipQueryValidator();
            _context = ValidationTestHelper.CreateCustomerContext();
        }

        [Fact]
        public void ValidateThrowsOnNullOption()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() =>
                _validator.Validate(null, new ODataValidationSettings()));
        }

        [Fact]
        public void ValidateThrowsOnNullSettings()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() =>
                _validator.Validate(new SkipQueryOption("2", _context), null));
        }

        [Fact]
        public void ValidateThrowsWhenLimitIsExceeded()
        {
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxSkip = 10
            };

            ExceptionAssert.Throws<ODataException>(() =>
                _validator.Validate(new SkipQueryOption("11", _context), settings),
                "The limit of '10' for Skip query has been exceeded. The value from the incoming request is '11'.");
        }

        [Fact]
        public void ValidatePassWhenLimitIsReached()
        {
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxSkip = 10
            };

            ExceptionAssert.DoesNotThrow(() => _validator.Validate(new SkipQueryOption("10", _context), settings));
        }

        [Fact]
        public void ValidatePassWhenLimitIsNotReached()
        {
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxSkip = 10
            };

            ExceptionAssert.DoesNotThrow(() => _validator.Validate(new SkipQueryOption("9", _context), settings));
        }
    }
}