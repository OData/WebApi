// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query.Validators
{
    public class ODataQueryValidatorTest
    {
        private ODataQueryValidator _validator;
        private ODataConventionModelBuilder _builder;
        private IEdmModel _model;
        private ODataQueryContext _context;

        public ODataQueryValidatorTest()
        {
            _validator = new ODataQueryValidator();
            _builder = new ODataConventionModelBuilder();
            _builder.EntitySet<QueryCompositionCustomer>("Customer");
            _model = _builder.GetEdmModel();
            _context = new ODataQueryContext(_model, typeof(QueryCompositionCustomer));
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
        public void ValidateDisallowFilter()
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://localhost/?$filter=Name eq 'abc'")
            );
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.OrderBy
            };

            // Act & Assert
             Assert.Throws<ODataException>(() => _validator.Validate(option, settings),
                 "Query option 'Filter' is not allowed. To allow it, try setting the 'AllowedQueryOptions' property on QueryableAttribute or QueryValidationSettings.");
        }

        [Fact]
        public void ValidateAllowOrderBy()
        {
            // Arrange
            HttpRequestMessage message = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("http://localhost/?$orderby=Name")
            );
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.OrderBy
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(option, settings));
        }
    }
}
