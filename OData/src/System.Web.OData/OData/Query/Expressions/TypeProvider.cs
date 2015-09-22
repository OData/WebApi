using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.OData.Formatter;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Factory for dynamic types
    /// </summary>
    /// <remarks>
    /// Implemented as "skyhook" so far. Need to look for DI in WebAPI
    /// </remarks>
    internal class TypeProvider
    {
        private static readonly MethodInfo getPropertyValueMethod = typeof(DynamicTypeWrapper).GetMethod("GetPropertyValue");
        private static readonly MethodInfo setPropertyValueMethod = typeof(DynamicTypeWrapper).GetMethod("SetPropertyValue");

        private const string ModuleName = "MainModule";

        /// <summary>
        /// Generates type by provided definition.
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// We create new assembly each time, but they will be collected by GC.
        /// Current performance testing results is 0.5ms per type. We should consider caching types, however trade off is between CPU perfomance and memory usage (might be it will we an option for library user)
        /// </remarks>
        public static Type GetResultType(IEdmStructuredType definition, IEdmModel model)
        {
            // Do not have properties, just return base class
            if (!definition.DeclaredProperties.Any())
            {
                return typeof(DynamicTypeWrapper);
            }

            TypeBuilder tb = GetTypeBuilder(definition.FullTypeName() + Guid.NewGuid().ToString());
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            foreach (var field in definition.Properties())
            {

                CreateProperty(tb, field.Name, EdmLibHelpers.GetClrType(field.Type, model));
            }

            return tb.CreateType();
        }

        private static TypeBuilder GetTypeBuilder(string typeSignature)
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
                                TypeAttributes.AutoLayout
                                , typeof(DynamicTypeWrapper));
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


            // Property set method
            // set 
            // {
            //  return this.SetPropertyValue("propertyName", value);
            // }

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
