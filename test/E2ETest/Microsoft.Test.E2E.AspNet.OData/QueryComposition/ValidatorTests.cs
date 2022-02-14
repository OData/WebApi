//-----------------------------------------------------------------------------
// <copyright file="ValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class ValidatorTests_Todo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
    }

    public class ValidatorTestsController : TestNonODataController
    {
        private static List<ValidatorTests_Todo> todoes = new List<ValidatorTests_Todo>();
        static ValidatorTestsController()
        {
            for (int i = 0; i < 100; i++)
            {
                todoes.Add(new ValidatorTests_Todo
                {
                    ID = i,
                    Name = "Test " + i,
                    DateTime = DateTime.Now.Add(TimeSpan.FromDays(i)),
                    Double = Math.Sqrt(i)
                });
            }
        }

        [HttpGet]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Skip | AllowedQueryOptions.Top)]
        public IQueryable<ValidatorTests_Todo> OnlySupportTopAndSkip()
        {
            return todoes.AsQueryable();
        }

        [HttpGet]
        [EnableQuery(MaxTop = 10, MaxSkip = 10)]
        public IQueryable<ValidatorTests_Todo> MaxTopSkipIs10()
        {
            return todoes.AsQueryable();
        }

        [HttpGet]
        [EnableQuery(AllowedOrderByProperties = "ID")]
        public IQueryable<ValidatorTests_Todo> OnlySupportOrderByID()
        {
            return todoes.AsQueryable();
        }

        [HttpGet]
        [EnableQuery(AllowedLogicalOperators = AllowedLogicalOperators.Equal)]
        public IQueryable<ValidatorTests_Todo> OnlySupportEqual()
        {
            return todoes.AsQueryable();
        }

        [HttpGet]
        [EnableQuery(AllowedArithmeticOperators = AllowedArithmeticOperators.Add | AllowedArithmeticOperators.Subtract)]
        public IQueryable<ValidatorTests_Todo> OnlySupportAddAndSub()
        {
            return todoes.AsQueryable();
        }

        [HttpGet]
        [EnableQuery(AllowedFunctions = AllowedFunctions.Substring)]
        public IQueryable<ValidatorTests_Todo> OnlySupportSubString()
        {
            return todoes.AsQueryable();
        }

        [HttpGet]
        [EnableQuery(AllowedFunctions = AllowedFunctions.Date | AllowedFunctions.Time)]
        public IQueryable<ValidatorTests_Todo> OnlySupportDateAndTime()
        {
            return todoes.AsQueryable();
        }

        [HttpPost]
        public ITestActionResult ValidateOptions([FromBody]ODataValidationSettings settings, ODataQueryOptions<ValidatorTests_Todo> options)
        {
            try
            {
                options.Validate(settings);
            }
            catch (ODataException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(options.ApplyTo(todoes.AsQueryable()) as IQueryable<ValidatorTests_Todo>);
        }

        [HttpGet]
        [EnableQuery(AllowedFunctions = AllowedFunctions.AllFunctions)]
        public IQueryable<ValidatorTests_Todo> SupportAllFunctions()
        {
            return todoes.AsQueryable();
        }

        [HttpGet]
        public ITestActionResult ValidateWithCustomValidator(ODataQueryOptions<ValidatorTests_Todo> options)
        {
            if (options.Filter != null)
            {
                options.Filter.Validator = new CustomFilterValidator(new DefaultQuerySettings
                {
                    EnableFilter = true
                });
            }
            try
            {
                ODataValidationSettings settings = new ODataValidationSettings();
                options.Validate(settings);
            }
            catch (ODataException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(options.ApplyTo(todoes.AsQueryable()) as IQueryable<ValidatorTests_Todo>);
        }
    }

    public class CustomFilterValidator : FilterQueryValidator
    {
        private bool visited = false;

        public CustomFilterValidator(DefaultQuerySettings defaultQuerySettings)
            :base(defaultQuerySettings)
        {
        }

        public override void ValidateSingleValuePropertyAccessNode(SingleValuePropertyAccessNode propertyAccessNode, ODataValidationSettings settings)
        {
            if (propertyAccessNode.Property.Name == "ID")
            {
                visited = true;
            }
            base.ValidateSingleValuePropertyAccessNode(propertyAccessNode, settings);
        }

        public override void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings)
        {
            base.Validate(filterQueryOption, settings);

            if (!visited)
            {
                throw new ODataException("$filter query must contain ID property");
            }
        }
    }

    public class ValidatorTests : WebHostTestBase
    {
        public ValidatorTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<string, bool> ValidateOptionsData
        {
            get
            {
                var set = new TheoryDataSet<string, bool>();
                set.Add("$top=10", true);
                set.Add("$top=11", false);
                set.Add(new AttackStringBuilder().Append("$filter=").Repeat("not ", 50).Append("(1 mul 1 eq 1)").ToString(), false);
                set.Add("$top=10&$orderby=ID,DateTime&$filter=contains(Name, 'Test') and (DateTime gt 2012-12-04T00:00:00Z or 1 add 1 eq 2) and floor(Double) gt 5", true);
                set.Add("$top=10&$orderby=ID,DateTime&$filter=contains(Name, 'Test') and (DateTime gt 2012-12-04T00:00:00Z or 1 add 1 eq 2) and floor(Decimal) gt 5", true);
                set.Add("$top=10&$orderby=ID,DateTime&$filter=contains(Name, 'Test') and (DateTime gt 2012-12-04T00:00:00Z or 1 add 1 eq 2) and length(Name) gt 5", false);

                return set;
            }
        }

        public static TheoryDataSet<AllowedFunctions, AllowedFunctions> AllowedFunctionsData
        {
            get
            {
                var data = new TheoryDataSet<AllowedFunctions, AllowedFunctions>();
                data.Add(AllowedFunctions.AllMathFunctions, AllowedFunctions.Floor);
                data.Add(AllowedFunctions.AllMathFunctions, AllowedFunctions.Ceiling);
                data.Add(AllowedFunctions.AllMathFunctions, AllowedFunctions.Round);

                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.Day);
                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.Hour);
                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.Minute);
                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.Month);
                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.Second);
                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.Year);
                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.FractionalSeconds);
                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.Date);
                data.Add(AllowedFunctions.AllDateTimeFunctions, AllowedFunctions.Time);

                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.Concat);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.EndsWith);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.IndexOf);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.Length);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.StartsWith);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.Substring);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.Contains);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.ToLower);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.ToUpper);
                data.Add(AllowedFunctions.AllStringFunctions, AllowedFunctions.Trim);

                var values = Enum.GetValues(typeof(AllowedFunctions));
                foreach (var value in values)
                {
                    data.Add(AllowedFunctions.AllFunctions, (AllowedFunctions)value);
                }

                return data;
            }
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [InlineData("/api/ValidatorTests/OnlySupportTopAndSkip?$top=1&$skip=10", true)]
        [InlineData("/api/ValidatorTests/OnlySupportTopAndSkip?$orderby=ID", false)]
        [InlineData("/api/ValidatorTests/MaxTopSkipIs10?$top=9&$skip=10", true)]
        [InlineData("/api/ValidatorTests/MaxTopSkipIs10?$top=11", false)]
        [InlineData("/api/ValidatorTests/MaxTopSkipIs10?$skip=11", false)]
        [InlineData("/api/ValidatorTests/OnlySupportOrderByID?$orderby=ID", true)]
        [InlineData("/api/ValidatorTests/OnlySupportOrderByID?$orderby=Name", false)]
        [InlineData("/api/ValidatorTests/OnlySupportEqual?$filter=ID eq 10", true)]
        [InlineData("/api/ValidatorTests/OnlySupportEqual?$filter=ID gt 10", false)]
        [InlineData("/api/ValidatorTests/OnlySupportAddAndSub?$filter=(1 add 1 eq 2) and (1 sub 1 eq 0)", true)]
        [InlineData("/api/ValidatorTests/OnlySupportAddAndSub?$filter=1 mul 2 eq 2", false)]
        [InlineData("/api/ValidatorTests/OnlySupportSubString?$filter=substring(Name, 5) eq '10'", true)]
        [InlineData("/api/ValidatorTests/OnlySupportSubString?$filter=contains(Name, '10')", false)]
        [InlineData("/api/ValidatorTests/SupportAllFunctions?$filter=contains(Name, '10')", true)]
        [InlineData("/api/ValidatorTests/OnlySupportDateAndTime?$filter=date(DateTime) eq 2015-02-28", true)]
        [InlineData("/api/ValidatorTests/OnlySupportDateAndTime?$filter=time(DateTime) eq 01:02:03.0040000", true)]
        [InlineData("/api/ValidatorTests/OnlySupportDateAndTime?$filter=year(DateTime) eq 2015", false)]
        public async Task VerifyQueryResult(string query, bool success)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + query);
            if (success)
            {
                response.EnsureSuccessStatusCode();
            }
            else
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Theory]
        [MemberData(nameof(ValidateOptionsData))]
        public async Task VerifyValidateOptions(string query, bool success)
        {
            var settings = new ODataValidationSettings
            {
                MaxTop = 10,
                MaxSkip = 10,
                AllowedQueryOptions = AllowedQueryOptions.Top | AllowedQueryOptions.OrderBy | AllowedQueryOptions.Filter,
                AllowedLogicalOperators = AllowedLogicalOperators.Not | AllowedLogicalOperators.Equal | AllowedLogicalOperators.GreaterThan | AllowedLogicalOperators.And | AllowedLogicalOperators.Or,
                AllowedFunctions = AllowedFunctions.AllMathFunctions | AllowedFunctions.Contains | AllowedFunctions.AllDateTimeFunctions,
                AllowedArithmeticOperators = AllowedArithmeticOperators.Add | AllowedArithmeticOperators.Subtract
            };
            settings.AllowedOrderByProperties.Add("ID");
            settings.AllowedOrderByProperties.Add("DateTime");

            var response = await this.Client.PostAsJsonAsync(this.BaseAddress + "/api/ValidatorTests/ValidateOptions?" + query, settings);
            if (success)
            {
                response.EnsureSuccessStatusCode();
            }
            else
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Theory]
        [MemberData(nameof(AllowedFunctionsData))]
        public void VerifyAllowedFunctions(AllowedFunctions value1, AllowedFunctions value2)
        {
            Assert.Equal(value2, value1 & value2);
        }

        [Theory]
        [InlineData("/api/ValidatorTests/ValidateWithCustomValidator?$filter=ID gt 2", (int)HttpStatusCode.OK)]
        [InlineData("/api/ValidatorTests/ValidateWithCustomValidator?$filter=Name eq 'One'", (int)HttpStatusCode.BadRequest)]
        public async Task VerifyCustomValidator(string url, int statusCode)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url);
            Assert.Equal(statusCode, (int)response.StatusCode);
        }
    }
}
