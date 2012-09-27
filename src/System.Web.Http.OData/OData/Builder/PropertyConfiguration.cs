// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Base class for all property configurations.
    /// </summary>
    public abstract class PropertyConfiguration
    {
        protected PropertyConfiguration(PropertyInfo property, IStructuralTypeConfiguration declaringType)
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
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name
        {
            get { return PropertyInfo.Name; }
        }

        public IStructuralTypeConfiguration DeclaringType { get; private set; }

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
    }
}
