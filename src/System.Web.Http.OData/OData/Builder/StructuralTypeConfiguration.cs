// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Builder
{
    public abstract class StructuralTypeConfiguration : IStructuralTypeConfiguration
    {
        protected StructuralTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
        {
            ClrType = clrType;
            ModelBuilder = modelBuilder;
            ExplicitProperties = new Dictionary<PropertyInfo, PropertyConfiguration>();
            IgnoredProperties = new List<PropertyInfo>();
        }

        public abstract StructuralTypeKind Kind { get; }

        public Type ClrType { get; private set; }

        public string FullName
        {
            get { return Namespace + "." + Name; }
        }

        public string Namespace
        {
            get { return ClrType.Namespace; }
        }

        public string Name
        {
            get { return ClrType.Name; }
        }

        public IEnumerable<PropertyConfiguration> Properties
        {
            get { return ExplicitProperties.Values; }
        }

        protected ODataModelBuilder ModelBuilder { get; private set; }

        protected ICollection<PropertyInfo> IgnoredProperties { get; private set; }

        protected Dictionary<PropertyInfo, PropertyConfiguration> ExplicitProperties { get; private set; }

        public virtual PrimitivePropertyConfiguration AddProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!propertyInfo.DeclaringType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType);
            }

            // Remove from the ignored properties
            if (IgnoredProperties.Contains(propertyInfo))
            {
                IgnoredProperties.Remove(propertyInfo);
            }

            PrimitivePropertyConfiguration propertyConfiguration = null;
            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                propertyConfiguration = ExplicitProperties[propertyInfo] as PrimitivePropertyConfiguration;
                if (propertyConfiguration == null)
                {
                    throw Error.Argument("propertyInfo", SRResources.MustBePrimitiveProperty, propertyInfo.Name);
                }
            }
            else
            {
                propertyConfiguration = new PrimitivePropertyConfiguration(propertyInfo);
                ExplicitProperties[propertyInfo] = propertyConfiguration;
            }
            return propertyConfiguration;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Helper validates non null propertyInfo")]
        public virtual ComplexPropertyConfiguration AddComplexProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!propertyInfo.DeclaringType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType);
            }

            if (propertyInfo.PropertyType == ClrType)
            {
                throw Error.Argument("propertyInfo", SRResources.RecursiveComplexTypesNotAllowed);
            }

            // Remove from the ignored properties
            if (IgnoredProperties.Contains(propertyInfo))
            {
                IgnoredProperties.Remove(propertyInfo);
            }

            ComplexPropertyConfiguration propertyConfiguration = null;
            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                propertyConfiguration = ExplicitProperties[propertyInfo] as ComplexPropertyConfiguration;
                if (propertyConfiguration == null)
                {
                    throw Error.Argument("propertyInfo", SRResources.MustBeComplexProperty, propertyInfo.Name);
                }
            }
            else
            {
                propertyConfiguration = new ComplexPropertyConfiguration(propertyInfo);
                ExplicitProperties[propertyInfo] = propertyConfiguration;
                // Make sure the complex type is in the model.

                ModelBuilder.AddComplexType(propertyInfo.PropertyType);
            }

            return propertyConfiguration;
        }

        public virtual void RemoveProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!propertyInfo.DeclaringType.IsAssignableFrom(ClrType))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyDoesNotBelongToType);
            }

            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                ExplicitProperties.Remove(propertyInfo);
            }

            if (!IgnoredProperties.Contains(propertyInfo))
            {
                IgnoredProperties.Add(propertyInfo);
            }
        }
    }

    public abstract class StructuralTypeConfiguration<TStructuralType> where TStructuralType : class
    {
        private IStructuralTypeConfiguration _configuration;

        protected StructuralTypeConfiguration(IStructuralTypeConfiguration configuration)
        {
            _configuration = configuration;
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
    }
}
