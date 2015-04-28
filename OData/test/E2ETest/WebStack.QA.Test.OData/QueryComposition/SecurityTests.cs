using System;
using System.Threading.Tasks;
using System.Web.Http;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class SecurityTests : ODataTestBase
    {
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
                    data.Add(new AttackStringBuilder().Append("$filter=substringof('").Repeat("a", i).Append("b").Append("','").Repeat("a", i * 100).Append("')").ToString());
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

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }

        [Theory]
        [PropertyData("DoSAttackData")]
        public void TestDosAttack(string filter)
        {
            Console.WriteLine(filter);
            var response = this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetProducts?" + filter).Result;
            var result = response.Content.ReadAsStringAsync().Result;
        }

        [Theory]
        [PropertyData("DoSAttackData")]
        public void TestDosAttackWithMultipleThreads(string filter)
        {
            Parallel.For(0, 3, i =>
            {
                TestDosAttack(filter);
            });
        }

        [Theory]
        [PropertyData("AnyAllDoSAttackData")]
        public void TestAnyAllDosAttack(string filter)
        {
            this.Client.Timeout = TimeSpan.FromDays(1);
            var response = this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetMoviesBig?" + filter).Result;
            var result = response.Content.ReadAsStringAsync().Result;
        }

        //[Theory]
        //[PropertyData("AnyAllDoSAttackData")]
        //public void TestAnyAllDosAttackWithMultipleThreads(string filter)
        //{
        //    Parallel.For(0, 10, i =>
        //    {
        //        TestAnyAllDosAttack(filter);
        //    });
        //}

        [Theory]
        [PropertyData("InvalidUnicodeData")]
        public void TestInvalidUnicodeAttack(string query)
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetProducts?" + query).Result;
            var result = response.Content.ReadAsStringAsync().Result;
        }
    }
}
