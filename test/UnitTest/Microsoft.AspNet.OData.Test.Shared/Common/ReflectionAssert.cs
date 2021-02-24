// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Common
{
    public class ReflectionAssert
    {
        private static PropertyInfo GetPropertyInfo<T, TProp>(Expression<Func<T, TProp>> property)
        {
            if (property.Body is MemberExpression)
            {
                return (PropertyInfo)((MemberExpression)property.Body).Member;
            }
            else if (property.Body is UnaryExpression && property.Body.NodeType == ExpressionType.Convert)
            {
                return (PropertyInfo)((MemberExpression)((UnaryExpression)property.Body).Operand).Member;
            }
            else
            {
                throw new InvalidOperationException("Could not determine property from lambda expression.");
            }
        }

        private static void TestPropertyValue<TInstance, TValue>(TInstance instance, Func<TInstance, TValue> getFunc, Action<TInstance, TValue> setFunc, TValue valueToSet, TValue valueToCheck)
        {
            setFunc(instance, valueToSet);
            TValue newValue = getFunc(instance);
            Assert.Equal(valueToCheck, newValue);
        }

        private static void TestPropertyValue<TInstance, TValue>(TInstance instance, Func<TInstance, TValue> getFunc, Action<TInstance, TValue> setFunc, TValue value)
        {
            TestPropertyValue(instance, getFunc, setFunc, value, value);
        }

        public static void Property<T, TResult>(
            T instance,
            Expression<Func<T, TResult>> propertyGetter,
            TResult expectedDefaultValue,
            bool allowNull = false,
            TResult roundTripTestValue = null)
            where TResult : class
        {
            Property<T, TResult, ArgumentNullException>(
                instance,
                propertyGetter,
                expectedDefaultValue,
                allowNull,
                roundTripTestValue);
        }

        public static void Property<T, TResult, TException>(
            T instance,
            Expression<Func<T, TResult>> propertyGetter,
            TResult expectedDefaultValue,
            bool allowNull = false,
            TResult roundTripTestValue = null)
            where TResult : class
            where TException : ArgumentException
        {
            PropertyInfo property = GetPropertyInfo(propertyGetter);
            Func<T, TResult> getFunc = (obj) => (TResult)property.GetValue(obj, index: null);
            Action<T, TResult> setFunc = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(expectedDefaultValue, getFunc(instance));

            if (allowNull)
            {
                TestPropertyValue(instance, getFunc, setFunc, null);
            }
            else
            {
                TException ex = ExceptionAssert.Throws<TException>(
                    () =>
                    {
                        try
                        {
                            setFunc(instance, null);
                        }
                        catch (TargetInvocationException e)
                        {
                            throw e.InnerException;
                        }
                    });

                Assert.Equal("value", ex.ParamName);
            }

            if (roundTripTestValue != null)
            {
                TestPropertyValue(instance, getFunc, setFunc, roundTripTestValue);
            }
        }

        public static void IntegerProperty<T, TResult>(T instance, Expression<Func<T, TResult>> propertyGetter, TResult expectedDefaultValue,
            TResult? minLegalValue, TResult? illegalLowerValue,
            TResult? maxLegalValue, TResult? illegalUpperValue,
            TResult roundTripTestValue) where TResult : struct
        {
            PropertyInfo property = GetPropertyInfo(propertyGetter);
            Func<T, TResult> getFunc = (obj) => (TResult)property.GetValue(obj, index: null);
            Action<T, TResult> setFunc = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(expectedDefaultValue, getFunc(instance));

            if (minLegalValue.HasValue)
            {
                TestPropertyValue(instance, getFunc, setFunc, minLegalValue.Value);
            }

            if (maxLegalValue.HasValue)
            {
                TestPropertyValue(instance, getFunc, setFunc, maxLegalValue.Value);
            }

            if (illegalLowerValue.HasValue)
            {
                ExceptionAssert.ThrowsArgumentGreaterThanOrEqualTo(() => { setFunc(instance, illegalLowerValue.Value); }, "value", minLegalValue.Value.ToString(), illegalLowerValue.Value);
            }

            if (illegalUpperValue.HasValue)
            {
                ExceptionAssert.ThrowsArgumentLessThanOrEqualTo(() => { setFunc(instance, illegalLowerValue.Value); }, "value", maxLegalValue.Value.ToString(), illegalUpperValue.Value);
            }

            TestPropertyValue(instance, getFunc, setFunc, roundTripTestValue);
        }

        public static void NullableIntegerProperty<T, TResult>(T instance, Expression<Func<T, TResult?>> propertyGetter, TResult? expectedDefaultValue,
            TResult? minLegalValue, TResult? illegalLowerValue,
            TResult? maxLegalValue, TResult? illegalUpperValue,
            TResult roundTripTestValue) where TResult : struct
        {
            PropertyInfo property = GetPropertyInfo(propertyGetter);
            Func<T, TResult?> getFunc = (obj) => (TResult?)property.GetValue(obj, index: null);
            Action<T, TResult?> setFunc = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(expectedDefaultValue, getFunc(instance));

            TestPropertyValue(instance, getFunc, setFunc, null);

            if (minLegalValue.HasValue)
            {
                TestPropertyValue(instance, getFunc, setFunc, minLegalValue.Value);
            }

            if (maxLegalValue.HasValue)
            {
                TestPropertyValue(instance, getFunc, setFunc, maxLegalValue.Value);
            }

            if (illegalLowerValue.HasValue)
            {
                ExceptionAssert.ThrowsArgumentGreaterThanOrEqualTo(() => { setFunc(instance, illegalLowerValue.Value); }, "value", minLegalValue.Value.ToString(), illegalLowerValue.Value);
            }

            if (illegalUpperValue.HasValue)
            {
                ExceptionAssert.ThrowsArgumentLessThanOrEqualTo(() => { setFunc(instance, illegalLowerValue.Value); }, "value", maxLegalValue.Value.ToString(), illegalUpperValue.Value);
            }

            TestPropertyValue(instance, getFunc, setFunc, roundTripTestValue);
        }

        public static void BooleanProperty<T>(T instance, Expression<Func<T, bool>> propertyGetter, bool expectedDefaultValue)
        {
            PropertyInfo property = GetPropertyInfo(propertyGetter);
            Func<T, bool> getFunc = (obj) => (bool)property.GetValue(obj, index: null);
            Action<T, bool> setFunc = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(expectedDefaultValue, getFunc(instance));

            TestPropertyValue(instance, getFunc, setFunc, !expectedDefaultValue);
        }

        public static void EnumProperty<T, TResult>(T instance, Expression<Func<T, TResult>> propertyGetter, TResult expectedDefaultValue, TResult illegalValue, TResult roundTripTestValue) where TResult : struct
        {
            PropertyInfo property = GetPropertyInfo(propertyGetter);
            Func<T, TResult> getFunc = (obj) => (TResult)property.GetValue(obj, index: null);
            Action<T, TResult> setFunc = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(expectedDefaultValue, getFunc(instance));

            ExceptionAssert.ThrowsInvalidEnumArgument(() => { setFunc(instance, illegalValue); }, "value", Convert.ToInt32(illegalValue), typeof(TResult));

            TestPropertyValue(instance, getFunc, setFunc, roundTripTestValue);
        }

        public static void EnumPropertyWithoutIllegalValueCheck<T, TResult>(T instance, Expression<Func<T, TResult>> propertyGetter, TResult expectedDefaultValue, TResult roundTripTestValue) where TResult : struct
        {
            PropertyInfo property = GetPropertyInfo(propertyGetter);
            Func<T, TResult> getFunc = (obj) => (TResult)property.GetValue(obj, index: null);
            Action<T, TResult> setFunc = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(expectedDefaultValue, getFunc(instance));

            TestPropertyValue(instance, getFunc, setFunc, roundTripTestValue);
        }

        public static void StringProperty<T>(T instance, Expression<Func<T, string>> propertyGetter, string expectedDefaultValue,
                                      bool allowNullAndEmpty = true, bool treatNullAsEmpty = true)
        {
            PropertyInfo property = GetPropertyInfo(propertyGetter);
            Func<T, string> getFunc = (obj) => (string)property.GetValue(obj, index: null);
            Action<T, string> setFunc = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(expectedDefaultValue, getFunc(instance));

            if (allowNullAndEmpty)
            {
                // Assert get/set works for null
                TestPropertyValue(instance, getFunc, setFunc, null, treatNullAsEmpty ? String.Empty : null);

                // Assert get/set works for String.Empty
                TestPropertyValue(instance, getFunc, setFunc, String.Empty, String.Empty);
            }
            else
            {
                ExceptionAssert.ThrowsArgumentNullOrEmpty(
                    delegate()
                    {
                        try
                        {
                            TestPropertyValue(instance, getFunc, setFunc, null);
                        }
                        catch (TargetInvocationException e)
                        {
                            throw e.InnerException;
                        }
                    },
                    "value");
                ExceptionAssert.ThrowsArgumentNullOrEmpty(
                    delegate()
                    {
                        try
                        {
                            TestPropertyValue(instance, getFunc, setFunc, String.Empty);
                        }
                        catch (TargetInvocationException e)
                        {
                            throw e.InnerException;
                        }
                    },
                    "value");
            }

            // Assert get/set works for arbitrary value
            TestPropertyValue(instance, getFunc, setFunc, "TestValue");
        }
    }
}
