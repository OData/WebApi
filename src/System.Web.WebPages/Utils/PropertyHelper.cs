// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.WebPages
{
    internal class PropertyHelper
    {
        private static ConcurrentDictionary<Type, PropertyHelper[]> _reflectionCache = new ConcurrentDictionary<Type, PropertyHelper[]>();

        private Func<object, object> _valueGetter;

        public static PropertyHelper[] GetProperties(object instance)
        {
            return AnonymousObjectReflectionHelper.GetProperties<PropertyHelper>(instance, _reflectionCache);
        }

        public void Initialize(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            Contract.Assert(_valueGetter == null);

            Name = property.Name;
            _valueGetter = MakeFastPropertyGetter(property);
        }

        public virtual string Name { get; protected set; }

        public object GetValue(object instance)
        {
            Contract.Assert(_valueGetter != null, "Must call Initialize before using this object");

            return _valueGetter(instance);
        }

        private delegate TOutput ByRefFunc<TInput, TOutput>(ref TInput arg);

        private static readonly MethodInfo _callPropertyGetterOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertyGetter", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _callPropertyGetterByReferenceOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertyGetterByReference", BindingFlags.NonPublic | BindingFlags.Static);

        // This method is more memory efficient than a dynamically compiled lambda, and about the same speed.
        private static Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo)
        {
            Contract.Assert(propertyInfo != null);

            MethodInfo getMethod = propertyInfo.GetGetMethod();
            Contract.Assert(getMethod != null);
            Contract.Assert(!getMethod.IsStatic);
            Contract.Assert(getMethod.GetParameters().Length == 0);

            // Instance methods in the CLR can be turned into static methods where the first parameter
            // is open over "this". This parameter is always passed by reference, so we have a code
            // path for value types and a code path for reference types.
            Type typeInput = getMethod.ReflectedType;
            Type typeOutput = getMethod.ReturnType;

            Delegate callPropertyGetterDelegate;
            if (typeInput.IsValueType)
            {
                // Create a delegate (ref TInput) -> TOutput
                var propertyGetterAsFunc = getMethod.CreateDelegate(typeof(ByRefFunc<,>).MakeGenericType(typeInput, typeOutput));
                var callPropertyGetterClosedGenericMethod = _callPropertyGetterByReferenceOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);
                callPropertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc, callPropertyGetterClosedGenericMethod);
            }
            else
            {
                // Create a delegate TInput -> TOutput
                var propertyGetterAsFunc = getMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(typeInput, typeOutput));
                var callPropertyGetterClosedGenericMethod = _callPropertyGetterOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);
                callPropertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc, callPropertyGetterClosedGenericMethod);
            }

            return (Func<object, object>)callPropertyGetterDelegate;
        }

        private static object CallPropertyGetter<TInput, TOutput>(Func<TInput, TOutput> getter, object @this)
        {
            return getter((TInput)@this);
        }

        private static object CallPropertyGetterByReference<TInput, TOutput>(ByRefFunc<TInput, TOutput> getter, object @this)
        {
            TInput unboxed = (TInput)@this;
            return getter(ref unboxed);
        }
    }
}
