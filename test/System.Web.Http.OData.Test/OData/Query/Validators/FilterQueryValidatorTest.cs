// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query.Validators
{
    public class FilterQueryValidatorTest
    {
        private MyFilterValidator _validator;
        private ODataConventionModelBuilder _builder;
        private IEdmModel _model;
        private ODataValidationSettings _settings;
        private ODataQueryContext _context;

        public FilterQueryValidatorTest()
        {
            _validator = new MyFilterValidator();
            _builder = new ODataConventionModelBuilder();
            _builder.EntitySet<QueryCompositionCustomer>("Customer");
            _model = _builder.GetEdmModel();
            _settings = new ODataValidationSettings();
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
                _validator.Validate(new FilterQueryOption("Name eq 'abc'", _context), null));
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
