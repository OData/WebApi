using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Net.Http.Formatting;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Validation
{
    public class ModelValidationRequiredMemberSelectorTest
    {
        [Theory]
        [InlineData("Customer", true)]
        [InlineData("ID", true)]
        [InlineData("Item", true)]
        [InlineData("UselessInfo", false)]
        public void RequiredMembersRecognized(string propertyName, bool isRequired)
        {
            HttpConfiguration config = new HttpConfiguration();
            IRequiredMemberSelector selector = new ModelValidationRequiredMemberSelector(config.ServiceResolver.GetModelMetadataProvider(), config.ServiceResolver.GetModelValidatorProviders());
            Assert.Equal(isRequired, selector.IsRequiredMember(typeof(PurchaseOrder).GetProperty(propertyName)));
        }
    }

    [DataContract]
    public class PurchaseOrder
    {
        [Required]
        public string Customer { get; set; }

        [DataMember(IsRequired=true)]
        public int ID { get; set; }

        [Required]
        [DataMember(IsRequired=true)]
        public string Item { get; set; }

        public string UselessInfo { get; set; }
    }
}
