using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;
using System.Reflection;

namespace System.Net.Http.Formatting
{
    public class JsonNetValidationTest
    {
        public static IEnumerable<object[]> Theories
        {
            get
            {
                return new TheoryDataSet<string, Type, int>()
                {
                    // Type coercion

                    {"null", typeof(int), 1},
                    {"45", typeof(string), 0},
                    {"random text", typeof(DateTimeOffset), 1},
                    {"[1,2,3]", typeof(string[]), 0},

                    {"\"foo\"", typeof(int), 1},
                    {"\"foo\"", typeof(DateTime), 1},

                    {"[\"a\",\"b\",\"45\",34]", typeof(int[]), 2},
                    {"[\"a\",\"b\",\"45\",34]", typeof(DateTime[]), 4},

                    // Required members

                    {"{}", typeof(DataContractWithRequiredMembers), 2},
                    {"[{},{},{}]", typeof(DataContractWithRequiredMembers[]), 6},

                    // Throwing setters

                    {"{\"Throws\":\"foo\"}", typeof(TypeWithThrowingSetter), 1},
                    {"[{\"Throws\":\"foo\"},{\"Throws\":\"foo\"}]", typeof(TypeWithThrowingSetter[]), 2},
                };
            }
        }

        [Theory]
        [PropertyData("Theories")]
        public void ModelErrorsPopulatedWithValidationErrors(string json, Type type, int expectedErrors)
        {
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            formatter.RequiredMemberSelector = new SimpleRequiredMemberSelector();

            int errors = 0;
            Mock<IFormatterLogger> mockLogger = new Mock<IFormatterLogger>();
            mockLogger.Setup(mock => mock.LogError(It.IsAny<string>(), It.IsAny<string>())).Callback(() => errors++);


            Assert.DoesNotThrow(() => JsonNetSerializationTest.Deserialize(json, type, formatter, mockLogger.Object));
            Assert.Equal(expectedErrors, errors);
        }
    }

    // this IRMS treats all member names that start with "Required" as required
    public class SimpleRequiredMemberSelector : IRequiredMemberSelector
    {
        public bool IsRequiredMember(MemberInfo member)
        {
            return member.Name.StartsWith("Required");
        }
    }

    public class DataContractWithRequiredMembers
    {
        public string Required1;
        public string Required2;
        public string Optional;
    }

    public class TypeWithThrowingSetter
    {
        public string Throws
        {
            get
            {
                return "foo";
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}