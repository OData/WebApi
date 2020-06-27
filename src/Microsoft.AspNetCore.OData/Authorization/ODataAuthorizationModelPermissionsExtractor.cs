using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Authorization
{
    internal static class ODataAuthorizationModelPermissionsExtractor
    {
        /// <summary>
        /// Extract permissions from the <paramref name="model"/> that should apply to the current request.
        /// </summary>
        /// <param name="model">The OData model.</param>
        /// <param name="context">The HTTP context.</param>
        /// <returns></returns>
        internal static IEnumerable<PermissionData> ExtractPermissionsForRequest(this IEdmModel model, HttpContext context)
        {
            var odataFeature = context.ODataFeature();

            var odataPath = odataFeature.Path;
            var template = odataPath.PathTemplate;
            var method = context.Request.Method;

            // $ref segment does not appear in odataPath.Segments, that's why we treat this case separately
            if (template.EndsWith("$ref"))
            {
                // for ref segments, we apply the permission of the entity that contains the navigation property
                // e.g. for GET Customers(10)/Products/$ref, we apply the read key permissions of Customers
                // for GET TopCustomer/Products/$ref, we apply the read permissions of TopCustomer
                // for DELETE Customers(10)/Products(10)/$ref we apply the update permissions of Customers
                var index = odataPath.Segments.Count - 2;
                while (!(odataPath.Segments[index] is KeySegment || odataPath.Segments[index] is SingletonSegment) && index > 0)
                {
                    index--;
                }

                if (odataPath.Segments[index] is SingletonSegment singletonSegment)
                {
                    return GetSingletonPropertyAccessPermissions(singletonSegment.Singleton, model, method);
                }
                else if (odataPath.Segments[index] is KeySegment keySegment)
                {
                    var entitySet = keySegment.NavigationSource as IEdmEntitySet;
                    return GetEntityPropertyAccessPermissions(entitySet, model, method);
                }
            }
            else
            {
                ODataPathSegment mainSegment = null;
                int index = odataPath.Segments.Count - 1;
                for (; index >= 0; index--)
                {
                    var segment = odataPath.Segments[index];
                    if (segment is EntitySetSegment ||
                        segment is SingletonSegment ||
                        segment is NavigationPropertySegment ||
                        segment is OperationSegment ||
                        segment is OperationImportSegment ||
                        segment is KeySegment ||
                        segment is PropertySegment)
                    {
                        mainSegment = segment;
                        break;
                    }
                }

                if (mainSegment is EntitySetSegment entitySetSegment)
                {
                    return GetNavigationSourceCrudPermissions(entitySetSegment.EntitySet, model, method);
                }
                else if (mainSegment is SingletonSegment singletonSegment)
                {
                    return GetNavigationSourceCrudPermissions(singletonSegment.Singleton, model, method);
                }
                else if (mainSegment is NavigationPropertySegment navigationSegment)
                {
                    var target = navigationSegment.NavigationSource as IEdmVocabularyAnnotatable;
                    return GetNavigationSourceCrudPermissions(target, model, method);
                }
                else if (mainSegment is KeySegment keySegment)
                {
                    var entitySet = keySegment.NavigationSource as IEdmEntitySet;
                    return GetEntityCrudPermissions(entitySet, model, method);
                }
                else if (mainSegment is OperationSegment operationSegment)
                {
                    var annotations = operationSegment.Operations.First().VocabularyAnnotations(model);
                    return GetOperationPermissions(annotations);
                }
                else if (mainSegment is OperationImportSegment operationImportSegment)
                {
                    var annotations = operationImportSegment.OperationImports.First().Operation.VocabularyAnnotations(model);
                    return GetOperationPermissions(annotations);
                }
                else if (mainSegment is PropertySegment)
                {
                    // for operations on a property, we apply the permissions of the
                    // entity or singleton that contain the property
                    // e.g. for GET Products(10)/Name, we apply the read key permissions of Products
                    // for GET TopProduct/Name, we apply the read permissions of TopProduct
                    int entityIndex = index;
                    while (!(odataPath.Segments[entityIndex] is SingletonSegment || odataPath.Segments[entityIndex] is KeySegment) && entityIndex > 0)
                    {
                        entityIndex--;
                    }

                    if (odataPath.Segments[entityIndex] is SingletonSegment containingSingleton)
                    {
                        return GetSingletonPropertyAccessPermissions(containingSingleton.Singleton, model, method);
                    }
                    else if (odataPath.Segments[entityIndex] is KeySegment containingEntity)
                    {
                        var entitySet = containingEntity.NavigationSource as IEdmEntitySet;
                        return GetEntityPropertyAccessPermissions(entitySet, model, method);
                    }
                }
            }

            return Enumerable.Empty<PermissionData>();
        }

        private static IEnumerable<PermissionData> GetSingletonPropertyAccessPermissions(IEdmVocabularyAnnotatable target, IEdmModel model, string method)
        {
            var annotations = target.VocabularyAnnotations(model);
            if (method == "GET")
            {
                return GetReadPermissions(annotations);
            }
            else if (method == "PATCH" || method == "PUT" || method == "MERGE" || method == "POST" || method == "DELETE")
            {
                return GetUpdatePermissions(annotations);
            }

            return Enumerable.Empty<PermissionData>();
        }

        private static IEnumerable<PermissionData> GetEntityPropertyAccessPermissions(IEdmVocabularyAnnotatable target, IEdmModel model, string method)
        {
            var annotations = target.VocabularyAnnotations(model);
            if (method == "GET")
            {
                return GetReadByKeyPermissions(annotations);
            }
            else if (method == "PATCH" || method == "PUT" || method == "MERGE" || method == "POST" || method == "DELETE")
            {
                return GetUpdatePermissions(annotations);
            }

            return Enumerable.Empty<PermissionData>();
        }

        private static IEnumerable<PermissionData> GetNavigationSourceCrudPermissions(IEdmVocabularyAnnotatable target, IEdmModel model, string method)
        {
            var annotations = target.VocabularyAnnotations(model);
            if (method == "GET")
            {
                return GetReadPermissions(annotations);
            }
            else if (method == "POST")
            {
                return GetInsertPermissions(annotations);
            }
            else if (method == "PATCH" || method == "PUT" || method == "MERGE")
            {
                return GetUpdatePermissions(annotations);
            }
            else if (method == "DELETE")
            {
                return GetDeletePermissions(annotations);
            }

            return Enumerable.Empty<PermissionData>();
        }

        private static IEnumerable<PermissionData> GetEntityCrudPermissions(IEdmVocabularyAnnotatable target, IEdmModel model, string method)
        {
            var annotations = target.VocabularyAnnotations(model);

            if (method == "GET")
            {
                return GetReadByKeyPermissions(annotations);
            }
            else if (method == "PUT" || method == "POST" || method == "MERGE" || method == "PATCH")
            {
                return GetUpdatePermissions(annotations);
            }
            else if (method == "DELETE")
            {
                return GetDeletePermissions(annotations);
            }

            return Enumerable.Empty<PermissionData>();
        }

        private static IEnumerable<PermissionData> GetReadPermissions(IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            return GetPermissions(ODataCapabilityRestrictionsConstants.ReadRestrictions, annotations);
        }

        private static IEnumerable<PermissionData> GetReadByKeyPermissions(IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            foreach (var annotation in annotations)
            {
                if (annotation.Term.FullName() == ODataCapabilityRestrictionsConstants.ReadRestrictions && annotation.Value is IEdmRecordExpression record)
                {
                    var readByKeyProperty = record.FindProperty("ReadByKeyRestrictions");
                    var readByKeyValue = readByKeyProperty?.Value as IEdmRecordExpression;
                    var permissionsProperty = readByKeyValue?.FindProperty("Permissions");
                    return ExtractPermissionsFromProperty(permissionsProperty);
                }
            }

            return Enumerable.Empty<PermissionData>();
        }

        private static IEnumerable<PermissionData> GetInsertPermissions(IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            return GetPermissions(ODataCapabilityRestrictionsConstants.InsertRestrictions, annotations);
        }

        private static IEnumerable<PermissionData> GetDeletePermissions(IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            return GetPermissions(ODataCapabilityRestrictionsConstants.DeleteRestrictions, annotations);
        }

        private static IEnumerable<PermissionData> GetUpdatePermissions(IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            return GetPermissions(ODataCapabilityRestrictionsConstants.UpdateRestrictions, annotations);
        }

        private static IEnumerable<PermissionData> GetOperationPermissions(IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            return GetPermissions(ODataCapabilityRestrictionsConstants.OperationRestrictions, annotations);
        }

        private static IEnumerable<PermissionData> GetPermissions(string restrictionType, IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            foreach (var annotation in annotations)
            {
                if (annotation.Term.FullName() == restrictionType)
                {
                    return ExtractPermissionsFromAnnotation(annotation);
                }
            }

            return Enumerable.Empty<PermissionData>();
        }

        private static IEnumerable<PermissionData> ExtractPermissionsFromAnnotation(IEdmVocabularyAnnotation annotation)
        {
            return ExtractPermissionsFromRecord(annotation.Value as IEdmRecordExpression);
        }

        private static IEnumerable<PermissionData> ExtractPermissionsFromRecord(IEdmRecordExpression record)
        {
            var permissionsProperty = record?.FindProperty("Permissions");
            return ExtractPermissionsFromProperty(permissionsProperty);
        }

        private static IEnumerable<PermissionData> ExtractPermissionsFromProperty(IEdmPropertyConstructor permissionsProperty)
        {
            if (permissionsProperty?.Value is IEdmCollectionExpression permissionsValue)
            {
                return permissionsValue.Elements.OfType<IEdmRecordExpression>().Select(p => GetPermissionData(p));
            }

            return Enumerable.Empty<PermissionData>();
        }

        private static PermissionData GetPermissionData(IEdmRecordExpression permissionRecord)
        {
            var schemeProperty = permissionRecord.FindProperty("SchemeName")?.Value as IEdmStringConstantExpression;
            var scopesProperty = permissionRecord.FindProperty("Scopes")?.Value as IEdmCollectionExpression;

            var scopes = scopesProperty.Elements.Select(s => GetScopeData(s as IEdmRecordExpression));

            return new PermissionData() { SchemeName = schemeProperty.Value, Scopes = scopes.ToList() };
        }

        private static PermissionScopeData GetScopeData(IEdmRecordExpression scopeRecord)
        {
            var scopeProperty = scopeRecord.FindProperty("Scope")?.Value as IEdmStringConstantExpression;
            var restrictedPropertiesProperty = scopeRecord.FindProperty("RestrictedProperties")?.Value as IEdmStringConstantExpression;

            return new PermissionScopeData() { Scope = scopeProperty.Value, RestrictedProperties = restrictedPropertiesProperty.Value };
        }
    }
}
