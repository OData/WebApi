﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.Spatial;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.AspNetCore.OData.Formatter
{
    public static class EdmLibHelpers
    {
        private static readonly EdmCoreModel _coreModel = EdmCoreModel.Instance;

        private static readonly IAssemblyProvider _defaultAssemblyProvider = null;

        private static readonly Dictionary<Type, IEdmPrimitiveType> _builtInTypesMapping =
            new[]
            {
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(string), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool?), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte?), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(decimal), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(decimal?), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(double), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(double?), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid?), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(short), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(short?), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(int), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(int?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(long), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(long?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(sbyte), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(sbyte?), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(float), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(float?), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte[]), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Stream), GetPrimitiveType(EdmPrimitiveTypeKind.Stream)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geography), GetPrimitiveType(EdmPrimitiveTypeKind.Geography)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geometry), GetPrimitiveType(EdmPrimitiveTypeKind.Geometry)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan?), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Date), GetPrimitiveType(EdmPrimitiveTypeKind.Date)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Date?), GetPrimitiveType(EdmPrimitiveTypeKind.Date)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeOfDay), GetPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeOfDay?), GetPrimitiveType(EdmPrimitiveTypeKind.TimeOfDay)),

                // Keep the Binary and XElement in the end, since there are not the default mappings for Edm.Binary and Edm.String.
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(XElement), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                //new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Binary), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ushort), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ushort?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(uint), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(uint?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ulong), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ulong?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char[]), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char?), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
            }
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public static IEdmType GetEdmType(this IEdmModel edmModel, Type clrType)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            return GetEdmType(edmModel, clrType, testCollections: true);
        }

        private static IEdmType GetEdmType(IEdmModel edmModel, Type clrType, bool testCollections)
        {
            Contract.Assert(edmModel != null);
            Contract.Assert(clrType != null);

            IEdmPrimitiveType primitiveType = GetEdmPrimitiveTypeOrNull(clrType);
            if (primitiveType != null)
            {
                return primitiveType;
            }
            else
            {
                if (testCollections)
                {
                    Type enumerableOfT = ExtractGenericInterface(clrType, typeof(IEnumerable<>));
                    if (enumerableOfT != null)
                    {
                        Type elementClrType = enumerableOfT.GetGenericArguments()[0];

                        // IEnumerable<SelectExpandWrapper<T>> is a collection of T.
                        Type entityType;
                        if (IsSelectExpandWrapper(elementClrType, out entityType))
                        {
                            elementClrType = entityType;
                        }

                        IEdmType elementType = GetEdmType(edmModel, elementClrType, testCollections: false);
                        if (elementType != null)
                        {
                            return new EdmCollectionType(elementType.ToEdmTypeReference(IsNullable(elementClrType)));
                        }
                    }
                }

                Type underlyingType = TypeHelper.GetUnderlyingTypeOrSelf(clrType);

                if (underlyingType.GetTypeInfo().IsEnum)
                {
                    clrType = underlyingType;
                }

                // search for the ClrTypeAnnotation and return it if present
                IEdmType returnType =
                    edmModel
                    .SchemaElements
                    .OfType<IEdmType>()
                    .Select(edmType => new { EdmType = edmType, Annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmType) })
                    .Where(tuple => tuple.Annotation != null && tuple.Annotation.ClrType == clrType)
                    .Select(tuple => tuple.EdmType)
                    .SingleOrDefault();

                // default to the EdmType with the same name as the ClrType name 
                returnType = returnType ?? edmModel.FindType(clrType.EdmFullName());

                if (clrType.GetTypeInfo().BaseType != null)
                {
                    // go up the inheritance tree to see if we have a mapping defined for the base type.
                    returnType = returnType ?? GetEdmType(edmModel, clrType.GetTypeInfo().BaseType, testCollections);
                }
                return returnType;
            }
        }

        public static IEdmTypeReference GetEdmTypeReference(this IEdmModel edmModel, Type clrType)
        {
            IEdmType edmType = edmModel.GetEdmType(clrType);
            if (edmType != null)
            {
                bool isNullable = IsNullable(clrType);
                return ToEdmTypeReference(edmType, isNullable);
            }

            return null;
        }

        public static IEdmTypeReference ToEdmTypeReference(this IEdmType edmType, bool isNullable)
        {
            Contract.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Collection:
                    return new EdmCollectionTypeReference(edmType as IEdmCollectionType);
                case EdmTypeKind.Complex:
                    return new EdmComplexTypeReference(edmType as IEdmComplexType, isNullable);
                case EdmTypeKind.Entity:
                    return new EdmEntityTypeReference(edmType as IEdmEntityType, isNullable);
                case EdmTypeKind.EntityReference:
                    return new EdmEntityReferenceTypeReference(edmType as IEdmEntityReferenceType, isNullable);
                case EdmTypeKind.Enum:
                    return new EdmEnumTypeReference(edmType as IEdmEnumType, isNullable);
                case EdmTypeKind.Primitive:
                    return _coreModel.GetPrimitive((edmType as IEdmPrimitiveType).PrimitiveKind, isNullable);
                default:
                    throw Error.NotSupported(SRResources.EdmTypeNotSupported, edmType.ToTraceString());
            }
        }

        public static IEdmCollectionType GetCollection(this IEdmEntityType entityType)
        {
            return new EdmCollectionType(new EdmEntityTypeReference(entityType, isNullable: false));
        }

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel)
        {
            return GetClrType(edmTypeReference, edmModel, _defaultAssemblyProvider);
        }

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel, IAssemblyProvider assembliesProvider)
        {
            if (edmTypeReference == null)
            {
                throw Error.ArgumentNull("edmTypeReference");
            }

            Type primitiveClrType = _builtInTypesMapping
                .Where(kvp => edmTypeReference.Definition.IsEquivalentTo(kvp.Value) && (!edmTypeReference.IsNullable || IsNullable(kvp.Key)))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (primitiveClrType != null)
            {
                return primitiveClrType;
            }
            else
            {
                Type clrType = GetClrType(edmTypeReference.Definition, edmModel, assembliesProvider);
                if (clrType != null && clrType.GetTypeInfo().IsEnum && edmTypeReference.IsNullable)
                {
                    return clrType.ToNullable();
                }

                return clrType;
            }
        }

        public static Type GetClrType(IEdmType edmType, IEdmModel edmModel)
        {
            return GetClrType(edmType, edmModel, _defaultAssemblyProvider);
        }

        public static Type GetClrType(IEdmType edmType, IEdmModel edmModel, IAssemblyProvider assembliesProvider)
        {
            IEdmSchemaType edmSchemaType = edmType as IEdmSchemaType;

            Contract.Assert(edmSchemaType != null);

            ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmSchemaType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            string typeName = edmSchemaType.FullName();
            IEnumerable<Type> matchingTypes = GetMatchingTypes(typeName, assembliesProvider);

            if (matchingTypes.Count() > 1)
            {
                throw Error.Argument("edmTypeReference", SRResources.MultipleMatchingClrTypesForEdmType,
                    typeName, String.Join(",", matchingTypes.Select(type => type.AssemblyQualifiedName)));
            }

            edmModel.SetAnnotationValue<ClrTypeAnnotation>(edmSchemaType, new ClrTypeAnnotation(matchingTypes.SingleOrDefault()));

            return matchingTypes.SingleOrDefault();
        }

        public static bool IsNotFilterable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotFilterable;
        }

        public static bool IsNotSortable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotSortable;
        }

        public static bool IsNotNavigable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotNavigable;
        }

        public static bool IsNotExpandable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotExpandable;
        }

        public static bool IsNotCountable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotCountable;
        }

        public static bool IsAutoExpand(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.AutoExpand;
        }

        private static QueryableRestrictionsAnnotation GetPropertyRestrictions(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            Contract.Assert(edmProperty != null);
            Contract.Assert(edmModel != null);

            return edmModel.GetAnnotationValue<QueryableRestrictionsAnnotation>(edmProperty);
        }

        public static string GetClrPropertyName(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            string propertyName = edmProperty.Name;
            ClrPropertyInfoAnnotation annotation = edmModel.GetAnnotationValue<ClrPropertyInfoAnnotation>(edmProperty);
            if (annotation != null)
            {
                PropertyInfo propertyInfo = annotation.ClrPropertyInfo;
                if (propertyInfo != null)
                {
                    propertyName = propertyInfo.Name;
                }
            }

            return propertyName;
        }

        public static PropertyInfo GetDynamicPropertyDictionary(IEdmStructuredType edmType, IEdmModel edmModel)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            DynamicPropertyDictionaryAnnotation annotation =
                edmModel.GetAnnotationValue<DynamicPropertyDictionaryAnnotation>(edmType);
            if (annotation != null)
            {
                return annotation.PropertyInfo;
            }

            return null;
        }

        public static IEdmPrimitiveType GetEdmPrimitiveTypeOrNull(Type clrType)
        {
            IEdmPrimitiveType primitiveType;
            return _builtInTypesMapping.TryGetValue(clrType, out primitiveType) ? primitiveType : null;
        }

        public static IEdmPrimitiveTypeReference GetEdmPrimitiveTypeReferenceOrNull(Type clrType)
        {
            IEdmPrimitiveType primitiveType = GetEdmPrimitiveTypeOrNull(clrType);
            return primitiveType != null ? _coreModel.GetPrimitive(primitiveType.PrimitiveKind, IsNullable(clrType)) : null;
        }

        // figures out if the given clr type is nonstandard edm primitive like uint, ushort, char[] etc.
        // and returns the corresponding clr type to which we map like uint => long.
        public static Type IsNonstandardEdmPrimitive(Type type, out bool isNonstandardEdmPrimitive)
        {
            IEdmPrimitiveTypeReference edmType = GetEdmPrimitiveTypeReferenceOrNull(type);
            if (edmType == null)
            {
                isNonstandardEdmPrimitive = false;
                return type;
            }

            Type reverseLookupClrType = GetClrType(edmType, EdmCoreModel.Instance);
            isNonstandardEdmPrimitive = (type != reverseLookupClrType);

            return reverseLookupClrType;
        }

        // Mangle the invalid EDM literal Type.FullName (System.Collections.Generic.IEnumerable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]) 
        // to a valid EDM literal (the C# type name IEnumerable<int>).
        public static string EdmName(this Type clrType)
        {
            // We cannot use just Type.Name here as it doesn't work for generic types.
            return MangleClrTypeName(clrType);
        }

        public static string EdmFullName(this Type clrType)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", clrType.Namespace, clrType.EdmName());
        }

        private static ConcurrentDictionary<IEdmEntitySet, IEnumerable<IEdmStructuralProperty>> _concurrencyProperties;

        public static IEnumerable<IEdmStructuralProperty> GetConcurrencyProperties(this IEdmModel model, IEdmEntitySet entitySet)
        {
            Contract.Assert(model != null);
            Contract.Assert(entitySet != null);

            IEnumerable<IEdmStructuralProperty> cachedProperties;
            if (_concurrencyProperties != null && _concurrencyProperties.TryGetValue(entitySet, out cachedProperties))
            {
                return cachedProperties;
            }

            IList<IEdmStructuralProperty> results = new List<IEdmStructuralProperty>();
            IEdmEntityType entityType = entitySet.EntityType();
            var annotations = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(entitySet, CoreVocabularyModel.ConcurrencyTerm);
            IEdmVocabularyAnnotation annotation = annotations.FirstOrDefault();
            if (annotation != null)
            {
                IEdmCollectionExpression properties = annotation.Value as IEdmCollectionExpression;
                if (properties != null)
                {
                    foreach (var property in properties.Elements)
                    {
                        IEdmPathExpression pathExpression = property as IEdmPathExpression;
                        if (pathExpression != null)
                        {
                            // So far, we only consider the single path, because only the direct properties from declaring type are used.
                            // However we have an issue tracking on: https://github.com/OData/WebApi/issues/472
                            string propertyName = pathExpression.PathSegments.First();
                            IEdmProperty edmProperty = entityType.FindProperty(propertyName);
                            IEdmStructuralProperty structuralProperty = edmProperty as IEdmStructuralProperty;
                            if (structuralProperty != null)
                            {
                                results.Add(structuralProperty);
                            }
                        }
                    }
                }
            }

            if (_concurrencyProperties == null)
            {
                _concurrencyProperties = new ConcurrentDictionary<IEdmEntitySet, IEnumerable<IEdmStructuralProperty>>();
            }

            _concurrencyProperties[entitySet] = results;
            return results;
        }

        private static IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind primitiveKind)
        {
            return _coreModel.GetPrimitiveType(primitiveKind);
        }

        public static bool IsNullable(Type type)
        {
            return !type.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static bool IsSelectExpandWrapper(Type type, out Type entityType)
        {
            if (type == null)
            {
                entityType = null;
                return false;
            }

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(SelectExpandWrapper<>))
            {
                entityType = type.GetGenericArguments()[0];
                return true;
            }

            return IsSelectExpandWrapper(type.GetTypeInfo().BaseType, out entityType);
        }

        private static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == interfaceType;
            return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        private static IEnumerable<Type> GetMatchingTypes(string edmFullName, IAssemblyProvider assembliesProvider)
        {
            return TypeHelper.GetLoadedTypes(assembliesProvider).Where(t => t.GetTypeInfo().IsPublic && t.EdmFullName() == edmFullName);
        }

        // TODO (workitem 336): Support nested types and anonymous types.
        private static string MangleClrTypeName(Type type)
        {
            Contract.Assert(type != null);

            if (!type.GetTypeInfo().IsGenericType)
            {
                return type.Name;
            }
            else
            {
                return String.Format(
                    CultureInfo.InvariantCulture,
                    "{0}Of{1}",
                    type.Name.Replace('`', '_'),
                    String.Join("_", type.GetGenericArguments().Select(t => MangleClrTypeName(t))));
            }
        }
    }
}
