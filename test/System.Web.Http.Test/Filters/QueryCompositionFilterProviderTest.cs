using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.Filters
{
    public class QueryCompositionFilterProviderTest
    {
        private QueryCompositionFilterProvider filterProvider = new QueryCompositionFilterProvider();

        public static TheoryDataSet<Type> GetFiltersReturnsEmptySetForNonQueryableReturnTypesData
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    { typeof(int) },
                    { typeof(string) },
                    { typeof(void) },
                    { typeof(IEnumerable<int>) },
                    { typeof(List<int>) }
                };
            }
        }

        public static TheoryDataSet<Type, Type> GetFiltersReturnsSingleFilterForQueryableReturnTypesData
        {
            get
            {
                return new TheoryDataSet<Type, Type>
                {
                    { typeof(IQueryable<int>), typeof(int) },
                    { typeof(IQueryable<string>), typeof(string)},
                    { typeof(IQueryable<IQueryable<int>>), typeof(IQueryable<int>) },
                    { typeof(Task<IQueryable<int>>), typeof(int) } 
                    // { typeof(HttpResponseMessage), typeof(int) } // static signature problems
                };
            }
        }

        [Theory]
        [PropertyData("GetFiltersReturnsEmptySetForNonQueryableReturnTypesData")]
        public void GetFiltersReturnsEmptySetForNonQueryableReturnTypes(Type returnType)
        {
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>();
            mockActionDescriptor.Setup((actionDescriptor) => actionDescriptor.ReturnType).Returns(returnType);

            var filters = filterProvider.GetFilters(configuration: null, actionDescriptor: mockActionDescriptor.Object);

            Assert.Empty(filters);
        }

        [Theory]
        [PropertyData("GetFiltersReturnsSingleFilterForQueryableReturnTypesData")]
        public void GetFiltersReturnsSingleFilterForQueryableReturnTypes(Type returnType, Type queryType)
        {
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>();
            mockActionDescriptor.Setup((actionDescriptor) => actionDescriptor.ReturnType).Returns(returnType);

            var filters = filterProvider.GetFilters(configuration: null, actionDescriptor: mockActionDescriptor.Object);

            Assert.True(filters.Count() == 1);
            Assert.Equal(Assert.IsType<QueryCompositionFilterAttribute>(filters.First().Instance).QueryElementType, queryType);
        }
    }
}
