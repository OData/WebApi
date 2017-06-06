using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Web.Http;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common.Controllers;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    public class ODataModelTypeCreator
    {
        private List<Type> primitiveTypes = new List<Type>(new Type[] 
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
            typeof(DateTime),
            typeof(Guid),
            //typeof(char[]),
            typeof(char),
        });

        private List<Type> complexTypes = new List<Type>(new Type[]
        {
            //typeof(object)
        });

        private List<Type> complexClientTypes = new List<Type>();

        private List<Type> entityTypes = new List<Type>();

        private List<Type> entityClientTypes = new List<Type>();

        private List<Type> controllerTypes = new List<Type>();

        public Assembly Assembly { get; set; }

        public List<Type> ComplexTypes
        {
            get
            {
                return complexTypes;
            }
        }

        public List<Type> EntityTypes
        {
            get
            {
                return entityTypes;
            }
        }

        public List<Type> EntityClientTypes
        {
            get
            {
                return entityClientTypes;
            }
        }

        public List<Type> ComplexClientTypes
        {
            get
            {
                return complexClientTypes;
            }
        }

        public List<Type> ControllerTypes
        {
            get
            {
                return controllerTypes;
            }
        }

        public object GenerateClientRandomData(Type clientType, Random rndGen)
        {
            var serverType = this.EntityTypes.Single(t => t.Name == clientType.Name);
            return GenerateClientStructureRandomData(clientType, serverType, rndGen);
        }

        private object GenerateClientStructureRandomData(Type clientType, Type serverType, Random rndGen, int? depth = null)
        {
            if (depth == null)
            {
                depth = 0;
            }
            else if (depth > 2)
            {
                return null;
            }

            depth++;

            var clientProps = clientType.GetProperties();
            var serverProps = serverType.GetProperties();
            if (clientProps.Length != serverProps.Length)
            {
                throw new InvalidOperationException("Client properties length must be same as server type");
            }

            object result = Activator.CreateInstance(clientType);
            for (int i = 0; i < clientProps.Length; i++)
            {
                if (clientProps[i].Name == "ID" && clientProps[i].PropertyType == typeof(string))
                {
                    var id = new Token(new CharsToUseBuilder().Append('a', 'z').Append('A', 'Z').Append('0', '9').ToString()).Generate(rndGen, new CreatorSettings() 
                    { 
                        NullValueProbability = 0.0 
                    });
                    clientProps[i].SetValue(result, id, null);
                    continue;
                }

                object value;
                if (GenerateClientRandomData(clientProps[i].PropertyType, serverProps[i].PropertyType, rndGen, out value, depth))
                {
                    clientProps[i].SetValue(result, value, null);
                }
            }

            return result;
        }

        public bool GenerateClientRandomData(Type clientType, Type serverType, Random rndGen, out object value, int? depth = null, CreatorSettings settings = null)
        {
            if (clientType == serverType)
            {
                value = InstanceCreator.CreateInstanceOf(clientType, rndGen, settings);
                return true;
            }
            else if (this.EntityClientTypes.Contains(clientType)
                    || this.ComplexClientTypes.Contains(clientType))
            {
                value = GenerateClientStructureRandomData(
                            clientType,
                            serverType,
                            rndGen,
                            depth);
                return true;
            }
            else if (serverType.IsEnum)
            {
                value = InstanceCreator.CreateInstanceOf(serverType, rndGen).ToString();
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
                        int length = rndGen.Next(10);
                        for (int j = 0; j < length; j++)
                        {
                            object elementValue;
                            if (GenerateClientRandomData(elementType, serverElementType, rndGen, out elementValue, depth, new CreatorSettings() { NullValueProbability = 0.0 }))
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
                    value = InstanceCreator.CreateInstanceOf<ushort>(rndGen);
                    return true;
                }
                else if (serverType == typeof(char))
                {
                    value = InstanceCreator.CreateInstanceOf<char>(rndGen).ToString();
                    return true;
                }
            }
            value = null;
            return false;
        }

        public void Test()
        {
            Random rndGen = new Random(RandomSeedGenerator.GetRandomSeed());
            CreateTypes(20, rndGen);

            foreach (var type in this.entityTypes)
            {
                Console.WriteLine(type.FullName);
                foreach (var p in type.GetProperties())
                {
                    Console.WriteLine("  {0} {1}", p.PropertyType, p.Name);
                }
                Activator.CreateInstance(type);
            }
        }

        public void CreateTypes(int count, Random rndGen)
        {
            var uniqueName = InstanceCreator.CreateInstanceOf<Guid>(rndGen).ToString("N");
            AssemblyName assemblyName = new AssemblyName(uniqueName);
            AssemblyBuilder ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mb =
                ab.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll");

            for (int i = 0; i < count; i++)
            {
                Type type = null;
                var choice = rndGen.Next(10);
                if (choice < 6)
                {
                    // Entity type
                    type = CreateEntityType(mb, rndGen, i.ToString());
                    CreateClientEntityType(mb, rndGen, type);

                    CreateControllerType(mb, type);
                }
                else if (choice < 7)
                {
                    // Enum type
                    type = CreateEnumType(mb, rndGen, i.ToString());
                }
                else
                {
                    // Complex Type
                    type = CreateCompexType(mb, rndGen, i.ToString());
                }
            }

            this.Assembly = ab;
        }

        private void CreateControllerType(ModuleBuilder mb, Type entityType)
        {
            var idType = entityType.GetProperty("ID").PropertyType;
            var baseType = typeof(InMemoryEntitySetController<,>).MakeGenericType(entityType, idType);
            var controllerType = mb.DefineType(entityType.Name + "Controller", TypeAttributes.Public, baseType);
            var ctor = controllerType.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);
            var ctorIL = ctor.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldstr, "ID");
            ctorIL.Emit(OpCodes.Call, baseType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(string) },
                null));
            ctorIL.Emit(OpCodes.Ret);
            //controllerType.DefineDefaultConstructor();
            this.controllerTypes.Add(controllerType.CreateType());
        }

        public Type CreateEntityType(ModuleBuilder mb, Random rndGen, string typeID)
        {
            Type baseType = null;
            if (rndGen.Next(4) == 0 && this.EntityTypes.Any())
            {
                baseType = this.EntityTypes[rndGen.Next(this.EntityTypes.Count())];
            }
            var name = mb.Assembly.GetName().Name + ".EntityType" + typeID;
            var tb = baseType == null
                ? mb.DefineType(name, TypeAttributes.Public)
                : mb.DefineType(name, TypeAttributes.Public, baseType);
            //tb.DefineDefaultConstructor();

            if (baseType == null)
            {
                var idTypes = (new Type[] { typeof(string), typeof(Guid), typeof(int), typeof(long) }).ToList();
                Type idType = idTypes[rndGen.Next(idTypes.Count())];
                tb.DefineAutoImplementedProperty("ID", idType);
            }
            tb.DefineProperties(rndGen.Next(20) + 1, rndGen, primitiveTypes.Union(complexTypes).Union(entityTypes).Union(new Type[] { tb }).ToArray());
            var result = tb.CreateType();
            this.EntityTypes.Add(result);
            return result;
        }

        public Type CreateCompexType(ModuleBuilder mb, Random rndGen, string typeID)
        {
            var name = mb.Assembly.GetName().Name + ".ComplexType" + typeID;
            var tb = mb.DefineType(name, TypeAttributes.Public);
            //tb.DefineDefaultConstructor();
            tb.DefineProperties(rndGen.Next(20) + 1, rndGen, primitiveTypes.Union(complexTypes).ToArray());
            var result = tb.CreateType();
            this.ComplexTypes.Add(result);
            return result;
        }

        public Type CreateEnumType(ModuleBuilder mb, Random rndGen, string typeID)
        {
            var name = mb.Assembly.GetName().Name + ".EnumType" + typeID;
            var eb = mb.DefineEnum(name, TypeAttributes.Public, typeof(int));
            for (int i = 0; i < rndGen.Next(10) + 1; i++)
            {
                eb.DefineLiteral("Literal" + i, i);
            }
            var result = eb.CreateType();
            this.ComplexTypes.Add(result);
            return result;
        }

        public Type CreateClientEntityType(ModuleBuilder mb, Random rndGen, Type entityType)
        {
            var name = entityType.Namespace + ".Client." + entityType.Name;
            var found = this.EntityClientTypes.FirstOrDefault(t => t.FullName == name);
            if (found != null)
            {
                return found;
            }

            var baseClientType = this.EntityTypes.Contains(entityType.BaseType)
                ? CreateClientEntityType(mb, rndGen, entityType.BaseType)
                : null;

            var tb = baseClientType == null
                ? mb.DefineType(name, TypeAttributes.Public)
                : mb.DefineType(name, TypeAttributes.Public, baseClientType);
            this.EntityClientTypes.Add(tb);

            foreach (var property in entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var type = ConvertToClientType(mb, rndGen, property.PropertyType);
                tb.DefineAutoImplementedProperty(property.Name, type);
            }

            CustomAttributeBuilder cab = new CustomAttributeBuilder(
                typeof(EntitySetAttribute).GetConstructor(new Type[] { typeof(string) }),
                new object[] { tb.Name });
            tb.SetCustomAttribute(cab);
            cab = new CustomAttributeBuilder(
                typeof(DataServiceKeyAttribute).GetConstructor(new Type[] { typeof(string) }),
                new object[] { "ID" });
            tb.SetCustomAttribute(cab);

            var result = tb.CreateType();
            this.EntityClientTypes.Remove(tb);
            this.EntityClientTypes.Add(result);
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

        private Type ConvertToClientType(ModuleBuilder mb, Random rndGen, Type type)
        {
            if (type.IsEnum)
            {
                return typeof(string);
            }
            else if (type == typeof(ushort))
            {
                return typeof(int);
            }
            else if (type == typeof(uint) || type == typeof(ulong))
            {
                return typeof(long);
            }
            else if (type == typeof(char) || type == typeof(char[]))
            {
                return typeof(string);
            }
            else if (this.EntityTypes.Contains(type))
            {
                return CreateClientEntityType(mb, rndGen, type);
            }
            else if (this.ComplexTypes.Contains(type))
            {
                return CreateClientComplexType(mb, rndGen, type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                var clientType = ConvertToClientType(mb, rndGen, elementType);
                return typeof(ObservableCollection<>).MakeGenericType(clientType);
            }

            return type;
        }
    }

    public static class TypeBuilderExtensions
    {
        public static PropertyBuilder DefineAutoImplementedProperty(this TypeBuilder tb, string name, Type type)
        {
            FieldBuilder field = tb.DefineField(
                "m_" + name,
                type,
                FieldAttributes.Private);

            PropertyBuilder property = tb.DefineProperty(
                name,
                PropertyAttributes.None,
                type,
                null);

            MethodAttributes getSetAttr =
                MethodAttributes.Public
                | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig;

            MethodBuilder getAccessor = tb.DefineMethod(
                "get_" + name,
                getSetAttr,
                type,
                Type.EmptyTypes);

            ILGenerator getIL = getAccessor.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, field);
            getIL.Emit(OpCodes.Ret);

            MethodBuilder setAccessor = tb.DefineMethod(
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

        public static void DefineProperties(this TypeBuilder tb, int propertyCount, Random rndGen, Type[] propertyTypes)
        {
            for (int j = 0; j < propertyCount; j++)
            {
                Type type = propertyTypes[rndGen.Next(propertyTypes.Length)];
                string propertyName = type.Name;
                Regex r = new Regex("[^a-zA-Z0-9_]");
                propertyName = r.Replace(propertyName, "_");
                var choice = rndGen.Next(4);
                if (choice == 0)
                {
                    type = typeof(List<>).MakeGenericType(type);
                    propertyName += "Collection";
                }

                tb.DefineAutoImplementedProperty(tb.Name + "_" + propertyName + "_" + j, type);
            }
        }
    }
}
