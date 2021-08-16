//-----------------------------------------------------------------------------
// <copyright file="ODataSwaggerConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// <para>QualityBand : Preview</para>
    /// Represents an <see cref="ODataSwaggerConverter"/> used to converter an Edm model to Swagger model.
    /// </summary>
    public class ODataSwaggerConverter
    {
        private static readonly Uri DefaultMetadataUri = new Uri("http://localhost");
        private const string DefaultHost = "default";
        private const string DefaultbasePath = "/odata";

        /// <summary>
        /// Gets or sets the metadata Uri in the Swagger model.
        /// </summary>
        public Uri MetadataUri { get; set; }

        /// <summary>
        /// Gets or sets the host in the Swagger model.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the base path in the Swagger model.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the Edm model.
        /// </summary>
        public IEdmModel EdmModel { get; private set; }

        /// <summary>
        /// Gets the version of Swagger spec.
        /// </summary>
        public virtual Version SwaggerVersion
        {
            get
            {
                return new Version(2, 0);
            }
        }

        /// <summary>
        /// Gets the document in the Swagger.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:EnableSetterForProperty", Justification = "Enable setter for virtual property")]
        protected virtual JObject SwaggerDocument { get; set; }

        /// <summary>
        /// Gets the paths in the Swagger.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:EnableSetterForProperty", Justification = "Enable setter for virtual property")]
        protected virtual JObject SwaggerPaths { get; set; }

        /// <summary>
        /// Gets the definitions in the Swagger.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:EnableSetterForProperty", Justification = "Enable setter for virtual property")]
        protected virtual JObject SwaggerTypeDefinitions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSwaggerConverter" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public ODataSwaggerConverter(IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            EdmModel = model;
            MetadataUri = DefaultMetadataUri;
            Host = DefaultHost;
            BasePath = DefaultbasePath;
        }

        /// <summary>
        /// Converts the Edm model to Swagger model.
        /// </summary>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject"/> represents the Swagger model.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property is not appropriate, method does work")]
        public virtual JObject GetSwaggerModel()
        {
            if (SwaggerDocument != null)
            {
                return SwaggerDocument;
            }

            InitializeStart();
            InitializeDocument();
            InitializeContainer();
            InitializeTypeDefinitions();
            InitializeOperations();
            InitializeEnd();

            return SwaggerDocument;
        }

        /// <summary>
        /// Start to initialize the Swagger model.
        /// </summary>
        protected virtual void InitializeStart()
        {
            SwaggerDocument = null;
            SwaggerPaths = null;
            SwaggerTypeDefinitions = null;
        }

        /// <summary>
        /// Initialize the document of Swagger model.
        /// </summary>
        protected virtual void InitializeDocument()
        {
            SwaggerDocument = new JObject()
            {
                { "swagger", SwaggerVersion.ToString() },
                { "info", new JObject()
                {
                    { "title", "OData Service" },
                    { "description", "The OData Service at " + MetadataUri },
                    { "version", "0.1.0" },
                    { "x-odata-version", "4.0" }
                }
                },
                { "host", Host },
                { "schemes", new JArray("http") },
                { "basePath", BasePath },
                { "consumes", new JArray("application/json") },
                { "produces", new JArray("application/json") },
            };
        }

        /// <summary>
        /// Initialize the entity container to Swagger model.
        /// </summary>
        protected virtual void InitializeContainer()
        {
            Contract.Assert(SwaggerDocument != null);
            Contract.Assert(EdmModel != null);

            SwaggerPaths = new JObject();
            SwaggerDocument.Add("paths", SwaggerPaths);

            if (EdmModel.EntityContainer == null)
            {
                return;
            }

            foreach (var entitySet in EdmModel.EntityContainer.EntitySets())
            {
                SwaggerPaths.Add("/" + entitySet.Name, ODataSwaggerUtilities.CreateSwaggerPathForEntitySet(entitySet));

                SwaggerPaths.Add(ODataSwaggerUtilities.GetPathForEntity(entitySet),
                    ODataSwaggerUtilities.CreateSwaggerPathForEntity(entitySet));
            }

            foreach (var operationImport in EdmModel.EntityContainer.OperationImports())
            {
                SwaggerPaths.Add(ODataSwaggerUtilities.GetPathForOperationImport(operationImport),
                    ODataSwaggerUtilities.CreateSwaggerPathForOperationImport(operationImport));
            }
        }

        /// <summary>
        /// Initialize the type definitions to Swagger model.
        /// </summary>
        protected virtual void InitializeTypeDefinitions()
        {
            Contract.Assert(SwaggerDocument != null);
            Contract.Assert(EdmModel != null);

            SwaggerTypeDefinitions = new JObject();
            SwaggerDocument.Add("definitions", SwaggerTypeDefinitions);

            foreach (var type in EdmModel.SchemaElements.OfType<IEdmStructuredType>())
            {
                SwaggerTypeDefinitions.Add(type.FullTypeName(),
                    ODataSwaggerUtilities.CreateSwaggerTypeDefinitionForStructuredType(type));
            }
        }

        /// <summary>
        /// Initialize the operations to Swagger model.
        /// </summary>
        protected virtual void InitializeOperations()
        {
            Contract.Assert(SwaggerDocument != null);
            Contract.Assert(EdmModel != null);
            Contract.Assert(SwaggerPaths != null);

            if (EdmModel.EntityContainer == null)
            {
                return;
            }

            foreach (var operation in EdmModel.SchemaElements.OfType<IEdmOperation>())
            {
                // skip unbound operation
                if (!operation.IsBound)
                {
                    continue;
                }

                var boundParameter = operation.Parameters.First();
                var boundType = boundParameter.Type.Definition;

                // skip operation bound to non entity (or entity collection)
                if (boundType.TypeKind == EdmTypeKind.Entity)
                {
                    IEdmEntityType entityType = (IEdmEntityType)boundType;
                    foreach (var entitySet in
                        EdmModel.EntityContainer.EntitySets().Where(es => es.EntityType().Equals(entityType)))
                    {
                        SwaggerPaths.Add(ODataSwaggerUtilities.GetPathForOperationOfEntity(operation, entitySet),
                            ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntity(operation, entitySet));
                    }
                }
                else if (boundType.TypeKind == EdmTypeKind.Collection)
                {
                    IEdmCollectionType collectionType = boundType as IEdmCollectionType;

                    if (collectionType != null && collectionType.ElementType.Definition.TypeKind == EdmTypeKind.Entity)
                    {
                        IEdmEntityType entityType = (IEdmEntityType)collectionType.ElementType.Definition;
                        foreach (var entitySet in
                            EdmModel.EntityContainer.EntitySets().Where(es => es.EntityType().Equals(entityType)))
                        {
                            SwaggerPaths.Add(ODataSwaggerUtilities.GetPathForOperationOfEntitySet(operation, entitySet),
                                ODataSwaggerUtilities.CreateSwaggerPathForOperationOfEntitySet(operation, entitySet));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// End to initialize the Swagger model.
        /// </summary>
        protected virtual void InitializeEnd()
        {
            Contract.Assert(SwaggerTypeDefinitions != null);

            SwaggerTypeDefinitions.Add("_Error", new JObject()
            {
                {
                    "properties", new JObject()
                    {
                        { "error", new JObject()
                        {
                            { "$ref", "#/definitions/_InError" }
                        }
                        }
                    }
                }
            });

            SwaggerTypeDefinitions.Add("_InError", new JObject()
            {
                {
                    "properties", new JObject()
                    {
                        { "code", new JObject()
                        {
                            { "type", "string" }
                        }
                        },
                        { "message", new JObject()
                        {
                            { "type", "string" }
                        }
                        }
                    }
                }
            });
        }
    }
}
