// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Microsoft.OData.Client;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;

namespace Microsoft.Test.E2E.AspNet.OData.Common.TypeCreator
{
    public class ODataModelTypeCreator
    {
        private readonly Token _valueToken;
        private readonly CreatorSettings _zeroNullProbability;

        public ODataModelTypeCreator()
        {
            _valueToken = new Token(new CharactorSet().Append('a', 'z').Append('A', 'Z').Append('0', '9').ToString());
            _zeroNullProbability = new CreatorSettings() { NullValueProbability = 0.0 };
        }

        private readonly Type[] primitiveTypes = new Type[] 
        {
            typeof(int),
            typeof(short),
            typeof(long),
            typeof(bool),
            typeof(string),
            typeof(byte[]),
            typeof(uint),
            typeof(ushort),
            typeof(ulong),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(DateTimeOffset),
            typeof(Guid),
            typeof(char),
            //typeof(char[]),
        };

        private List<Type> _complexTypes = new List<Type>();

        private List<Type> _complexClientTypes = new List<Type>();

        private List<Type> _entityTypes = new List<Type>();

        private List<Type> _entityClientTypes = new List<Type>();

        private List<Type> _controllerTypes = new List<Type>();

        public Assembly Assembly { get; set; }

        public List<Type> ComplexTypes
        {
            get { return _complexTypes; }
        }

        public List<Type> EntityTypes
        {
            get { return _entityTypes; }
        }

        public List<Type> EntityClientTypes
        {
            get { return _entityClientTypes; }
        }

        public List<Type> ComplexClientTypes
        {
            get { return _complexClientTypes; }
        }

        public List<Type> ControllerTypes
        {
            get { return _controllerTypes; }
        }

        public object GenerateClientRandomData(Type clientType, Random rndGen)
        {
            var serverType = this.EntityTypes.Single(t => t.Name == clientType.Name);
            return GenerateClientStructureRandomData(clientType, serverType, rndGen);
        }

        private object GenerateClientStructureRandomData(
            Type clientType,
            Type serverType,
            Random rand,
            int? depth = null)
        {
            depth = depth ?? 0;

            if (depth > 2)
            {
                return null;
            }

            depth++;

            PropertyInfo[] clientProps = clientType.GetProperties();
            PropertyInfo[] serverProps = serverType.GetProperties();

            if (clientProps.Length != serverProps.Length)
            {
                throw new InvalidOperationException("Client properties length must be same as server type");
            }

            object result = Activator.CreateInstance(clientType);
            for (int i = 0; i < clientProps.Length; i++)
            {
                if (clientProps[i].Name == "ID" && clientProps[i].PropertyType == typeof(string))
                {
                    var id = _valueToken.Generate(rand, _zeroNullProbability);
                    clientProps[i].SetValue(result, id, null);
                }
                else
                {
                    object value;
                    if (GenerateRandomDataForClient(clientProps[i].PropertyType, serverProps[i].PropertyType, rand, out value, depth))
                    {
                        clientProps[i].SetValue(result, value, null);
                    }
                }
            }

            return result;
        }

