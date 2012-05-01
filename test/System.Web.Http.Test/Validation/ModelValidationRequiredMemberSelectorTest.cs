// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Runtime.Serialization;
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
            IRequiredMemberSelector selector = new ModelValidationRequiredMemberSelector(config.Services.GetModelMetadataProvider(), config.Services.GetModelValidatorProviders());
            Assert.Equal(isRequired, selector.IsRequiredMember(typeof(PurchaseOrder).GetProperty(propertyName)));
        }

        [Theory]
        [InlineData("ProtectedGet")]
        [InlineData("NoGet")]
        [InlineData("Internal")]
        public void IsRequiredMemberReturnsFalseForInvalidProperties(string propertyName)
        {
            HttpConfiguration config = new HttpConfiguration();
            IRequiredMemberSelector selector = new ModelValidationRequiredMemberSelector(config.Services.GetModelMetadataProvider(), config.Services.GetModelValidatorProviders());
            Assert.False(selector.IsRequiredMember(typeof(BadProperties).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
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

    public class BadProperties
    {
        public string ProtectedGet
        {
            protected get;
            set;
        }

        public string NoGet
        {
            set { }
        }

        internal string Internal
        {
            get;
            set;
        }
    }
}
