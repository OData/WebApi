// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Web.Http.Data.Metadata;

namespace Microsoft.Web.Http.Data.EntityFramework.Metadata
{
    /// <summary>
    /// Attribute applied to a <see cref="DbDataController{DbContext}"/> that exposes LINQ to Entities mapped
    /// Types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class DbMetadataProviderAttribute : MetadataProviderAttribute
    {
        private Type _dbContextType;

        /// <summary>
        /// Default constructor. Using this constructor, the Type of the LINQ To Entities
        /// DbContext will be inferred from the <see cref="DataController"/> the
        /// attribute is applied to.
        /// </summary>
        public DbMetadataProviderAttribute()
            : base(typeof(LinqToEntitiesMetadataProvider))
        {
        }

        /// <summary>
        /// Constructs an attribute for the specified LINQ To Entities
        /// DbContext Type.
        /// </summary>
        /// <param name="dbContextType">The LINQ To Entities ObjectContext Type.</param>
        public DbMetadataProviderAttribute(Type dbContextType)
            : base(typeof(LinqToEntitiesMetadataProvider))
        {
            _dbContextType = dbContextType;
        }

        /// <summary>
        /// The Linq To Entities DbContext Type.
        /// </summary>
        public Type DbContextType
        {
            get { return _dbContextType; }
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

            if (_dbContextType == null)
            {
                _dbContextType = GetContextType(controllerType);
            }

            if (!typeof(DbContext).IsAssignableFrom(_dbContextType))
            {
                throw Error.InvalidOperation(Resource.InvalidDbMetadataProviderSpecification, _dbContextType);
            }

            return new LinqToEntitiesMetadataProvider(_dbContextType, parent, true);
        }

        /// <summary>
        /// Extracts the context type from the specified <paramref name="dataControllerType"/>.
        /// </summary>
        /// <param name="dataControllerType">A LINQ to Entities data controller type.</param>
        /// <returns>The type of the object context.</returns>
        private static Type GetContextType(Type dataControllerType)
        {
            Type efDataControllerType = dataControllerType.BaseType;
            while (!efDataControllerType.IsGenericType || efDataControllerType.GetGenericTypeDefinition() != typeof(DbDataController<>))
            {
                if (efDataControllerType == typeof(object))
                {
                    throw Error.InvalidOperation(Resource.InvalidMetadataProviderSpecification, typeof(DbMetadataProviderAttribute).Name, dataControllerType.Name, typeof(DbDataController<>).Name);
                }
                efDataControllerType = efDataControllerType.BaseType;
            }

            return efDataControllerType.GetGenericArguments()[0];
        }
    }
}
