// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter.Serialization.Models;
using Microsoft.AspNet.OData.Test.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Validators
{
    public class SelectExpandQueryValidatorTest
    {
        private ODataQueryContext _queryContext;

        public const string MaxExpandDepthExceededErrorString =
            "The request includes a $expand path which is too deep. The maximum depth allowed is {0}. " +
            "To increase the limit, set the 'MaxExpansionDepth' property on EnableQueryAttribute or ODataValidationSettings, or set the 'MaxDepth' property in ExpandAttribute.";

        public SelectExpandQueryValidatorTest()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            _queryContext = new ODataQueryContext(model.Model, typeof(Customer), null);
            _queryContext.RequestContainer = new MockContainer();
            _queryContext.DefaultQuerySettings.EnableExpand = true;
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
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator(_queryContext.DefaultQuerySettings);
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 1;

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
                String.Format(CultureInfo.CurrentCulture, MaxExpandDepthExceededErrorString, maxExpansionDepth));

            ExceptionAssert.DoesNotThrow(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }));
        }

        [Theory]
        [InlineData("Orders($expand=Customer)", 1)]
        [InlineData("Orders,Orders($expand=Customer)", 1)]
        [InlineData("Orders($expand=Customer($expand=Orders))", 2)]
        [InlineData("Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))", 5)]
        [InlineData("Orders($expand=NS.SpecialOrder/SpecialCustomer)", 1)]
        public void Validate_DepthChecks_QuerySettings(string expand, int maxExpansionDepth)
        {
            // Arrange
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator(new DefaultQuerySettings { EnableExpand = true });
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            queryContext.RequestContainer = new MockContainer();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 1;
            IEdmStructuredType customerType =
                model.Model.SchemaElements.First(e => e.Name.Equals("Customer")) as IEdmStructuredType;
            ModelBoundQuerySettings querySettings = new ModelBoundQuerySettings();
            querySettings.ExpandConfigurations.Add("Orders", new ExpandConfiguration
            {
                ExpandType = SelectExpandType.Allowed,
                MaxDepth = maxExpansionDepth
            });
            model.Model.SetAnnotationValue(customerType, querySettings);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }),
                String.Format(CultureInfo.CurrentCulture, MaxExpandDepthExceededErrorString, maxExpansionDepth));
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
            var validator = new SelectExpandQueryValidator(new DefaultQuerySettings {EnableExpand = true});
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            context.RequestContainer = new MockContainer();
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 1;

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
                String.Format(
                    CultureInfo.CurrentCulture,
                    MaxExpandDepthExceededErrorString,
                    maxExpansionDepth));

            ExceptionAssert.DoesNotThrow(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth + 1 }));
        }

        [Fact]
        public void ValidateDoesNotThrow_IfExpansionDepthIsZero_DollarLevels()
        {
            // Arrange
            string expand = "Parent($expand=Parent($expand=Parent($levels=10)))";
            var validator = new SelectExpandQueryValidator(new DefaultQuerySettings { EnableExpand = true });
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            context.RequestContainer = new MockContainer();
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = 0 }));
        }

        [Fact]
        public void Validate_Throws_LevelsMaxLiteralExpansionDepthGreaterThanMaxExpansionDepth()
        {
            // Arrange
            string expand = "Parent($levels=2)";
            var validator = new SelectExpandQueryValidator(new DefaultQuerySettings { EnableExpand = true });
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            context.RequestContainer = new MockContainer();
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = 4;

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = 3 }),
                "'LevelsMaxLiteralExpansionDepth' should be less than or equal to 'MaxExpansionDepth'.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Validate_DoesNotThrow_DefaultLevelsMaxLiteralExpansionDepth(int maxExpansionDepth)
        {
            // Arrange
            string expand = "Parent($levels=1)";
            var validator = new SelectExpandQueryValidator(new DefaultQuerySettings { EnableExpand = true });
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            context.RequestContainer = new MockContainer();
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }));
        }

        [Fact]
        public void Validate_Throw_WithInvalidMaxExpansionDepth()
        {
            int maxExpansionDepth = -1;
            // Arrange
            string expand = "Parent($levels=1)";
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            context.RequestContainer = new MockContainer();
            var validator = SelectExpandQueryValidator.GetSelectExpandQueryValidator(context);
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);

            // Act & Assert
#if NETCOREAPP3_0
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
                "Value must be greater than or equal to 0. (Parameter 'value')\r\nActual value was -1.");
#else
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }),
                "Value must be greater than or equal to 0.\r\nParameter name: value\r\nActual value was -1.");
