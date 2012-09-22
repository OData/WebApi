// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public abstract class StructuralTypeConfiguration : IStructuralTypeConfiguration
    {
        private const string DefaultNamespace = "Default";

        protected StructuralTypeConfiguration(ODataModelBuilder modelBuilder, Type clrType)
        {
            ClrType = clrType;
            ModelBuilder = modelBuilder;
            Name = ClrType.EdmName();
            Namespace = ClrType.Namespace ?? DefaultNamespace;
            ExplicitProperties = new Dictionary<PropertyInfo, PropertyConfiguration>();
            RemovedProperties = new List<PropertyInfo>();
        }

        public abstract EdmTypeKind Kind { get; }

        public Type ClrType { get; private set; }

        public string FullName
        {
            get { return Namespace + "." + Name; }
        }

        public string Namespace
        {
            get;
            protected set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public IEnumerable<PropertyConfiguration> Properties
        {
            get { return ExplicitProperties.Values; }
        }

        public IEnumerable<PropertyInfo> IgnoredProperties
        {
            get { return RemovedProperties; }
        }

        public ODataModelBuilder ModelBuilder { get; private set; }

        protected ICollection<PropertyInfo> RemovedProperties { get; private set; }

        protected Dictionary<PropertyInfo, PropertyConfiguration> ExplicitProperties { get; private set; }

        public virtual PrimitivePropertyConfiguration AddProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!propertyInfo.ReflectedType.IsAssignableFrom(ClrType))
            {
                throw Error.InvalidOperation(SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            // Remove from the ignored properties
            if (RemovedProperties.Contains(propertyInfo))
            {
                RemovedProperties.Remove(propertyInfo);
            }

            PrimitivePropertyConfiguration propertyConfiguration = null;
            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                propertyConfiguration = ExplicitProperties[propertyInfo] as PrimitivePropertyConfiguration;
                if (propertyConfiguration == null)
                {
                    throw Error.Argument("propertyInfo", SRResources.MustBePrimitiveProperty, propertyInfo.Name, ClrType.FullName);
                }
            }
            else
            {
                propertyConfiguration = new PrimitivePropertyConfiguration(propertyInfo, this);
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

            if (!propertyInfo.ReflectedType.IsAssignableFrom(ClrType))
            {
                throw Error.InvalidOperation(SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            if (propertyInfo.PropertyType == ClrType)
            {
                throw Error.InvalidOperation(SRResources.RecursiveComplexTypesNotAllowed, ClrType.FullName, propertyInfo.Name);
            }

            // Remove from the ignored properties
            if (RemovedProperties.Contains(propertyInfo))
            {
                RemovedProperties.Remove(propertyInfo);
            }

            ComplexPropertyConfiguration propertyConfiguration = null;
            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                propertyConfiguration = ExplicitProperties[propertyInfo] as ComplexPropertyConfiguration;
                if (propertyConfiguration == null)
                {
                    throw Error.Argument("propertyInfo", SRResources.MustBeComplexProperty, propertyInfo.Name, ClrType.FullName);
                }
            }
            else
            {
                propertyConfiguration = new ComplexPropertyConfiguration(propertyInfo, this);
                ExplicitProperties[propertyInfo] = propertyConfiguration;
                // Make sure the complex type is in the model.

                ModelBuilder.AddComplexType(propertyInfo.PropertyType);
            }

            return propertyConfiguration;
        }

        public virtual CollectionPropertyConfiguration AddCollectionProperty(PropertyInfo propertyInfo)
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
                RemovedProperties.Remove(propertyInfo);
            }

            CollectionPropertyConfiguration propertyConfiguration = null;
            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                propertyConfiguration = ExplicitProperties[propertyInfo] as CollectionPropertyConfiguration;
                if (propertyConfiguration == null)
                {
                    throw Error.Argument("propertyInfo", SRResources.MustBeCollectionProperty, propertyInfo.Name, propertyInfo.DeclaringType.FullName);
                }
            }
            else
            {
                propertyConfiguration = new CollectionPropertyConfiguration(propertyInfo, this);
                ExplicitProperties[propertyInfo] = propertyConfiguration;

                // If the ElementType is the same as this type this is recursive complex type nesting
                if (propertyConfiguration.ElementType == ClrType)
                {
                    throw Error.Argument("propertyInfo",
                                         SRResources.RecursiveComplexTypesNotAllowed,
                                         ClrType.Name,
                                         propertyConfiguration.Name);
                }

                // If the ElementType is not primitive treat as a ComplexType and Add to the model.
                IEdmPrimitiveTypeReference edmType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(propertyConfiguration.ElementType);
                if (edmType == null)
                {
                    ModelBuilder.AddComplexType(propertyConfiguration.ElementType);
                }
            }

            return propertyConfiguration;
        }

        public virtual void RemoveProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            if (!propertyInfo.ReflectedType.IsAssignableFrom(ClrType))
            {
                throw Error.InvalidOperation(SRResources.PropertyDoesNotBelongToType, propertyInfo.Name, ClrType.FullName);
            }

            if (ExplicitProperties.ContainsKey(propertyInfo))
            {
                ExplicitProperties.Remove(propertyInfo);
            }

            if (!RemovedProperties.Contains(propertyInfo))
            {
                RemovedProperties.Add(propertyInfo);
            }
        }
    }
}
