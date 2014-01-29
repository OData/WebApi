// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Query.Expressions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query.Validators
{
    public class FilterQueryValidatorTest
    {
        private MyFilterValidator _validator;
        private ODataValidationSettings _settings = new ODataValidationSettings();
        private ODataQueryContext _context;
        private ODataQueryContext _productContext;

        public static TheoryDataSet<string> LongInputs
        {
            get
            {
                return GetLongInputsTestData(100);
            }
        }

        public static TheoryDataSet<string> CloseToLongInputs
        {
            get
            {
                return GetLongInputsTestData(95);
            }
        }

        public static TheoryDataSet<string> NestedAnyAllInputs
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "Category/QueryableProducts/any(P: P/Category/EnumerableProducts/any(PP: PP/ProductName eq 'Snacks'))",
                    "Category/QueryableProducts/all(P: P/Category/EnumerableProducts/all(PP: PP/ProductName eq 'Snacks'))",
                    "Category/QueryableProducts/any(P: P/Category/EnumerableProducts/all(PP: PP/ProductName eq 'Snacks'))",
                    "Category/QueryableProducts/all(P: P/Category/EnumerableProducts/any(PP: PP/ProductName eq 'Snacks'))",
                };
            }
        }

        public FilterQueryValidatorTest()
        {
            _validator = new MyFilterValidator();
            _context = ValidationTestHelper.CreateCustomerContext();
            _productContext = ValidationTestHelper.CreateProductContext();
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
                _validator.Validate(new FilterQueryOption("Name eq 'abc'", _context), null));
        }

        [Fact]
        public void ValidateThrowsIfSubStringIsNotAllowed()
        {
            Assert.DoesNotThrow(() =>
                _validator.Validate(new FilterQueryOption("substring(Name,8,1) eq '7'", _context),
                new ODataValidationSettings() { AllowedFunctions = AllowedFunctions.Substring }));

            Assert.Throws<ODataException>(() =>
                 _validator.Validate(new FilterQueryOption("substring(Name,8,1) eq '7'", _context),
                new ODataValidationSettings() { AllowedFunctions = AllowedFunctions.AllMathFunctions }),
                "Function 'substring' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");
        }

        [Fact]
        public void ValidateThrowsIfNotIsNotAllowed()
        {
            Assert.DoesNotThrow(() =>
                _validator.Validate(new FilterQueryOption("not (Name eq 'David')", _context),
                new ODataValidationSettings() { AllowedLogicalOperators = AllowedLogicalOperators.Not | AllowedLogicalOperators.Equal }));

            Assert.Throws<ODataException>(() =>
                   _validator.Validate(new FilterQueryOption("not (Name eq 'David')", _context),
                 new ODataValidationSettings() { AllowedLogicalOperators = AllowedLogicalOperators.Equal }),
                "Logical operator 'Not' is not allowed. To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.");
        }

        [Fact]
        public void ValidateThrowsIfModIsNotAllowed()
        {
            Assert.DoesNotThrow(() =>
                _validator.Validate(new FilterQueryOption("Id mod 2 eq 0", _context),
                new ODataValidationSettings() { AllowedArithmeticOperators = AllowedArithmeticOperators.All }));

            Assert.Throws<ODataException>(() =>
                   _validator.Validate(new FilterQueryOption("Id mod 2 eq 0", _context),
                 new ODataValidationSettings() { AllowedArithmeticOperators = AllowedArithmeticOperators.Add }),
                "Arithmetic operator 'Modulo' is not allowed. To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.");
        }

        // want to test if all the virtual methods are being invoked correctly
        [Fact]
        public void ValidateVisitAll()
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption("Tags/all(t: t eq '42')", _context);

            // Act
            _validator.Validate(option, _settings);

            // Assert
            Assert.Equal(7, _validator.Times.Keys.Count);
            Assert.Equal(1, _validator.Times["Validate"]); // entry
            Assert.Equal(1, _validator.Times["ValidateAllQueryNode"]); // all
            Assert.Equal(1, _validator.Times["ValidateLogicalOperator"]); // eq
            Assert.Equal(1, _validator.Times["ValidateCollectionPropertyAccessNode"]); // Tags
            Assert.Equal(1, _validator.Times["ValidateConstantQueryNode"]); // 42
            Assert.Equal(1, _validator.Times["ValidateBinaryOperatorQueryNode"]); // eq
            Assert.Equal(2, _validator.Times["ValidateParameterQueryNode"]); // $it, t
        }

        [Fact]
        public void ValidateVisitAny()
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption("Tags/any(t: t eq '42')", _context);

            // Act
            _validator.Validate(option, _settings);

            // Assert
            Assert.Equal(7, _validator.Times.Keys.Count);
            Assert.Equal(1, _validator.Times["Validate"]); // entry
            Assert.Equal(1, _validator.Times["ValidateAnyQueryNode"]); // all
            Assert.Equal(1, _validator.Times["ValidateLogicalOperator"]); // eq
            Assert.Equal(1, _validator.Times["ValidateCollectionPropertyAccessNode"]); // Tags
            Assert.Equal(1, _validator.Times["ValidateConstantQueryNode"]); // 42
            Assert.Equal(1, _validator.Times["ValidateBinaryOperatorQueryNode"]); // eq
            Assert.Equal(2, _validator.Times["ValidateParameterQueryNode"]); // $it, t
        }

        [Theory]
        [PropertyData("NestedAnyAllInputs")]
        public void MaxAnyAllExpressionDepthLimitExceeded(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.MaxAnyAllExpressionDepth = 1;

            // Act & Assert
            Assert.Throws<ODataException>(() => _validator.Validate(new FilterQueryOption(filter, _productContext), settings), "The Any/All nesting limit of '1' has been exceeded. 'MaxAnyAllExpressionDepth' can be configured on ODataQuerySettings or EnableQueryAttribute.");
        }

        [Theory]
        [PropertyData("NestedAnyAllInputs")]
        public void IncreaseMaxAnyAllExpressionDepthWillAllowNestedAnyAllInputs(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings();
            settings.MaxAnyAllExpressionDepth = 2;

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(new FilterQueryOption(filter, _productContext), settings));
        }

        [Theory]
        [PropertyData("LongInputs")]
        public void LongInputs_CauseMaxNodeCountExceededException(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                MaxAnyAllExpressionDepth = Int32.MaxValue
            };

            FilterQueryOption option = new FilterQueryOption(filter, _productContext);

            // Act & Assert
            Assert.Throws<ODataException>(() => _validator.Validate(option, settings), "The node count limit of '100' has been exceeded. To increase the limit, set the 'MaxNodeCount' property on EnableQueryAttribute or ODataValidationSettings.");
        }

        [Theory]
        [PropertyData("LongInputs")]
        public void IncreaseMaxNodeCountWillAllowLongInputs(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                MaxAnyAllExpressionDepth = Int32.MaxValue,
                MaxNodeCount = 105,
            };

            FilterQueryOption option = new FilterQueryOption(filter, _productContext);

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [PropertyData("CloseToLongInputs")]
        public void AlmostLongInputs_DonotCauseMaxNodeCountExceededExceptionOrTimeoutDuringCompilation(string filter)
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                MaxAnyAllExpressionDepth = Int32.MaxValue
            };

            FilterQueryOption option = new FilterQueryOption(filter, _productContext);

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(option, settings));
            Assert.DoesNotThrow(() => option.ApplyTo(new List<Product>().AsQueryable(), new ODataQuerySettings()));
        }

        [Fact]
        public void AllowedArithmeticOperators_ThrowsOnNotAllowedOperators()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~AllowedArithmeticOperators.Modulo
            };

            FilterQueryOption option = new FilterQueryOption("ProductID mod 2 eq 0", _productContext);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _validator.Validate(option, settings),
                "Arithmetic operator 'Modulo' is not allowed. To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.");
        }

        [Fact]
        public void AllowedFunctions_ThrowsOnNotAllowedFunctions()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                AllowedFunctions = AllowedFunctions.All & ~AllowedFunctions.Length
            };

            FilterQueryOption option = new FilterQueryOption("length(ProductName) eq 6", _productContext);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _validator.Validate(option, settings),
                "Function 'length' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");

        }

        [Fact]
        public void AllowedLogicalOperators_ThrowsOnNotAllowedOperators()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings
            {
                AllowedLogicalOperators = AllowedLogicalOperators.All & ~AllowedLogicalOperators.NotEqual
            };

            FilterQueryOption option = new FilterQueryOption("length(ProductName) ne 6", _productContext);

            // Act & Assert
            Assert.Throws<ODataException>(
                () => _validator.Validate(option, settings),
                "Logical operator 'NotEqual' is not allowed. To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.");
        }

        [Fact]
        public void ValidateVisitLogicalOperatorEqual()
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption("Id eq 1", _context);

            // Act
            _validator.Validate(option, _settings);

            // Assert
            Assert.Equal(6, _validator.Times.Keys.Count);
            Assert.Equal(1, _validator.Times["Validate"]); // entry
            Assert.Equal(1, _validator.Times["ValidateSingleValuePropertyAccessNode"]); // Id
            Assert.Equal(1, _validator.Times["ValidateLogicalOperator"]); // eq
            Assert.Equal(1, _validator.Times["ValidateConstantQueryNode"]); // 1
            Assert.Equal(1, _validator.Times["ValidateBinaryOperatorQueryNode"]); // eq
            Assert.Equal(1, _validator.Times["ValidateParameterQueryNode"]); // $it
        }

        [Theory]
        [InlineData("Id eq 1")]
        [InlineData("Id ne 1")]
        [InlineData("Id gt 1")]
        [InlineData("Id lt 1")]
        [InlineData("Id ge 1")]
        [InlineData("Id le 1")]
        [InlineData("Id eq Id add 1")]
        [InlineData("Id eq Id sub 1")]
        [InlineData("Id eq Id mul 1")]
        [InlineData("Id eq Id div 1")]
        [InlineData("Id eq Id mod 1")]
        [InlineData("startswith(Name, 'Microsoft')")]
        [InlineData("endswith(Name, 'Microsoft')")]
        [InlineData("substringof(Name, 'Microsoft')")]
        [InlineData("substring(Name, 1) eq 'Name'")]
        [InlineData("substring(Name, 1, 2) eq 'Name'")]
        [InlineData("length(Name) eq 1")]
        [InlineData("tolower(Name) eq 'Name'")]
        [InlineData("toupper(Name) eq 'Name'")]
        [InlineData("trim(Name) eq 'Name'")]
        [InlineData("indexof(Name, 'Microsoft') eq 1")]
        [InlineData("concat(Name, 'Microsoft') eq 'Microsoft'")]
        [InlineData("year(Birthday) eq 2000")]
        [InlineData("month(Birthday) eq 2000")]
        [InlineData("day(Birthday) eq 2000")]
        [InlineData("hour(Birthday) eq 2000")]
        [InlineData("minute(Birthday) eq 2000")]
        [InlineData("round(AmountSpent) eq 0")]
        [InlineData("floor(AmountSpent) eq 0")]
        [InlineData("ceiling(AmountSpent) eq 0")]
        [InlineData("Tags/any()")]
        [InlineData("Tags/all(t : t eq '1')")]
        [InlineData("System.Web.Http.OData.Query.QueryCompositionCustomerBase/Id eq 1")]
        [InlineData("Contacts/System.Web.Http.OData.Query.QueryCompositionCustomerBase/any()")]
        public void Validator_Doesnot_Throw_For_ValidQueries(string filter)
        {
            // Arrange
            FilterQueryOption option = new FilterQueryOption(filter, _context);

            // Act & Assert
            Assert.DoesNotThrow(() => _validator.Validate(option, _settings));
        }

        private static TheoryDataSet<string> GetLongInputsTestData(int maxCount)
        {
            return new TheoryDataSet<string>
                {
                    "" + String.Join(" and ", Enumerable.Range(1, (maxCount/5) + 1).Select(_ => "SupplierID eq 1")),
                    "" + String.Join(" ", Enumerable.Range(1, maxCount).Select(_ => "not")) + " Discontinued",
                    "" + String.Join(" add ", Enumerable.Range(1, maxCount/2)) + " eq 5050",
                    "" + String.Join("/", Enumerable.Range(1, maxCount/2).Select(_ => "Category/Product")) + "/ProductID eq 1",
                    "" + String.Join("/", Enumerable.Range(1, maxCount/2).Select(_ => "Category/Product")) + "/UnsignedReorderLevel eq 1",
                    "" + Enumerable.Range(1,maxCount).Aggregate("'abc'", (prev,i) => String.Format("trim({0})", prev)) + " eq '123'",
                    " Category/Products/any(" + Enumerable.Range(1,maxCount/4).Aggregate("", (prev,i) => String.Format("p{1}: p{1}/Category/Products/any({0})", prev, i)) +")"
                };
        }

        private class MyFilterValidator : FilterQueryValidator
        {
            private Dictionary<string, int> _times = new Dictionary<string, int>();

            public Dictionary<string, int> Times
            {
                get
                {
                    return _times;
                }
            }

            public override void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings)
            {
                IncrementCount("Validate");
                base.Validate(filterQueryOption, settings);
            }

            public override void ValidateAllNode(AllNode allQueryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateAllQueryNode");
                base.ValidateAllNode(allQueryNode, settings);
            }

            public override void ValidateAnyNode(AnyNode anyQueryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateAnyQueryNode");
                base.ValidateAnyNode(anyQueryNode, settings);
            }

            public override void ValidateArithmeticOperator(BinaryOperatorNode binaryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateArithmeticOperator");
                base.ValidateArithmeticOperator(binaryNode, settings);
            }

            public override void ValidateBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateBinaryOperatorQueryNode");
                base.ValidateBinaryOperatorNode(binaryOperatorNode, settings);
            }

            public override void ValidateConstantNode(ConstantNode constantNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateConstantQueryNode");
                base.ValidateConstantNode(constantNode, settings);
            }

            public override void ValidateConvertNode(ConvertNode convertQueryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateConvertQueryNode");
                base.ValidateConvertNode(convertQueryNode, settings);
            }

            public override void ValidateLogicalOperator(BinaryOperatorNode binaryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateLogicalOperator");
                base.ValidateLogicalOperator(binaryNode, settings);
            }

            public override void ValidateNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty, ODataValidationSettings settings)
            {
                IncrementCount("ValidateNavigationPropertyNode");
                base.ValidateNavigationPropertyNode(sourceNode, navigationProperty, settings);
            }

            public override void ValidateRangeVariable(RangeVariable rangeVariable, ODataValidationSettings settings)
            {
                IncrementCount("ValidateParameterQueryNode");
                base.ValidateRangeVariable(rangeVariable, settings);
            }

            public override void ValidateSingleValuePropertyAccessNode(SingleValuePropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateSingleValuePropertyAccessNode");
                base.ValidateSingleValuePropertyAccessNode(propertyAccessNode, settings);
            }

            public override void ValidateCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateCollectionPropertyAccessNode");
                base.ValidateCollectionPropertyAccessNode(propertyAccessNode, settings);
            }

            public override void ValidateSingleValueFunctionCallNode(SingleValueFunctionCallNode node, ODataValidationSettings settings)
            {
                IncrementCount("ValidateSingleValueFunctionCallQueryNode");
                base.ValidateSingleValueFunctionCallNode(node, settings);
            }

            public override void ValidateUnaryOperatorNode(UnaryOperatorNode unaryOperatorQueryNode, ODataValidationSettings settings)
            {
                IncrementCount("ValidateUnaryOperatorQueryNode");
                base.ValidateUnaryOperatorNode(unaryOperatorQueryNode, settings);
            }

            private void IncrementCount(string functionName)
            {
                int count = 0;
                if (_times.TryGetValue(functionName, out count))
                {
                    _times[functionName] = ++count;
                }
                else
                {
                    // first time
                    _times[functionName] = 1;
                }
            }
        }
    }
}
