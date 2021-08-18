//-----------------------------------------------------------------------------
// <copyright file="InstanceCreator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    public class InstanceCreator
    {
        #region Public Methods and Operators

        /// <summary>
        /// Creates an instance of the given type.
        /// </summary>
        /// <typeparam name="T">The type to create an instance from.</typeparam>
        /// <param name="rndGen">A random generator used to populate the instance.</param>
        /// <returns>An instance of the given type.</returns>
        public static T CreateInstanceOf<T>(Random rndGen)
        {
            return (T)CreateInstanceOf(typeof(T), rndGen, new CreatorSettings());
        }

        /// <summary>
        /// Creates an instance of the given type.
        /// </summary>
        /// <typeparam name="T">The type to create an instance from.</typeparam>
        /// <param name="rndGen">A random generator used to populate the instance.</param>
        /// <param name="creatorSettings">Creator settings which override the global values.</param>
        /// <returns>An instance of the given type.</returns>
        public static T CreateInstanceOf<T>(Random rndGen, CreatorSettings creatorSettings)
        {
            return (T)CreateInstanceOf(typeof(T), rndGen, creatorSettings);
        }

        /// <summary>
        /// Creates an instance of the given type.
        /// </summary>
        /// <param name="type">The type to create an instance from.</param>
        /// <param name="rndGen">A random generator used to populate the instance.</param>
        /// <param name="creatorSettings">Settings used to create the object.  This is an optional parameter and a default CreatorSettings is
        /// created if none is passed in.</param>
        /// <returns>An instance of the given type.</returns>
        public static object CreateInstanceOf(Type type, Random rndGen, CreatorSettings creatorSettings = null)
        {
            if (creatorSettings == null)
            {
                creatorSettings = new CreatorSettings();
            }

            if (creatorSettings.CreatorSurrogate != null)
            {
                if (creatorSettings.CreatorSurrogate.CanCreateInstanceOf(type))
                {
                    return creatorSettings.CreatorSurrogate.CreateInstanceOf(type, rndGen, creatorSettings);
                }
            }

            if (PrimitiveCreator.CanCreateInstanceOf(type))
            {
                return PrimitiveCreator.CreatePrimitiveInstance(type, rndGen, creatorSettings);
            }
            else if (type.IsArray)
            {
                return CreateInstanceOfArray(type, rndGen, creatorSettings);
            }
            else if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return CreateInstanceOfNullableOfT(type, rndGen, creatorSettings);
                }

                if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    return CreateInstanceOfDictionaryOfKAndV(type, rndGen, creatorSettings);
                }

                if (HasInterface(type, typeof(ICollection<>)) || HasInterface(type, typeof(IList<>)))
                {
                    return CreateInstanceOfListOfT(type, rndGen, creatorSettings);
                }
            }
            else if (type.IsEnum)
            {
                return CreateInstanceOfEnum(type, rndGen);
            }
            else if (type == typeof(JToken))
            {
                return JTokenInstanceCreator.CreateInstanceOfJToken(rndGen, creatorSettings);
            }
            else if (type == typeof(JValue))
            {
                return JTokenInstanceCreator.CreateInstanceOfJValue(rndGen, creatorSettings);
            }
            else if (type == typeof(JObject))
            {
                return JTokenInstanceCreator.CreateInstanceOfJObject(rndGen, creatorSettings);
            }
            else if (type == typeof(JArray))
            {
                return JTokenInstanceCreator.CreateInstanceOfJArray(rndGen, creatorSettings);
            }
#if NETFX // CookieHeaderValue is only supported in the AspNet version.
            else if (type == typeof(System.Net.Http.Headers.CookieHeaderValue))
            {
                return CookieHeaderValueInstanceCreator.CreateInstanceOfCookieHeaderValue(rndGen, creatorSettings);
            }
