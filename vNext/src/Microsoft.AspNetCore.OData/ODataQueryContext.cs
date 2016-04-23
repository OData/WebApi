// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Properties;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;
using System.Linq;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// This defines some context information used to perform query composition. 
    /// </summary>
    public class ODataQueryContext
    {
	    /// <summary>
	    /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" />, element CLR type,
	    /// and <see cref="ODataPath" />.
	    /// </summary>
	    /// <param name="model">The EdmModel that includes the <see cref="IEdmType"/> corresponding to
	    /// the given <paramref name="elementClrType"/>.</param>
	    /// <param name="elementClrType">The CLR type of the element of the collection being queried.</param>
	    /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
	    /// <param name="assemblyNames"></param>
	    public ODataQueryContext(IEdmModel model, Type elementClrType, AssemblyNames assemblyNames, ODataPath path)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (elementClrType == null)
            {
                throw Error.ArgumentNull("elementClrType");
            }

            ElementType = model.GetEdmType(elementClrType);

            if (ElementType == null)
            {
                throw Error.Argument("elementClrType", SRResources.ClrTypeNotInModel, elementClrType.FullName);
            }

            ElementClrType = elementClrType;
            Model = model;
            Path = path;
	        AssemblyNames = assemblyNames;
	        NavigationSource = GetNavigationSource(Model, ElementType, path);
        }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" />, element EDM type,
        /// and <see cref="ODataPath" />.
        /// </summary>
        /// <param name="model">The EDM model the given EDM type belongs to.</param>
        /// <param name="elementType">The EDM type of the element of the collection being queried.</param>
        /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
        public ODataQueryContext(IEdmModel model, IEdmType elementType, ODataPath path)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }

            Model = model;
            ElementType = elementType;
            Path = path;
            NavigationSource = GetNavigationSource(Model, ElementType, path);
        }

        //internal ODataQueryContext(IEdmModel model, Type elementClrType, string assemblyNames)
        //    : this(model, elementClrType, assemblyNames, path: null)
        //{
        //}

        //internal ODataQueryContext(IEdmModel model, IEdmType elementType, string assemblyNames)
        //    : this(model, elementType, assemblyNames, path: null)
        //{
        //}

        /// <summary>
        /// Gets the given <see cref="IEdmModel"/> that contains the EntitySet.
        /// </summary>
        public IEdmModel Model { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmType"/> of the element.
        /// </summary>
        public IEdmType ElementType { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmNavigationSource"/> that contains the element.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets the CLR type of the element.
        /// </summary>
        public Type ElementClrType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataPath"/>.
        /// </summary>
        public ODataPath Path { get; private set; }

	    public AssemblyNames AssemblyNames { get; set; }

	    private static IEdmNavigationSource GetNavigationSource(IEdmModel model, IEdmType elementType, ODataPath odataPath)
        {
            Contract.Assert(model != null);
            Contract.Assert(elementType != null);

            IEdmNavigationSource navigationSource = odataPath?.NavigationSource;
            if (navigationSource != null)
            {
                return navigationSource;
            }

            IEdmEntityContainer entityContainer = model.EntityContainer;
            if (entityContainer == null)
            {
                return null;
            }

            List<IEdmEntitySet> matchedNavigationSources =
                entityContainer.EntitySets().Where(e => e.EntityType() == elementType).ToList();

            return (matchedNavigationSources.Count != 1) ? null : matchedNavigationSources[0];
        }
    }
}
