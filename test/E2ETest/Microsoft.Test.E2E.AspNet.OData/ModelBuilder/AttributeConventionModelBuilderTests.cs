// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    [DataContract]
    public class SimpleDataContractModel
    {
        [Key]
        [DataMember]
        public int KeyProperty { get; set; }

        [DataMember]
        public string Name { get; set; }

        [Required]
        [DataMember]
        public string RequiredAttribute { get; set; }

        [DataMember(IsRequired = true)]
        public string RequiredProperty2 { get; set; }

        [DataMember(IsRequired = true)]
        [DefaultValue("default")]
        public string RequiredPropertyWithDefaultValue { get; set; }

        public string NotDataMember { get; set; }

        [DataMember(IsRequired = true)]
        public IgnoreMemberModel NavigationProperty1 { get; set; }

        public IgnoreMemberModel NavigationProperty2 { get; set; }

        [DataMember]
        public string ReadOnlyProperty
        {
            get
            {
                return "Readonly Value";
            }
        }
    }

    public class DerivedDataContractModel : SimpleDataContractModel
    {
    }

    public class IgnoreMemberModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        [NotMapped]
        public string IgnoreProperty1 { get; set; }
        [IgnoreDataMember]
        public string IgnoreProperty2 { get; set; }
    }

    public class AttributeConventionModelBuilderTests
    {
        [Fact]
        public void TestSimpleDataContractModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SimpleDataContractModel>("SimpleDataContractModels");
            var model = builder.GetEdmModel();

            var simpleDataContractModel = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == typeof(SimpleDataContractModel).Name);
            Assert.Equal(6, simpleDataContractModel.Properties().Count());

            Assert.Equal("KeyProperty", simpleDataContractModel.Key().Single().Name);

            var requiredProperty2 = simpleDataContractModel.Properties().Single(p => p.Name == "RequiredProperty2") as EdmProperty;

            Assert.False(requiredProperty2.Type.IsNullable);

            var requiredPropertyWithDefaultValue = simpleDataContractModel.Properties().Single(p => p.Name == "RequiredPropertyWithDefaultValue") as EdmStructuralProperty;

            Assert.Equal("default", requiredPropertyWithDefaultValue.DefaultValueString);

            var navigationProperty1 = simpleDataContractModel.Properties().Single(p => p.Name == "NavigationProperty1") as EdmNavigationProperty;
            Assert.Equal(EdmMultiplicity.One, navigationProperty1.TargetMultiplicity());

            Assert.DoesNotContain(simpleDataContractModel.Properties(), (p) => p.Name == "ReadOnlyProperty");

            var derivedDCModel = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == typeof(DerivedDataContractModel).Name);
            Assert.DoesNotContain(derivedDCModel.Properties(), (p) => p.Name == "NotDataMember");
        }
    }
}
