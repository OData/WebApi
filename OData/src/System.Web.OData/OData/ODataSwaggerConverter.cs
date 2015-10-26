// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;

namespace System.Web.OData
{
    /// <summary>
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
        /// Gets the Swagger model.
        /// </summary>
        public virtual JObject SwaggerModel
        {
            get
            {
                if (SwaggerDoc == null)
                {
                    ConvertToSwaggerModel();
                }

                Contract.Assert(SwaggerDoc != null);
                return SwaggerDoc;
            }
        }

        /// <summary>
        /// Gets the document in the Swagger.
        /// </summary>
        protected virtual JObject SwaggerDoc { get; set; }

        /// <summary>
        /// Gets the paths in the Swagger.
        /// </summary>
        protected virtual JObject SwaggerPaths { get; set; }

        /// <summary>
        /// Gets the definitions in the Swagger.
        /// </summary>
        protected virtual JObject SwaggerDefinitions { get; set; }

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
        public virtual JObject ConvertToSwaggerModel()
        {
            if (SwaggerDoc != null)
            {
                return SwaggerDoc;
            }

            InitializeStart();
            InitializeDocument();
            InitializeContainer();
            InitializeTypeDefinitions();
            InitializeOperations();
            InitializeEnd();

            return SwaggerDoc;
        }

        /// <summary>
        /// Start to initialize the Swagger model.
        /// </summary>
        protected virtual void InitializeStart()
        {
            SwaggerDoc = null;
            SwaggerPaths = null;
            SwaggerDefinitions = null;
        }

        /// <summary>
        /// Initialize the document of Swagger model.
        /// </summary>
        protected virtual void InitializeDocument()
        {
            SwaggerDoc = new JObject()
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
            Contract.Assert(SwaggerDoc != null);
            Contract.Assert(EdmModel != null);

            SwaggerPaths = new JObject();
            SwaggerDoc.Add("paths", SwaggerPaths);

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
            Contract.Assert(SwaggerDoc != null);
            Contract.Assert(EdmModel != null);

            SwaggerDefinitions = new JObject();
            SwaggerDoc.Add("definitions", SwaggerDefinitions);

            foreach (var type in EdmModel.SchemaElements.OfType<IEdmStructuredType>())
            {
                SwaggerDefinitions.Add(type.FullTypeName(),
                    ODataSwaggerUtilities.CreateSwaggerDefinitionForStructureType(type));
            }
        }

        /// <summary>
        /// Initialize the operations to Swagger model.
        /// </summary>
        protected virtual void InitializeOperations()
        {
            Contract.Assert(SwaggerDoc != null);
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
            Contract.Assert(SwaggerDefinitions != null);

            SwaggerDefinitions.Add("_Error", new JObject()
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

            SwaggerDefinitions.Add("_InError", new JObject()
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
