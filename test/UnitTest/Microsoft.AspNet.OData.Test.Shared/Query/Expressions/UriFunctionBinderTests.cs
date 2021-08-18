//-----------------------------------------------------------------------------
// <copyright file="UriFunctionBinderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Expressions
{
    /// <summary>
    /// Tests to UriFunctions binder.
    /// </summary>
    public class UriFunctionBinderTests
    {
        #region BindUriFunctionName

        // Bind

        // Validations:
        // method name null
        // method name string.Empty
        // methodInfo null

        [Fact]
        public void BindUriFunctionName_FunctionNameNull()
        {
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            Action bindUriFunction = () =>
                UriFunctionsBinder.BindUriFunctionName(null, padRightStringMethodInfo);

            ExceptionAssert.ThrowsArgumentNull(bindUriFunction, "functionName");
        }

        [Fact]
        public void BindUriFunctionName_FunctionNameStringEmpty()
        {
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            Action bindUriFunction = () =>
                UriFunctionsBinder.BindUriFunctionName(string.Empty, padRightStringMethodInfo);

            ExceptionAssert.ThrowsArgumentNull(bindUriFunction, "functionName");
        }

        [Fact]
        public void BindUriFunctionName_MethodInfoNull()
        {
            Action bindUriFunction = () =>
                UriFunctionsBinder.BindUriFunctionName("startswith", null);

            ExceptionAssert.ThrowsArgumentNull(bindUriFunction, "methodInfo");
        }

        // Add - succ
        // Add already exists - fail

        // Type of MethodInfo:
        // Static MethodInfo
        // Static Extenral MethoInfo
        // Instance
        // Instance different declaring type
        // Add instance when static exists
        // Add static when instance exists

        [Fact]
        public void BindUriFunctionName_CanBind()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

                MethodInfo resultMethoInfo;
                UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(int) }, out resultMethoInfo);

                Assert.Equal(padRightStringMethodInfo, resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CannotBindIfAlreadyBinded()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticMethodInfo = typeof(UriFunctionBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                // Add for the first time
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo);

                // Add for the second time
                Action bindExisting = () => UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo);

                ExceptionAssert.Throws<ODataException>(bindExisting);
            }
            finally
            {
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CanBindStaticMethod()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticMethodInfo = typeof(UriFunctionBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo);
                MethodInfo resultMethoInfo;
                UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo);
                Assert.Equal(addStrTwiceStaticMethodInfo, resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CanBindStaticExtensionMethod()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticExtensionMethodInfo =
                typeof(UriFunctionClrBinderTestsStaticExtensionMethods).GetMethod("AddStringTwice", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticExtensionMethodInfo);

                MethodInfo resultMethoInfo;
                UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo);

                Assert.Equal(addStrTwiceStaticExtensionMethodInfo, resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticExtensionMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CanBindInstanceMethod()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightInstanceMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightInstanceMethodInfo);

                MethodInfo resultMethoInfo;
                UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(int) }, out resultMethoInfo);

                Assert.Equal(padRightInstanceMethodInfo, resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightInstanceMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CannotBindInstanceMethodOfDifferentDeclaringType()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addTwiceInstanceThisDelcaringTypeMethodInfo =
                typeof(UriFunctionBinderTests).GetMethod("AddStringTwiceInstance", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addTwiceInstanceThisDelcaringTypeMethodInfo);

                MethodInfo resultMethoInfo;
                bool couldFindBinding =
                    UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo);

                Assert.False(couldFindBinding);
                Assert.Null(resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addTwiceInstanceThisDelcaringTypeMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CannotBindStaticAndInstanceMethodWithSameArguments()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStaticMethodInfo = typeof(UriFunctionBinderTests).GetMethod("PadRightStatic", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo padRightInstanceMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightStaticMethodInfo);
                Action bindingInstance = () => UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightInstanceMethodInfo);

                ExceptionAssert.Throws<ODataException>(bindingInstance);
            }
            finally
            {
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStaticMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CanBindStaticAndInstanceOfDifferentDeclerationType()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addTwiceStaticMethodInfo = typeof(UriFunctionBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo addTwiceInstanceMethodInfo = typeof(UriFunctionBinderTests).GetMethod("AddStringTwiceInstance", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addTwiceStaticMethodInfo);
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addTwiceInstanceMethodInfo);

                MethodInfo resultMethoInfoStatic;
                UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfoStatic);

                Assert.Equal(addTwiceStaticMethodInfo, resultMethoInfoStatic);

                MethodInfo resultMethoInfoInstance;
                UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(UriFunctionBinderTests), typeof(string) }, out resultMethoInfoInstance);

                Assert.Equal(addTwiceInstanceMethodInfo, resultMethoInfoInstance);
            }
            finally
            {
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addTwiceStaticMethodInfo));
                Assert.True(
                    UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addTwiceInstanceMethodInfo));
            }
        }

        #endregion

        #region UnbindUriFunctionName

        // Unbind

        // Validations
        // method name null
        // method name string.Empty
        // methodInfo null

        [Fact]
        public void UnbindUriFunctionName_FunctionNameNull()
        {
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            Action bindUriFunction = () =>
                UriFunctionsBinder.UnbindUriFunctionName(null, padRightStringMethodInfo);

            ExceptionAssert.ThrowsArgumentNull(bindUriFunction, "functionName");
        }

        [Fact]
        public void UnbindUriFunctionName_FunctionNameStringEmpty()
        {
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            Action bindUriFunction = () =>
                UriFunctionsBinder.UnbindUriFunctionName(string.Empty, padRightStringMethodInfo);

            ExceptionAssert.ThrowsArgumentNull(bindUriFunction, "functionName");
        }

        [Fact]
        public void UnbindUriFunctionName_MethodInfoNull()
        {
            Action bindUriFunction = () =>
                UriFunctionsBinder.UnbindUriFunctionName("startswith", null);

            ExceptionAssert.ThrowsArgumentNull(bindUriFunction, "methodInfo");
        }

        // Remove -
        // Removed not existing
        // Remove static when instance is tregistered - faile
        // Remove instance when static is tregistered - faile

        [Fact]
        public void UnbindUriFunctionName_CanUnbind()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));


            MethodInfo resultMethoInfo;
            Assert.False(UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(int) }, out resultMethoInfo));

            Assert.Null(resultMethoInfo);
        }

        [Fact]
        public void UnbindUriFunctionName_CannotUnbindNotBindedFunction_DifferentFunctionName()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

                Assert.False(UriFunctionsBinder.UnbindUriFunctionName("AnotherFunctionName", padRightStringMethodInfo));
            }
            finally
            {
                Assert.True(UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void UnbindUriFunctionName_CannotUnbindNotBindedFunction_DifferentMethodInfo()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

                MethodInfo differentMethodInfo = typeof(UriFunctionBinderTests).GetMethod("AddStringTwiceInstance", BindingFlags.NonPublic | BindingFlags.Instance);

                Assert.False(UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, differentMethodInfo));
            }
            finally
            {
                Assert.True(UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void UnbindUriFunctionName_CanUnbindInstanceMethod()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));


            MethodInfo resultMethoInfo;
            Assert.False(UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(int) }, out resultMethoInfo));

            Assert.Null(resultMethoInfo);
        }

        [Fact]
        public void UnbindUriFunctionName_CanUnbindStaticMethod()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticMethodInfo = typeof(UriFunctionBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);

            UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo);

            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo));


            MethodInfo resultMethoInfo;
            Assert.False(UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo));

            Assert.Null(resultMethoInfo);
        }

        [Fact]
        public void UnbindUriFunctionName_CanUnbindExtensionStaticMethod()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticExtensionMethodInfo =
                typeof(UriFunctionClrBinderTestsStaticExtensionMethods).GetMethod("AddStringTwice", BindingFlags.NonPublic | BindingFlags.Static);

            UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticExtensionMethodInfo);

            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticExtensionMethodInfo));

            MethodInfo resultMethoInfo;
            Assert.False(UriFunctionsBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo));

            Assert.Null(resultMethoInfo);
        }

        [Fact]
        public void UnbindUriFunctionName_CannotUnbindWithDifferentMethod()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

                MethodInfo addStrTwiceStaticMethodInfo = typeof(UriFunctionBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);

                Assert.False(UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo));
            }
            finally
            {
                UriFunctionsBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);
            }
        }

        #endregion

        #region Private Methods - Helpers (Used by reflection)

        /// <summary>
        /// Is used by reflection.
        /// </summary>
        private static string AddStringTwiceStatic(string str, string strToAdd)
        {
            return null;
        }

        private string AddStringTwiceInstance(string strToAdd)
        {
            return null;
        }

        private static string PadRightStatic(string str, int width)
        {
            return str.PadRight(width);
        }

        #endregion
    }

    public static class UriFunctionClrBinderTestsStaticExtensionMethods
    {
        /// <summary>
        /// Is used by reflection.
        /// </summary>
        private static string AddStringTwice(this string str, string strToAdd)
        {
            return null;
        }
    }
}
