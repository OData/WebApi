// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Query
{
    public class QueryValidatorTest
    {
        QueryValidator _queryValidator = QueryValidator.Instance;

        [Fact]
        public void XmlIgnoreAttributeCausesInvalidOperation()
        {
            IQueryable<QueryValidatorSampleClass> query = new QueryValidatorSampleClass[0].AsQueryable();

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Where((sample) => sample.XmlIgnoreProperty == 0)),
                "The property or field 'XmlIgnoreProperty' in type 'QueryValidatorSampleClass' is not accessible.");
        }

        [Fact]
        public void IgnoreDataMemberAttributeCausesInvalidOperation()
        {
            IQueryable<QueryValidatorSampleClass> query = new QueryValidatorSampleClass[0].AsQueryable();

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Where((sample) => sample.IgnoreDataMemberProperty == 0)),
                "The property or field 'IgnoreDataMemberProperty' in type 'QueryValidatorSampleClass' is not accessible.");
        }

        [Fact]
        public void NonSerializedAttributeCausesInvalidOperation()
        {
            IQueryable<QueryValidatorSampleClass> query = new QueryValidatorSampleClass[0].AsQueryable();

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Where((sample) => sample.NonSerializedAttributeField == 0)),
                "The property or field 'NonSerializedAttributeField' in type 'QueryValidatorSampleClass' is not accessible.");
        }

        [Fact]
        public void NormalPropertyAccessDoesnotThrow()
        {
            IQueryable<QueryValidatorSampleClass> query = new QueryValidatorSampleClass[0].AsQueryable();

            _queryValidator.Validate(query.Where((sample) => sample.NormalProperty == 0));
        }

        [Fact]
        public void NormalFieldAccessDoesnotThrow()
        {
            IQueryable<QueryValidatorSampleClass> query = new QueryValidatorSampleClass[0].AsQueryable();

            _queryValidator.Validate(query.Where((sample) => sample.PublicField == 0));
        }

        [Fact]
        public void DataContractDataMemberPropertyAccessDoesnotThrow()
        {
            IQueryable<QueryValidatorSampleDataContractClass> query = new QueryValidatorSampleDataContractClass[0].AsQueryable();

            _queryValidator.Validate(query.Where((sample) => sample.DataMemberProperty == 0));
        }

        [Fact]
        public void DataContractNonDataMemberPropertyAccessCausesInvalidOperation()
        {
            IQueryable<QueryValidatorSampleDataContractClass> query = new QueryValidatorSampleDataContractClass[0].AsQueryable();

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Where((sample) => sample.NonDataMemberProperty == 0)),
                "The property or field 'NonDataMemberProperty' in type 'QueryValidatorSampleDataContractClass' is not accessible.");
        }

        [Fact]
        public void DataContractNonDataMemberFieldAccessCausesInvalidOperation()
        {
            IQueryable<QueryValidatorSampleDataContractClass> query = new QueryValidatorSampleDataContractClass[0].AsQueryable();

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Where((sample) => sample.NonDataMemberField == 0)),
                "The property or field 'NonDataMemberField' in type 'QueryValidatorSampleDataContractClass' is not accessible.");
        }

        [Fact]
        public void SetOnlyPropertyAccessCausesInvalidOperation()
        {
            Mock<IQueryable> query = new Mock<IQueryable>();
            query
                .Setup(q => q.Expression)
                .Returns(
                Expression.PropertyOrField(
                    Expression.Constant(new QueryValidatorSampleClass()),
                    "SetOnlyProperty"));

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Object),
                "The property or field 'SetOnlyProperty' in type 'QueryValidatorSampleClass' is not accessible.");
        }

        [Fact]
        public void NonPublicGetPropertyAccessCausesInvalidOperation()
        {
            Mock<IQueryable> query = new Mock<IQueryable>();
            query
                .Setup(q => q.Expression)
                .Returns(
                Expression.PropertyOrField(
                    Expression.Constant(new QueryValidatorSampleClass()),
                    "NonPublicGetProperty"));

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Object),
                "The property or field 'NonPublicGetProperty' in type 'QueryValidatorSampleClass' is not accessible.");
        }

        public class QueryValidatorSampleClass
        {
            [XmlIgnore]
            public int XmlIgnoreProperty { get; set; }

            [IgnoreDataMember]
            public int IgnoreDataMemberProperty { get; set; }

            [NonSerialized]
            public int NonSerializedAttributeField = 0;

            public int NormalProperty { get; set; }

            public int SetOnlyProperty { set { } }

            public int NonPublicGetProperty { set; private get; }

            public int PublicField;
        }

        [DataContract]
        public class QueryValidatorSampleDataContractClass
        {
            [DataMember]
            public int DataMemberProperty { get; set; }

            public int NonDataMemberProperty { get; set; }

            public int NonDataMemberField;
        }
    }
}
