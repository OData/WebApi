//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryOptionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Query.Validators;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
using Address = Microsoft.AspNet.OData.Test.Builder.TestModels.Address;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class ApplyQueryOptionTest
    {
        [Fact]
        public void Constructor_ChecksArguments_AndParsesApplyClause()
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            const string rawValue = "groupby((Name))";
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource, new Dictionary<string, string> { { "$apply", rawValue } });

            Assert.Equal("rawValue", Assert.Throws<ArgumentException>(() => new ApplyQueryOption(null, context, parser)).ParamName);
            Assert.Equal("rawValue", Assert.Throws<ArgumentException>(() => new ApplyQueryOption(string.Empty, context, parser)).ParamName);
            ExceptionAssert.ThrowsArgumentNull(() => new ApplyQueryOption(rawValue, null, parser), "context");
            ExceptionAssert.ThrowsArgumentNull(() => new ApplyQueryOption(rawValue, context, null), "queryOptionParser");
            var option = new ApplyQueryOption(rawValue, context, parser);
            Assert.Equal(rawValue, option.RawValue);
            Assert.Single(option.ApplyClause.Transformations);
            Assert.Same(option.ApplyClause, option.ApplyClause);
        }

        [Fact]
        public void Validate_CanBeOverridden_WithOrWithoutBaseEnforcement()
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            const string rawValue = "groupby((NotFilterableProperty))";
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource, new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);
            var settings = new ODataValidationSettings();
            var validator = new Mock<ApplyQueryValidator> { CallBase = true };
            option.Validator = validator.Object;

            Assert.Same(validator.Object, option.Validator);
            ExceptionAssert.Throws<ODataException>(() => option.Validate(settings), "The property 'NotFilterableProperty' cannot be used in the $filter query option.");
            validator.Verify(v => v.Validate(option, settings), Times.Once());

            validator.CallBase = false;
            ExceptionAssert.DoesNotThrow(() => option.Validate(settings));
            validator.Verify(v => v.Validate(option, settings), Times.Exactly(2));
        }

        [Fact]
        public void Validate_UsesDefaultValidator_AndChecksArguments()
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            const string rawValue = "groupby((Name))";
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);

            Assert.IsType<ApplyQueryValidator>(option.Validator);
            ExceptionAssert.ThrowsArgumentNull(() => option.Validate(null), "validationSettings");
            ExceptionAssert.ThrowsArgumentNull(() => option.Validator.Validate(null, new ODataValidationSettings()), "applyQueryOption");
            ExceptionAssert.ThrowsArgumentNull(() => option.Validator.Validate(option, null), "validationSettings");
            ExceptionAssert.DoesNotThrow(() => option.Validate(new ODataValidationSettings()));

            option.Validator = null;
            ExceptionAssert.DoesNotThrow(() => option.Validate(new ODataValidationSettings()));
            ExceptionAssert.ThrowsArgumentNull(() => option.Validate(null), "validationSettings");
        }

        [Fact]
        public void Validate_UsesValidatorFromRequestContainer()
        {
            var validator = new Mock<ApplyQueryValidator>();
            var context = ValidationTestHelper.CreateCustomerContext(false);
            context.RequestContainer = new MockContainer(builder => builder.AddService<ApplyQueryValidator>(
                ServiceLifetime.Singleton, services => validator.Object));
            const string rawValue = "groupby((NotFilterableProperty))";
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } }, context.RequestContainer);
            var option = new ApplyQueryOption(rawValue, context, parser);
            var settings = new ODataValidationSettings();

            option.Validate(settings);

            Assert.Same(validator.Object, option.Validator);
            validator.Verify(v => v.Validate(option, settings), Times.Once());
        }

        [Fact]
        public void Validate_FilterTransformation_UsesConfiguredFilterValidator()
        {
            var validator = new Mock<FilterQueryValidator>(new DefaultQuerySettings());
            var context = ValidationTestHelper.CreateCustomerContext(false);
            context.RequestContainer = new MockContainer(builder => builder.AddService<FilterQueryValidator>(
                ServiceLifetime.Scoped, services => validator.Object));
            const string rawValue = "filter(NotFilterableProperty eq 'restricted')";
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } }, context.RequestContainer);
            var option = new ApplyQueryOption(rawValue, context, parser);
            var settings = new ODataValidationSettings();
            var filterClause = ((Microsoft.OData.UriParser.Aggregation.FilterTransformationNode)
                option.ApplyClause.Transformations.Single()).FilterClause;

            option.Validate(settings);

            validator.Verify(v => v.Validate(It.Is<FilterQueryOption>(
                filter => filter.Context == context && filter.FilterClause == filterClause), settings), Times.Once());
        }

        [Theory]
        [InlineData("groupby((NotFilterableProperty))", "NotFilterableProperty")]
        [InlineData("groupby((NonFilterableProperty))", "NonFilterableProperty")]
        [InlineData("groupby((Id,NotFilterableProperty))", "NotFilterableProperty")]
        [InlineData("groupby((Address/NotFilterableProperty))", "NotFilterableProperty")]
        [InlineData("groupby((Id,Address/NotFilterableProperty))", "NotFilterableProperty")]
        [InlineData("aggregate(NotFilterableProperty with countdistinct as R)", "NotFilterableProperty")]
        [InlineData("aggregate(NonFilterableProperty with countdistinct as R)", "NonFilterableProperty")]
        [InlineData("groupby((Id),aggregate(NotFilterableProperty with countdistinct as R))", "NotFilterableProperty")]
        [InlineData("aggregate(Contacts(NotFilterableProperty with countdistinct as R))", "NotFilterableProperty")]
        [InlineData("aggregate(Contacts/$count($filter=NotFilterableProperty eq 'x') with sum as R)", "NotFilterableProperty")]
        [InlineData("compute(length(NotFilterableProperty) as L)", "NotFilterableProperty")]
        [InlineData("compute(NotFilterableProperty eq 'x' as F)", "NotFilterableProperty")]
        [InlineData("compute(Name eq 'a' or NotFilterableProperty eq 'b' as F)", "NotFilterableProperty")]
        [InlineData("compute(Address/NotFilterableProperty eq 'x' as F)", "NotFilterableProperty")]
        [InlineData("compute(Contacts/$count($filter=NotFilterableProperty eq 'x') as C)", "NotFilterableProperty")]
        [InlineData("compute(RelationshipManager/NotFilterableProperty eq 'x' as F)", "NotFilterableProperty")]
        [InlineData("compute(NotFilterableNavigationProperty/Name eq 'x' as F)", "NotFilterableNavigationProperty")]
        [InlineData("compute(Contacts/any(c:c/NotFilterableProperty ne null) as F)", "NotFilterableProperty")]
        [InlineData("compute(Contacts/all(c:c/NotFilterableProperty ne null) as F)", "NotFilterableProperty")]
        [InlineData("compute(length(Name) as L)/groupby((NotFilterableProperty))", "NotFilterableProperty")]
        [InlineData("filter(Name eq 'x')/groupby((NotFilterableProperty))", "NotFilterableProperty")]
        public void Validate_RejectsRestrictedProperties_InApplyExpressions(string rawValue, string property)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            context.DefaultQuerySettings.EnableFilter = rawValue.StartsWith("filter(", StringComparison.Ordinal);
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);

            ExceptionAssert.Throws<ODataException>(() => option.Validate(new ODataValidationSettings()),
                "The property '" + property + "' cannot be used in the $filter query option.");
        }

        [Theory]
        [InlineData("groupby((Name))")]
        [InlineData("groupby((Id))")]
        [InlineData("groupby((Name,Id))")]
        [InlineData("groupby((Address/City))")]
        [InlineData("groupby((Name),aggregate(AmountSpent with sum as Total))")]
        [InlineData("aggregate(AmountSpent with sum as Total)")]
        [InlineData("aggregate($count as Count)")]
        [InlineData("aggregate(Contacts/$count($filter=Name eq 'x') with sum as R)")]
        [InlineData("aggregate(Contacts(Name with countdistinct as R))")]
        [InlineData("compute(AmountSpent mul 2 as Double)")]
        [InlineData("compute(length(Name) as L)")]
        [InlineData("compute(Name eq 'a' as F)")]
        [InlineData("compute(Address/City eq 'a' as F)")]
        [InlineData("compute(Contacts/any(c:c/Name ne null) as F)")]
        [InlineData("compute(Contacts/all(c:c/Name ne null) as F)")]
        [InlineData("compute(RelationshipManager/Name eq 'x' as F)")]
        [InlineData("compute(Contacts/$count as C)")]
        [InlineData("compute(Contacts/$count($filter=Name eq 'x') as C)")]
        [InlineData("compute(Contacts/$count($search=red AND blue) as C)")]
        [InlineData("compute(Contacts/$count($filter=Name eq 'x';$search=red AND blue) as C)")]
        public void Validate_AllowsUnrestrictedExpressions_WithGlobalFilterAndSelectDisabled(string rawValue)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            context.DefaultQuerySettings.EnableFilter = false;
            context.DefaultQuerySettings.EnableSelect = false;
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);

            ExceptionAssert.DoesNotThrow(() => option.Validate(new ODataValidationSettings()));
        }

        [Theory]
        [InlineData("groupby((Name))")]
        [InlineData("aggregate(Name with countdistinct as R)")]
        [InlineData("compute(length(Name) as L)")]
        [InlineData("compute(Contacts/$count($filter=Name eq 'x') as C)")]
        public void Validate_RejectsModelBoundPropertyRestrictions(string rawValue)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            var entityType = context.Model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == nameof(QueryCompositionCustomer));
            var querySettings = new ModelBoundQuerySettings();
            querySettings.SelectConfigurations.Add("Name", SelectExpandType.Disabled);
            context.Model.SetAnnotationValue(entityType, querySettings);
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);

            ExceptionAssert.Throws<ODataException>(() => option.Validate(new ODataValidationSettings()),
                "The property 'Name' cannot be used in the $select query option.");

            querySettings.SelectConfigurations.Clear();
            querySettings.FilterConfigurations.Add("Name", false);
            ExceptionAssert.Throws<ODataException>(() => option.Validate(new ODataValidationSettings()),
                "The property 'Name' cannot be used in the $filter query option.");

            querySettings.FilterConfigurations.Clear();
            ExceptionAssert.DoesNotThrow(() => option.Validate(new ODataValidationSettings()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Validate_FilterTransformation_MatchesFilterEnableSwitch(bool enableFilter)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            context.DefaultQuerySettings.EnableFilter = enableFilter;
            const string rawValue = "filter(Name eq 'x')";
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);

            if (enableFilter)
            {
                ExceptionAssert.DoesNotThrow(() => option.Validate(new ODataValidationSettings()));
            }
            else
            {
                ExceptionAssert.Throws<ODataException>(() => option.Validate(new ODataValidationSettings()),
                    "The property 'Name' cannot be used in the $filter query option.");
            }
        }

        [Theory]
        [InlineData("compute(length(Name) as L)", AllowedFunctions.Length, "length")]
        [InlineData("aggregate(length(Name) with max as L)", AllowedFunctions.Length, "length")]
        [InlineData("compute(Contacts/any(c:c/Name ne null) as F)", AllowedFunctions.Any, "any")]
        [InlineData("compute(Contacts/all(c:c/Name ne null) as F)", AllowedFunctions.All, "all")]
        [InlineData("compute(Contacts/$count($filter=length(Name) gt 1) as C)", AllowedFunctions.Length, "length")]
        public void Validate_EnforcesFunctionAllowList(string rawValue, AllowedFunctions excluded, string functionName)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);
            var settings = new ODataValidationSettings { AllowedFunctions = AllowedFunctions.AllFunctions & ~excluded };

            ExceptionAssert.Throws<ODataException>(() => option.Validate(settings),
                "Function '" + functionName + "' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");
            settings.AllowedFunctions = AllowedFunctions.AllFunctions;
            ExceptionAssert.DoesNotThrow(() => option.Validate(settings));
        }

        [Theory]
        [InlineData("compute(AmountSpent mul 2 as D)")]
        [InlineData("aggregate(AmountSpent mul 2 with sum as Total)")]
        [InlineData("compute(Contacts/$count($filter=AmountSpent mul 2 gt 10) as C)")]
        public void Validate_EnforcesArithmeticOperatorAllowList(string rawValue)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);
            var settings = new ODataValidationSettings { AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~AllowedArithmeticOperators.Multiply };

            ExceptionAssert.Throws<ODataException>(() => option.Validate(settings),
                "Arithmetic operator 'Multiply' is not allowed. To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.");
            settings.AllowedArithmeticOperators = AllowedArithmeticOperators.All;
            ExceptionAssert.DoesNotThrow(() => option.Validate(settings));
        }

        [Theory]
        [InlineData("compute(Name eq 'x' as F)", AllowedLogicalOperators.Equal, "Logical operator 'Equal'")]
        [InlineData("compute(not (Name eq 'x') as F)", AllowedLogicalOperators.Not, "Logical operator 'Not'")]
        [InlineData("compute(-AmountSpent as N)", AllowedLogicalOperators.Not, "Logical operator 'Negate'")]
        [InlineData("compute(Contacts/$count($filter=Name eq 'x') as C)", AllowedLogicalOperators.Equal, "Logical operator 'Equal'")]
        public void Validate_EnforcesLogicalOperatorAllowList(string rawValue, AllowedLogicalOperators excluded, string message)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);
            var settings = new ODataValidationSettings { AllowedLogicalOperators = AllowedLogicalOperators.All & ~excluded };

            ExceptionAssert.Throws<ODataException>(() => option.Validate(settings),
                message + " is not allowed. To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.");
            settings.AllowedLogicalOperators = AllowedLogicalOperators.All;
            ExceptionAssert.DoesNotThrow(() => option.Validate(settings));
        }

        [Theory]
        [InlineData("groupby((Name))")]
        [InlineData("aggregate(AmountSpent with sum as Total)")]
        [InlineData("compute(AmountSpent mul 2 as D)")]
        public void Validate_EnforcesNodeCountLimit(string rawValue)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);

            ExceptionAssert.Throws<ODataException>(() => option.Validate(new ODataValidationSettings { MaxNodeCount = 1 }),
                "The node count limit of '1' has been exceeded. To increase the limit, set the 'MaxNodeCount' property on EnableQueryAttribute or ODataValidationSettings.");
            ExceptionAssert.DoesNotThrow(() => option.Validate(new ODataValidationSettings()));
        }

        [Theory]
        [InlineData("compute(length(trim(Name)) as L)", "compute(Contacts/any(c:c/Contacts/all(d:d/Name ne null)) as F)")]
        [InlineData("compute(Contacts/$count($filter=length(trim(Name)) gt 1) as C)", "compute(Contacts/any(c:c/Contacts/$count($filter=Contacts/any(d:d/Name ne null)) gt 0) as C)")]
        public void Validate_EnforcesFunctionAndLambdaDepthLimits(string functions, string lambdas)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            var functionParser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", functions } });
            var functionOption = new ApplyQueryOption(functions, context, functionParser);
            var lambdaParser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$apply", lambdas } });
            var lambdaOption = new ApplyQueryOption(lambdas, context, lambdaParser);

            ExceptionAssert.Throws<ODataException>(() => functionOption.Validate(new ODataValidationSettings { MaxFunctionCallDepth = 1 }),
                "The function call depth limit of '1' has been exceeded. 'MaxFunctionCallDepth' can be configured on ODataValidationSettings, ODataQuerySettings or EnableQueryAttribute.");
            ExceptionAssert.DoesNotThrow(() => functionOption.Validate(new ODataValidationSettings { MaxFunctionCallDepth = 2 }));
            ExceptionAssert.Throws<ODataException>(() => lambdaOption.Validate(new ODataValidationSettings { MaxAnyAllExpressionDepth = 1 }),
                "The Any/All nesting limit of '1' has been exceeded. 'MaxAnyAllExpressionDepth' can be configured on ODataQuerySettings or EnableQueryAttribute.");
            ExceptionAssert.DoesNotThrow(() => lambdaOption.Validate(new ODataValidationSettings { MaxAnyAllExpressionDepth = 2 }));
        }

        [Theory]
        [InlineData("compute(Contacts/$count($filter=Name eq 'x') as C)")]
        [InlineData("compute(Contacts/$count($search=red AND blue) as C)")]
        public void Validate_EnforcesNodeCountLimit_InCountClause(string rawValue)
        {
            var context = ValidationTestHelper.CreateCustomerContext(false);
            var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource, new Dictionary<string, string> { { "$apply", rawValue } });
            var option = new ApplyQueryOption(rawValue, context, parser);

            // The count, collection navigation and range variable alone fit within this limit.
            ExceptionAssert.Throws<ODataException>(
                () => option.Validate(new ODataValidationSettings { MaxNodeCount = 3 }),
                "The node count limit of '3' has been exceeded. To increase the limit, set the 'MaxNodeCount' property on EnableQueryAttribute or ODataValidationSettings.");
            ExceptionAssert.DoesNotThrow(() => option.Validate(new ODataValidationSettings()));
        }

        [Theory]
        [InlineData(false, "$apply=filter(NotFilterableProperty eq 'R1')", "The property 'NotFilterableProperty' cannot be used in the $filter query option.")]
        [InlineData(false, "$apply=groupby((NotSortableProperty))", "The property 'NotSortableProperty' cannot be used in the $select query option.")]
        [InlineData(false, "$apply=aggregate(AmountSpent with max as MaxRestricted)", "The property 'AmountSpent' cannot be used in the $filter query option.")]
        [InlineData(false, "$apply=aggregate(Contacts(AmountSpent with sum as SumRestricted))", "The property 'AmountSpent' cannot be used in the $filter query option.")]
        [InlineData(false, "$apply=compute(NotFilterableProperty eq 'R1' as Flag)", "The property 'NotFilterableProperty' cannot be used in the $filter query option.")]
        [InlineData(false, "$filter=NotFilterableProperty eq 'R1'", "The property 'NotFilterableProperty' cannot be used in the $filter query option.")]
        [InlineData(false, "$select=NotSortableProperty", "The property 'NotSortableProperty' cannot be used in the $select query option.")]
        [InlineData(true, "$apply=compute(trim(Name) as L)", "Function 'trim' is not allowed.")]
        [InlineData(true, "$apply=compute(Id mul 2 as D)", "Arithmetic operator 'Multiply' is not allowed.")]
        [InlineData(true, "$apply=aggregate(Id mul 2 with sum as D)", "Arithmetic operator 'Multiply' is not allowed.")]
        public async Task Validate_ControllerRejectsRestrictedPropertiesAndLimits(bool restrictedLimits, string query, string error)
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<QueryCompositionCustomer>("QueryCompositionCustomer");
            builder.EntitySet<QueryCompositionCustomer>("QueryCompositionCustomerValidation");
            var model = builder.GetEdmModel();
            var entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == nameof(QueryCompositionCustomer));
            var querySettings = new ModelBoundQuerySettings();
            querySettings.SelectConfigurations.Add(nameof(QueryCompositionCustomer.NotSortableProperty), SelectExpandType.Disabled);
            querySettings.FilterConfigurations.Add(nameof(QueryCompositionCustomer.AmountSpent), false);
            model.SetAnnotationValue(entityType, querySettings);
            var controllers = new[] { typeof(QueryCompositionCustomerController), typeof(QueryCompositionCustomerValidationController) };

            using (var server = TestServerFactory.Create(controllers, config =>
            {
                config.Filter().Select();
                config.MapODataServiceRoute("odata", "odata", model);
            }))
            using (HttpClient client = TestServerFactory.CreateClient(server))
            using (var response = await client.GetAsync("http://localhost/odata/" + (restrictedLimits ? "QueryCompositionCustomerValidation" : "QueryCompositionCustomer") + "?" + query))
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Contains(error, await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [InlineData(false, "$apply=filter(Name eq 'Lowest')", 1)]
        [InlineData(false, "$apply=groupby((Name))", 4)]
        [InlineData(false, "$apply=aggregate(Id with sum as TotalAmount)", 1)]
        [InlineData(false, "$apply=filter(Contacts/any())/aggregate(Contacts(Id with sum as SumAmount))", 1)]
        [InlineData(false, "$apply=aggregate($count as Total)", 1)]
        [InlineData(false, "$apply=compute(Id mul 2 as DoubleAmount)", 4)]
        [InlineData(false, "$apply=groupby((Name),aggregate(Id with sum as Total))/filter(Total gt 0)", 4)]
        [InlineData(true, "$apply=compute(Name eq 'Lowest' as Flag)", 4)]
        [InlineData(true, "$apply=aggregate(Id with sum as TotalAmount)", 1)]
        [InlineData(true, "$apply=groupby((Name))", 4)]
        public async Task Validate_ControllerAllowsUnrestrictedPropertiesAndLimits(bool restrictedLimits, string query, int count)
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<QueryCompositionCustomer>("QueryCompositionCustomer");
            builder.EntitySet<QueryCompositionCustomer>("QueryCompositionCustomerValidation");
            var model = builder.GetEdmModel();
            var entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == nameof(QueryCompositionCustomer));
            var querySettings = new ModelBoundQuerySettings();
            querySettings.SelectConfigurations.Add(nameof(QueryCompositionCustomer.NotSortableProperty), SelectExpandType.Disabled);
            querySettings.FilterConfigurations.Add(nameof(QueryCompositionCustomer.AmountSpent), false);
            model.SetAnnotationValue(entityType, querySettings);
            var controllers = new[] { typeof(QueryCompositionCustomerController), typeof(QueryCompositionCustomerValidationController) };

            using (var server = TestServerFactory.Create(controllers, config =>
            {
                config.Filter().Select();
                config.MapODataServiceRoute("odata", "odata", model);
            }))
            using (HttpClient client = TestServerFactory.CreateClient(server))
            using (var response = await client.GetAsync("http://localhost/odata/" + (restrictedLimits ? "QueryCompositionCustomerValidation" : "QueryCompositionCustomer") + "?" + query))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