        private bool GenerateRandomDataForClient(
            Type clientType,
            Type serverType,
            Random rand,
            out object value,
            int? depth = null,
            CreatorSettings settings = null)
        {
            if (clientType == serverType)
            {
                value = InstanceCreator.CreateInstanceOf(clientType, rand, settings);

                return true;
            }
            else if (this.EntityClientTypes.Contains(clientType) ||
                     this.ComplexClientTypes.Contains(clientType))
            {
                value = GenerateClientStructureRandomData(
                            clientType,
                            serverType,
                            rand,
                            depth);

                return true;
            }
            else if (serverType.IsEnum)
            {
                value = InstanceCreator.CreateInstanceOf(serverType, rand).ToString();

                return true;
            }
            else if (clientType.IsGenericType)
            {
                if (clientType.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
                {
                    var values = Activator.CreateInstance(clientType);

                    if (depth <= 2)
                    {
                        var addMethod = clientType.GetMethod("Add");
                        Type elementType = clientType.GetGenericArguments()[0];
                        Type serverElementType = serverType.GetGenericArguments()[0];
                        int length = rand.Next(10);
                        for (int j = 0; j < length; j++)
                        {
                            object elementValue;
                            if (GenerateRandomDataForClient(elementType, serverElementType, rand, out elementValue, depth, new CreatorSettings() { NullValueProbability = 0.0 }))
                            {
                                addMethod.Invoke(values, new object[] { elementValue });
                            }
                        }
                    }

                    value = values;
                    return true;
                }
            }
            else if (this.primitiveTypes.Contains(clientType))
            {
                if (serverType == typeof(ushort)
                    || serverType == typeof(uint)
                    || serverType == typeof(ulong))
                {
                    value = InstanceCreator.CreateInstanceOf<ushort>(rand);
                    return true;
                }
                else if (serverType == typeof(char))
                {
                    value = InstanceCreator.CreateInstanceOf<char>(rand).ToString();
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Create a group of types randomly
        /// </summary>
        /// <param name="count">number of the types to be created</param>
        /// <param name="rand">random generator</param>
        public void CreateTypes(int count, Random rand)
        {
            // create a random GUID in the form of 00000000000000000000000000000000
            var uniqueName = InstanceCreator.CreateInstanceOf<Guid>(rand).ToString("N");

            var assemblyName = new AssemblyName(uniqueName);

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            ModuleBuilder modelBuilder = assemblyBuilder.DefineDynamicModule(
                assemblyName.Name);

            for (int i = 0; i < count; i++)
            {
                var choice = rand.Next(10);

                if (choice < 6)
                {
                    // Entity type
                    var newType = CreateEntityType(modelBuilder, rand, i.ToString());
                    CreateClientEntityType(modelBuilder, rand, newType);

                    var controller = CreateControllerType(modelBuilder, newType);
                    _controllerTypes.Add(controller);
                }
                else if (choice < 7)
                {
                    // Enum type
                    var newType = CreateEnumType(modelBuilder, rand, i.ToString());
                    ComplexTypes.Add(newType);
                }
                else
                {
                    // Complex Type
                    CreateCompexType(modelBuilder, rand, i.ToString());
                }
            }

            this.Assembly = assemblyBuilder;
        }

        private static Type CreateControllerType(ModuleBuilder builder, Type entityType)
        {
            var typeId = entityType.GetProperty("ID").PropertyType;
            var typeBase = typeof(InMemoryODataController<,>).MakeGenericType(entityType, typeId);
            var typeCtrl = builder.DefineType(entityType.Name + "Controller", TypeAttributes.Public, typeBase);

            var builderCtor = typeCtrl.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            // Intermediate Lanaguage of contructor
            var ilCtor = builderCtor.GetILGenerator();
            ilCtor.Emit(OpCodes.Ldarg_0);
            ilCtor.Emit(OpCodes.Ldstr, "ID");
            ilCtor.Emit(OpCodes.Call, typeBase.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(string) },
                null));
            ilCtor.Emit(OpCodes.Ret);

            return typeCtrl.CreateType();
        }

        private Type CreateEntityType(ModuleBuilder builder, Random rand, string id)
        {
            Type baseType = null;

            // for 25% chance, find a type which was already created as a base type
            if (rand.Next(4) == 0 && this.EntityTypes.Any())
            {
                baseType = this.EntityTypes[rand.Next(this.EntityTypes.Count())];
            }

            string name = builder.Assembly.GetName().Name + ".EntityType" + id;
            TypeBuilder typeBuilder = baseType == null
                ? builder.DefineType(name, TypeAttributes.Public)
                : builder.DefineType(name, TypeAttributes.Public, baseType);

            if (baseType == null)
            {
                var idTypes = (new Type[] { typeof(string), typeof(Guid), typeof(int), typeof(long) }).ToList();
                Type idType = idTypes[rand.Next(idTypes.Count())];
                typeBuilder.DefineAutoImplementedProperty("ID", idType);
            }

            // define 1~20 properties on the new entity type
            typeBuilder.DefineProperties(
                rand.Next(20) + 1,
                rand,
                primitiveTypes.Union(_complexTypes)
                              .Union(_entityTypes)
                              .Union(new Type[] { typeBuilder }).ToArray());


            var result = typeBuilder.CreateType();
            EntityTypes.Add(result);

            return result;
        }

        private Type CreateCompexType(ModuleBuilder builder, Random rand, string id)
        {
            var name = builder.Assembly.GetName().Name + ".ComplexType" + id;

            TypeBuilder typeBuilder = builder.DefineType(name, TypeAttributes.Public);

            // define 1~20 properties on the new complex type
            typeBuilder.DefineProperties(
                rand.Next(20) + 1,
                rand,
                primitiveTypes.Union(_complexTypes).ToArray());

            var result = typeBuilder.CreateType();
            ComplexTypes.Add(result);

            return result;
        }

        private static Type CreateEnumType(ModuleBuilder builder, Random rand, string id)
        {
            var name = builder.Assembly.GetName().Name + ".EnumType" + id;

            EnumBuilder enumBuilder = builder.DefineEnum(name, TypeAttributes.Public, typeof(int));

            // define 1~10 literal in the enum
            for (int i = 0; i < rand.Next(10) + 1; i++)
            {
                enumBuilder.DefineLiteral("Literal" + i, i);
            }

            var result = enumBuilder.CreateTypeInfo();

            return result;
        }

        public Type CreateClientEntityType(ModuleBuilder builder, Random rand, Type serverType)
        {
            var name = serverType.Namespace + ".Client." + serverType.Name;

            // if the same type has been already created, return the type directly.
            var existingClientType = this.EntityClientTypes.FirstOrDefault(t => t.FullName == name);
            if (existingClientType != null)
            {
                return existingClientType;
            }

            // recursively create client type of the base type of the server type
            var baseClientType = this.EntityTypes.Contains(serverType.BaseType)
                ? CreateClientEntityType(builder, rand, serverType.BaseType)
                : null;

            TypeBuilder typeBuilder = baseClientType == null
                ? builder.DefineType(name, TypeAttributes.Public)
                : builder.DefineType(name, TypeAttributes.Public, baseClientType);

            // ?? why add the type builder?
            EntityClientTypes.Add(typeBuilder);

            foreach (var property in serverType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var type = ConvertToClientType(builder, rand, property.PropertyType);
                typeBuilder.DefineAutoImplementedProperty(property.Name, type);
            }

            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                typeof(EntitySetAttribute).GetConstructor(new Type[] { typeof(string) }),
                new object[] { typeBuilder.Name });
            typeBuilder.SetCustomAttribute(cab);
            cab = new CustomAttributeBuilder(
                typeof(KeyAttribute).GetConstructor(new Type[] { typeof(string) }),
                new object[] { "ID" });
            typeBuilder.SetCustomAttribute(cab);

            var result = typeBuilder.CreateType();

            // ?? why remove it now?
            EntityClientTypes.Remove(typeBuilder);

            EntityClientTypes.Add(result);

            return result;
        }

        public Type CreateClientComplexType(ModuleBuilder mb, Random rndGen, Type complexType)
        {
            var name = complexType.Namespace + ".Client." + complexType.Name;

            var found = this.ComplexClientTypes.FirstOrDefault(t => t.FullName == name);
            if (found != null)
            {
                return found;
            }

            var tb = mb.DefineType(name, TypeAttributes.Public);
            this.ComplexClientTypes.Add(tb);
            foreach (var property in complexType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var type = ConvertToClientType(mb, rndGen, property.PropertyType);
                tb.DefineAutoImplementedProperty(property.Name, type);
            }

            var result = tb.CreateType();
            this.ComplexClientTypes.Remove(tb);
            this.ComplexClientTypes.Add(result);
            return result;
        }

        private Type ConvertToClientType(ModuleBuilder builder, Random rand, Type serverType)
        {
            if (serverType.IsEnum)
            {
                // ???
                //return typeof(string);
                return serverType;
            }
            else if (serverType == typeof(ushort))
            {
                return typeof(int);
            }
            else if (serverType == typeof(uint) || serverType == typeof(ulong))
            {
                return typeof(long);
            }
            else if (serverType == typeof(char) || serverType == typeof(char[]))
            {
                return typeof(string);
            }
            else if (this.EntityTypes.Contains(serverType))
            {
                return CreateClientEntityType(builder, rand, serverType);
            }
            else if (this.ComplexTypes.Contains(serverType))
            {
                return CreateClientComplexType(builder, rand, serverType);
            }
            else if (serverType.IsGenericType && serverType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = serverType.GetGenericArguments()[0];
                var clientType = ConvertToClientType(builder, rand, elementType);

                return typeof(ObservableCollection<>).MakeGenericType(clientType);
            }

            return serverType;
        }
    }

