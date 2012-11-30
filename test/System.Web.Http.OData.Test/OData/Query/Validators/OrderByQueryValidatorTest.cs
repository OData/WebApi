// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query.Validators
{
    public class OrderByQueryValidatorTest
    {
        private OrderByQueryValidator _validator;
        private ODataConventionModelBuilder _builder;
        private IEdmModel _model;
        private ODataQueryContext _context;

        public OrderByQueryValidatorTest()
        {
            _validator = new OrderByQueryValidator();
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
                _validator.Validate(new OrderByQueryOption("Name eq 'abc'", _context), null));
        }

        [Fact]
        public void ValidateWillNotAllowName()
        {
            // Arrange
            OrderByQueryOption option = new OrderByQueryOption("Name", _context);
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.AllowedOrderByProperties.Add("Id");

            // Act & Assert
             Assert.Throws<ODataException>(() => _validator.Validate(option, settings),
                 "Order by 'Name' is not allowed. To allow it, try setting the 'AllowedOrderByProperties' property on QueryableAttribute or QueryValidationSettings.");
        }

        [Fact]
        public void ValidateWillAllowId()
        {
            // Arrange
            OrderByQueryOption option = new OrderByQueryOption("Id", _context);
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.AllowedOrderByProperties.Add("Id");

            // Act & Assert
            Assert.DoesNotThrow(()=>_validator.Validate(option, settings));
        }
    }
}
