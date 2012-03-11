using System.Linq;
using System.Runtime.Serialization;
using System.Web.TestUtil;
using System.Xml.Serialization;
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
                "No property or field 'XmlIgnoreProperty' exists in type 'QueryValidatorSampleClass'");
        }

        [Fact]
        public void IgnoreDataMemberAttributeCausesInvalidOperation()
        {
            IQueryable<QueryValidatorSampleClass> query = new QueryValidatorSampleClass[0].AsQueryable();

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Where((sample) => sample.IgnoreDataMemberProperty == 0)),
                "No property or field 'IgnoreDataMemberProperty' exists in type 'QueryValidatorSampleClass'");
        }

        [Fact]
        public void NonSerializedAttributeCausesInvalidOperation()
        {
            IQueryable<QueryValidatorSampleClass> query = new QueryValidatorSampleClass[0].AsQueryable();

            Assert.Throws<InvalidOperationException>(
                () => _queryValidator.Validate(query.Where((sample) => sample.NonSerializedAttributeField == 0)),
                "No property or field 'NonSerializedAttributeField' exists in type 'QueryValidatorSampleClass'");
        }

        [Fact]
        public void NormalPropertyAccessDoesnotThrow()
        {
            IQueryable<QueryValidatorSampleClass> query = new QueryValidatorSampleClass[0].AsQueryable();

            _queryValidator.Validate(query.Where((sample) => sample.NormalProperty == 0));
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
                "No property or field 'NonDataMemberProperty' exists in type 'QueryValidatorSampleDataContractClass'");
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
        }

        [DataContract]
        public class QueryValidatorSampleDataContractClass
        {
            [DataMember]
            public int DataMemberProperty { get; set; }

            public int NonDataMemberProperty { get; set; }
        }
    }
}
