// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Xunit;

namespace System.Json
{
    /// <summary>
    ///This is a test class for JsonValueDynamicMetaObjectTest and is intended to perform sanity tests on this class.
    ///Extended tests are performed by the JsonValue dynamic feature tests.
    ///</summary>
    public class JsonValueDynamicMetaObjectTest
    {
        const string NonSingleNonNullIndexNotSupported = "Null index or multidimensional indexing is not supported by this indexer; use 'System.Int32' or 'System.String' for array and object indexing respectively.";

        /// <summary>
        /// A test for GetMetaObject
        ///</summary>
        [Fact]
        public void GetMetaObjectTest()
        {
            ExceptionHelper.Throws<ArgumentNullException>(() => { var target = GetJsonValueDynamicMetaObject(AnyInstance.AnyJsonObject, null); });
        }

        /// <summary>
        /// A test for BindInvokeMember
        ///</summary>
        [Fact]
        public void BindInvokeMemberTest()
        {
            JsonValue value = AnyInstance.AnyJsonValue1;
            DynamicMetaObject target = GetJsonValueDynamicMetaObject(value);

            TestInvokeMemberBinder.TestBindParams(target);

            string methodName;
            object[] arguments;
            object result = null;

            methodName = "ToString";
            arguments = new object[] { };
            TestInvokeMemberBinder.TestMetaObject(target, methodName, arguments);

            methodName = "TryReadAs";
            arguments = new object[] { typeof(int), result };
            TestInvokeMemberBinder.TestMetaObject(target, methodName, arguments);

            methodName = "TryReadAsType";
            arguments = new object[] { typeof(Person), result };
            TestInvokeMemberBinder.TestMetaObject(target, methodName, arguments, true);
        }

        /// <summary>
        /// A test for BindConvert
        ///</summary>
        [Fact]
        public void BindConvertTest()
        {
            JsonValue value;
            DynamicMetaObject target;

            value = (JsonValue)AnyInstance.AnyInt;
            target = GetJsonValueDynamicMetaObject(value);
            TestConvertBinder.TestBindParams(target);

            Type[] intTypes = { typeof(int), typeof(uint), typeof(long), };

            foreach (Type type in intTypes)
            {
                TestConvertBinder.TestMetaObject(target, type);
            }

            value = (JsonValue)AnyInstance.AnyString;
            target = GetJsonValueDynamicMetaObject(value);
            TestConvertBinder.TestMetaObject(target, typeof(string));

            value = (JsonValue)AnyInstance.AnyJsonValue1;
            target = GetJsonValueDynamicMetaObject(value);
            TestConvertBinder.TestMetaObject(target, typeof(JsonValue));
            TestConvertBinder.TestMetaObject(target, typeof(IEnumerable<KeyValuePair<string, JsonValue>>));
            TestConvertBinder.TestMetaObject(target, typeof(IDynamicMetaObjectProvider));
            TestConvertBinder.TestMetaObject(target, typeof(object));

            TestConvertBinder.TestMetaObject(target, typeof(Person), false);
        }

        /// <summary>
        /// A test for BindGetIndex
        ///</summary>
        [Fact]
        public void BindGetIndexTest()
        {
            JsonValue value = AnyInstance.AnyJsonArray;

            DynamicMetaObject target = GetJsonValueDynamicMetaObject(value);

            TestGetIndexBinder.TestBindParams(target);

            foreach (KeyValuePair<string, JsonValue> pair in value)
            {
                TestGetIndexBinder.TestMetaObject(target, Int32.Parse(pair.Key));
            }
        }

        /// <summary>
        /// A test for BindSetIndex
        ///</summary>
        [Fact]
        public void BindSetIndexTest()
        {
            JsonValue jsonValue = AnyInstance.AnyJsonArray;

            DynamicMetaObject target = GetJsonValueDynamicMetaObject(jsonValue);

            TestSetIndexBinder.TestBindParams(target);

            int value = 0;

            foreach (KeyValuePair<string, JsonValue> pair in jsonValue)
            {
                TestSetIndexBinder.TestMetaObject(target, Int32.Parse(pair.Key), value++);
            }
        }