#endif
            else if (type.IsPublic)
            {
                return ComplexTypeInstanceCreator.CreateInstanceOf(type, rndGen, creatorSettings);
            }

            throw new ArgumentException("Cannot create instance of " + type.FullName);
        }

        #endregion

        #region Methods

        private static object CreateInstanceOfArray(Type arrayType, Random rndGen, CreatorSettings creatorSettings)
        {
            Type type = arrayType.GetElementType();
            double rndNumber = rndGen.NextDouble();
            if (rndNumber < creatorSettings.NullValueProbability)
            {
                return null; // 1% chance of null value
            }

            int size = (int)Math.Pow(creatorSettings.MaxArrayLength, rndNumber);
            // this will create more small arrays than large ones
            if (creatorSettings.AllowEmptyCollection)
            {
                size--;
            }
            Array result = Array.CreateInstance(type, size);
            for (int i = 0; i < size; i++)
            {
                result.SetValue(CreateInstanceOf(type, rndGen, creatorSettings), i);
            }

            return result;
        }

        private static object CreateInstanceOfDictionaryOfKAndV(
            Type dictionaryType, Random rndGen, CreatorSettings creatorSettings)
        {
            double nullValueProbability = creatorSettings.NullValueProbability;
            Type[] genericArgs = dictionaryType.GetGenericArguments();
            Type typeK = genericArgs[0];
            Type typeV = genericArgs[1];
            double rndNumber = rndGen.NextDouble();
            if (rndNumber < creatorSettings.NullValueProbability)
            {
                return null; // 1% chance of null value
            }

            int size = (int)Math.Pow(creatorSettings.MaxListLength, rndNumber);
            // this will create more small dictionaries than large ones
            if (creatorSettings.AllowEmptyCollection)
            {
                size--;
            }
            object result = Activator.CreateInstance(dictionaryType);
            MethodInfo addMethod = dictionaryType.GetMethod("Add");
            MethodInfo containsKeyMethod = dictionaryType.GetMethod("ContainsKey");
            for (int i = 0; i < size; i++)
            {
                //Dictionary Keys cannot be null.Set null probability to 0
                creatorSettings.NullValueProbability = 0;
                object newKey = CreateInstanceOf(typeK, rndGen, creatorSettings);
                //Reset null instance probability
                creatorSettings.NullValueProbability = nullValueProbability;
                bool containsKey = (bool)containsKeyMethod.Invoke(result, new object[] { newKey });
                if (!containsKey)
                {
                    object newValue = CreateInstanceOf(typeV, rndGen, creatorSettings);
                    addMethod.Invoke(result, new object[] { newKey, newValue });
                }
            }

            return result;
        }

        private static object CreateInstanceOfEnum(Type enumType, Random rndGen)
        {
            bool hasFlags = enumType.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0;
            Array possibleValues = Enum.GetValues(enumType);
            if (!hasFlags)
            {
                return possibleValues.GetValue(rndGen.Next(possibleValues.Length));
            }
            else
            {
                int result = 0;
                if (rndGen.Next(10) > 0)
                {
                    // 10% chance of value zero
                    foreach (object value in possibleValues)
                    {
                        if (rndGen.Next(2) == 0)
                        {
                            result |= ((IConvertible)value).ToInt32(null);
                        }
                    }
                }

                return result;
            }
        }

        private static object CreateInstanceOfListOfT(Type listType, Random rndGen, CreatorSettings creatorSettings)
        {
            Type type = listType.GetGenericArguments()[0];
            double rndNumber = rndGen.NextDouble();
            if (rndNumber < creatorSettings.NullValueProbability)
            {
                return null; // 1% chance of null value
            }

            int size = (int)Math.Pow(creatorSettings.MaxListLength, rndNumber);
            // this will create more small lists than large ones
            if (creatorSettings.AllowEmptyCollection)
            {
                size--;
            }
            object result = Activator.CreateInstance(listType);
            MethodInfo addMethod = listType.GetMethod("Add");
            for (int i = 0; i < size; i++)
            {
                addMethod.Invoke(result, new object[] { CreateInstanceOf(type, rndGen, creatorSettings) });
            }

            return result;
        }

        private static object CreateInstanceOfNullableOfT(
            Type nullableOfTType, Random rndGen, CreatorSettings creatorSettings)
        {
            if (rndGen.Next(5) == 0)
            {
                return null;
            }

            Type type = nullableOfTType.GetGenericArguments()[0];
            return CreateInstanceOf(type, rndGen, creatorSettings);
        }

        private static bool HasInterface(Type type, Type interfaceType)
        {
            foreach (var intType in type.GetInterfaces())
            {
                if (intType.IsGenericType
                    && intType.GetGenericTypeDefinition() == interfaceType)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