    public static class TypeBuilderExtensions
    {
        public static PropertyBuilder DefineAutoImplementedProperty(this TypeBuilder self, string name, Type type)
        {
            FieldBuilder field = self.DefineField(
                "m_" + name,
                type,
                FieldAttributes.Private);

            PropertyBuilder property = self.DefineProperty(
                name,
                PropertyAttributes.None,
                type,
                null);

            MethodAttributes getSetAttr =
                MethodAttributes.Public
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig;

            MethodBuilder getAccessor = self.DefineMethod(
                "get_" + name,
                getSetAttr,
                type,
                Type.EmptyTypes);

            ILGenerator getIL = getAccessor.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, field);
            getIL.Emit(OpCodes.Ret);

            MethodBuilder setAccessor = self.DefineMethod(
                "set_" + name,
                getSetAttr,
                null,
                new Type[] { type });

            ILGenerator setIL = setAccessor.GetILGenerator();
            // Load the instance and then the numeric argument, then store the 
            // argument in the field.
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, field);
            setIL.Emit(OpCodes.Ret);

            // Last, map the "get" and "set" accessor methods to the 
            // PropertyBuilder. The property is now complete. 
            property.SetGetMethod(getAccessor);
            property.SetSetMethod(setAccessor);

            return property;
        }

        public static ConstructorBuilder DefineDefaultConstructor(this TypeBuilder tb)
        {
            var baseType = tb.BaseType;
            if (baseType == null)
            {
                baseType = typeof(object);
            }
            ConstructorBuilder ctor0 = tb.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);
            ILGenerator ctor0IL = ctor0.GetILGenerator();
            ctor0IL.Emit(OpCodes.Ldarg_0);
            ctor0IL.Emit(OpCodes.Call, baseType.GetConstructor(Type.EmptyTypes));
            ctor0IL.Emit(OpCodes.Ret);

            return ctor0;
        }

        public static void DefineProperties(this TypeBuilder self, int count, Random rand, Type[] types)
        {
            for (int j = 0; j < count; j++)
            {
                Type type = types[rand.Next(types.Length)];
                string propertyName = type.Name;
                Regex r = new Regex("[^a-zA-Z0-9_]");
                propertyName = r.Replace(propertyName, "_");
                var choice = rand.Next(4);
                if (choice == 0)
                {
                    type = typeof(List<>).MakeGenericType(type);
                    propertyName += "Collection";
                }

                self.DefineAutoImplementedProperty(self.Name + "_" + propertyName + "_" + j, type);
            }
        }
    }
}
