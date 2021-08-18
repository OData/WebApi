//-----------------------------------------------------------------------------
// <copyright file="RecursiveComplexTypesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Builder.TestModels.Recursive;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class RecursiveComplexTypesTests
    {
        [Fact]
        public void CanBuildModelWithDirectRecursiveReference()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<JustAddress>("justaddress");
            IEdmModel model = builder.GetEdmModel();

            var address =
                model.SchemaElements.First(e => e.Name == typeof(Address).Name)
                as EdmComplexType;

            var previousAddressProperty =
                address.Properties().Single(p => p.Name == "PreviousAddress") as EdmStructuralProperty;

            Assert.Equal(
                typeof(Address).Name,
                previousAddressProperty.Type.AsComplex()?.ComplexDefinition().Name);
        }

        [Fact]
        public void CanBuildModelWithCollectionRecursiveReference()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<JustCustomFields>("justcustomfields");
            IEdmModel model = builder.GetEdmModel();

            var field =
                model.SchemaElements.First(e => e.Name == typeof(Field).Name)
                as EdmComplexType;

            var subFieldsProperty =
                field.Properties().Single(p => p.Name == "SubFields") as EdmStructuralProperty;

            Assert.Equal(
                typeof(Field).Name,
                subFieldsProperty.Type.AsCollection()?.ElementType().AsComplex()?.ComplexDefinition().Name);
        }

        [Fact]
        public void CanBuildModelWithIndirectRecursiveReference()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<JustCustomer>("justcustomer");
            IEdmModel model = builder.GetEdmModel();

            string customerTypeName = typeof(Customer).Name;

            var customer = model.SchemaElements.First(e => e.Name == customerTypeName) as EdmComplexType;

            var accountsProperty =
                customer.Properties().Single(p => p.Name == "Accounts") as EdmStructuralProperty;

            string accountTypeName = typeof(Account).Name;

            Assert.Equal(
                accountTypeName,
                accountsProperty.Type.AsCollection()?.ElementType().AsComplex()?.ComplexDefinition().Name);

            var account = model.SchemaElements.First(e => e.Name == accountTypeName) as EdmComplexType;
            var ownerProperty = account.Properties().Single(p => p.Name == "Owner") as EdmStructuralProperty;

            Assert.Equal(customerTypeName, ownerProperty.Type.AsComplex()?.ComplexDefinition().Name);
        }

        [Fact]
        public void CanBuildModelWithCollectionReferenceViaInheritance()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<JustHomeDirectory>("justhomedirectory");
            IEdmModel model = builder.GetEdmModel();

            string fileTypeName = typeof(File).Name;
            string directoryTypeName = typeof(Directory).Name;

            var file = model.SchemaElements.First(e => e.Name == fileTypeName) as EdmComplexType;
            var directory = model.SchemaElements.First(e => e.Name == directoryTypeName) as EdmComplexType;

            Assert.Equal(fileTypeName, directory.BaseComplexType()?.Name);

            var filesProperty = directory.Properties().Single(p => p.Name == "Files") as EdmStructuralProperty;

            Assert.Equal(
                fileTypeName,
                filesProperty.Type.AsCollection()?.ElementType().AsComplex()?.ComplexDefinition().Name);
        }

        [Fact]
        public void CanBuildModelWithMutuallyRecursiveInheritance()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<JustBase>("justbase");
            builder.EntitySet<JustDerived>("justderived");
            IEdmModel model = builder.GetEdmModel();

            string baseTypeName = typeof(Base).Name;
            string derivedTypeName = typeof(Derived).Name;

            var baseType = model.SchemaElements.First(e => e.Name == baseTypeName) as EdmComplexType;
            var derivedType = model.SchemaElements.First(e => e.Name == derivedTypeName) as EdmComplexType;

            Assert.Equal(baseTypeName, derivedType.BaseComplexType()?.Name);

            var baseProperty = derivedType.Properties().Single(p => p.Name == "Base") as EdmStructuralProperty;
            var derivedProperty = baseType.Properties().Single(p => p.Name == "Derived") as EdmStructuralProperty;

            Assert.Equal(baseTypeName, baseProperty.Type.AsComplex()?.ComplexDefinition().Name);
            Assert.Equal(derivedTypeName, derivedProperty.Type.AsComplex()?.ComplexDefinition().Name);
        }
    }

    public class JustCustomer
    {
        public int ID { get; set; }

        public Customer Customer { get; set; }
    }

    public class JustAddress
    {
        public int ID { get; set; }

        public Address Address { get; set; }
    }

    public class JustHomeDirectory
    {
        public int ID { get; set; }

        public Directory HomeDirectory { get; set; }
    }

    public class JustCustomFields
    {
        public int ID { get; set; }

        public List<Field> CustomFields { get; set; }
    }

    public class JustBase
    {
        public int ID { get; set; }

        public Base Base { get; set; }
    }

    public class JustDerived
    {
        public int ID { get; set; }

        public Derived Derived { get; set; }
    }
}
