//-----------------------------------------------------------------------------
// <copyright file="AttributeConventionModelBuilderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
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

    public class AttributeConventionModelBuilderTests : WebHostTestBase
    {
        WebRouteConfiguration _configuration;

        public AttributeConventionModelBuilderTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Fact]
        public void TestSimpleDataContractModel()
        {
            var builder = _configuration.CreateConventionModelBuilder();
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
