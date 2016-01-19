// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.OData.Formatter;
using Microsoft.OData.Core.UriParser.Extensions.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Factory for dynamic types in aggregation result
    /// </summary>
    /// <remarks>
    /// Implemented as "skyhook" so far. Need to look for DI in WebAPI
    /// </remarks>
    internal class AggregationDynamicTypeProvider
    {
        private static readonly MethodInfo getPropertyValueMethod = typeof(DynamicTypeWrapper).GetMethod("GetPropertyValue");
        private static readonly MethodInfo setPropertyValueMethod = typeof(DynamicTypeWrapper).GetMethod("SetPropertyValue");

        private const string ModuleName = "MainModule";
        private const string DynamicTypeName = "DynamicTypeWrapper";

        /// <summary>
        /// Generates type by provided definition.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="propertyNodes"></param>
        /// <param name="statements"></param>
        /// <returns></returns>
        /// <remarks>
        /// We create new assembly each time, but they will be collected by GC.
        /// Current performance testing results is 0.5ms per type. We should consider caching types, however trade off is between CPU perfomance and memory usage (might be it will we an option for library user)
        /// </remarks>
        public static Type GetResultType<T>(IEdmModel model, IEnumerable<GroupByPropertyNode> propertyNodes = null, IEnumerable<AggregateStatement> statements = null) where T : DynamicTypeWrapper
        {
            Contract.Assert(model != null);

            // Do not have properties, just return base class
            if ((statements == null || !statements.Any()) && (propertyNodes == null || !propertyNodes.Any()))
            {
                return typeof(T);
            }

            TypeBuilder tb = GetTypeBuilder<T>(DynamicTypeName);
            if (statements != null && statements.Any())
            {
                foreach (var field in statements)
                {
                    if (field.TypeReference.Definition.TypeKind == EdmTypeKind.Primitive)
                    {
                        var primitiveType = EdmLibHelpers.GetClrType(field.TypeReference, model);
                        CreateProperty(tb, field.AsAlias, primitiveType);
                    }
                }
            }

            if (propertyNodes != null && propertyNodes.Any())
            {
                foreach (var field in propertyNodes)
                {
                    if (field.Accessor != null && field.TypeReference.Definition.TypeKind == EdmTypeKind.Primitive)
                    {
                        var primitiveType = EdmLibHelpers.GetClrType(field.TypeReference, model);
                        CreateProperty(tb, field.Name, primitiveType);
                    }
                    else
                    {
                        var complexProp = GetResultType<DynamicTypeWrapper>(model, field.Children);
                        CreateProperty(tb, field.Name, complexProp);
                    }
                }
            }

            return tb.CreateType();
        }

        private static TypeBuilder GetTypeBuilder<T>(string typeSignature) where T : DynamicTypeWrapper
        {
            var an = new AssemblyName(typeSignature);

            // Create GC collectable assembly. It will be collected after usage and we don't need to worry about memmory usage
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(ModuleName);
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                                TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout,
                                typeof(T));
            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // Property get method
            // get
            // {
            //  return (propertyType)this.GetPropertyValue("propertyName");
            // }
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldstr, propertyName);
            getIl.Emit(OpCodes.Callvirt, getPropertyValueMethod);

            if (propertyType.IsValueType)
            {
                // for value type (type) means unboxing
                getIl.Emit(OpCodes.Unbox_Any, propertyType);
            }
            else
            {
                // for ref types (type) means cast
                getIl.Emit(OpCodes.Castclass, propertyType);
            }

            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldstr, propertyName);
            setIl.Emit(OpCodes.Ldarg_1);
            if (propertyType.IsValueType)
            {
                // Boxing value types to store as an object
                setIl.Emit(OpCodes.Box, propertyType);
            }

            setIl.Emit(OpCodes.Callvirt, setPropertyValueMethod);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}