        /// <summary>
        /// A test for BindGetMember.
        ///</summary>
        [Fact]
        public void BindGetMemberTest()
        {
            JsonValue value = AnyInstance.AnyJsonObject;

            DynamicMetaObject target = GetJsonValueDynamicMetaObject(value);

            TestGetMemberBinder.TestBindParams(target);

            foreach (KeyValuePair<string, JsonValue> pair in value)
            {
                TestGetMemberBinder.TestMetaObject(target, pair.Key);
            }
        }

        /// <summary>
        /// A test for BindSetMember.
        ///</summary>
        [Fact]
        public void BindSetMemberTest()
        {
            JsonValue value = AnyInstance.AnyJsonObject;

            string expectedMethodSignature = "System.Json.JsonValue SetValue(System.String, System.Object)";

            DynamicMetaObject target = GetJsonValueDynamicMetaObject(value);
            DynamicMetaObject arg = new DynamicMetaObject(Expression.Parameter(typeof(int)), BindingRestrictions.Empty, AnyInstance.AnyInt);

            TestSetMemberBinder.TestBindParams(target, arg);

            foreach (KeyValuePair<string, JsonValue> pair in value)
            {
                TestSetMemberBinder.TestMetaObject(target, pair.Key, arg, expectedMethodSignature);
            }
        }

        /// <summary>
        /// A test for GetDynamicMemberNames
        ///</summary>
        [Fact]
        public void GetDynamicMemberNamesTest()
        {
            JsonValue[] values = AnyInstance.AnyJsonValueArray;

            foreach (JsonValue value in values)
            {
                DynamicMetaObject target = GetJsonValueDynamicMetaObject(value);

                List<string> expected = new List<string>();
                foreach (KeyValuePair<string, JsonValue> pair in value)
                {
                    expected.Add(pair.Key);
                }

                IEnumerable<string> retEnumerable = target.GetDynamicMemberNames();
                Assert.NotNull(retEnumerable);

                List<string> actual = new List<string>(retEnumerable);
                Assert.Equal(expected.Count, actual.Count);

                for (int i = 0; i < expected.Count; i++)
                {
                    Assert.Equal<string>(expected[i], actual[i]);
                }
            }
        }

        /// <summary>
        /// Helper method for getting a <see cref="JsonValueDynamicMetaObject"/>.
        /// </summary>
        /// <param name="jsonValue">The <see cref="JsonValue"/> instance to get the dynamic meta-object from.</param>
        /// <returns></returns>
        private static DynamicMetaObject GetJsonValueDynamicMetaObject(JsonValue jsonValue)
        {
            return GetJsonValueDynamicMetaObject(jsonValue, Expression.Parameter(typeof(object)));
        }

        private static DynamicMetaObject GetJsonValueDynamicMetaObject(JsonValue jsonValue, Expression expression)
        {
            return ((IDynamicMetaObjectProvider)jsonValue).GetMetaObject(expression);
        }

        /// <summary>
        /// Test binder for method call operation.
        /// </summary>
        private class TestInvokeMemberBinder : InvokeMemberBinder
        {
            public TestInvokeMemberBinder(string name, int argCount)
                : base(name, false, new CallInfo(argCount, new string[] { }))
            {
            }

