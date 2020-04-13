// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE2x
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class SecurityTests : WebHostTestBase<SecurityTests>
    {
        public SecurityTests(WebHostTestFixture<SecurityTests> fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<string> DoSAttackData
        {
            get
            {
                var data = new TheoryDataSet<string>();
                for (int i = 5; i <= 100; i += 20)
                {
                    data.Add(new AttackStringBuilder().Append("$filter=RelatedProduct").Repeat("/RelatedProduct", i).Append("/ID gt 10").ToString());
                    data.Add(new AttackStringBuilder().Append("$filter=").Repeat("Price eq 1 and ", i).Remove(4).ToString());
                    data.Add(new AttackStringBuilder().Append("$filter=").Repeat("trim(", i).Append("Name").Repeat(")", i).Append(" eq 'a'").ToString());
                    data.Add(new AttackStringBuilder().Append("$filter=").Repeat("(", i).Append("Price eq 1").Repeat(")", i).ToString());
                    data.Add(new AttackStringBuilder().Append("$filter=").Repeat("Price div ", i).Append("Price eq 1").ToString());
                    data.Add(new AttackStringBuilder().Append("$filter=").Repeat("not ", i).Append("(1 eq 1)").ToString());
                    data.Add(new AttackStringBuilder().Append("$filter=").Repeat("substring(", i).Append("Name").Repeat(",0)", i).Append(" eq 'a'").ToString());
                    data.Add(new AttackStringBuilder().Append("$filter=contains('").Repeat("a", i).Append("b").Append("','").Repeat("a", i * 100).Append("')").ToString());
                }
                return data;
            }
        }

        public static TheoryDataSet<string> AnyAllDoSAttackData
        {
            get
            {
                var data = new TheoryDataSet<string>();
                for (int i = 0; i <= 10; i += 5)
                {
                    data.Add(new AttackStringBuilder().Append("$filter=Actors/any(").Repeat("a{0}: a{0}/Movies/any(m{0}: m{0}/Actors/any(", i).Append("actor: actor/Name eq 'Kevin'").Repeat(")", (2 * i) + 1).ToString());
                    data.Add(new AttackStringBuilder().Append("$filter=Actors/all(").Repeat("a{0}: a{0}/Movies/all(m{0}: m{0}/Actors/all(", i).Append("actor: actor/Name eq 'Kevin'").Repeat(")", (2 * i) + 1).ToString());
                }
                //data.Add(new AttackStringBuilder().Append("$filter=Actors/any(a0: a0/Movies/any(m0: m0/MovieId eq 1000))").ToString());
                return data;
            }
        }

        public static TheoryDataSet<string> InvalidUnicodeData
        {
            get
            {
                var data = new TheoryDataSet<string>();
                data.Add("$å¤§å½©ç½??=å¤§å½©ç½??");
                data.Add("$top=å¤§å½©ç½??");
                data.Add("$skip=å¤§å½©ç½??");
                data.Add("$orderby=å¤§å½©ç½??");
                data.Add("$filter=å¤§å½©ç½??");
                data.Add("$filter=Name eq 'å¤§å½©ç½??'");
                data.Add("$filter=å¤§å½©ç½?? eq 'å¤§å½©ç½??'");
                data.Add("$filter=Name eqå¤§ 'abc'");
                data.Add("$filter=å¤§å½©ç½??('abc')");
                data.Add("$filter=Price eq -å¤§å½©ç½??");
                return data;
            }
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
        }

        [Theory]
        [MemberData(nameof(DoSAttackData))]
        public async Task TestDosAttack(string filter)
        {
            Console.WriteLine(filter);
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetProducts?" + filter);
            var result = await response.Content.ReadAsStringAsync();
        }

        [Theory]
        [MemberData(nameof(DoSAttackData))]
        public void TestDosAttackWithMultipleThreads(string filter)
        {
            Parallel.For(0, 3, async i =>
            {
                await TestDosAttack(filter);
            });
        }

        [Theory]
        [MemberData(nameof(AnyAllDoSAttackData))]
        public async Task TestAnyAllDosAttack(string filter)
        {
            this.Client.Timeout = TimeSpan.FromDays(1);
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetMoviesBig?" + filter);
            var result = await response.Content.ReadAsStringAsync();
        }

        //[Theory]
        //[MemberData(nameof(AnyAllDoSAttackData))]
        //public void TestAnyAllDosAttackWithMultipleThreads(string filter)
        //{
        //    Parallel.For(0, 10, i =>
        //    {
        //        TestAnyAllDosAttack(filter);
        //    });
        //}

        [Theory]
        [MemberData(nameof(InvalidUnicodeData))]
        public async Task TestInvalidUnicodeAttack(string query)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetProducts?" + query);
            var result = await response.Content.ReadAsStringAsync();
        }
    }
}
#endif
