using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Builder;
using System.Web.OData.Formatter;
using System.Web.OData.Query.Expressions;

namespace System.Web.OData.Test.OData.Query.Expressions
{
    public class TypeProviderTests
    {
        /*private static Dictionary<Type, IEdmModel> _modelCache = new Dictionary<Type, IEdmModel>();

        [Fact]
        public void CanGenerateSimpleType()
        {
            var model = GetModel();
            var className = "TestClass";
            var propName = "TestProp";
            var edmType = new EdmEntityType(string.Empty, className);
            edmType.AddStructuralProperty(propName, EdmPrimitiveTypeKind.String);
            var type = TypeProvider.GetResultType<DynamicTypeWrapper>(edmType.ToEdmTypeReference(true), model);

            Assert.True(type.Name.StartsWith(@".{className}"), @"Incorrect class name. Expected to start with {className}. Actual {type.Name}");
            var prop = type.GetProperty(propName);
            Assert.NotNull(prop);
            Assert.Equal(typeof(string), prop.PropertyType);
        }

        [Fact]
        public void CanGenerateNestedType()
        {
            var model = GetModel();
            var className = "TestClass";
            var complexPropName = "ComplexProp";
            var propName = "TestProp";

            var edmComplexType = new EdmEntityType(string.Empty, className + complexPropName);
            edmComplexType.AddStructuralProperty(propName, EdmPrimitiveTypeKind.String);
            var edmType = new EdmEntityType(string.Empty, className);
            edmType.AddStructuralProperty(complexPropName, edmComplexType.ToEdmTypeReference(true));

            var type = TypeProvider.GetResultType<DynamicTypeWrapper>(edmType.ToEdmTypeReference(true), model);

            Assert.True(type.Name.StartsWith(@".{className}"), @"Incorrect class name. Expected to start with {className}. Actual {type.Name}");
            var complexProp = type.GetProperty(complexPropName);
            Assert.True(complexProp.PropertyType.Name.StartsWith(@".{className}{complexPropName}"), @"Incorrect class name. Expected to start with {className}{complexPropName}. Actual {complexProp.PropertyType.Name}");
            var prop = complexProp.PropertyType.GetProperty(propName);
            Assert.Equal(typeof(string), prop.PropertyType);
        }

        [Fact]
        public void CanSetAndGetPropertyInGeneratedType()
        {
            var model = GetModel();
            var className = "TestClass";
            var propName = "TestProp";
            var propValue = "TestValue";
            var edmType = new EdmEntityType(string.Empty, className);
            edmType.AddStructuralProperty(propName, EdmPrimitiveTypeKind.String);
            var type = TypeProvider.GetResultType<DynamicTypeWrapper>(edmType.ToEdmTypeReference(true), model);
            var prop = type.GetProperty(propName);

            var instance = Activator.CreateInstance(type) as DynamicTypeWrapper;
            prop.SetValue(instance, propValue);

            Assert.Equal(propValue, instance.GetPropertyValue(propName));
        }

        private IEdmModel GetModel() 
        {
            Type key = typeof(Product);
            IEdmModel value;

            if (!_modelCache.TryGetValue(key, out value))
            {
                ODataModelBuilder model = new ODataConventionModelBuilder();
                model.EntitySet<Product>("Products");
                if (key == typeof(Product))
                {
                    model.EntityType<DerivedProduct>().DerivesFrom<Product>();
                    model.EntityType<DerivedCategory>().DerivesFrom<Category>();
                }

                value = _modelCache[key] = model.GetEdmModel();
            }
            return value;
        }*/
    }
}