            public static void TestBindParams(DynamicMetaObject target)
            {
                string methodName = "ToString";
                object[] arguments = new object[] { };

                InvokeMemberBinder binder = new TestInvokeMemberBinder(methodName, arguments.Length);
                DynamicMetaObject[] args = new DynamicMetaObject[arguments.Length];

                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindInvokeMember(null, args); });
                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindInvokeMember(binder, null); });
            }

            public static void TestMetaObject(DynamicMetaObject target, string methodName, object[] arguments, bool isExtension = false)
            {
                InvokeMemberBinder binder = new TestInvokeMemberBinder(methodName, arguments.Length);
                DynamicMetaObject[] args = new DynamicMetaObject[arguments.Length];

                for (int idx = 0; idx < args.Length; idx++)
                {
                    object value = arguments[idx];
                    Type valueType = value != null ? value.GetType() : typeof(object);
                    args[idx] = new DynamicMetaObject(Expression.Parameter(valueType), BindingRestrictions.Empty, value);
                }

                DynamicMetaObject result = target.BindInvokeMember(binder, args);
                Assert.NotNull(result);

                if (isExtension)
                {
                    UnaryExpression expression = result.Expression as UnaryExpression;
                    Assert.NotNull(expression);

                    MethodCallExpression callExpression = expression.Operand as MethodCallExpression;
                    Assert.NotNull(callExpression);

                    Assert.True(callExpression.Method.ToString().Contains(methodName));
                }
                else
                {
                    Assert.Same(target, result.Value);
                }
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }

            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
            {
                // This is where the C# binder does the actual binding.
                return new DynamicMetaObject(Expression.Constant("FallbackInvokeMember called"), BindingRestrictions.Empty, target);
            }
        }

        /// <summary>
        /// The binder for the cast operation.
        /// </summary>
        private class TestConvertBinder : ConvertBinder
        {
            public TestConvertBinder(Type type)
                : base(type, false)
            {
            }

            public static void TestBindParams(DynamicMetaObject target)
            {
                ConvertBinder binder = new TestConvertBinder(typeof(int));
                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindConvert(null); });
            }

            public static void TestMetaObject(DynamicMetaObject target, Type type, bool isValid = true)
            {
                ConvertBinder binder = new TestConvertBinder(type);
                DynamicMetaObject result = target.BindConvert(binder);
                Assert.NotNull(result);

                // Convert expression
                UnaryExpression expression = result.Expression as UnaryExpression;
                Assert.NotNull(expression);
                Assert.Equal<Type>(binder.Type, expression.Type);

                if (isValid)
                {
                    MethodCallExpression methodCallExp = expression.Operand as MethodCallExpression;

                    if (methodCallExp != null)
                    {
                        Assert.True(methodCallExp.Method.ToString().Contains("CastValue"));
                    }
                    else
                    {
                        ParameterExpression paramExpression = expression.Operand as ParameterExpression;
                        Assert.NotNull(paramExpression);
                    }
                }
                else
                {
                    Expression<Action> throwExp = Expression.Lambda<Action>(Expression.Block(expression), new ParameterExpression[] { });
                    ExceptionHelper.Throws<InvalidCastException>(() => throwExp.Compile().Invoke());
                }
            }

            public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Test binder for int indexer getter operation.
        /// </summary>
        private class TestGetIndexBinder : GetIndexBinder
        {
            public TestGetIndexBinder()
                : base(new CallInfo(0, new string[] { }))
            {
            }

            public static void TestBindParams(DynamicMetaObject target)
            {
                GetIndexBinder binder = new TestGetIndexBinder();
                Expression typeExpression = Expression.Parameter(typeof(int));

                DynamicMetaObject[] indexes = 
                { 
                    new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, 0), 
                    new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, 1), 
                    new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, 2) 
                };

                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindGetIndex(null, indexes); });
                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindGetIndex(binder, null); });

                DynamicMetaObject[][] invalidIndexesParam =
                {
                    indexes,
                    new DynamicMetaObject[] { new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, null) },  
                    new DynamicMetaObject[] { null },
                    new DynamicMetaObject[] { }
                };

                foreach (DynamicMetaObject[] indexesParam in invalidIndexesParam)
                {
                    DynamicMetaObject metaObj = target.BindGetIndex(binder, indexesParam);

                    Expression<Action> expression = Expression.Lambda<Action>(Expression.Block(metaObj.Expression), new ParameterExpression[] { });
                    ExceptionHelper.Throws<ArgumentException>(() => { expression.Compile().Invoke(); }, NonSingleNonNullIndexNotSupported);
                }
            }

            public static void TestMetaObject(DynamicMetaObject target, int index, bool isValid = true)
            {
                string expectedMethodSignature = "System.Json.JsonValue GetValue(Int32)";

                GetIndexBinder binder = new TestGetIndexBinder();
                DynamicMetaObject[] indexes = { new DynamicMetaObject(Expression.Parameter(typeof(int)), BindingRestrictions.Empty, index) };

                DynamicMetaObject result = target.BindGetIndex(binder, indexes);
                Assert.NotNull(result);

                MethodCallExpression expression = result.Expression as MethodCallExpression;
                Assert.NotNull(expression);
                Assert.Equal<string>(expectedMethodSignature, expression.Method.ToString());
            }

            public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Test binder for int indexer setter operation.
        /// </summary>
        private class TestSetIndexBinder : SetIndexBinder
        {
            public TestSetIndexBinder()
                : base(new CallInfo(0, new string[] { }))
            {
            }

            public static void TestBindParams(DynamicMetaObject target)
            {
                SetIndexBinder binder = new TestSetIndexBinder();
                Expression typeExpression = Expression.Parameter(typeof(int));
                DynamicMetaObject[] indexes = new DynamicMetaObject[] { new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, 0) };
                DynamicMetaObject value = new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, (JsonValue)10);

                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindSetIndex(null, indexes, value); });
                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindSetIndex(binder, null, value); });
                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindSetIndex(binder, indexes, null); });

                DynamicMetaObject[][] invalidIndexesParam =
                {
                    new DynamicMetaObject[]
                    { 
                        new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, 0), 
                        new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, 1), 
                        new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, 2) 
                    },

                    new DynamicMetaObject[] 
                    {
                        new DynamicMetaObject(typeExpression, BindingRestrictions.Empty, null)
                    },           

                    new DynamicMetaObject[]
                    {
                    }
                };

                foreach (DynamicMetaObject[] indexesParam in invalidIndexesParam)
                {
                    DynamicMetaObject metaObj = target.BindSetIndex(binder, indexesParam, value);

                    Expression<Action> expression = Expression.Lambda<Action>(Expression.Block(metaObj.Expression), new ParameterExpression[] { });
                    ExceptionHelper.Throws<ArgumentException>(() => { expression.Compile().Invoke(); }, NonSingleNonNullIndexNotSupported);
                }
            }

            public static void TestMetaObject(DynamicMetaObject target, int index, JsonValue jsonValue, bool isValid = true)
            {
                string expectedMethodSignature = "System.Json.JsonValue SetValue(Int32, System.Object)";

                SetIndexBinder binder = new TestSetIndexBinder();
                DynamicMetaObject[] indexes = { new DynamicMetaObject(Expression.Parameter(typeof(int)), BindingRestrictions.Empty, index) };
                DynamicMetaObject value = new DynamicMetaObject(Expression.Parameter(jsonValue.GetType()), BindingRestrictions.Empty, jsonValue);
                DynamicMetaObject result = target.BindSetIndex(binder, indexes, value);
                Assert.NotNull(result);

                MethodCallExpression expression = result.Expression as MethodCallExpression;
                Assert.NotNull(expression);
                Assert.Equal<string>(expectedMethodSignature, expression.Method.ToString());
            }

            public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Test binder for key indexer getter.
        /// </summary>
        private class TestGetMemberBinder : GetMemberBinder
        {
            public TestGetMemberBinder(string name)
                : base(name, false)
            {
            }

            public static void TestBindParams(DynamicMetaObject target)
            {
                GetMemberBinder binder = new TestGetMemberBinder("AnyProperty");
                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindGetMember(null); });
            }

            public static void TestMetaObject(DynamicMetaObject target, string name, bool isValid = true)
            {
                string expectedMethodSignature = "System.Json.JsonValue GetValue(System.String)";

                GetMemberBinder binder = new TestGetMemberBinder(name);

                DynamicMetaObject result = target.BindGetMember(binder);
                Assert.NotNull(result);

                MethodCallExpression expression = result.Expression as MethodCallExpression;
                Assert.NotNull(expression);
                Assert.Equal<string>(expectedMethodSignature, expression.Method.ToString());
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Test binder for key indexer setter.
        /// </summary>
        private class TestSetMemberBinder : SetMemberBinder
        {
            public TestSetMemberBinder(string name)
                : base(name, false)
            {
            }

            public static void TestBindParams(DynamicMetaObject target, DynamicMetaObject value)
            {
                SetMemberBinder binder = new TestSetMemberBinder("AnyProperty");

                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindSetMember(null, value); });
                ExceptionHelper.Throws<ArgumentNullException>(() => { var result = target.BindSetMember(binder, null); });
            }

            public static void TestMetaObject(DynamicMetaObject target, string name, DynamicMetaObject value, string expectedMethodSignature, bool isValid = true)
            {
                SetMemberBinder binder = new TestSetMemberBinder(name);

                DynamicMetaObject result = target.BindSetMember(binder, value);
                Assert.NotNull(result);

                MethodCallExpression expression = result.Expression as MethodCallExpression;
                Assert.NotNull(expression);
                Assert.Equal<string>(expectedMethodSignature, expression.Method.ToString());
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }
    }
}
