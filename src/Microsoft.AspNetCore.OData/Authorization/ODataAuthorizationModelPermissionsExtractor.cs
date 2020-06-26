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

            if (template.EndsWith("$ref"))
            {
                // for ref segments, we apply the permission of the entity that contains the navigation property
                // e.g. for Customers(10)/Products/$ref, we apply the read key permissions of Customers
                var index = odataPath.Segments.Count - 2;
                while (!(odataPath.Segments[index] is KeySegment || odataPath.Segments[index] is SingletonSegment) && index > 0)
                {
                    index--;
                }

                if (odataPath.Segments[index] is SingletonSegment singletonSegment)
                {
                    var annotations = singletonSegment.Singleton.VocabularyAnnotations(model);
                    if (method == "GET")
                    {
                        return GetReadPermissions(annotations);
                    }
                    else if (method == "PATCH" || method == "PUT" || method == "MERGE" || method == "POST" || method == "DELETE")
                    {
                        return GetUpdatePermissions(annotations);
                    }
                }
                else if (odataPath.Segments[index] is KeySegment keySegment)
                {
                    var entitySet = keySegment.NavigationSource as IEdmEntitySet;
                    var annotations = entitySet.VocabularyAnnotations(model);

                    if (method == "GET")
                    {
                        return GetReadByKeyPermissions(annotations);
                    }
                    else if (method == "PUT" || method == "POST" || method == "MERGE" || method == "PATCH" || method == "DELETE")
                    {
                        return GetUpdatePermissions(annotations);
                    }
                }
            }
            else if (template.EndsWith("/property") ||
                template.EndsWith("/property/$value") ||
                template.EndsWith("/property/$count"))
            {
                // find the key segment, or singleton the precedes the property
                var index = odataPath.Segments.Count - 1;
                while (!(odataPath.Segments[index] is SingletonSegment || odataPath.Segments[index] is KeySegment) && index > 0)
                {
                    index--;
                }

                if (odataPath.Segments[index] is SingletonSegment singletonSegment)
                {
                    var annotations = singletonSegment.Singleton.VocabularyAnnotations(model);
                    if (method == "GET")
                    {
                        return GetReadPermissions(annotations);
                    }
                    else if (method == "PATCH" || method == "PUT" || method == "MERGE" || method == "POST" || method == "DELETE")
                    {
                        return GetUpdatePermissions(annotations);
                    }
                }
                else if (odataPath.Segments[index] is KeySegment keySegment)
                { 
                    var entitySet = keySegment.NavigationSource as IEdmEntitySet;
                    var annotations = entitySet.VocabularyAnnotations(model);

                    if (method == "GET")
                    {
                        return GetReadByKeyPermissions(annotations);
                    }
                    else if (method == "PUT" || method == "POST" || method == "MERGE" || method == "PATCH" || method == "DELETE")
                    {
                        return GetUpdatePermissions(annotations);
                    }
                }
            }
            else
            {
                ODataPathSegment mainSegment = null;
                for (var index = odataPath.Segments.Count - 1; index >= 0; index--)
                {
                    var segment = odataPath.Segments[index];
                    if (segment is EntitySetSegment ||
                        segment is SingletonSegment ||
                        segment is NavigationPropertySegment ||
                        segment is OperationSegment ||
                        segment is OperationImportSegment ||
                        segment is KeySegment)
                    {
                        mainSegment = segment;
                        break;
                    }
                }

                if (mainSegment is EntitySetSegment entitySetSegment)
                {
                    var annotations = entitySetSegment.EntitySet.VocabularyAnnotations(model);
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
                }
                else if (mainSegment is SingletonSegment singletonSegment)
                {
                    var annotations = singletonSegment.Singleton.VocabularyAnnotations(model);
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
                }
                else if (mainSegment is KeySegment keySegment)
                {
                    var entitySet = keySegment.NavigationSource as IEdmEntitySet;
                    var annotations = entitySet.VocabularyAnnotations(model);

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
                }
                else if (mainSegment is NavigationPropertySegment navigationSegment)
                {
                    var entitySet = navigationSegment.NavigationSource as IEdmEntitySet;
                    var singleton = navigationSegment.NavigationSource as IEdmSingleton;
                    var annotations = entitySet?.VocabularyAnnotations(model) ?? singleton?.VocabularyAnnotations(model);

                    if (method == "GET")
                    {
                        return GetReadPermissions(annotations);
                    }
                    else if (method == "PATCH" || method == "PUT" || method == "MERGE")
                    {
                        return GetUpdatePermissions(annotations);
                    }
                    else if (method == "POST")
                    {
                        return GetInsertPermissions(annotations);
                    }
                    else if (method == "DELETE")
                    {
                        return GetDeletePermissions(annotations);
                    }
                }
                else if (mainSegment is OperationSegment operationSegment)
                {
                    //var annotations = operationSegment.Operations.First().VocabularyAnnotations(model);
                    var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForOperation(a, odataPath));
                    return GetOperationPermissions(annotations);
                }
                else if (mainSegment is OperationImportSegment operationImportSegment)
                {
                    var annotations = operationImportSegment.OperationImports.First().Operation.VocabularyAnnotations(model);
                    return GetOperationPermissions(annotations);
                }
            }

            return Enumerable.Empty<PermissionData>();
        }


        private static bool IsAnnotationForOperation(IEdmVocabularyAnnotation annotation, Routing.ODataPath path)
        {
            var segment = path.Segments.Last() as OperationSegment;
            if (segment == null)
            {
                segment = path.Segments[path.Segments.Count() - 2] as OperationSegment;
            }

            if (annotation.Target is IEdmOperation target && segment != null)
            {
                return target.FullName() == segment.Operations.First().FullName();
            }

            return false;
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

        private static IEnumerable<PermissionData> GetNavigationPermissions(IEnumerable<IEdmVocabularyAnnotation> annotations, string restrictionsType)
        {
            var annotation = annotations.FirstOrDefault(a => a.Term.FullName() == ODataCapabilityRestrictionsConstants.NavigationRestrictions);
            if (annotation != null && annotation.Value is IEdmRecordExpression record)
            {
                var restriction = record.Properties.FirstOrDefault(p => p.Name == restrictionsType);
                return ExtractPermissionsFromRecord(restriction?.Value as IEdmRecordExpression);
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
