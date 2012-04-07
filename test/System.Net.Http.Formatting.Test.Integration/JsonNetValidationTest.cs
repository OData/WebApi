// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;

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
            Mock<IFormatterLogger> mockLogger = new Mock<IFormatterLogger>() { };


            JsonNetSerializationTest.Deserialize(json, type, formatter, mockLogger.Object);

            mockLogger.Verify(mock => mock.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(expectedErrors));
        }

        [Fact]
        public void HittingMaxDepthRaisesOnlyOneValidationError()
        {
            // Arrange
            JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            Mock<IFormatterLogger> mockLogger = new Mock<IFormatterLogger>();

            StringBuilder sb = new StringBuilder("{'A':null}");
            for (int i = 0; i < 5000; i++)
            {
                sb.Insert(0, "{'A':");
                sb.Append('}');
            }
            string json = sb.ToString();

            // Act
            JsonNetSerializationTest.Deserialize(json, typeof(Nest), formatter, mockLogger.Object);

            // Assert
            mockLogger.Verify(mock => mock.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
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

    public class Nest
    {
        public Nest A { get; set; }
    }
}