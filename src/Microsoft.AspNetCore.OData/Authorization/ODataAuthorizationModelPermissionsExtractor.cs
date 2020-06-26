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
            if (template.StartsWith("~/entityset/key/property") ||
                template.StartsWith("~/entityset/key/cast/property") ||
                template.StartsWith("~/singleton/property") ||
                template.StartsWith("~/singleton/cast/property") ||
                template.EndsWith("/dynamicproperty") ||
                // navigation properties $ref
                template == "~/entityset/key/navigation/$ref" ||
                template == "~/entityset/key/cast/navigation/$ref" ||
                template == "~/singleton/navigation/$ref" ||
                template == "~/singleton/cast/navigation/$ref" ||
                template == "~/entityset/key/navigation/key/$ref" ||
                template == "~/entityset/key/cast/navigation/key/$ref" ||
                template == "~/singleton/navigation/key/$ref" ||
                template == "~/singleton/cast/navigation/key/$ref")
            {
                var isSingleton = template.StartsWith("~/singleton");
                var annotations = isSingleton ?
                    model.VocabularyAnnotations.Where(a => IsAnnotationForSingleton(a, odataPath)) :
                    model.VocabularyAnnotations.Where(a => IsAnnotationForEntitySet(a, odataPath));

                if (method == "GET")
                {
                    return isSingleton ?
                        GetReadPermissions(annotations) :
                        GetReadByKeyPermissions(annotations);
                }
                else if (method == "PUT" || method == "MERGE" || method == "PATCH" || method == "DELETE" || method == "POST")
                {
                    return GetUpdatePermissions(annotations);
                }
            }

            // navigation properties
            if (template.EndsWith("/navigation") || template.EndsWith("/navigation/$count"))
            {
                NavigationPropertySegment navigationSegment =
                    (odataPath.Segments.Last() as NavigationPropertySegment) ??
                    odataPath.Segments[odataPath.Segments.Count - 2] as NavigationPropertySegment;

                var setName = navigationSegment.NavigationSource.Name;
                var entitySet = model.FindDeclaredEntitySet(setName);
                var annotations = entitySet.VocabularyAnnotations(model);
                //var annotations = navigationSegment.NavigationProperty.VocabularyAnnotations(model);
                //navigationSegment.NavigationSource.FindNavigationTarget(navigationSegment.NavigationProperty)
                
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

            // functions and actions
            if (template.StartsWith("~/entityset/key/function") ||
                template.StartsWith("~/entityset/key/cast/function") ||
                template.StartsWith("~/entityset/function") ||
                template.StartsWith("~/entityset/cast/function") ||
                template.StartsWith("~/singleton/function") ||
                template.StartsWith("~/singleton/cast/function") ||
                template.StartsWith("~/entityset/key/action") ||
                template.StartsWith("~/entityset/key/cast/action") ||
                template.StartsWith("~/entityset/action") ||
                template.StartsWith("~/entityset/cast/action") ||
                template.StartsWith("~/singleton/action") ||
                template.StartsWith("~/singleton/cast/action"))
            {
                var annotations = model.VocabularyAnnotations.Where(a => IsAnnotationForOperation(a, odataPath));
                return GetOperationPermissions(annotations);
            }

            // unbound functions and actions
            if (template == "~/unboundaction" || template == "~/unboundfunction")
            {
                var segment = odataPath.Segments.Last() as OperationImportSegment;
                var annotations = segment.OperationImports.FirstOrDefault()?.Operation.VocabularyAnnotations(model);
                if (annotations != null)
                {
                    return GetOperationPermissions(annotations);
                }
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
