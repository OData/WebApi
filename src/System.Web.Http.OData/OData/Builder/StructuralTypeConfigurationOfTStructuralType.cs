// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    public abstract class StructuralTypeConfiguration<TStructuralType> where TStructuralType : class
    {
        private IStructuralTypeConfiguration _configuration;

        protected StructuralTypeConfiguration(IStructuralTypeConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            _configuration = configuration;
        }

        public IEnumerable<PropertyConfiguration> Properties
        {
            get { return _configuration.Properties; }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        public virtual void Ignore<TProperty>(Expression<Func<TStructuralType, TProperty>> propertyExpression)
        {
            PropertyInfo ignoredProperty = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            _configuration.RemoveProperty(ignoredProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, string>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, byte[]>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property(Expression<Func<TStructuralType, Stream>> propertyExpression)
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property<T>(Expression<Func<TStructuralType, T?>> propertyExpression) where T : struct
        {
            return GetPrimitivePropertyConfiguration(propertyExpression, optional: true);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public PrimitivePropertyConfiguration Property<T>(Expression<Func<TStructuralType, T>> propertyExpression) where T : struct
        {
            return GetPrimitivePropertyConfiguration(propertyExpression);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public ComplexPropertyConfiguration ComplexProperty<TComplexType>(Expression<Func<TStructuralType, TComplexType>> propertyExpression)
        {
            return GetComplexPropertyConfiguration(propertyExpression);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics appropriate here")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "More specific expression type is clearer")]
        public CollectionPropertyConfiguration CollectionProperty<TElementType>(Expression<Func<TStructuralType, IEnumerable<TElementType>>> propertyExpression)
        {
            return GetCollectionPropertyConfiguration(propertyExpression);
        }

        private PrimitivePropertyConfiguration GetPrimitivePropertyConfiguration(Expression propertyExpression, bool optional = false)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            PrimitivePropertyConfiguration property = _configuration.AddProperty(propertyInfo);
            if (optional)
            {
                property.IsOptional();
            }

            return property;
        }

        private ComplexPropertyConfiguration GetComplexPropertyConfiguration(Expression propertyExpression, bool optional = false)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            ComplexPropertyConfiguration property = _configuration.AddComplexProperty(propertyInfo);
            if (optional)
            {
                property.IsOptional();
            }

            return property;
        }

        private CollectionPropertyConfiguration GetCollectionPropertyConfiguration(Expression propertyExpression, bool optional = false)
        {
            PropertyInfo propertyInfo = PropertySelectorVisitor.GetSelectedProperty(propertyExpression);
            CollectionPropertyConfiguration property = _configuration.AddCollectionProperty(propertyInfo);
            if (optional)
            {
                property.IsOptional();
            }

            return property;
        }
    }
}