#endif
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
            var validator = new SelectExpandQueryValidator(new DefaultQuerySettings { EnableExpand = true });
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<ODataLevelsTest.LevelsEntity>("Entities");
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(ODataLevelsTest.LevelsEntity));
            context.RequestContainer = new MockContainer();
            var selectExpandQueryOption = new SelectExpandQueryOption(null, expand, context);
            selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = levelsMaxLiteralExpansionDepth;

            // Act & Assert
            ExceptionAssert.DoesNotThrow(
                () => validator.Validate(
                    selectExpandQueryOption,
                    new ODataValidationSettings { MaxExpansionDepth = maxExpansionDepth }));
        }

        [Fact]
        public void ValidateDoesNotThrow_IfExpansionDepthIsZero()
        {
            string expand = "Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))";
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator(new DefaultQuerySettings { EnableExpand = true });
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, _queryContext);

            ExceptionAssert.DoesNotThrow(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = 0 }));
        }

        [Fact]
        public void ValidateDoesNotThrow_IfExpansionDepthIsZero_QuerySettings()
        {
            // Arrange
            string expand =
                "Orders($expand=Customer($expand=Orders($expand=Customer($expand=Orders($expand=Customer)))))";
            SelectExpandQueryValidator validator = new SelectExpandQueryValidator(new DefaultQuerySettings { EnableExpand = true });
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            queryContext.RequestContainer = new MockContainer();
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
            IEdmStructuredType customerType =
                model.Model.SchemaElements.First(e => e.Name.Equals("Customer")) as IEdmStructuredType;
            ModelBoundQuerySettings querySettings = new ModelBoundQuerySettings();
            querySettings.ExpandConfigurations.Add("Orders", new ExpandConfiguration
            {
                ExpandType = SelectExpandType.Allowed,
                MaxDepth = 0
            });
            model.Model.SetAnnotationValue(customerType, querySettings);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings { MaxExpansionDepth = 0 }));
        }

        [Fact]
        public void ValidateThrowException_IfNotNavigable()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            queryContext.RequestContainer = new MockContainer();
            model.Model.SetAnnotationValue(
                model.Customer.FindProperty("Orders"),
                new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotNavigable = true }));

            string select = "Orders";
            SelectExpandQueryValidator validator = SelectExpandQueryValidator.GetSelectExpandQueryValidator(queryContext);
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(select, null, queryContext);
            ExceptionAssert.Throws<ODataException>(
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
            queryContext.RequestContainer = new MockContainer();
            EdmEntityType classType = (className == "Customer") ? model.Customer : model.SpecialCustomer;
            model.Model.SetAnnotationValue(classType.FindProperty(propertyName), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotNavigable = true }));

            string select = "NS.SpecialCustomer/" + propertyName;
            SelectExpandQueryValidator validator = SelectExpandQueryValidator.GetSelectExpandQueryValidator(queryContext);
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(select, null, queryContext);
            ExceptionAssert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
                String.Format(CultureInfo.InvariantCulture, "The property '{0}' cannot be used for navigation.", propertyName));
        }

        [Fact]
        public void ValidateThrowException_IfNotExpandable()
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            queryContext.RequestContainer = new MockContainer();
            model.Model.SetAnnotationValue(model.Customer.FindProperty("Orders"), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotExpandable = true }));

            string expand = "Orders";
            SelectExpandQueryValidator validator = SelectExpandQueryValidator.GetSelectExpandQueryValidator(queryContext);
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
            ExceptionAssert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
                "The property 'Orders' cannot be used in the $expand query option.");
        }

        [Fact]
        public void ValidateThrowException_IfNotExpandable_QuerySettings()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, typeof(Customer));
            queryContext.RequestContainer = new MockContainer();
            SelectExpandQueryValidator validator = SelectExpandQueryValidator.GetSelectExpandQueryValidator(queryContext);
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, "Orders", queryContext);
            IEdmStructuredType customerType =
                model.Model.SchemaElements.First(e => e.Name.Equals("Customer")) as IEdmStructuredType;
            ModelBoundQuerySettings querySettings = new ModelBoundQuerySettings();
            querySettings.ExpandConfigurations.Add("Orders", new ExpandConfiguration
            {
                ExpandType = SelectExpandType.Disabled,
                MaxDepth = 0
            });
            model.Model.SetAnnotationValue(customerType, querySettings);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
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
            queryContext.RequestContainer = new MockContainer();
            EdmEntityType classType = (className == "Customer") ? model.Customer : model.SpecialCustomer;
            model.Model.SetAnnotationValue(classType.FindProperty(propertyName), new QueryableRestrictionsAnnotation(new QueryableRestrictions { NotExpandable = true }));

            string expand = "NS.SpecialCustomer/" + propertyName;
            SelectExpandQueryValidator validator = SelectExpandQueryValidator.GetSelectExpandQueryValidator(queryContext);
            SelectExpandQueryOption selectExpandQueryOption = new SelectExpandQueryOption(null, expand, queryContext);
            ExceptionAssert.Throws<ODataException>(
                () => validator.Validate(selectExpandQueryOption, new ODataValidationSettings()),
                String.Format(CultureInfo.InvariantCulture, "The property '{0}' cannot be used in the $expand query option.", propertyName));
        }
    }
}
