// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Http.OData.Formatter.Serialization.Models;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query.Validators
{
    public class SelectExpandQueryValidatorTest
    {
        private ODataQueryContext _queryContext;

        public SelectExpandQueryValidatorTest()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            _queryContext = new ODataQueryContext(model.Model, typeof(Customer));
        }

        [Theory]
        [InlineData("Orders/Customer", 1)]
        [InlineData("Orders,Orders/Customer", 1)]
        [InlineData("Orders/Customer/Orders", 2)]
        [InlineData("Orders/Customer/Orders/Customer/Orders/Customer", 5)]
        [InlineData("Orders/NS.SpecialOrder/SpecialCustomer", 1)]
        public void Validate_DepthChecks(string expand, int maxExpansionDepth)
        {
            // Arrange
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
                String.Format(CultureInfo.CurrentCulture, "The request includes a $expand path which is too deep. The maximum depth allowed is {0}. " +
                "To increase the limit, set the 'MaxExpansionDepth' property on EnableQueryAttribute or ODataValidationSettings.", maxExpansionDepth));

            Assert.DoesNotThrow(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }));
        }

        [Fact]
        public void ValidateDoesNotThrow_IfExpansionDepthIsZero()
        {
            string expand = "Orders/Customer/Orders/Customer/Orders/Customer";
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

            Assert.DoesNotThrow(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = 0 }));
        }
    }
}