#if NETCORE
                var result = await response.Content.ReadAsObject<JObject>();
                Assert.Equal(count, Assert.IsType<JArray>(result["value"]).Count);
#else
                var result = await response.Content.ReadAsObject<JArray>();
                Assert.Equal(count, result.Count);
#endif
            }
        }

        // Legal apply queries usable against CustomerApplyTestData.
        // Tuple is: apply, expected number
        public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestApplies
        {
            get
            {
                return new TheoryDataSet<string, List<Dictionary<string, object>>>
                {
                    {
                        "aggregate($count as Count)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Count", 5L} }
                        }
                    },
                    {
                        "aggregate(CustomerId with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 15} }
                        }
                    },
                    {
                        "aggregate(cast(CustomerId, Edm.Int64) with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 15L} }
                        }
                    },
                    {
                        "aggregate(SharePrice with sum as SharePrice)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePrice", 22.5M} }
                        }
                    },
                    {
                        "aggregate(SharePrice with min as SharePrice)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePrice", 2.5M} }
                        }
                    },
                     {
                        "aggregate(SharePrice with max as SharePrice)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePrice", 10M} }
                        }
                    },
                    {
                        "aggregate(SharePrice with average as SharePrice)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePrice", 7.5M} }
                        }
                    },
                    {
                        "aggregate(CustomerId with sum as Total, SharePrice with countdistinct as SharePriceDistinctCount)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePriceDistinctCount", 3L}, { "Total", 15} }
                        }
                    },
                    {
                        "groupby((Name))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"} },
                            new Dictionary<string, object> { { "Name", "Highest"} },
                            new Dictionary<string, object> { { "Name", "Middle"} }
                        }
                    },
                    {
                        "groupby((Name), aggregate(CustomerId with sum as Total))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"}, { "Total", 10} },
                            new Dictionary<string, object> { { "Name", "Highest"}, { "Total", 2} },
                            new Dictionary<string, object> { { "Name", "Middle"}, { "Total", 3 } }
                        }
                    },
                    {
                        "groupby((Name), aggregate($count as Count))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"}, { "Count", 3L} },
                            new Dictionary<string, object> { { "Name", "Highest"}, { "Count", 1L} },
                            new Dictionary<string, object> { { "Name", "Middle"}, { "Count", 1L} }
                        }
                    },
                    {
                        "filter(Name eq 'Lowest')/groupby((Name))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"} }
                        }
                    },
                    {
                        "groupby((Name), aggregate(CustomerId with sum as Total))/filter(Total eq 3)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Middle"}, { "Total", 3 } }
                        }
                    },
                    {
                        "groupby((Name))/filter(Name eq 'Lowest')",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"} }
                        }
                    },
                    {
                        "groupby((Address/City))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Address/City", "redmond"} },
                            new Dictionary<string, object> { { "Address/City", "seattle"} },
                            new Dictionary<string, object> { { "Address/City", "hobart"} },
                            new Dictionary<string, object> { { "Address/City", null} },
                        }
                    },
                    {
                        "groupby((Address/City, Address/State))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Address/City", "redmond"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "Address/City", "seattle"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "Address/City", "hobart"}, { "Address/State", null} },
                            new Dictionary<string, object> { { "Address/City", null}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/City, Address/State))/groupby((Address/State), aggregate(Address/City with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(Address/City with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(startswith(Address/City, 's') with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", false}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(endswith(Address/City, 't') with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", false}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(contains(Address/City, 'o') with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(length(Address/City) with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", 7}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", 6}, { "Address/State", null} },
                        }
                    },
                    {
                        "aggregate(year(StartDate) with max as MaxYear, year(StartDate) with min as MinYear)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxYear", 2018}, { "MinYear", 2016} },
                        }
                    },
                    {
                        "aggregate(month(StartDate) with max as MaxMonth, month(StartDate) with min as MinMonth)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxMonth", 5}, { "MinMonth", 1} },
                        }
                    },
                    {
                        "aggregate(day(StartDate) with max as MaxDay, day(StartDate) with min as MinDay)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxDay", 7}, { "MinDay", 1} },
                        }
                    },
                    {
                        "aggregate(hour(StartDate) with max as MaxHour, hour(StartDate) with min as MinHour)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxHour", 5 }, { "MinHour", 1} },
                        }
                    },
                    {
                        "aggregate(minute(StartDate) with max as MaxMinute, minute(StartDate) with min as MinMinute)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxMinute", 6}, { "MinMinute", 2} },
                        }
                    },
                    {
                        "aggregate(second(StartDate) with max as MaxSecond, second(StartDate) with min as MinSecond)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxSecond", 7}, { "MinSecond", 3} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(concat(Address/City,Address/State) with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattleWA"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", null}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(Address/City with max as MaxCity, Address/City with min as MinCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "MinCity", "redmond"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "MinCity", "hobart" }, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(Address/City with max as MaxCity, CustomerId mul CustomerId with sum as CustomerId))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "CustomerId", 30}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "CustomerId", 25 }, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(CustomerId mul CustomerId with sum as CustomerId))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 30}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "CustomerId", 25}, { "Address/State", null} },
                        }
                    },
                    {
                        "filter(Company/CEO/EmployeeName eq 'john')/groupby((Company/CEO/EmployeeName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"} }
                        }
                    },
                    {
                        "groupby((Company/CEO/EmployeeName))/filter(Company/CEO/EmployeeName eq 'john')",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"} }
                        }
                    },
                    {
                        "groupby((Name, Company/CEO/EmployeeName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"}, { "Company/CEO/EmployeeName", "john" } },
                            new Dictionary<string, object> { { "Name", "Highest"}, { "Company/CEO/EmployeeName", "tom" } },
                            new Dictionary<string, object> { { "Name", "Middle"}, { "Company/CEO/EmployeeName", "john" } },
                            new Dictionary<string, object> { { "Name", "Lowest"}, { "Company/CEO/EmployeeName", "alex" } },
                            new Dictionary<string, object> { { "Name", "Lowest"}, { "Company/CEO/EmployeeName", null } }
                        }
                    },
                    {
                        "groupby((Address/City, Company/CEO/EmployeeName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Address/City", "redmond"}, { "Company/CEO/EmployeeName", "john" } },
                            new Dictionary<string, object> { { "Address/City", "seattle"}, { "Company/CEO/EmployeeName", "tom" } },
                            new Dictionary<string, object> { { "Address/City", "hobart"}, { "Company/CEO/EmployeeName", "john" } },
                            new Dictionary<string, object> { { "Address/City", null}, { "Company/CEO/EmployeeName", "alex" } },
                            new Dictionary<string, object> { { "Address/City", "redmond"}, { "Company/CEO/EmployeeName", null } }
                        }
                    },
                    {
                        "groupby((Company/CEO/HomeAddress/City))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "redmond"} },
                            new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "seattle"} },
                            new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "hobart"} },
                            new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", null} },
                        }
                    },
                    {
                        "groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "redmond"}, { "Company/CEO/HomeAddress/State", "WA"} },
                            new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "seattle"}, { "Company/CEO/HomeAddress/State", "WA"} },
                            new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "hobart"}, { "Company/CEO/HomeAddress/State", null} },
                            new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", null}, { "Company/CEO/HomeAddress/State", null} },
                        }
                    },
                    {
                        "groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))/groupby((Company/CEO/HomeAddress/State), aggregate(Company/CEO/HomeAddress/City with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Company/CEO/HomeAddress/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Company/CEO/HomeAddress/State", null} },
                        }
                    },
                    {
                        "groupby((Company/CEO/EmployeeName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null} }
                        }
                    },
                    {
                        "groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"}, { "Company/CEO/BaseSalary", 20M} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"}, { "Company/CEO/BaseSalary", 0M} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null}, { "Company/CEO/BaseSalary", null} }
                        }
                    },
                    {
                        "groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))/groupby((Company/CEO/BaseSalary), aggregate(Company/CEO/EmployeeName with max as MaxEmployeeName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "MaxEmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} },
                            new Dictionary<string, object> {{ "MaxEmployeeName", "alex"}, { "Company/CEO/BaseSalary", 0M} },
                            new Dictionary<string, object> {{ "MaxEmployeeName", null}, { "Company/CEO/BaseSalary", null} }
                        }
                    },
                    {
                        "groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))/groupby((Company/CEO/EmployeeName), aggregate(Company/CEO/BaseSalary with average as AverageBaseSalary))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "AverageBaseSalary", 20M }, { "Company/CEO/EmployeeName", "john"} },
                            new Dictionary<string, object> {{ "AverageBaseSalary", 20M }, { "Company/CEO/EmployeeName", "tom"} },
                            new Dictionary<string, object> {{ "AverageBaseSalary", 0M }, { "Company/CEO/EmployeeName", "alex"} },
                            new Dictionary<string, object> {{ "AverageBaseSalary", null }, { "Company/CEO/EmployeeName", null} }
                        }
                    },
                    {
                        "aggregate(CustomerId mul CustomerId with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 55} }
                        }
                    },
                    {
                        // Note SharePrice and CustomerId have different type
                        "aggregate(SharePrice mul CustomerId with sum as Result)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Result", 65.0M} }
                        }
                    },
                    {
                        "groupby((Website))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Website", null} },
                        }
                    },
                    {
                        "aggregate(IntProp with max as MaxIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxIntProp", 2} }
                        }
                    },
                    {
                        "aggregate(IntProp with min as MinIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MinIntProp", 1} }
                        }
                    },
                    {
                        "aggregate(IntProp with countdistinct as DistinctIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "DistinctIntProp", 3L} }
                        }
                    },
                    {
                        "aggregate(IntProp with sum as TotalIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "TotalIntProp", 3M} }
                        }
                    },
                    {
                        "aggregate(IntProp with average as TotalIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "TotalIntProp", 1.5M} }
                        }
                    },
                    {
                        "aggregate(MixedProp with sum as TotalMixedProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "TotalMixedProp", 1M} }
                        }
                    },
                    {
                        "groupby((StringProp), aggregate(IntProp with min as MinIntProp))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "StringProp", "Test1" }, { "MinIntProp", 1} },
                            new Dictionary<string, object> { { "StringProp", "Test2" }, { "MinIntProp", 2} },
                            new Dictionary<string, object> { { "StringProp", "Test3" }, { "MinIntProp", null} },
                            new Dictionary<string, object> { { "StringProp", null }, { "MinIntProp", null} },
                        }
                    },
                    {
                        "groupby((StringProp), aggregate(IntProp with min as MinIntProp))/groupby((StringProp))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "StringProp", "Test1" } },
                            new Dictionary<string, object> { { "StringProp", "Test2" } },
                            new Dictionary<string, object> { { "StringProp", "Test3" } },
                            new Dictionary<string, object> { { "StringProp", null } },
                        }
                    },
                    {
                        "aggregate($count as Count)/compute(Count add Count as DoubleCount)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Count", 5L}, { "DoubleCount", 10L } }
                        }
                    },
                    {
                        "groupby((Name), aggregate(CustomerId with sum as Total))/compute(Total add Total as DoubleTotal, length(Name) as NameLen)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"},  { "Total", 10}, { "DoubleTotal", 20}, { "NameLen", 6},},
                            new Dictionary<string, object> { { "Name", "Highest"}, { "Total", 2} , { "DoubleTotal", 4} , { "NameLen", 7} ,},
                            new Dictionary<string, object> { { "Name", "Middle"},  { "Total", 3 }, { "DoubleTotal", 6 }, { "NameLen", 6 }, }
                        }
                    },
                    {
                        "compute(length(Name) as NameLen)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", 6}, { "CustomerId", 1},},
                            new Dictionary<string, object> { { "Name", "Highest"},  { "NameLen", 7}, { "CustomerId", 2},},
                            new Dictionary<string, object> { { "Name", "Middle" },  { "NameLen", 6}, { "CustomerId", 3},},
                            new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", 6}, { "CustomerId", 4},},
                            new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", 6}, { "CustomerId", 5},},
                        }
                    },
                    {
                        "compute(length(ShareSymbol) as NameLen)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", null}, { "CustomerId", 1},},
                            new Dictionary<string, object> { { "Name", "Highest"},  { "NameLen", null}, { "CustomerId", 2},},
                            new Dictionary<string, object> { { "Name", "Middle" },  { "NameLen", null}, { "CustomerId", 3},},
                            new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", null}, { "CustomerId", 4},},
                            new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", null}, { "CustomerId", 5},},
                        }
                    },
                    {
                        "compute(length(Name) as NameLen)/aggregate(NameLen with sum as TotalLen)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "TotalLen", 31} }
                        }
                    },
                    {
                        "compute(length(Name) as NameLen)/aggregate(NameLen add CustomerId with sum as TotalLen)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "TotalLen", 46} }
                        }
                    },
                    {
                        "compute(length(Name) as NameLen)/groupby((Name),aggregate( CustomerId with sum as Total, NameLen with max as MaxNameLen))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"},  { "Total", 10},  { "MaxNameLen", 6},},
                            new Dictionary<string, object> { { "Name", "Highest"}, { "Total", 2} ,  { "MaxNameLen", 7} ,},
                            new Dictionary<string, object> { { "Name", "Middle"},  { "Total", 3 },  { "MaxNameLen", 6 }, }
                        }
                    },
                    {
                        "compute(length(Name) as NameLen)/groupby((NameLen),aggregate( CustomerId with sum as Total))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Total", 13},  { "NameLen", 6},},
                            new Dictionary<string, object> { { "Total", 2} ,  { "NameLen", 7} ,},
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(Address/City with max as MaxCity, Address/City with min as MinCity))/compute(length(MaxCity) as MaxCityLen)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "MinCity", "redmond"}, { "Address/State", "WA"}, {"MaxCityLen", 7 } },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "MinCity", "hobart" }, { "Address/State", null}, {"MaxCityLen", 6 } },
                        }
                    },
                    {
                        "compute(length(Address/City) as CityLength)/groupby((Address/State), aggregate(Address/City with max as MaxCity, Address/City with min as MinCity, CityLength with max as MaxCityLen))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "MinCity", "redmond"}, { "Address/State", "WA"}, {"MaxCityLen", 7 } },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "MinCity", "hobart" }, { "Address/State", null}, {"MaxCityLen", 6 } },
                        }
                    },
                };
            }
        }

        public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestAppliesMixedWithOthers
        {
            get
            {
                return new TheoryDataSet<string, List<Dictionary<string, object>>>
                {
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$filter=Total eq 3",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}}
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                            new Dictionary<string, object> {{"Name", "Middle"}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))&$orderby=Total",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))&$orderby=Total, Total2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$orderby=Name, Total",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$orderby=Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}},
                            new Dictionary<string, object> {{"Address/City", "redmond"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$filter=Address/City eq 'redmond'&$orderby=Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))&$filter=Address/State eq 'WA'&$orderby=Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))&$orderby=Address/State desc, Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                            new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$filter=Address/City eq 'redmond'",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City))&$orderby=Company/CEO/HomeAddress/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", null}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "hobart"}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City))&$filter=Company/CEO/HomeAddress/City eq 'redmond'&$orderby=Company/CEO/HomeAddress/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))&$filter=Company/CEO/HomeAddress/State eq 'WA'&$orderby=Company/CEO/HomeAddress/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}, {"Company/CEO/HomeAddress/State", "WA"}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}, {"Company/CEO/HomeAddress/State", "WA"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))&$orderby=Company/CEO/HomeAddress/State desc, Company/CEO/HomeAddress/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}, {"Company/CEO/HomeAddress/State", "WA"}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}, {"Company/CEO/HomeAddress/State", "WA"}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", null}, {"Company/CEO/HomeAddress/State", null}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "hobart"}, {"Company/CEO/HomeAddress/State", null}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City))&$filter=Company/CEO/HomeAddress/City eq 'redmond'",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/EmployeeName))&$orderby=Company/CEO/EmployeeName",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"} }
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/EmployeeName))&$filter=Company/CEO/EmployeeName eq 'alex'&$orderby=Company/CEO/EmployeeName",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/EmployeeName", "alex"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))&$filter= Company/CEO/BaseSalary eq 20&$orderby=Company/CEO/EmployeeName",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"}, { "Company/CEO/BaseSalary", 20M} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} }
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))&$orderby=Company/CEO/BaseSalary desc, Company/CEO/EmployeeName desc",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"}, { "Company/CEO/BaseSalary", 20M} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"}, { "Company/CEO/BaseSalary", 0M} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null}, { "Company/CEO/BaseSalary", null} }
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/EmployeeName))&$filter=Company/CEO/EmployeeName eq 'john'",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/EmployeeName", "john"}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))/compute(Total mul 2 as NewTotal)&$orderby=Name, NewTotal",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2},  {"NewTotal", 4}, },
                            new Dictionary<string, object> {{"Name", "Lowest"},  {"Total", 10}, {"NewTotal", 20},},
                            new Dictionary<string, object> {{"Name", "Middle"},  {"Total", 3},  {"NewTotal", 6}, },
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))/compute(Total mul 2 as NewTotal)/filter(NewTotal gt 6)&$orderby=Name, NewTotal",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest"},  {"Total", 10}, {"NewTotal", 20},},
                        }
                    },
                    //{
                    //    "$apply=groupby((Name))&$top=1",
                    //    new List<Dictionary<string, object>>
                    //    {
                    //        new Dictionary<string, object> {{"Name", "Highest"}},
                    //    }
                    //},
                    //{
                    //    "$apply=groupby((Name))&$skip=1",
                    //    new List<Dictionary<string, object>>
                    //    {
                    //        new Dictionary<string, object> {{"Name", "Lowest"}},
                    //        new Dictionary<string, object> {{"Name", "Middle"}},
                    //    }
                    //},
                };
            }
        }

        public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestAppliesForPaging
        {
            get
            {
                return new TheoryDataSet<string, List<Dictionary<string, object>>>
                {
                    {
                        "$apply=aggregate(CustomerId with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 15} }
                        }
                    },
                    {
                        "$apply=aggregate(CustomerId with sum as Total)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Total", 15} }
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        }
                    },
                    {
                        "$apply=groupby((Name))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}},
                        }
                    },
                    {
                        "$apply=groupby((CustomerId, Name))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 1}},
                            new Dictionary<string, object> {{"Name", "Highest"}, { "CustomerId", 2}},
                        }
                    },
                    {
                        "$apply=groupby((Name, CustomerId))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest" }, { "CustomerId", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest" }, { "CustomerId", 1}},
                        }
                    },
                    {
                        "$apply=groupby((Name, CustomerId))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest" }, { "CustomerId", 4}},
                            new Dictionary<string, object> {{"Name", "Lowest" }, { "CustomerId", 5}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", null}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "hobart"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", null}, {"Company/CEO/HomeAddress/State", null}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "hobart"}, {"Company/CEO/HomeAddress/State", null}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}, {"Company/CEO/HomeAddress/State", "WA"}},
                            new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}, {"Company/CEO/HomeAddress/State", "WA"}},
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/EmployeeName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"} }
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null}, { "Company/CEO/BaseSalary", null} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"}, { "Company/CEO/BaseSalary", 0M} }
                        }
                    },
                    {
                        "$apply=groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"}, { "Company/CEO/BaseSalary", 20M} },
                            new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} }
                        }
                    },
                    {
                        "$apply=groupby((Name))&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$skip=2&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$orderby=Name desc",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$skip=2&$orderby=Name desc",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                        }
                    },
                    {
                        "$apply=groupby((CustomerId, Name))&$orderby=CustomerId",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 1}},
                            new Dictionary<string, object> {{"Name", "Highest"}, { "CustomerId", 2}},
                        }
                    },
                    {
                        "$apply=groupby((CustomerId, Name))&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, { "CustomerId", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 1}},
                        }
                    },
                    {
                        "$apply=groupby((CustomerId, Name))&$orderby=Name&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 4}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 5}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$orderby=Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))&$orderby=Address/State",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                        }
                    },
                    {
                        "$apply=compute(0 as ComputeProperty)/groupby((ComputeProperty))&$orderby=ComputeProperty desc",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{ "ComputeProperty", 0}},
                        }
                    },
                };
            }
        }


        // Legal filter queries usable against CustomerFilterTestData.
        // Tuple is: filter, expected list of customer ID's
        public static TheoryDataSet<string, int[]> CustomerTestFilters
        {
            get
            {
                return new TheoryDataSet<string, int[]>
                {
                    // Primitive properties
                    { "Name eq 'Highest'", new int[] { 2 } },
                    { "endswith(Name, 'est')", new int[] { 1, 2, 4, 5 } },

                    // Complex properties
                    { "Address/City eq 'redmond'", new int[] { 1, 5 } },
                    { "contains(Address/City, 'e')", new int[] { 1, 2, 5 } },
                    { "Company/CEO/HomeAddress/City eq 'redmond'", new int[] { 1, 4 } },
                    { "contains(Company/CEO/HomeAddress/City, 'e')", new int[] { 1, 2, 4 } },

                    // Primitive property collections
                    { "Aliases/any(alias: alias eq 'alias34')", new int[] { 3, 4 } },
                    { "Aliases/any(alias: alias eq 'alias4')", new int[] { 4 } },
                    { "Aliases/all(alias: alias eq 'alias2')", new int[] { 2 } },

                    // Navigational properties
                    { "Orders/any(order: order/OrderId eq 12)", new int[] { 1 } },
                    { "startswith(Company/CompanyName, 'company')", new int[] { 1, 2, 3, 4 } },
                    { "Company/CompanyName eq 'company1'", new int[] { 1, 2 } },
                    { "Company/CompanyName eq 'company2'", new int[] { 3 } },
                    { "Company/CompanyName eq 'company3'", new int[] { 4 } },
                    { "Company/CEO/EmployeeName eq 'john'", new int[] { 1, 3 } },
                    { "Company/CEO/EmployeeName eq 'tom'", new int[] { 2 } },
                    { "Company/CEO/EmployeeName eq 'alex'", new int[] { 4 } },
                    { "Company/CEO/BaseSalary eq 0", new int[] { 4 } },
                    { "Company/CEO/BaseSalary eq 20", new int[] { 1, 2, 3 } },
                };
            }
        }

        // Test data used by CustomerTestApplies TheoryDataSet
        public static List<Customer> CustomerApplyTestData
        {
            get
            {
                List<Customer> customerList = new List<Customer>();

                Customer c = new Customer
                {
                    CustomerId = 1,
                    Name = "Lowest",
                    SharePrice = 10,
                    Address = new Address { City = "redmond", State = "WA" },
                    DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test1" }, { "IntProp", 1 }, { "MixedProp", 1 } },
                    StartDate = new DateTimeOffset(new DateTime(2018, 02, 07, 1, 2, 3)),
                };
                c.Company = new Company()
                {
                    CompanyName = "company1",
                    CEO = new Employee()
                    {
                        EmployeeName = "john",
                        BaseSalary = 20,
                        HomeAddress = new Address { City = "redmond", State = "WA" }
                    }
                };
                c.Orders = new List<Order>
                {
                    new Order { OrderId = 11, Customer = c },
                    new Order { OrderId = 12, Customer = c },
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 2,
                    Name = "Highest",
                    SharePrice = 2.5M,
                    Address = new Address { City = "seattle", State = "WA" },
                    Aliases = new List<string> { "alias2", "alias2" },
                    DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test2" }, { "IntProp", 2 }, { "MixedProp", "String" } },
                    StartDate = new DateTimeOffset(new DateTime(2017, 03, 07, 5, 6, 7))
                };
                c.Company = new Company()
                {
                    CompanyName = "company1",
                    CEO = new Employee()
                    {
                        EmployeeName = "tom",
                        BaseSalary = 20,
                        HomeAddress = new Address { City = "seattle", State = "WA" }
                    }
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 3,
                    Name = "Middle",
                    Address = new Address { City = "hobart" },
                    Aliases = new List<string> { "alias2", "alias34", "alias31" },
                    DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test3" } },
                    StartDate = new DateTimeOffset(new DateTime(2018, 01, 01, 2, 3, 4)),
                };
                c.Company = new Company()
                {
                    CompanyName = "company2",
                    CEO = new Employee()
                    {
                        EmployeeName = "john",
                        BaseSalary = 20,
                        HomeAddress = new Address { City = "hobart" }
                    }
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 4,
                    Name = "Lowest",
                    Aliases = new List<string> { "alias34", "alias4" },
                    StartDate = new DateTimeOffset(new DateTime(2016, 05, 07, 2, 3, 4)),
                };
                c.Company = new Company()
                {
                    CompanyName = "company3",
                    CEO = new Employee()
                    {
                        EmployeeName = "alex",
                        HomeAddress = new Address { City = "redmond", State = "WA" }
                    }
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 5,
                    Name = "Lowest",
                    SharePrice = 10,
                    Address = new Address { City = "redmond", State = "WA" },
                };
                customerList.Add(c);

                return customerList;
            }
        }

        public static TheoryDataSet<string> AppliesWithReferencesOnGroupedOut
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "$apply=groupby((Name))&$filter=CustomerId eq 1",
                    "$apply=groupby((Name))/filter(CustomerId eq 1)",
                    "$apply=groupby((Name))/filter(Address/City eq 1)",
                    "$apply=groupby((Name))/groupby((CustomerId))",
                    "$apply=groupby((Company/CEO/EmployeeName))&$filter=CustomerId eq 1",
                    "$apply=groupby((Company/CEO/EmployeeName))/filter(CustomerId eq 1)",
                    "$apply=groupby((Company/CEO/EmployeeName))/filter(Address/City eq 1)",
                    "$apply=groupby((Company/CEO/EmployeeName))/groupby((CustomerId))",
                    "$apply=groupby((Company/CEO/EmployeeName))/groupby((Company/CEO/BaseSalary))",
                    "$apply=groupby((Company/CEO/EmployeeName))/filter(Company/CEO/BaseSalary eq 20)",
                    "$apply=groupby((Company/CEO/EmployeeName))&$filter=Company/CEO/BaseSalary eq 20",
                    "$apply=groupby((Company/CEO/EmployeeName))/groupby((Company/CEO/BaseSalary))"
                };
            }
        }

        [Theory]
        [MemberData(nameof(CustomerTestApplies))]
        public void ApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Company_EntityType()
                            .Add_CustomerCompany_Relationship()
                            .Add_Employee_EntityType_With_HomeAddress()
                            .Add_CompanyEmployees_Relationship()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockContainer() };
            var queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$apply", filter } });
            var applyOption = new ApplyQueryOption(filter, context, queryOptionParser);
            IEnumerable<Customer> customers = CustomerApplyTestData;

            // Act
            IQueryable queryable = applyOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

            Assert.Equal(aggregation.Count(), actualCustomers.Count());

            var aggEnum = actualCustomers.GetEnumerator();

            foreach (var expected in aggregation)
            {
                aggEnum.MoveNext();
                var agg = aggEnum.Current;
                foreach (var key in expected.Keys)
                {
                    object value = GetValue(agg, key);
                    Assert.Equal(expected[key], value);
                }
            }
        }

        [Theory]
        [MemberData(nameof(CustomerTestAppliesMixedWithOthers))]
        public void ClausesAfterApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Company_EntityType()
                            .Add_CustomerCompany_Relationship()
                            .Add_Employee_EntityType_With_HomeAddress()
                            .Add_CompanyEmployees_Relationship()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));

            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + filter, configuration, "OData");

            var options = new ODataQueryOptions(context, request);

            IEnumerable<Customer> customers = CustomerApplyTestData;

            // Act
            IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

            Assert.Equal(aggregation.Count(), actualCustomers.Count());

            var aggEnum = actualCustomers.GetEnumerator();

            foreach (var expected in aggregation)
            {
                aggEnum.MoveNext();
                var agg = aggEnum.Current;
                foreach (var key in expected.Keys)
                {
                    object value = GetValue(agg, key);
                    Assert.Equal(expected[key], value);
                }
            }
        }

        [Theory]
        [MemberData(nameof(AppliesWithReferencesOnGroupedOut))]
        public void ClausesWithGroupedOutReferences_Throw_ODataException(string clause)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));

            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + clause, configuration, "OData");

            var options = new ODataQueryOptions(context, request);

            IEnumerable<Customer> customers = CustomerApplyTestData;

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() =>
            {
                IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });
            });
        }

        [Theory]
        [MemberData(nameof(CustomerTestAppliesForPaging))]
        public void StableSortingAndPagingApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Company_EntityType()
                            .Add_CustomerCompany_Relationship()
                            .Add_Employee_EntityType_With_HomeAddress()
                            .Add_CompanyEmployees_Relationship()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));

            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + filter, configuration, "OData");

            var options = new ODataQueryOptions(context, request);

            IEnumerable<Customer> customers = CustomerApplyTestData;
            // Act
            IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True, PageSize = 2 });
            
            // Assert
            Assert.NotNull(queryable);
            var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

            Assert.Equal(aggregation.Count(), actualCustomers.Count());

            var aggEnum = actualCustomers.GetEnumerator();

            foreach (var expected in aggregation)
            {
                aggEnum.MoveNext();
                var agg = aggEnum.Current;
                foreach (var key in expected.Keys)
                {
                    object value = GetValue(agg, key);
                    Assert.Equal(expected[key], value);
                }
            }
        }

