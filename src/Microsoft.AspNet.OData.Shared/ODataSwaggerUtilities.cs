//-----------------------------------------------------------------------------
// <copyright file="ODataSwaggerUtilities.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    ///  Utility methods used to convert the Swagger model.
    /// </summary>
    internal static class ODataSwaggerUtilities
    {
        /// <summary>
        /// Create the Swagger path for the Edm entity set.
        /// </summary>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject"/> represents the related Edm entity set.</returns>
        public static JObject CreateSwaggerPathForEntitySet(IEdmNavigationSource navigationSource)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                return new JObject();
            }

            return new JObject()
            {
                {
                    "get", new JObject()
                        .Summary("Get EntitySet " + entitySet.Name)
                        .OperationId(entitySet.Name + "_Get")
                        .Description("Returns the EntitySet " + entitySet.Name)
                        .Tags(entitySet.Name)
                        .Parameters(new JArray()
                            .Parameter("$expand", "query", "Expand navigation property", "string")
                            .Parameter("$select", "query", "select structural property", "string")
                            .Parameter("$orderby", "query", "order by some property", "string")
                            .Parameter("$top", "query", "top elements", "integer")
                            .Parameter("$skip", "query", "skip elements", "integer")
                            .Parameter("$count", "query", "include count in response", "boolean"))
                        .Responses(new JObject()
                            .Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType())
                            .DefaultErrorResponse())
                },
                {
                    "post", new JObject()
                        .Summary("Post a new entity to EntitySet " + entitySet.Name)
                        .OperationId(entitySet.Name + "_Post")
                        .Description("Post a new entity to EntitySet " + entitySet.Name)
                        .Tags(entitySet.Name)
                        .Parameters(new JArray()
                            .Parameter(entitySet.EntityType().Name, "body", "The entity to post",
                                entitySet.EntityType()))
                        .Responses(new JObject()
                            .Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType())
                            .DefaultErrorResponse())
                }
            };
        }

        /// <summary>
        /// Create the Swagger path for the Edm entity.
        /// </summary>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject"/> represents the related Edm entity.</returns>
        public static JObject CreateSwaggerPathForEntity(IEdmNavigationSource navigationSource)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                return new JObject();
            }

            var keyParameters = new JArray();
            foreach (var key in entitySet.EntityType().Key())
            {
                string format;
                string type = GetPrimitiveTypeAndFormat(key.Type.Definition as IEdmPrimitiveType, out format);
                keyParameters.Parameter(key.Name, "path", "key: " + key.Name, type, format);
            }

            return new JObject()
            {
                {
                    "get", new JObject()
                        .Summary("Get entity from " + entitySet.Name + " by key.")
                        .OperationId(entitySet.Name + "_GetById")
                        .Description("Returns the entity with the key from " + entitySet.Name)
                        .Tags(entitySet.Name)
                        .Parameters((keyParameters.DeepClone() as JArray)
                            .Parameter("$select", "query", "description", "string"))
                        .Responses(new JObject()
                            .Response("200", "EntitySet " + entitySet.Name, entitySet.EntityType())
                            .DefaultErrorResponse())
                },
                {
                    "patch", new JObject()
                        .Summary("Update entity in EntitySet " + entitySet.Name)
                        .OperationId(entitySet.Name + "_PatchById")
                        .Description("Update entity in EntitySet " + entitySet.Name)
                        .Tags(entitySet.Name)
                        .Parameters((keyParameters.DeepClone() as JArray)
                            .Parameter(entitySet.EntityType().Name, "body", "The entity to patch",
                                entitySet.EntityType()))
                        .Responses(new JObject()
                            .Response("204", "Empty response")
                            .DefaultErrorResponse())
                },
                {
                    "delete", new JObject()
                        .Summary("Delete entity in EntitySet " + entitySet.Name)
                        .OperationId(entitySet.Name + "_DeleteById")
                        .Description("Delete entity in EntitySet " + entitySet.Name)
                        .Tags(entitySet.Name)
                        .Parameters((keyParameters.DeepClone() as JArray)
                            .Parameter("If-Match", "header", "If-Match header", "string"))
                        .Responses(new JObject()
                            .Response("204", "Empty response")
                            .DefaultErrorResponse())
                }
            };
        }

        /// <summary>
        /// Create the Swagger path for the Edm operation import.
        /// </summary>
        /// <param name="operationImport">The Edm operation import</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject"/> represents the related Edm operation import.</returns>
        public static JObject CreateSwaggerPathForOperationImport(IEdmOperationImport operationImport)
        {
            if (operationImport == null)
            {
                return new JObject();
            }

            bool isFunctionImport = operationImport is IEdmFunctionImport;
            JArray swaggerParameters = new JArray();
            foreach (var parameter in operationImport.Operation.Parameters)
            {
                swaggerParameters.Parameter(parameter.Name, isFunctionImport ? "path" : "body",
                    "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            JObject swaggerResponses = new JObject();
            if (operationImport.Operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operationImport.Name,
                    operationImport.Operation.ReturnType.Definition);
            }

            JObject swaggerOperationImport = new JObject()
                .Summary("Call operation import  " + operationImport.Name)
                .OperationId(operationImport.Name + (isFunctionImport ? "_FunctionImportGet" : "_ActionImportPost"))
                .Description("Call operation import  " + operationImport.Name)
                .Tags(isFunctionImport ? "Function Import" : "Action Import");

            if (swaggerParameters.Count > 0)
            {
                swaggerOperationImport.Parameters(swaggerParameters);
            }
            swaggerOperationImport.Responses(swaggerResponses.DefaultErrorResponse());

            return new JObject()
            {
                { isFunctionImport ? "get" : "post", swaggerOperationImport }
            };
        }

        /// <summary>
        /// Create the Swagger path for the Edm operation bound to the Edm entity set.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject"/> represents the related Edm operation bound to the Edm entity set.</returns>
        public static JObject CreateSwaggerPathForOperationOfEntitySet(IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return new JObject();
            }

            bool isFunction = operation is IEdmFunction;
            JArray swaggerParameters = new JArray();
            foreach (var parameter in operation.Parameters.Skip(1))
            {
                swaggerParameters.Parameter(parameter.Name, isFunction ? "path" : "body",
                    "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            JObject swaggerResponses = new JObject();
            if (operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operation.Name,
                    operation.ReturnType.Definition);
            }

            JObject swaggerOperation = new JObject()
                .Summary("Call operation  " + operation.Name)
                .OperationId(operation.Name + (isFunction ? "_FunctionGet" : "_ActionPost"))
                .Description("Call operation  " + operation.Name)
                .Tags(entitySet.Name, isFunction ? "Function" : "Action");

            if (swaggerParameters.Count > 0)
            {
                swaggerOperation.Parameters(swaggerParameters);
            }
            swaggerOperation.Responses(swaggerResponses.DefaultErrorResponse());

            return new JObject()
            {
                { isFunction ? "get" : "post", swaggerOperation }
            };
        }

        /// <summary>
        /// Create the Swagger path for the Edm operation bound to the Edm entity.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="Newtonsoft.Json.Linq.JObject"/> represents the related Edm operation bound to the Edm entity.</returns>
        public static JObject CreateSwaggerPathForOperationOfEntity(IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return new JObject();
            }

            bool isFunction = operation is IEdmFunction;
            JArray swaggerParameters = new JArray();

            foreach (var key in entitySet.EntityType().Key())
            {
                string format;
                string type = GetPrimitiveTypeAndFormat(key.Type.Definition as IEdmPrimitiveType, out format);
                swaggerParameters.Parameter(key.Name, "path", "key: " + key.Name, type, format);
            }

            foreach (var parameter in operation.Parameters.Skip(1))
            {
                swaggerParameters.Parameter(parameter.Name, isFunction ? "path" : "body",
                    "parameter: " + parameter.Name, parameter.Type.Definition);
            }

            JObject swaggerResponses = new JObject();
            if (operation.ReturnType == null)
            {
                swaggerResponses.Response("204", "Empty response");
            }
            else
            {
                swaggerResponses.Response("200", "Response from " + operation.Name,
                    operation.ReturnType.Definition);
            }

            JObject swaggerOperation = new JObject()
                .Summary("Call operation  " + operation.Name)
                .OperationId(operation.Name + (isFunction ? "_FunctionGetById" : "_ActionPostById"))
                .Description("Call operation  " + operation.Name)
                .Tags(entitySet.Name, isFunction ? "Function" : "Action");

            if (swaggerParameters.Count > 0)
            {
                swaggerOperation.Parameters(swaggerParameters);
            }
            swaggerOperation.Responses(swaggerResponses.DefaultErrorResponse());
            return new JObject()
                {
                    { isFunction ? "get" : "post", swaggerOperation }
                };
        }

        /// <summary>
        /// Get the Uri Swagger path for the Edm entity set.
        /// </summary>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="System.String"/> path represents the related Edm entity set.</returns>
        public static string GetPathForEntity(IEdmNavigationSource navigationSource)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                return String.Empty;
            }

            string singleEntityPath = "/" + entitySet.Name + "(";
            foreach (var key in entitySet.EntityType().Key())
            {
                if (key.Type.Definition.TypeKind == EdmTypeKind.Primitive &&
                    ((IEdmPrimitiveType)key.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.String)
                {
                    singleEntityPath += "'{" + key.Name + "}', ";
                }
                else
                {
                    singleEntityPath += "{" + key.Name + "}, ";
                }
            }
            singleEntityPath = singleEntityPath.Substring(0, singleEntityPath.Length - 2);
            singleEntityPath += ")";

            return singleEntityPath;
        }

        /// <summary>
        /// Get the Uri Swagger path for Edm operation import.
        /// </summary>
        /// <param name="operationImport">The Edm operation import.</param>
        /// <returns>The <see cref="System.String"/> path represents the related Edm operation import.</returns>
        public static string GetPathForOperationImport(IEdmOperationImport operationImport)
        {
            if (operationImport == null)
            {
                return String.Empty;
            }

            string swaggerOperationImportPath = "/" + operationImport.Name + "(";
            if (operationImport.IsFunctionImport())
            {
                foreach (var parameter in operationImport.Operation.Parameters)
                {
                    swaggerOperationImportPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                }
            }
            if (swaggerOperationImportPath.EndsWith(",", StringComparison.Ordinal))
            {
                swaggerOperationImportPath = swaggerOperationImportPath.Substring(0,
                    swaggerOperationImportPath.Length - 1);
            }
            swaggerOperationImportPath += ")";

            return swaggerOperationImportPath;
        }

        /// <summary>
        /// Get the Uri Swagger path for Edm operation bound to entity set.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="System.String"/> path represents the related Edm operation.</returns>
        public static string GetPathForOperationOfEntitySet(IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return String.Empty;
            }

            string swaggerOperationPath = "/" + entitySet.Name + "/" + operation.FullName() + "(";
            if (operation.IsFunction())
            {
                foreach (var parameter in operation.Parameters.Skip(1))
                {
                    if (parameter.Type.Definition.TypeKind == EdmTypeKind.Primitive &&
                   ((IEdmPrimitiveType)parameter.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.String)
                    {
                        swaggerOperationPath += parameter.Name + "=" + "'{" + parameter.Name + "}',";
                    }
                    else
                    {
                        swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                    }
                }
            }
            if (swaggerOperationPath.EndsWith(",", StringComparison.Ordinal))
            {
                swaggerOperationPath = swaggerOperationPath.Substring(0, swaggerOperationPath.Length - 1);
            }
            swaggerOperationPath += ")";

            return swaggerOperationPath;
        }

        /// <summary>
        /// Get the Uri Swagger path for Edm operation bound to entity.
        /// </summary>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <returns>The <see cref="System.String"/> path represents the related Edm operation.</returns>
        public static string GetPathForOperationOfEntity(IEdmOperation operation, IEdmNavigationSource navigationSource)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (operation == null || entitySet == null)
            {
                return String.Empty;
            }

            string swaggerOperationPath = GetPathForEntity(entitySet) + "/" + operation.FullName() + "(";
            if (operation.IsFunction())
            {
                foreach (var parameter in operation.Parameters.Skip(1))
                {
                    if (parameter.Type.Definition.TypeKind == EdmTypeKind.Primitive &&
                   ((IEdmPrimitiveType)parameter.Type.Definition).PrimitiveKind == EdmPrimitiveTypeKind.String)
                    {
                        swaggerOperationPath += parameter.Name + "=" + "'{" + parameter.Name + "}',";
                    }
                    else
                    {
                        swaggerOperationPath += parameter.Name + "=" + "{" + parameter.Name + "},";
                    }
                }
            }
            if (swaggerOperationPath.EndsWith(",", StringComparison.Ordinal))
            {
                swaggerOperationPath = swaggerOperationPath.Substring(0, swaggerOperationPath.Length - 1);
            }
            swaggerOperationPath += ")";

            return swaggerOperationPath;
        }

        /// <summary>
        /// Create the Swagger definition for the structure Edm type.
        /// </summary>
        /// <param name="edmType">The structure Edm type.</param>
        /// <returns>The <see cref="JObject"/> represents the related structure Edm type.</returns>
        public static JObject CreateSwaggerTypeDefinitionForStructuredType(IEdmStructuredType edmType)
        {
            if (edmType == null)
            {
                return new JObject();
            }

            JObject swaggerProperties = new JObject();
            foreach (var property in edmType.StructuralProperties())
            {
                JObject swaggerProperty = new JObject().Description(property.Name);
                SetSwaggerType(swaggerProperty, property.Type.Definition);
                swaggerProperties.Add(property.Name, swaggerProperty);
            }

            return new JObject()
            {
                { "properties", swaggerProperties }
            };
        }

        private static void SetSwaggerType(JObject obj, IEdmType edmType)
        {
            Contract.Assert(obj != null);
            Contract.Assert(edmType != null);

            if (edmType.TypeKind == EdmTypeKind.Complex || edmType.TypeKind == EdmTypeKind.Entity)
            {
                obj.Add("$ref", "#/definitions/" + edmType.FullTypeName());
            }
            else if (edmType.TypeKind == EdmTypeKind.Primitive)
            {
                string format;
                string type = GetPrimitiveTypeAndFormat((IEdmPrimitiveType)edmType, out format);
                obj.Add("type", type);
                if (format != null)
                {
                    obj.Add("format", format);
                }
            }
            else if (edmType.TypeKind == EdmTypeKind.Enum)
            {
                obj.Add("type", "string");
            }
            else if (edmType.TypeKind == EdmTypeKind.Collection)
            {
                IEdmType itemEdmType = ((IEdmCollectionType)edmType).ElementType.Definition;
                JObject nestedItem = new JObject();
                SetSwaggerType(nestedItem, itemEdmType);
                obj.Add("type", "array");
                obj.Add("items", nestedItem);
            }
        }

        private static string GetPrimitiveTypeAndFormat(IEdmPrimitiveType primitiveType, out string format)
        {
            Contract.Assert(primitiveType != null);

            format = null;
            switch (primitiveType.PrimitiveKind)
            {
                case EdmPrimitiveTypeKind.String:
                    return "string";
                case EdmPrimitiveTypeKind.Int16:
                case EdmPrimitiveTypeKind.Int32:
                    format = "int32";
                    return "integer";
                case EdmPrimitiveTypeKind.Int64:
                    format = "int64";
                    return "integer";
                case EdmPrimitiveTypeKind.Boolean:
                    return "boolean";
                case EdmPrimitiveTypeKind.Byte:
                    format = "byte";
                    return "string";
                case EdmPrimitiveTypeKind.Date:
                    format = "date";
                    return "string";
                case EdmPrimitiveTypeKind.DateTimeOffset:
                    format = "date-time";
                    return "string";
                case EdmPrimitiveTypeKind.Double:
                    format = "double";
                    return "number";
                case EdmPrimitiveTypeKind.Single:
                    format = "float";
                    return "number";
                default:
                    return "string";
            }
        }

        private static JObject Responses(this JObject obj, JObject responses)
        {
            obj.Add("responses", responses);
            return obj;
        }

        private static JObject ResponseRef(this JObject responses, string name, string description, string refType)
        {
            responses.Add(name, new JObject()
            {
                { "description", description },
                {
                    "schema", new JObject()
                    {
                        { "$ref", refType }
                    }
                }
            });

            return responses;
        }

        private static JObject Response(this JObject responses, string name, string description, IEdmType type)
        {
            var schema = new JObject();
            SetSwaggerType(schema, type);

            responses.Add(name, new JObject()
            {
                { "description", description },
                { "schema", schema }
            });

            return responses;
        }

        private static JObject DefaultErrorResponse(this JObject responses)
        {
            return responses.ResponseRef("default", "Unexpected error", "#/definitions/_Error");
        }

        private static JObject Response(this JObject responses, string name, string description)
        {
            responses.Add(name, new JObject()
            {
                { "description", description },
            });

            return responses;
        }

        private static JObject Parameters(this JObject obj, JArray parameters)
        {
            obj.Add("parameters", parameters);
            return obj;
        }

        private static JArray Parameter(this JArray parameters, string name, string kind, string description, string type, string format = null)
        {
            var newParameter = new JObject()
            {
                { "name", name },
                { "in", kind },
                { "description", description },
                { "type", type },
            };

            if (!String.IsNullOrEmpty(format))
            {
                newParameter.Add("format", format);
            }

            parameters.Add(newParameter);

            return parameters;
        }

        private static JArray Parameter(this JArray parameters, string name, string kind, string description, IEdmType type)
        {
            var parameter = new JObject()
            {
                { "name", name },
                { "in", kind },
                { "description", description },
            };

            if (kind != "body")
            {
                SetSwaggerType(parameter, type);
            }
            else
            {
                var schema = new JObject();
                SetSwaggerType(schema, type);
                parameter.Add("schema", schema);
            }

            parameters.Add(parameter);
            return parameters;
        }

        private static JObject Tags(this JObject obj, params string[] tags)
        {
            obj.Add("tags", new JArray(tags));
            return obj;
        }

        private static JObject Summary(this JObject obj, string summary)
        {
            obj.Add("summary", summary);
            return obj;
        }

        private static JObject Description(this JObject obj, string description)
        {
            obj.Add("description", description);
            return obj;
        }

        private static JObject OperationId(this JObject obj, string operationId)
        {
            obj.Add("operationId", operationId);
            return obj;
        }
    }
}
