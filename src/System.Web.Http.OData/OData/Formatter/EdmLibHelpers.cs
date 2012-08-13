// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Spatial;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Properties;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace System.Web.Http.OData.Formatter
{
    public static class EdmLibHelpers
    {
        private static readonly EdmCoreModel _coreModel = EdmCoreModel.Instance;

        private static readonly Dictionary<Type, IEdmPrimitiveType> _builtInTypesMapping =
            new[]
            {
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(string), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Boolean), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Boolean?), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Byte), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Byte?), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime), GetPrimitiveType(EdmPrimitiveTypeKind.DateTime)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTime)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Decimal), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Decimal?), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Double), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Double?), GetPrimitiveType(EdmPrimitiveTypeKind.Double)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid?), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Int16), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Int16?), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Int32), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Int32?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Int64), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Int64?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(SByte), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(SByte?), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Single), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Single?), GetPrimitiveType(EdmPrimitiveTypeKind.Single)),
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
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan), GetPrimitiveType(EdmPrimitiveTypeKind.Time)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan?), GetPrimitiveType(EdmPrimitiveTypeKind.Time)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),

                // Keep the Binary and XElement in the end, since there are not the default mappings for Edm.Binary and Edm.String.
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(XElement), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Binary), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)),
            }
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public static IEdmType GetEdmType(this IEdmModel edmModel, Type clrType)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            IEdmPrimitiveType primitiveType;
            if (_builtInTypesMapping.TryGetValue(clrType, out primitiveType))
            {
                return primitiveType;
            }
            else
            {
                Type enumerableOfT = ExtractGenericInterface(clrType, typeof(IEnumerable<>));
                if (enumerableOfT != null)
                {
                    IEdmTypeReference elementType = GetEdmTypeReference(edmModel, enumerableOfT.GetGenericArguments()[0]);
                    if (elementType != null)
                    {
                        return new EdmCollectionType(elementType);
                    }
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
                returnType = returnType ?? edmModel.FindType(clrType.FullName);
                return returnType;
            }
        }

        public static IEdmTypeReference GetEdmTypeReference(this IEdmModel edmModel, Type clrType)
        {
            IEdmType edmType = edmModel.GetEdmType(clrType);
            if (edmType != null)
            {
                bool isNullable = IsNullable(clrType);
                switch (edmType.TypeKind)
                {
                    case EdmTypeKind.Collection:
                        return new EdmCollectionTypeReference(edmType as IEdmCollectionType, isNullable);
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
                    case EdmTypeKind.Row:
                        return new EdmRowTypeReference(edmType as IEdmRowType, isNullable);
                    default:
                        throw Error.NotSupported(SRResources.EdmTypeNotSupported, edmType.ToTraceString());
                }
            }

            return null;
        }

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel)
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
                ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmTypeReference.Definition);
                if (annotation != null)
                {
                    return annotation.ClrType;
                }

                // search all the loaded assemblies for a type with the same name
                IEnumerable<Type> matchingTypes =
                    AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(assembly =>
                        {
                            Type[] types;
                            try
                            {
                                types = assembly.GetTypes();
                            }
                            catch (ReflectionTypeLoadException e)
                            {
                                types = e.Types;
                            }

                            return types ?? Enumerable.Empty<Type>();
                        })
                    .Where(type => type != null && type.FullName == edmTypeReference.FullName());

                if (matchingTypes.Count() > 1)
                {
                    throw Error.InvalidOperation(SRResources.MultipleMatchingClrTypesForEdmType,
                        edmTypeReference.FullName(), String.Join(",", matchingTypes.Select(type => type.AssemblyQualifiedName)));
                }

                edmModel.SetAnnotationValue<ClrTypeAnnotation>(edmTypeReference.Definition, new ClrTypeAnnotation(matchingTypes.SingleOrDefault()));

                return matchingTypes.SingleOrDefault();
            }
        }

        public static IEdmPrimitiveType GetEdmPrimitiveTypeOrNull(Type clrType)
        {
            IEdmPrimitiveType primitiveType;
            return _builtInTypesMapping.TryGetValue(clrType, out primitiveType) ? primitiveType : null;
        }

        public static IEdmPrimitiveTypeReference GetEdmPrimitiveTypeReferenceOrNull(Type clrType)
        {
            IEdmPrimitiveType primitiveType = GetEdmPrimitiveTypeOrNull(clrType);
            return primitiveType != null ? new EdmPrimitiveTypeReference(primitiveType, IsNullable(clrType)) : null;
        }

        private static IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind primitiveKind)
        {
            return _coreModel.GetPrimitiveType(primitiveKind);
        }

        private static bool IsNullable(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType;
            return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }
    }
}