#if !NETCORE // TODO 939: This crashes on AspNetCore
        [Theory]
        [MemberData(nameof(CustomerTestFilters))]
        public void ApplyTo_Returns_Correct_Queryable_ForFilter(string filter, int[] customerIds)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Company_EntityType()
                            .Add_CustomerCompany_Relationship()
                            .Add_Employee_EntityType_With_HomeAddress()
                            .Add_CompanyEmployees_Relationship()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockContainer() };
            var queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$apply", string.Format("filter({0})", filter) } });
            var filterOption = new ApplyQueryOption(string.Format("filter({0})", filter), context, queryOptionParser);
            IEnumerable<Customer> customers = CustomerApplyTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(
                customerIds,
                actualCustomers.Select(customer => customer.CustomerId));
        }
#endif

        [Fact]
        public async Task ApplyToSerializationWorks()
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();

            var controllers = new[] { typeof(MetadataController), typeof(CustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/odata/Customers?$apply=groupby((Name), aggregate(CustomerId with sum as TotalId))");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response);
            var result = await response.Content.ReadAsObject<JObject>();
            var results = result["value"] as JArray;
            Assert.Equal(3, results.Count);
            Assert.Equal("10", results[0]["TotalId"].ToString());
            Assert.Equal("Lowest", results[0]["Name"].ToString());
            Assert.Equal("2", results[1]["TotalId"].ToString());
            Assert.Equal("Highest", results[1]["Name"].ToString());
            Assert.Equal("3", results[2]["TotalId"].ToString());
            Assert.Equal("Middle", results[2]["Name"].ToString());
        }

        [Fact]
        public async Task ApplyToSerializationWorksForCompelxTypes()
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();

            var controllers = new[] { typeof(MetadataController), typeof(CustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", model);
            });

            HttpClient client = TestServerFactory.CreateClient(server);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/odata/Customers?$apply=groupby((Address/City), aggregate(CustomerId with sum as TotalId))");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response);
            var result = await response.Content.ReadAsObject<JObject>();
            var results = result["value"] as JArray;
            Assert.Equal(4, results.Count);
            Assert.Equal("6", results[0]["TotalId"].ToString());
            var address0 = results[0]["Address"] as JObject;
            Assert.Equal("redmond", address0["City"].ToString());
        }


        private object GetValue(DynamicTypeWrapper wrapper, string path)
        {
            var parts = path.Split('/');
            foreach (var part in parts)
            {
                object value;
                wrapper.TryGetPropertyValue(part, out value);
                wrapper = value as DynamicTypeWrapper;
                if (wrapper == null)
                {
                    return value;
                }
            }

            Assert.False(true, "Property " + path + " not found");
            return null;
        }
    }

    public class CustomersController : TestODataController
    {
        private List<Customer> _customers;

        public CustomersController()
        {
            _customers = ApplyQueryOptionTest.CustomerApplyTestData;
        }

#if NETCORE
        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            return _customers.AsQueryable();
        }
#else
        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(_customers);
        }
#endif
    }
}
