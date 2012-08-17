// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// This defines some context information used to perform query composition. 
    /// </summary>
    public class ODataQueryContext
    {
        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> based only on a CLR type. 
        /// </summary>
        /// <remarks>This is intended to be used only for primitive types.</remarks>
        /// <param name="clrType">The CLR type information.</param>
        public ODataQueryContext(Type clrType)
        {
            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            if (!TypeHelper.IsQueryPrimitiveType(clrType))
            {
                throw Error.Argument("clrType", SRResources.PrimitiveTypeRequired, clrType.Name);
            }

            EntityClrType = clrType;
            IsPrimitiveClrType = true;
        }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with EdmModel and Entity's CLR type. 
        /// By default we assume the full name of the CLR type is used for the name for the EntitySet stored in the model.
        /// </summary>
        /// <param name="model">The EdmModel that includes the Entity and EntitySet information.</param>
        /// <param name="entityClrType">The entity's CLR type information.</param>
        public ODataQueryContext(IEdmModel model, Type entityClrType)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (entityClrType == null)
            {
                throw Error.ArgumentNull("entityClrType");
            }

            // check if we can successfully retrieve an entitySet from the model with the given entityClrType
            IEnumerable<IEdmEntityContainer> containers = model.EntityContainers();
            List<IEdmEntitySet> entities = new List<IEdmEntitySet>();
            foreach (IEdmEntityContainer container in containers)
            {
                entities.AddRange(container.EntitySets().Where(s => s.ElementType.IsEquivalentTo(model.GetEdmType(entityClrType))));
            }

            if (entities == null || entities.Count == 0)
            {
                throw Error.InvalidOperation(SRResources.EntitySetNotFound, entityClrType.FullName);
            }

            if (entities.Count > 1)
            {
                throw Error.InvalidOperation(SRResources.MultipleEntitySetMatchedClrType, entityClrType.FullName);
            }

            Model = model;
            EntityClrType = entityClrType;
            EntitySet = entities[0];
        }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with EdmModel, Entity's CLR type and the name
        /// of EntitySet stored in the model.
        /// </summary>
        /// <param name="model">The EdmModel that includes the Entity and EntitySet information.</param>
        /// <param name="entityClrType">The entity's CLR type information.</param>
        /// <param name="entitySetName">The name of EntitySet stored in the model.</param>
        public ODataQueryContext(IEdmModel model, Type entityClrType, string entitySetName)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (entityClrType == null)
            {
                throw Error.ArgumentNull("entityClrType");
            }

            if (String.IsNullOrEmpty(entitySetName))
            {
                throw Error.ArgumentNullOrEmpty("entitySetName");
            }

            // check if we can successfully retrieve an entitySet from the model with the given entitySetName
            IEnumerable<IEdmEntityContainer> containers = model.EntityContainers();
            foreach (IEdmEntityContainer container in containers)
            {
                EntitySet = container.FindEntitySet(entitySetName);

                if (EntitySet != null)
                {
                    break;
                }
            }

            if (EntitySet == null)
            {
                throw Error.Argument("entitySetName", SRResources.EntitySetNotFoundForName, entitySetName);
            }

            // Check if the model contains the entityClrType
            IEdmEntityType edmType = model.GetEdmType(entityClrType) as IEdmEntityType;
            if (edmType == null)
            {
                throw Error.Argument("entityClrType", SRResources.EntityClrTypeNotFound, entityClrType.FullName);
            }

            // Check if the entitySetName matches the entityClrType
            if (!edmType.IsEquivalentTo(EntitySet.ElementType))
            {
                throw Error.Argument("entityClrType", SRResources.EntityClrTypeNotMatchEntitySetName, entityClrType.FullName, entitySetName);
            }

            Model = model;
            EntityClrType = entityClrType;
        }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with EdmModel, Entity's CLR type and the corresponding
        /// EntitySet stored in the model. If the given EntitySet is not in the model, this constructor will throw.
        /// </summary>
        /// <param name="model">The EdmModel that includes the Entity and EntitySet information.</param>
        /// <param name="entityClrType">The entity's CLR type information.</param>
        /// <param name="entitySet">The corresponding EntitySet stored in the model.</param>
        public ODataQueryContext(IEdmModel model, Type entityClrType, IEdmEntitySet entitySet)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (entityClrType == null)
            {
                throw Error.ArgumentNull("entityClrType");
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull("entitySet");
            }

            // check if the model contains the entitySet
            IEnumerable<IEdmEntityContainer> containers = model.EntityContainers();
            if (containers != null)
            {
                IEdmEntityContainer singleContainer = containers.Single();

                if (singleContainer != null)
                {
                    IEnumerable<IEdmEntitySet> entitySets = singleContainer.EntitySets();
                    if (!entitySets.Contains(entitySet))
                    {
                        throw Error.Argument(parameterName: "entitySet", messageFormat: SRResources.EntitySetMustBeInTheModel);
                    }
                }
            }

            // Check if the model contains the entityClrType
            IEdmEntityType edmType = model.GetEdmType(entityClrType) as IEdmEntityType;
            if (edmType == null)
            {
                throw Error.Argument("entityClrType", SRResources.EntityClrTypeNotFound, entityClrType.FullName);
            }

            // Check if the entitySetName matches the entityClrType
            if (!edmType.IsEquivalentTo(entitySet.ElementType))
            {
                throw Error.Argument("entityClrType", SRResources.EntityClrTypeNotMatchEntitySet, entityClrType.FullName, entitySet.Name);
            }

            // Now we can set everything
            Model = model;
            EntityClrType = entityClrType;
            EntitySet = entitySet;
        }

        /// <summary>
        /// Gets the <see cref="IEdmEntitySet"/> that represents a group of entities.
        /// </summary>
        public IEdmEntitySet EntitySet { get; private set; }

        /// <summary>
        /// Gets the given <see cref="IEdmModel"/> that contains the EntitySet.
        /// </summary>
        public IEdmModel Model { get; private set; }

        /// <summary>
        /// Gets the CLR type of the entity.
        /// </summary>
        public Type EntityClrType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current
        /// <see cref="ODataQueryContext"/> instance is associated
        /// with a primitive CLR type.
        /// </summary>
        public bool IsPrimitiveClrType { get; private set; }
    }
}
