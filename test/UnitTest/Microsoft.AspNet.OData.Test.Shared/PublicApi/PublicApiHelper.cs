//-----------------------------------------------------------------------------
// <copyright file="PublicApiHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.AspNet.OData.Test.PublicApi
{
    internal static class PublicApiHelper
    {
        private static bool AlphabeticalGrouping = false;
        private static readonly List<Assembly> Assemblies = new List<Assembly>();

        private static Hashtable _synonms;
        public static Hashtable Synonms
        {
            get
            {
                if (_synonms == null)
                {
                    _synonms = CreateSynonms();
                }

                return _synonms;
            }
        }

        public static void DumpPublicApi(StreamWriter streamWriter, params string[] assemblyNames)
        {
            IList<Assembly> assemblies = new List<Assembly>();
            for (int k = 0; k < assemblyNames.Length; ++k)
            {
                try
                {
                    Assembly assembly;
                    if (File.Exists(assemblyNames[k]))
                    {
                        assembly = Assembly.LoadFrom(assemblyNames[k]);
                    }
                    else
                    {
                        assembly = Assembly.Load(assemblyNames[k]);
                    }

                    assemblies.Add(assembly);
                }
                catch (Exception e)
                {
                    streamWriter.WriteLine(@"Error loading types from assembly '{0}':", assemblyNames[k]);
                    streamWriter.WriteLine(e.ToString());
                    Environment.Exit(1);
                }
            }

            DumpPublicApi(streamWriter, assemblies.ToArray());
        }

        public static void DumpPublicApi(StreamWriter streamWriter, params Assembly[] assemblies)
        {
            Reset();

            if (assemblies.Length <= 0)
            {
                return;
            }

            ArrayList typesList = new ArrayList();
            foreach (var assembly in assemblies)
            {
                Assemblies.Add(assembly);
                typesList.AddRange(assembly.GetTypes());
            }

            typesList.Sort(TypeCompare.Default);
            DumpPublicApiImplementation(streamWriter, typesList);
        }

        private static Hashtable CreateSynonms()
        {
            Hashtable synonms = new Hashtable();

            synonms.Add("System.Void", "void");
            synonms.Add("System.Object", "object");
            synonms.Add("System.String", "string");
            synonms.Add("System.Int16", "short");
            synonms.Add("System.Int32", "int");
            synonms.Add("System.Int64", "long");
            synonms.Add("System.Byte", "byte");
            synonms.Add("System.Boolean", "bool");
            synonms.Add("System.Char", "char");
            synonms.Add("System.Decimal", "decimal");
            synonms.Add("System.Double", "double");
            synonms.Add("System.Single", "float");

            synonms.Add("System.Object[]", "object[]");
            synonms.Add("System.Char[]", "char[]");
            synonms.Add("System.Byte[]", "byte[]");
            synonms.Add("System.Int32[]", "int[]");
            synonms.Add("System.String[]", "string[]");

            return synonms;
        }

        private static void DumpPublicApiImplementation(StreamWriter streamWriter, ArrayList sortedTypeList)
        {
            StringBuilder builder = new StringBuilder();
            string lastNamespace = "";
            foreach (Type type in sortedTypeList)
            {
                builder.Length = 0;
                
                if (type.IsSpecialName)
                {
                    continue;
                }

                string typeFullName = type.FullName;
                if (typeFullName.StartsWith("<PrivateImplementationDetails>"))
                {
                    continue;
                }

                Type declaringType = type;
                while (null != declaringType)
                {
                    switch (TypeAttributes.VisibilityMask & declaringType.Attributes)
                    {
                        case TypeAttributes.Public:
                        case TypeAttributes.NestedPublic:
                        case TypeAttributes.NestedFamily:
                        case TypeAttributes.NestedFamANDAssem:
                        case TypeAttributes.NestedFamORAssem:
                            declaringType = declaringType.DeclaringType;
                            continue;
                        case TypeAttributes.NotPublic:
                        case TypeAttributes.NestedPrivate:
                        case TypeAttributes.NestedAssembly:
                            Debug.Assert(null != declaringType, "Null declaringType");
                            break;
                        default:
                            Debug.Assert(false, "Unknown type");
                            break;
                    }
                    break;
                }

                if (typeof(TypeConverter).IsAssignableFrom(type))
                {
                    ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.Instance, null, EmptyTypes, EmptyParameterModifiers);
                    if (null != ctor)
                    {
                        streamWriter.WriteLine("{0}", type.FullName);
                    }
                    else
                    {
                        streamWriter.WriteLine("{0} missing public ctor", type.FullName);
                    }
                }

                if (null != declaringType)
                {
                    continue;
                }

                bool abort = AppendCustomAttributes(builder, type.GetCustomAttributes(false), false, type.IsEnum, true);
                if (abort)
                {
                    continue;
                }

                AppendClassDeclarationApi(builder, type);
                builder.Append(" {");
                builder.Append(Environment.NewLine);

                string currentNamespace = type.Namespace;
                if (lastNamespace != currentNamespace)
                {
                    lastNamespace = currentNamespace;
                }

                if (type.Name.Contains("UnmappedRequestRoutingConvention"))
                {
                    int kk = 0;
                    kk += 1;
                }
                AppendClassMemberApi(builder, type);
                if (builder.Length > 0)
                {
                    AssemblyFilter(builder);
                    streamWriter.Write(builder.ToString());
                    builder.Length = 0;
                }
                streamWriter.Write("}");
                streamWriter.Write(Environment.NewLine);
                streamWriter.Write(Environment.NewLine);
            }
        }

        private static void Reset()
        {
            _outputFilter = null;
            Assemblies.Clear();
        }

        private static String[] _outputFilter;
        private static void AssemblyFilter(StringBuilder builder)
        {
            string[] filter = _outputFilter;
            if (null == filter)
            {
                filter = new string[2 + Assemblies.Count];
                filter[0] = ", " + typeof(object).Assembly.ToString();
                filter[1] = ", " + typeof(Uri).Assembly.ToString();
                for (int i = 2; i < filter.Length; i++)
                {
                    filter[i] = ", " + Assemblies[i - 2].ToString();
                }
                _outputFilter = filter;
            }
            for (int i = 0; i < filter.Length; ++i)
            {
                builder.Replace(filter[i], "");
            }
        }

        private static void AppendClassDeclarationApi(StringBuilder builder, Type type)
        {
            if (type.IsPublic | type.IsNestedPublic)
            {
                builder.Append("public ");
            }
            else if (type.IsNestedFamily | type.IsNestedFamORAssem | type.IsNestedFamANDAssem)
            {
                builder.Append("protected ");
            }
            else
            {
                Debug.Assert(false, "non public or protected type");
            }

            if (type.IsInterface)
            {
                builder.Append("interface ");
            }
            else if (type.IsEnum)
            {
                builder.Append("enum ");
            }
            else if (type.IsValueType)
            {
                builder.Append("struct ");
            }
            else if (type.IsClass)
            {
                if (type.IsSealed)
                {
                    builder.Append("sealed ");
                }
                else if (type.IsAbstract)
                {
                    builder.Append("abstract ");
                }
                builder.Append("class ");
            }
            else
            {
                builder.Append("? ");
            }
            builder.Append(type.FullName);

            bool haveColon = false;
            Type baseType = type.BaseType;
            if ((null != baseType) && (typeof(object) != baseType) && (typeof(ValueType) != baseType))
            {
                if (typeof(Enum) == baseType)
                {
                    baseType = Enum.GetUnderlyingType(type);
                }
                haveColon = true;
                builder.Append(" : ");
                AppendParameterType(builder, baseType);
            }

            if (!type.IsEnum)
            {
                Type[] baseInterfaces = type.GetInterfaces();
                Array.Sort(baseInterfaces, TypeCompare.Default);
                foreach (Type baseInterface in baseInterfaces)
                {
                    if (haveColon)
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        haveColon = true;
                        builder.Append(" : ");
                    }
                    builder.Append(baseInterface.Name);
                }
            }
        }

        private static void AppendClassMemberApi(StringBuilder builder, Type type)
        {
            MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (members.Length <= 0)
            {
                return;
            }

            Array.Sort(members, new MemberCompare(type));

            bool lastHadAttributes = false;
            MemberTypes lastMemberType = 0;

            foreach (MemberInfo info in members)
            {
                bool rememberLast = lastHadAttributes;
                MemberTypes rememberType = lastMemberType;
                int startLength = builder.Length;
                if (((lastMemberType != info.MemberType) && (0 != lastMemberType)) || lastHadAttributes)
                {
                    builder.Append(Environment.NewLine);
                    lastHadAttributes = false;
                }
                lastMemberType = info.MemberType;
                int newlineLength = builder.Length;

                bool abort = AppendCustomAttributes(builder, info.GetCustomAttributes(true), true, false, true);
                if (abort)
                {
                    builder.Length = startLength;
                    lastHadAttributes = rememberLast;
                    lastMemberType = rememberType;
                    continue;
                }
                lastHadAttributes = (newlineLength != builder.Length);
                builder.Append("\t");
                int attributeLength = builder.Length;

                switch (info.MemberType)
                {
                    case MemberTypes.Constructor:
                        AppendConstructorInfo(builder, type, info as ConstructorInfo);
                        break;
                    case MemberTypes.Event:
                        AppendEventInfo(builder, type, info as EventInfo);
                        break;
                    case MemberTypes.Field:
                        AppendFieldInfo(builder, type, info as FieldInfo);
                        break;
                    case MemberTypes.Method:
                        AppendMethodInfo(builder, type, info as MethodInfo);
                        break;
                    case MemberTypes.Property:
                        AppendPropertyInfo(builder, type, info as PropertyInfo);
                        break;
                    case MemberTypes.NestedType:
                        //DumpClassAPI(builder, info as Type);
                        break;
                    default:
                        builder.Append(" ");
                        builder.Append(info.Name);
                        builder.Append(" ");
                        break;
                }
                if (attributeLength == builder.Length)
                {
                    builder.Length = startLength;
                    lastHadAttributes = rememberLast;
                    lastMemberType = rememberType;
                }
            }
        }

        private static bool AppendCustomAttributes(StringBuilder builder, object[] attributes, bool indent, bool isEnum, bool appendNewLine)
        {
            if (attributes.Length > 0)
            {
                int count = 0;
                int startLength = builder.Length;
                Array.Sort(attributes, ObjectTypeCompare.Default);

                if (indent)
                {
                    builder.Append("\t");
                }
                builder.Append("[");
                if (appendNewLine)
                {
                    builder.Append(Environment.NewLine);
                }
                foreach (object attribute in attributes)
                {
                    if (attribute is MarshalAsAttribute)
                    {
                        continue;
                    }
                    if (attribute is StructLayoutAttribute)
                    {
                        continue;
                    }
                    if (attribute is CompilerGeneratedAttribute)
                    {
                        continue;
                    }
                    if (attribute is MethodImplAttribute)
                    {
                        continue;
                    }
                    if (attribute is TargetedPatchingOptOutAttribute)
                    {
                        continue;
                    }
                    if (attribute is SuppressMessageAttribute)
                    {
                        continue;
                    }
                    if (attribute is IteratorStateMachineAttribute)
                    {
                        continue;
                    }
                    if (attribute is DebuggerStepThroughAttribute)
                    {
                        continue;
                    }
                    if (isEnum && (attribute is SerializableAttribute))
                    {
                        continue;
                    }
                    count++;

                    if (indent)
                    {
                        builder.Append("\t");
                    }
                    builder.Append(attribute.GetType().Name);
                    builder.Append("(");

                    builder.Append("),");
                    if (appendNewLine)
                    {
                        builder.Append(Environment.NewLine);
                    }
                }
                if (0 < count)
                {
                    if (indent)
                    {
                        builder.Append("\t");
                    }
                    builder.Append("]");
                    if (appendNewLine)
                    {
                        builder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    builder.Length = startLength;
                }
            }

            return false;
        }

        private static void AppendConstructorInfo(StringBuilder builder, Type type, ConstructorInfo info)
        {
            if (info.IsPublic)
            {
                builder.Append("public");
            }
            else if (!(type.IsClass && type.IsSealed) && (info.IsFamily || info.IsFamilyAndAssembly || info.IsFamilyOrAssembly))
            {
                builder.Append("protected");
            }
            else return;

            builder.Append(" ");
            builder.Append(type.Name);
            builder.Append(" ");
            AppendParameterInfo(builder, info.GetParameters(), true, true);
            builder.Append(Environment.NewLine);
        }

        private static void AppendEventInfo(StringBuilder builder, Type type, EventInfo info)
        {
            int propertyStart = builder.Length;

            AppendParameterType(builder, info.EventHandlerType);
            builder.Append(" ");
            builder.Append(info.Name);

            builder.Append(" {");
            bool gettable = AppendPropertyMethod(builder, type, info.GetAddMethod(), "add");
            bool settable = AppendPropertyMethod(builder, type, info.GetRemoveMethod(), "remove");
            if (gettable || settable)
            {
                builder.Append(" }");
                builder.Append(Environment.NewLine);
            }
            else
            {
                builder.Length = propertyStart;
            }
        }

        private static void AppendFieldInfo(StringBuilder builder, Type type, FieldInfo info)
        {
            if (type.IsEnum && info.IsSpecialName)
            {
                return;
            }
            if (info.IsPublic)
            {
                if (type.IsEnum)
                {
                    builder.Append("");
                }
                else
                {
                    builder.Append("public");
                }
            }
            else if (!(type.IsClass && type.IsSealed) && (info.IsFamily || info.IsFamilyAndAssembly || info.IsFamilyOrAssembly))
            {
                if (type.IsEnum)
                {
                    builder.Append("");
                }
                else
                {
                    builder.Append("protected");
                }
            }
            else return;

            if (!type.IsEnum)
            {
                if (info.IsStatic)
                {
                    builder.Append(" static");
                }
                else if (info.IsInitOnly)
                {
                    builder.Append(" const");
                }
                if (info.IsInitOnly)
                {
                    builder.Append(" readonly");
                }
            }
            if (!type.IsEnum)
            {
                builder.Append(" ");
                AppendParameterType(builder, info.FieldType);
                builder.Append(" ");
            }

            builder.Append(info.Name);
            builder.Append(" = ");

            if (info.IsLiteral || info.IsStatic)
            {
                object fieldValue = null;
                try
                {
                    fieldValue = info.GetValue(null);
                }
                catch (Exception)
                {
                }

                if (null != fieldValue)
                {
                    if (fieldValue is string)
                    {
                        builder.Append('\"');
                        builder.Append((string)fieldValue);
                        builder.Append('\"');
                    }
                    else if (fieldValue is long)
                    {
                        builder.Append(((long)fieldValue).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (fieldValue is byte)
                    {
                        builder.Append(((byte)fieldValue).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (fieldValue is bool)
                    {
                        builder.Append(((bool)fieldValue).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (fieldValue is double)
                    {
                        builder.Append(((double)fieldValue).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (fieldValue is short)
                    {
                        builder.Append(((short)fieldValue).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (fieldValue is float)
                    {
                        builder.Append(((float)fieldValue).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (fieldValue is Guid)
                    {
                        builder.Append('{');
                        builder.Append((Guid)fieldValue);
                        builder.Append('}');
                    }
                    else if (fieldValue is Enum)
                    {
                        // remove the enumness, without assuming a particular underlying type.
                        builder.Append(Convert.ChangeType(fieldValue, Enum.GetUnderlyingType(type)));
                    }
                    else
                    {
                        string svalue;
                        try
                        {
                            MethodInfo tostring = fieldValue.GetType().GetMethod("ToString", ToStringFormatParameter);
                            if (null != tostring)
                            {
                                svalue = (string)tostring.Invoke(fieldValue, ToSTringFormatValues);
                            }
                            else
                            {
                                svalue = fieldValue.ToString();
                            }
                        }
                        catch (Exception e)
                        {
                            svalue = e.ToString();
                        }
                        builder.Append(svalue);
                    }
                }
            }
            builder.Append(Environment.NewLine);
        }

        private static readonly Type[] EmptyTypes = new Type[0];
        private static readonly ParameterModifier[] EmptyParameterModifiers = new ParameterModifier[0];

        private static readonly Type[] ToStringFormatParameter = new Type[] { typeof(IFormatProvider) };
        private static readonly object[] ToSTringFormatValues = new object[] { CultureInfo.InvariantCulture };

        private static void AppendMethodInfo(StringBuilder builder, Type type, MethodInfo info)
        {
            string infoName = info.Name;
            if ("IsRowOptimized" == infoName)
            {
                return;
            }
            if (info.IsSpecialName)
            {
                return;
            }
            if (info.IsPublic)
            {
                if (!type.IsInterface)
                {
                    builder.Append("public ");
                }
            }
            else if (!(type.IsClass && type.IsSealed) && (info.IsFamily || info.IsFamilyAndAssembly || info.IsFamilyOrAssembly))
            {
                if (!type.IsInterface)
                {
                    builder.Append("protected ");
                }
            }
            else if (infoName.StartsWith("Reset") && ("Reset" != infoName))
            {
                PropertyInfo propInfo = type.GetProperty(infoName.Substring("Reset".Length), BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                if (null != propInfo && (0 == info.GetParameters().Length))
                {
                    builder.Append("private ");
                }
                else return;
            }
            else if (infoName.StartsWith("ShouldSerialize") && ("ShouldSerialize" != infoName))
            {
                PropertyInfo propInfo = type.GetProperty(infoName.Substring("ShouldSerialize".Length), BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
                if (null != propInfo && (0 == info.GetParameters().Length))
                {
                    builder.Append("private ");
                }
                else return;
            }
            else if (!(type.IsClass && type.IsSealed) && info.IsVirtual)
            {
                if (-1 == info.Name.IndexOf("."))
                {
                    builder.Append("internal ");
                }
            }
            else return;

            if (!type.IsInterface)
            {
                if (info.IsAbstract)
                {
                    builder.Append("abstract ");
                }
                else if (info.IsVirtual && (-1 == info.Name.IndexOf(".")))
                {
                    builder.Append("virtual ");
                }
                else if (info.IsStatic)
                {
                    builder.Append("static ");
                }
            }

            AppendParameterType(builder, info.ReturnType);
            builder.Append(" ");
            builder.Append(infoName);
            builder.Append(" ");
            AppendParameterInfo(builder, info.GetParameters(), true, true);
            builder.Append(Environment.NewLine);
        }

        private static void AppendPropertyInfo(StringBuilder builder, Type type, PropertyInfo info)
        {
            int propertyStart = builder.Length;

            builder.Append("");
            AppendParameterType(builder, info.PropertyType);
            builder.Append(" ");
            builder.Append(info.Name);
            builder.Append(" ");

            ParameterInfo[] parameters = info.GetIndexParameters();
            if (0 < parameters.Length)
            {
                AppendParameterInfo(builder, parameters, false, true);
            }

            builder.Append(" { ");
            bool gettable = AppendPropertyMethod(builder, type, info.GetGetMethod(true), "get");
            if (gettable)
            {
                builder.Append(' ');
            }
            bool settable = AppendPropertyMethod(builder, type, info.GetSetMethod(true), "set");
            if (settable)
            {
                builder.Append(' ');
            }
            if (gettable || settable)
            {
                builder.Append("}");
                builder.Append(Environment.NewLine);
            }
            else
            {
                builder.Length = propertyStart;
            }
        }

        private static bool AppendPropertyMethod(StringBuilder builder, Type type, MethodInfo info, string method)
        {
            if (null != info)
            {
                int setStart = builder.Length;

                AppendCustomAttributes(builder, info.GetCustomAttributes(true), false, false, false);

                if (info.IsPublic)
                {
                    builder.Append("public ");
                }
                else if (!(type.IsClass && type.IsSealed) && (info.IsFamily || info.IsFamilyAndAssembly || info.IsFamilyOrAssembly))
                {
                    builder.Append("protected ");
                }
                else
                {
                    builder.Length = setStart;
                    return false;
                }
                if (info.IsAbstract)
                {
                    builder.Append("abstract ");
                }
                else if (info.IsVirtual)
                {
                    builder.Append("virtual ");
                }
                else if (info.IsStatic)
                {
                    builder.Append("static ");
                }
                builder.Append(method);
                builder.Append(';');
                return true;
            }

            return false;
        }

        private static void AppendParameterInfo(StringBuilder builder, ParameterInfo[] parameters, bool asMethod, bool withNames)
        {
            if (parameters.Length > 0)
            {
                builder.Append(asMethod ? '(' : '[');
                for (int i = 0; i < parameters.Length; ++i)
                {
                    if (0 < i)
                    {
                        builder.Append(", ");
                    }
                    if (withNames)
                    {
                        AppendParameterInfo(builder, parameters[i]);
                    }
                    else
                    {
                        builder.Append(parameters[i].ParameterType.FullName);
                    }
                }

                builder.Append(asMethod ? ')' : ']');
            }
            else
            {
                builder.Append("()");
            }
        }

        private static void AppendParameterInfo(StringBuilder builder, ParameterInfo info)
        {
            if (info.IsOut)
            {
                builder.Append("out ");
            }
            else if (info.IsOptional)
            {
                builder.Append("params ");
            }
            AppendParameterType(builder, info.ParameterType);
            builder.Append(" ");
            builder.Append(info.Name);
        }

        private static void AppendParameterType(StringBuilder builder, Type parameterType)
        {
            string name = parameterType.FullName ?? parameterType.Name;
            string synonm = (string)Synonms[name];
            if (null != synonm)
            {
                builder.Append(synonm);
            }
            else if (parameterType.IsGenericType && name.Contains("Version="))
            {
                // If there is generic type with generic parameter (for e.g. IEnumerable<T>),
                // then AppendGenericTypeName produces 'System.IEnumerable[[]]' whereas
                // type.Name is IEnumerable'1. Also, to avoid too any changes with the existing baseline,
                // only going into this method if there is a "Version=" present in the name.
                AppendGenericTypeName(builder, parameterType);
            }
            else if (name.StartsWith("Microsoft.AspNet.OData."))
            {
                builder.Append(parameterType.Name);
            }
            else
            {
                builder.Append(name);
            }
        }

        private static void AppendGenericTypeName(StringBuilder builder, Type type)
        {
            if (type.IsGenericType)
            {
                builder.Append(type.GetGenericTypeDefinition().FullName);
                builder.Append("[");
                bool first = true;
                foreach (var argType in type.GetGenericArguments())
                {
                    if (!first)
                    {
                        builder.Append(",");
                    }
                    builder.Append("[");
                    AppendGenericTypeName(builder, argType);
                    builder.Append("]");
                    first = false;
                }

                builder.Append("]");
            }
            else
            {
                builder.Append(type.FullName);
            }
        }

        public sealed class AssemblyCompare : IComparer
        {
            public int Compare(object x, object y)
            {
                string a = ((Assembly)x).GetName().Name;
                string b = ((Assembly)y).GetName().Name;
                int ac = 0, bc = 0;

                for (int i = 0; i < a.Length; ++i)
                {
                    if ('.' == a[i]) ac++;
                }
                for (int i = 0; i < b.Length; ++i)
                {
                    if ('.' == b[i]) bc++;
                }
                int cmp = ac - bc;
                if (0 == cmp)
                {
                    cmp = String.Compare(a, b, StringComparison.Ordinal);
                }
                return cmp;
            }
        }

        public sealed class TypeCompare : IComparer
        {
            public static readonly TypeCompare Default = new TypeCompare();

            public int Compare(object x, object y)
            {
                Type a = x as Type;
                Type b = y as Type;

                string c = a.FullName ?? a.Name;
                string d = b.FullName ?? b.Name;

                int ac = 0, bc = 0;

                for (int i = 0; i < c.Length; ++i)
                {
                    if ('.' == c[i]) ac++;
                }
                for (int i = 0; i < d.Length; ++i)
                {
                    if ('.' == d[i]) bc++;
                }
                int cmp = ac - bc;
                if (0 == cmp)
                {
                    if (!AlphabeticalGrouping)
                    {
                        string e = (0 < ac) ? c.Substring(0, c.LastIndexOf('.')) : null;
                        string f = (0 < bc) ? d.Substring(0, d.LastIndexOf('.')) : null;

                        if (0 == String.Compare(e, f, false, CultureInfo.InvariantCulture))
                        {
                            if (a.IsEnum)
                            {
                                if (!b.IsEnum)
                                {
                                    cmp = -1;
                                }
                            }
                            else if (a.IsValueType)
                            {
                                if (b.IsEnum)
                                {
                                    cmp = 1;
                                }
                                else if (!b.IsValueType)
                                {
                                    cmp = -1;
                                }
                            }
                            else if (b.IsEnum || b.IsValueType)
                            {
                                cmp = 1;
                            }
                            if (0 == cmp)
                            {
                                if (a.IsInterface != b.IsInterface)
                                {
                                    cmp = (a.IsInterface ? -1 : 1);
                                }
                                else if (a.IsAbstract != b.IsAbstract)
                                {
                                    cmp = (a.IsAbstract ? -1 : 1);
                                }
                                else if (a.IsSealed != b.IsSealed)
                                {
                                    cmp = (a.IsSealed ? 1 : -1);
                                }
                            }
                        }
                    }
                    if (0 == cmp)
                    {
                        cmp = String.Compare(c, d, false, CultureInfo.InvariantCulture);
                    }
                }
                return cmp;
            }
        }

        public sealed class ObjectTypeCompare : IComparer
        {
            public static readonly ObjectTypeCompare Default = new ObjectTypeCompare();
            public int Compare(object x, object y)
            {
                string a = x.GetType().FullName;
                string b = y.GetType().FullName;
                int ac = 0, bc = 0;

                for (int i = 0; i < a.Length; ++i)
                {
                    if ('.' == a[i]) ac++;
                }
                for (int i = 0; i < b.Length; ++i)
                {
                    if ('.' == b[i]) bc++;
                }
                int cmp = ac - bc;
                if (0 == cmp)
                {
                    cmp = String.Compare(a, b, false, CultureInfo.InvariantCulture);
                }
                return cmp;
            }
        }

        public sealed class MemberCompare : IComparer
        {
            private static readonly Hashtable MemberType;
            static MemberCompare()
            {
                Hashtable memberType = new Hashtable();
                memberType.Add(MemberTypes.Field, 1);
                memberType.Add(MemberTypes.Constructor, 2);
                memberType.Add(MemberTypes.Property, 3);
                memberType.Add(MemberTypes.Event, 4);
                memberType.Add(MemberTypes.Method, 5);
                memberType.Add(MemberTypes.NestedType, 6);
                memberType.Add(MemberTypes.TypeInfo, 7);
                memberType.Add(MemberTypes.Custom, 8);
                MemberType = memberType;
            }

            private readonly Hashtable _hash;

            public MemberCompare(Type type)
            {
                _hash = new Hashtable();
                for (int i = 0; null != type; ++i, type = type.BaseType)
                {
                    _hash.Add(type, i);
                }
            }

            public int Compare(object x, object y)
            {
                return Compare((MemberInfo)x, (MemberInfo)y);
            }

            public int Compare(MemberInfo x, MemberInfo y)
            {
                if (x.MemberType == y.MemberType)
                {
                    Type xt = x.DeclaringType;
                    Type yt = y.DeclaringType;
                    if (xt != yt)
                    {
                        return (int)_hash[yt] - (int)_hash[xt];
                    }

                    int cmp = String.Compare(x.Name, y.Name, false, CultureInfo.InvariantCulture);
                    if (0 == cmp)
                    {
                        MethodInfo xMethodInfo = null, yMethodInfo = null;
                        ParameterInfo[] xParameterInfos, yParameterInfos;
                        switch (x.MemberType)
                        {
                            case MemberTypes.Constructor:
                                xParameterInfos = ((ConstructorInfo)x).GetParameters();
                                yParameterInfos = ((ConstructorInfo)y).GetParameters();
                                break;
                            case MemberTypes.Method:
                                xMethodInfo = (MethodInfo)x;
                                yMethodInfo = (MethodInfo)y;
                                xParameterInfos = xMethodInfo.GetParameters();
                                yParameterInfos = yMethodInfo.GetParameters();
                                break;
                            case MemberTypes.Property:
                                xParameterInfos = ((PropertyInfo)x).GetIndexParameters();
                                yParameterInfos = ((PropertyInfo)y).GetIndexParameters();
                                break;
                            default:
                                xParameterInfos = yParameterInfos = new ParameterInfo[0];
                                break;
                        }
                        cmp = xParameterInfos.Length - yParameterInfos.Length;
                        if (0 == cmp)
                        {
                            int count = xParameterInfos.Length;
                            for (int i = 0; i < count; ++i)
                            {
                                cmp = String.Compare(xParameterInfos[i].ParameterType.FullName, yParameterInfos[i].ParameterType.FullName, false, CultureInfo.InvariantCulture);
                                if (cmp == 0)
                                {
                                    // For generic parameters, FullName is null. Hence comparing the names
                                    cmp = String.Compare(xParameterInfos[i].ParameterType.Name, yParameterInfos[i].ParameterType.Name, false, CultureInfo.InvariantCulture);
                                }
                                if (0 != cmp)
                                {
                                    break;
                                }
                            }

                            if (0 == cmp && xMethodInfo != null)
                            {
                                // Two methods with same name, same parameters. Sort by the # of generic type parameters.
                                cmp = xMethodInfo.GetGenericArguments().Count() - yMethodInfo.GetGenericArguments().Count();
                            }
                        }
                    }

                    return cmp;
                }
                return ((int)MemberType[x.MemberType] - (int)MemberType[y.MemberType]);
            }
        }
    }
}
