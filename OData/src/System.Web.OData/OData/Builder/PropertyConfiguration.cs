// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Base class for all property configurations.
    /// </summary>
    public abstract class PropertyConfiguration
    {
        private string _name;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="declaringType">The declaring EDM type of the property.</param>
        protected PropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            if (declaringType == null)
            {
                throw Error.ArgumentNull("declaringType");
            }

            PropertyInfo = property;
            DeclaringType = declaringType;
            AddedExplicitly = true;
            _name = property.Name;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                
                _name = value;
            }
        }

        /// <summary>
        /// Gets the declaring type.
        /// </summary>
        public StructuralTypeConfiguration DeclaringType { get; private set; }

        /// <summary>
        /// Gets the mapping CLR <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets the CLR <see cref="Type"/> of the property.
        /// </summary>
        public abstract Type RelatedClrType { get; }

        /// <summary>
        /// Gets the <see cref="PropertyKind"/> of the property.
        /// </summary>
        public abstract PropertyKind Kind { get; }

        /// <summary>
        /// Gets or sets a value that is <see langword="true"/> if the property was added by the user; <see langword="false"/> if it was inferred through conventions.
        /// </summary>
        /// <remarks>The default value is <see langword="true"/></remarks>
        public bool AddedExplicitly { get; set; }

        /// <summary>
        /// Gets whether the property is restricted, i.e. nonfilterable, unsortable, not navigable, or not expandable.
        /// </summary>
        public bool IsRestricted
        {
            get { return NonFilterable || Unsortable || NotNavigable || NotExpandable; }
        }

        /// <summary>
        /// Gets or sets whether the property is nonfilterable. default is false.
        /// </summary>
        public bool NonFilterable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is unsortable. default is false.
        /// </summary>
        public bool Unsortable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is not navigable. default is false.
        /// </summary>
        public bool NotNavigable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is not expandable. default is false.
        /// </summary>
        public bool NotExpandable { get; set; }

        /// <summary>
        /// Sets the property as nonfilterable.
        /// </summary>
        public PropertyConfiguration IsNonFilterable()
        {
            NonFilterable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as filterable.
        /// </summary>
        public PropertyConfiguration IsFilterable()
        {
            NonFilterable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as unsortable.
        /// </summary>
        public PropertyConfiguration IsUnsortable()
        {
            Unsortable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as sortable.
        /// </summary>
        public PropertyConfiguration IsSortable()
        {
            Unsortable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not navigable.
        /// </summary>
        public PropertyConfiguration IsNotNavigable()
        {
            IsUnsortable();
            IsNonFilterable();
            NotNavigable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as navigable.
        /// </summary>
        public PropertyConfiguration IsNavigable()
        {
            NotNavigable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not expandable.
        /// </summary>
        public PropertyConfiguration IsNotExpandable()
        {
            NotExpandable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as expandable.
        /// </summary>
        public PropertyConfiguration IsExpandable()
        {
            NotExpandable = false;
            return this;
        }
    }
}
