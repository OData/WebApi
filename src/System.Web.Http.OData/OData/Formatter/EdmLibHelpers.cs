// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Spatial;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query.Expressions;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace System.Web.Http.OData.Formatter
{
    internal static class EdmLibHelpers
    {
        private static readonly EdmCoreModel _coreModel = EdmCoreModel.Instance;

        private static readonly IAssembliesResolver _defaultAssemblyResolver = new DefaultAssembliesResolver();

        private static readonly Dictionary<Type, IEdmPrimitiveType> _builtInTypesMapping =
            new[]
            {
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(string), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool?), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte?), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime), GetPrimitiveType(EdmPrimitiveTypeKind.DateTime)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTime?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTime)),
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
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan), GetPrimitiveType(EdmPrimitiveTypeKind.Time)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan?), GetPrimitiveType(EdmPrimitiveTypeKind.Time)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset?), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)),

                // Keep the Binary and XElement in the end, since there are not the default mappings for Edm.Binary and Edm.String.
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(XElement), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Binary), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ushort), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ushort?), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(uint), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(uint?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ulong), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(ulong?), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char[]), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(char?), GetPrimitiveType(EdmPrimitiveTypeKind.String)),
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

                if (clrType.BaseType != null)
                {
                    // go up the inheritance tree to see if we have a mapping defined for the base type.
                    returnType = returnType ?? GetEdmType(edmModel, clrType.BaseType, testCollections);
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

        public static IEdmCollectionType GetCollection(this IEdmEntityType entityType)
        {
            return new EdmCollectionType(new EdmEntityTypeReference(entityType, isNullable: false));
        }

        private static bool CanBindTo(this IEdmFunctionImport function, IEdmEntityType entity)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (!function.IsBindable)
            {
                return false;
            }

            // The binding parameter is the first parameter by convention
            IEdmFunctionParameter bindingParameter = function.Parameters.FirstOrDefault();
            if (bindingParameter == null)
            {
                return false;
            }

            IEdmEntityType bindingParameterType = bindingParameter.Type.Definition as IEdmEntityType;
            if (bindingParameterType == null)
            {
                return false;
            }

            return entity.IsOrInheritsFrom(bindingParameterType);
        }

        private static bool CanBindTo(this IEdmFunctionImport function, IEdmCollectionType collection)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }
            if (collection == null)
            {
                throw Error.ArgumentNull("collection");
            }
            if (!function.IsBindable)
            {
                return false;
            }

            // The binding parameter is the first parameter by convention
            IEdmFunctionParameter bindingParameter = function.Parameters.FirstOrDefault();
            if (bindingParameter == null)
            {
                return false;
            }

            IEdmCollectionType bindingParameterType = bindingParameter.Type.Definition as IEdmCollectionType;
            if (bindingParameterType == null)
            {
                return false;
            }

            IEdmEntityType bindingParameterElementType = bindingParameterType.ElementType.Definition as IEdmEntityType;
            IEdmEntityType entity = collection.ElementType.Definition as IEdmEntityType;
            if (bindingParameterElementType == null || entity == null)
            {
                return false;
            }

            return entity.IsOrInheritsFrom(bindingParameterElementType);
        }

        public static IEnumerable<IEdmFunctionImport> GetMatchingActions(this IEnumerable<IEdmFunctionImport> functions, string actionIdentifier)
        {
            if (functions == null)
            {
                throw Error.ArgumentNull("functions");
            }
            if (actionIdentifier == null)
            {
                throw Error.ArgumentNull("actionIdentifier");
            }

            string[] nameParts = actionIdentifier.Split('.');
            Contract.Assert(nameParts.Length != 0);

            if (nameParts.Length == 1)
            {
                // Name
                string name = nameParts[0];
                return functions.Where(f => f.IsSideEffecting && f.Name == name);
            }
            else if (nameParts.Length == 2)
            {
                // Container.Name
                string name = nameParts[nameParts.Length - 1];
                string container = nameParts[nameParts.Length - 2];
                return functions.Where(f => f.IsSideEffecting && f.Name == name && f.Container.Name == container);
            }
            else
            {
                // Namespace.Container.Name
                string name = nameParts[nameParts.Length - 1];
                string container = nameParts[nameParts.Length - 2];
                string nspace = String.Join(".", nameParts.Take(nameParts.Length - 2));
                return functions.Where(f => f.IsSideEffecting && f.Name == name && f.Container.Name == container && f.Container.Namespace == nspace);
            }
        }

        public static IEdmFunctionImport FindBindableAction(this IEnumerable<IEdmFunctionImport> functions,
            IEdmEntityType entityType, string actionIdentifier)
        {
            if (functions == null)
            {
                throw Error.ArgumentNull("functions");
            }
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }
            if (actionIdentifier == null)
            {
                throw Error.ArgumentNull("actionIdentifier");
            }

            IEnumerable<IEdmFunctionImport> matches =
                functions.GetMatchingActions(actionIdentifier).Where(fi => fi.CanBindTo(entityType));
            return FindBest(actionIdentifier, matches, entityType, isCollection: false);
        }

        // Performs overload resolution between a set of matching bindable actions. OData protocol ensures that there 
        // cannot be multiple bindable actions with same name and different sets of non-bindable paramters. 
        // The resolution logic is simple and is dependant only on the binding parameter and chooses the action that is defined
        // closest to the binding parameter in the inheritance hierarchy.
        private static IEdmFunctionImport FindBest(string actionIdentifier, IEnumerable<IEdmFunctionImport> bindableActions,
            IEdmEntityType bindingParameterType, bool isCollection)
        {
            if (bindingParameterType == null)
            {
                return null;
            }

            List<IEdmFunctionImport> actionsBoundToThisType = new List<IEdmFunctionImport>();
            foreach (IEdmFunctionImport action in bindableActions)
            {
                IEdmType actionParameterType = action.Parameters.First().Type.Definition;
                if (isCollection)
                {
                    actionParameterType = ((IEdmCollectionType)actionParameterType).ElementType.Definition;
                }

                if (actionParameterType == bindingParameterType)
                {
                    actionsBoundToThisType.Add(action);
                }
            }

            if (actionsBoundToThisType.Count > 1)
            {
                throw Error.Argument(
                    "actionIdentifier",
                    SRResources.ActionResolutionFailed,
                    actionIdentifier,
                    String.Join(", ", actionsBoundToThisType.Select(match => match.Container.FullName() + "." + match.Name)));
            }
            else if (actionsBoundToThisType.Count == 1)
            {
                return actionsBoundToThisType[0];
            }
            else
            {
                return FindBest(actionIdentifier, bindableActions, bindingParameterType.BaseEntityType(), isCollection);
            }
        }

        public static IEdmFunctionImport FindBindableAction(this IEnumerable<IEdmFunctionImport> functions,
            IEdmCollectionType collectionType, string actionIdentifier)
        {
            if (functions == null)
            {
                throw Error.ArgumentNull("functions");
            }
            if (collectionType == null)
            {
                throw Error.ArgumentNull("collectionType");
            }
            if (actionIdentifier == null)
            {
                throw Error.ArgumentNull("actionIdentifier");
            }

            IEnumerable<IEdmFunctionImport> matches =
                functions.GetMatchingActions(actionIdentifier).Where(fi => fi.CanBindTo(collectionType));

            IEdmEntityType elementType = (IEdmEntityType)collectionType.ElementType.Definition;
            return FindBest(actionIdentifier, matches, elementType, isCollection: true);
        }

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel)
        {
            return GetClrType(edmTypeReference, edmModel, _defaultAssemblyResolver);
        }

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel, IAssembliesResolver assembliesResolver)
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
                return GetClrType(edmTypeReference.Definition, edmModel, assembliesResolver);
            }
        }

        public static Type GetClrType(IEdmType edmType, IEdmModel edmModel)
        {
            return GetClrType(edmType, edmModel, _defaultAssemblyResolver);
        }

        public static Type GetClrType(IEdmType edmType, IEdmModel edmModel, IAssembliesResolver assembliesResolver)
        {
            IEdmSchemaType edmSchemaType = edmType as IEdmSchemaType;

            Contract.Assert(edmSchemaType != null);

            ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmSchemaType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            string typeName = edmSchemaType.FullName();
            IEnumerable<Type> matchingTypes = GetMatchingTypes(typeName, assembliesResolver);

            if (matchingTypes.Count() > 1)
            {
                throw Error.Argument("edmTypeReference", SRResources.MultipleMatchingClrTypesForEdmType,
                    typeName, String.Join(",", matchingTypes.Select(type => type.AssemblyQualifiedName)));
            }

            edmModel.SetAnnotationValue<ClrTypeAnnotation>(edmSchemaType, new ClrTypeAnnotation(matchingTypes.SingleOrDefault()));

            return matchingTypes.SingleOrDefault();
        }

        public static IEdmPrimitiveType GetEdmPrimitiveTypeOrNull(Type clrType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;
            if (underlyingType.IsEnum)
            {
                // Enums are treated as strings
                clrType = typeof(string);
            }

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

        private static IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind primitiveKind)
        {
            return _coreModel.GetPrimitiveType(primitiveKind);
        }

        public static bool IsNullable(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static bool IsSelectExpandWrapper(Type type, out Type entityType)
        {
            if (type == null)
            {
                entityType = null;
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SelectExpandWrapper<>))
            {
                entityType = type.GetGenericArguments()[0];
                return true;
            }

            return IsSelectExpandWrapper(type.BaseType, out entityType);
        }

        private static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType;
            return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        private static IEnumerable<Type> GetMatchingTypes(string edmFullName, IAssembliesResolver assembliesResolver)
        {
            return TypeHelper.GetLoadedTypes(assembliesResolver).Where(t => t.IsPublic && t.EdmFullName() == edmFullName);
        }

        // TODO (workitem 336): Support nested types and anonymous types.
        private static string MangleClrTypeName(Type type)
        {
            Contract.Assert(type != null);

            if (!type.IsGenericType)
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
