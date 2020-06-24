using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.OData.Authorization
{
    internal static class ODataAuthorizationModelExtractor
    {
        internal static IEnumerable<PermissionData> ExtractPermissionRestrictions(this IEdmModel model, HttpContext context)
        {
            var odataFeature = context.ODataFeature();

            var odataPath = odataFeature.Path;
            var template = odataPath.PathTemplate;
            var method = context.Request.Method;

            if ((template == "~/entityset" && method == "GET")
                || (template == "~/entityset/cast" && method == "GET")
                || (template == "~/entityset/$count" && method == "GET")
                || (template == "~/entityset/cast/$count" && method == "GET"))
            {
                var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForEntitySet(a, odataPath));
                return GetReadPermissions(annotations);
            }

            if ((template == "~/entityset" && method == "POST")
                || (template == "~/entityset/cast" && method == "POST"))
            {
                // insert
                var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForEntitySet(a, odataPath));
                return GetInsertPermissions(annotations);
            }

            if ((template == "~/entityset/key" && method == "GET")
                || (template == "~/entityset/key/cast" && method == "GET"))
            {
                var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForEntitySet(a, odataPath));
                return GetReadByKeyPermissions(annotations);
            }

            if ((template == "~/entityset/key" || template == "~/entityset/key/cast")
                && new string[] { "PUT", "PATCH", "MERGE" }.Contains(method))
            {
                // update
                var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForEntitySet(a, odataPath));
                return GetUpdatePermissions(annotations);
            }

            if ((template == "~/entityset/key" || template == "~/entityset/key/cast") && method == "DELETE")
            {
                var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForEntitySet(a, odataPath));
                return GetDeletePermissions(annotations);
            }

            if ((template == "~/singleton" || template == "~/singleton/cast"))
            {
                var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForSingleton(a, odataPath));
                if (method == "GET")
                {
                    return GetReadPermissions(annotations);
                }
                else if (method == "PUT" || method == "MERGE" || method == "PATCH")
                {
                    return GetUpdatePermissions(annotations);
                }
                else if (method == "DELETE")
                {
                    return GetDeletePermissions(annotations);
                }
            }

            // properties
            if (template == "~/entityset/key/property" ||
                template == "~/entityset/key/cast/property" ||
                template == "~/entityset/key/property/cast" ||
                template == "~/entityset/key/cast/property/cast" ||
                template == "~/entityset/key/property/$value" ||
                template == "~/entityset/key/cast/property/$value" ||
                template == "~/entityset/key/property/$count" ||
                template == "~/entityset/key/cast/property/$count" ||
                template == "~/singleton/property" ||
                template == "~/singleton/cast/property" ||
                template == "~/singleton/property/cast" ||
                template == "~/singleton/cast/property/cast" ||
                template == "~/singleton/property/$value" ||
                template == "~/singleton/cast/property/$value" ||
                template == "~/singleton/property/$count" ||
                template == "~/singleton/cast/property/$count")
            {
                var isSingleton = template.StartsWith("~/singleton");
                var annotations = isSingleton ?
                    model.VocabularyAnnotations.Where(a => IsAnnotationForSingleton(a, odataPath)):
                    model.VocabularyAnnotations.Where(a => IsAnnotationForEntitySet(a, odataPath));

                if (method == "GET")
                {
                    return isSingleton ?
                        GetReadPermissions(annotations):
                        GetReadByKeyPermissions(annotations);
                }
                else if (method == "PUT" || method == "MERGE" || method == "PATCH" || method == "DELETE" || method == "POST")
                {
                    return GetUpdatePermissions(annotations);
                }
            }

            // functions and actions
            if (template == "~/entityset/key/function"
                || template == "~/entityset/key/cast/function"
                || template == "~/entityset/key/function/$count"
                || template == "~/entityset/key/cast/function/$count"
                || template == "~/entityset/function"
                || template == "~/entityset/function/$count"
                || template == "~/entityset/cast/function"
                || template == "~/entityset/cast/function/$count"
                || template == "~/singleton/function"
                || template == "~/singleton/function/$count"
                || template == "~/singleton/cast/function"
                || template == "~/singleton/cast/function/$count"
                // actions
                || template == "~/entityset/action"
                || template == "~/entityset/cast/action"
                || template == "~/entityset/key/action"
                || template == "~/entityset/key/cast/action"
                || template == "~/singleton/action"
                || template == "~/singleton/cast/action"
                )
            {
                var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForOperation(a, odataPath));
                return GetOperationPermissions(annotations);
            }


            return Enumerable.Empty<PermissionData>();
        }

        private static bool IsAnnotationForEntitySet(IEdmVocabularyAnnotation annotation, Routing.ODataPath path)
        {
            if (annotation.Target is IEdmEntitySet target && path.Segments[0] is EntitySetSegment segment)
            {
                return target.Name == segment.EntitySet.Name;
            }

            return false;
        }

        private static bool IsAnnotationForSingleton(IEdmVocabularyAnnotation annotation, Routing.ODataPath path)
        {
            if (annotation.Target is IEdmSingleton target && path.Segments[0] is SingletonSegment segment)
            {
                return target.Name == segment.Singleton.Name;
            }

            return false;
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

        private static IEnumerable<PermissionData> ExtractPermissionsFromAnnotation(IEdmVocabularyAnnotation annotation)
        {
            var value = annotation.Value;
            if (value is IEdmRecordExpression record)
            {
                var permissionsProperty = record.FindProperty("Permissions");
                return ExtractPermissionsFromProperty(permissionsProperty);
            }

            return Enumerable.Empty<PermissionData>();
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
