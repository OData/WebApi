//-----------------------------------------------------------------------------
// <copyright file="ODataQueryContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// This defines some context information used to perform query composition.
    /// </summary>
    public class ODataQueryContext
    {
        private DefaultQuerySettings _defaultQuerySettings;

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryContext"/> with <see cref="IEdmModel" />, element CLR type,
        /// and <see cref="ODataPath" />.
        /// </summary>
        /// <param name="model">The EdmModel that includes the <see cref="IEdmType"/> corresponding to
        /// the given <paramref name="elementClrType"/>.</param>
        /// <param name="elementClrType">The CLR type of the element of the collection being queried.</param>
        /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
        /// <remarks>
        /// This is a public constructor used for stand-alone scenario; in this case, the services
        /// container may not be present.
        /// </remarks>
        public ODataQueryContext(IEdmModel model, Type elementClrType, ODataPath path)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (elementClrType == null)
            {
                throw Error.ArgumentNull("elementClrType");
            }

            ElementType = model.GetTypeMappingCache().GetEdmType(elementClrType, model)?.Definition;

            if (ElementType == null)
            {
                throw Error.Argument("elementClrType", SRResources.ClrTypeNotInModel, elementClrType.FullName);
            }

            ElementClrType = elementClrType;
            Model = model;
            Path = path;
            NavigationSource = GetNavigationSource(Model, ElementType, path);
            GetPathContext();
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
            GetPathContext();
        }

        internal ODataQueryContext(IEdmModel model, Type elementClrType)
            : this(model, elementClrType, path: null)
        {
        }

        internal ODataQueryContext(IEdmModel model, IEdmType elementType)
            : this(model, elementType, path: null)
        {
        }

        /// <summary>
        /// Gets the given <see cref="DefaultQuerySettings"/>.
        /// </summary>
        public DefaultQuerySettings DefaultQuerySettings
        {
            get
            {
                if (_defaultQuerySettings == null)
                {
                    _defaultQuerySettings = RequestContainer == null
                        ? new DefaultQuerySettings()
                        : RequestContainer.GetRequiredService<DefaultQuerySettings>();
                }

                return _defaultQuerySettings;
            }
        }

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
        public Type ElementClrType { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ODataPath"/>.
        /// </summary>
        public ODataPath Path { get; private set; }

        /// <summary>
        /// Gets the request container.
        /// </summary>
        /// <remarks>
        /// The services container may not be present. See the constructor in this file for
        /// use in stand-alone scenarios.
        /// </remarks>
        public IServiceProvider RequestContainer { get; internal set; }

        internal IEdmProperty TargetProperty { get; private set; }

        internal IEdmStructuredType TargetStructuredType { get; private set; }

        internal string TargetName { get; private set; }

        /// <summary>
        /// Gets or sets the query validation settings.
        /// </summary>
        internal ODataValidationSettings ValidationSettings { get; set; }

        private static IEdmNavigationSource GetNavigationSource(IEdmModel model, IEdmType elementType, ODataPath odataPath)
        {
            Contract.Assert(model != null);
            Contract.Assert(elementType != null);

            IEdmNavigationSource navigationSource = (odataPath != null) ? odataPath.NavigationSource : null;
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

        private void GetPathContext()
        {
            if (Path != null)
            {
                IEdmProperty property;
                IEdmStructuredType structuredType;
                string name;
                EdmLibHelpers.GetPropertyAndStructuredTypeFromPath(
                    Path.Segments,
                    out property,
                    out structuredType,
                    out name);

                TargetProperty = property;
                TargetStructuredType = structuredType;
                TargetName = name;
            }
            else
            {
                TargetStructuredType = ElementType as IEdmStructuredType;
            }
        }
    }
}
