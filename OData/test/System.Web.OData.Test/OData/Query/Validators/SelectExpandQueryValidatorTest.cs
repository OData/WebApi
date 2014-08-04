// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.OData.Builder;
using System.Web.OData.Formatter.Serialization.Models;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Query.Validators
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
        [InlineData("Orders($expand=Customer)", 1)]
        [InlineData("Orders,Orders($expand=Customer)", 1)]
        [InlineData("Orders($expand=Customer($expand=Orders))", 2)]
        [InlineData("Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))", 5)]
        [InlineData("Orders($expand=NS.SpecialOrder/SpecialCustomer)", 1)]
        public void Validate_DepthChecks(string expand, int maxExpansionDepth)
        {
            // Arrange
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 1;

            // Act & Assert
            Assert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
                String.Format(CultureInfo.CurrentCulture, "The request includes a $expand path which is too deep. The maximum depth allowed is {0}. " +
                "To increase the limit, set the 'MaxExpansionDepth' property on EnableQueryAttribute or ODataValidationSettings.", maxExpansionDepth));

            Assert.DoesNotThrow(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }));
        }

        [Theory]
        [InlineData("Parent($levels=5)", 4)]
        [InlineData("Parent($expand=Parent($levels=4))", 4)]
        [InlineData("Parent($expand=Parent($expand=Parent($levels=0)))", 1)]
        [InlineData("Parent($expand=Parent($levels=4);$levels=5)", 8)]
        [InlineData("Parent($levels=4),DerivedAncestors($levels=5)", 4)]
        [InlineData("DerivedAncestors($levels=5),Parent($levels=4)", 4)]
        public void Validate_DepthChecks_DollarLevels(string expand, int maxExpansionDepth)
        {
            // Arrange
            var validator = new SelectExpandQueryValidator();
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 1;

            // Act & Assert
            Assert.Throws<ODataException>(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
                String.Format(
                    CultureInfo.CurrentCulture,
                    "The request includes a $expand path which is too deep. The maximum depth allowed is {0}. " +
                    "To increase the limit, set the 'MaxExpansionDepth' property on EnableQueryAttribute or ODataValidationSettings.",
                    maxExpansionDepth));

            Assert.DoesNotThrow(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }));
        }

        [Fact]
        public void ValidateDoesNotThrow_IfExpansionDepthIsZero_DollarLevels()
        {
            // Arrange
            string expand = "Parent($expand=Parent($expand=Parent($levels=10)))";
            var validator = new SelectExpandQueryValidator();
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);

            // Act & Assert
            Assert.DoesNotThrow(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = 0 }));
        }

        [Fact]
        public void Validate_Throws_LevelsMaxLiteralExpansionDepthGreaterThanMaxExpansionDepth()
        {
            // Arrange
            string expand = "Parent($levels=2)";
            var validator = new SelectExpandQueryValidator();
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 4;

            // Act & Assert
            Assert.Throws<ODataException>(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = 3 }),
                "'LevelsMaxLiteralExpansionDepth' should be less than or equal to 'MaxExpansionDepth'.");
        }

        [Theory]
        [InlineData(2, 3)]
        [InlineData(4, 4)]
        [InlineData(3, 0)]
        public void ValidateDoesNotThrow_LevelsMaxLiteralExpansionDepthAndMaxExpansionDepth(
            int levelsMaxLiteralExpansionDepth,
            int maxExpansionDepth)
        {
            // Arrange
            string expand = "Parent($levels=2)";
            var validator = new SelectExpandQueryValidator();
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = levelsMaxLiteralExpansionDepth;

            // Act & Assert
            Assert.DoesNotThrow(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }));
        }

        [Fact]
        public void ValidateDoesNotThrow_IfExpansionDepthIsZero()
        {
            string expand = "Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))";
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

            Assert.DoesNotThrow(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = 0 }));
        }

        [Fact]
        public void ValidateThrowException_IfNotNavigable()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            model.Model.SetAnnotationValue(
                model.Customer.FindProperty("Orders"),
                new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotNavigable = true }));

            string select = "Orders";
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(select, null, queryContext);
            Assert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
                "The property 'Orders' cannot be used for navigation.");
        }

        [Theory]
        [InlineData("Customer", "Orders")]
        [InlineData("SpecialCustomer", "SpecialOrders")]
        public void ValidateThrowException_IfBaseOrDerivedClassPropertyNotNavigable(string className, string propertyName)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.SpecialCustomer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            EdmEntityType classType = (className == "Customer") ? model.Customer : model.SpecialCustomer;
            model.Model.SetAnnotationValue(classType.FindProperty(propertyName), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotNavigable = true }));

            string select = "NS.SpecialCustomer/" + propertyName;
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(select, null, queryContext);
            Assert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
                String.Format(CultureInfo.InvariantCulture, "The property '{0}' cannot be used for navigation.", propertyName));
        }

        [Fact]
        public void ValidateThrowException_IfNotExpandable()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            model.Model.SetAnnotationValue(model.Customer.FindProperty("Orders"), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotExpandable = true }));

            string expand = "Orders";
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
            Assert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
                "The property 'Orders' cannot be used in the $expand query option.");
        }

        [Theory]
        [InlineData("Customer", "Orders")]
        [InlineData("SpecialCustomer", "SpecialOrders")]
        public void ValidateThrowException_IfBaseOrDerivedClassPropertyNotExpandable(string className, string propertyName)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.SpecialCustomer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            EdmEntityType classType = (className == "Customer") ? model.Customer : model.SpecialCustomer;
            model.Model.SetAnnotationValue(classType.FindProperty(propertyName), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotExpandable = true }));

            string expand = "NS.SpecialCustomer/" + propertyName;
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
            Assert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
                String.Format(CultureInfo.InvariantCulture, "The property '{0}' cannot be used in the $expand query option.", propertyName));
        }
    }
}
