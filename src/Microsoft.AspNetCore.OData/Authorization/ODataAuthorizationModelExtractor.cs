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
                return GetEntitySetReadRestrictions(annotations);
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
                return GetEntitySetReadByKeyRestrictions(annotations);
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

        private static IEnumerable<PermissionData> GetEntitySetReadRestrictions(IEnumerable<IEdmVocabularyAnnotation> annotations)
        {
            return GetPermissions(ODataCapabilityRestrictionsConstants.ReadRestrictions, annotations);
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

        private static IEnumerable<PermissionData> GetEntitySetReadByKeyRestrictions(IEnumerable<IEdmVocabularyAnnotation> annotations)
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
