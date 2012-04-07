// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Objects;
using System.Web.Http;
using Microsoft.Web.Http.Data.Metadata;

namespace Microsoft.Web.Http.Data.EntityFramework.Metadata
{
    /// <summary>
    /// Attribute applied to a <see cref="DataController"/> that exposes LINQ to Entities mapped
    /// Types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class LinqToEntitiesMetadataProviderAttribute : MetadataProviderAttribute
    {
        private Type _objectContextType;

        /// <summary>
        /// Default constructor. Using this constructor, the Type of the LINQ To Entities
        /// ObjectContext will be inferred from the <see cref="DataController"/> the
        /// attribute is applied to.
        /// </summary>
        public LinqToEntitiesMetadataProviderAttribute()
            : base(typeof(LinqToEntitiesMetadataProvider))
        {
        }

        /// <summary>
        /// Constructs an attribute for the specified LINQ To Entities
        /// ObjectContext Type.
        /// </summary>
        /// <param name="objectContextType">The LINQ To Entities ObjectContext Type.</param>
        public LinqToEntitiesMetadataProviderAttribute(Type objectContextType)
            : base(typeof(LinqToEntitiesMetadataProvider))
        {
            _objectContextType = objectContextType;
        }

        /// <summary>
        /// The Linq To Entities ObjectContext Type.
        /// </summary>
        public Type ObjectContextType
        {
            get { return _objectContextType; }
        }

        /// <summary>
        /// This method creates an instance of the <see cref="MetadataProvider"/>.
        /// </summary>
        /// <param name="controllerType">The <see cref="DataController"/> Type to create a metadata provider for.</param>
        /// <param name="parent">The existing parent metadata provider.</param>
        /// <returns>The metadata provider.</returns>
        public override MetadataProvider CreateProvider(Type controllerType, MetadataProvider parent)
        {
            if (controllerType == null)
            {
                throw Error.ArgumentNull("controllerType");
            }

            if (_objectContextType == null)
            {
                _objectContextType = GetContextType(controllerType);
            }

            if (!typeof(ObjectContext).IsAssignableFrom(_objectContextType))
            {
                throw Error.InvalidOperation(Resource.InvalidLinqToEntitiesMetadataProviderSpecification, _objectContextType);
            }

            return new LinqToEntitiesMetadataProvider(_objectContextType, parent, false);
        }

        /// <summary>
        /// Extracts the context type from the specified <paramref name="dataControllerType"/>.
        /// </summary>
        /// <param name="dataControllerType">A LINQ to Entities data controller type.</param>
        /// <returns>The type of the object context.</returns>
        private static Type GetContextType(Type dataControllerType)
        {
            Type efDataControllerType = dataControllerType.BaseType;
            while (!efDataControllerType.IsGenericType || efDataControllerType.GetGenericTypeDefinition() != typeof(LinqToEntitiesDataController<>))
            {
                if (efDataControllerType == typeof(object))
                {
                    throw Error.InvalidOperation(Resource.InvalidMetadataProviderSpecification, typeof(LinqToEntitiesMetadataProviderAttribute).Name, dataControllerType.Name, typeof(LinqToEntitiesDataController<>).Name);
                }
                efDataControllerType = efDataControllerType.BaseType;
            }

            return efDataControllerType.GetGenericArguments()[0];
        }
    }
}
