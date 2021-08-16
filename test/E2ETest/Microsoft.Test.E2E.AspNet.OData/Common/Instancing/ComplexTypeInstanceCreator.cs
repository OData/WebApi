//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeInstanceCreator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    internal class ComplexTypeInstanceCreator
    {
        #region Methods

        internal static object CreateInstanceOf(Type type, Random rndGen, CreatorSettings creatorSettings)
        {
            if (!creatorSettings.EnterRecursion())
            {
                return null;
            }

            if (rndGen.NextDouble() < creatorSettings.NullValueProbability && !type.IsValueType)
            {
                return null;
            }

            // Test convention #1: if the type has a .ctor(Random), call it and the
            // type will initialize itself.
            ConstructorInfo randomCtor = type.GetConstructor(new Type[] { typeof(Random) });
            if (randomCtor != null)
            {
                return randomCtor.Invoke(new object[] { rndGen });
            }

            // Test convention #2: if the type has a static method CreateInstance(Random, CreatorSettings), call it and
            // an new instance will be returned
            var createInstanceMtd = type.GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public);
            if (createInstanceMtd != null)
            {
                return createInstanceMtd.Invoke(null, new object[] { rndGen, creatorSettings });
            }

            // Test convention #3: use the default constructor for classes and set public
            // properties and fields to random values.
            object result = null;
            if (type.IsValueType)
            {
                result = Activator.CreateInstance(type);
            }
            else
            {
                ConstructorInfo defaultCtor = type.GetConstructor(Type.EmptyTypes);
                if (defaultCtor == null)
                {
                    throw new ArgumentException("Type " + type.FullName + " does not have a default constructor.");
                }

                result = defaultCtor.Invoke(new object[0]);
            }

            SetPublicProperties(type, result, rndGen, creatorSettings);
            SetPublicFields(type, result, rndGen, creatorSettings);
            creatorSettings.LeaveRecursion();
            return result;
        }

        private static int CompareMemberNames(MemberInfo member1, MemberInfo member2)
        {
            return member1.Name.CompareTo(member2.Name);
        }

        private static void SetPublicProperties(Type type, object obj, Random rndGen, CreatorSettings creatorSettings)
        {
            List<PropertyInfo> properties =
                new List<PropertyInfo>(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            properties.Sort(new Comparison<PropertyInfo>(CompareMemberNames));
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    object propertyValue = InstanceCreator.CreateInstanceOf(property.PropertyType, rndGen, creatorSettings);
                    property.SetValue(obj, propertyValue, null);
                }
            }
        }

        private static void SetPublicFields(Type type, object obj, Random rndGen, CreatorSettings creatorSettings)
        {
            List<FieldInfo> fields = new List<FieldInfo>(type.GetFields(BindingFlags.Public | BindingFlags.Instance));
            fields.Sort(new Comparison<FieldInfo>(CompareMemberNames));
            foreach (FieldInfo field in fields)
            {
                object fieldValue = InstanceCreator.CreateInstanceOf(field.FieldType, rndGen, creatorSettings);
                field.SetValue(obj, fieldValue);
            }
        }

        #endregion
    }
}
