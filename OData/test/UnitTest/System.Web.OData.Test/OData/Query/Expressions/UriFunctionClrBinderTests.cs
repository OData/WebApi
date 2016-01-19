
using Microsoft.OData.Core;
using Microsoft.TestCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Properties;
using System.Web.OData.Query.Expressions;

namespace System.Web.OData.Test.OData.Query.Expressions
{
    /// <summary>
    /// Tests to UriFunctions binder.
    /// </summary>
    public class UriFunctionClrBinderTests
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
                UriFunctionsToClrBinder.BindUriFunctionName(null, padRightStringMethodInfo);

            Assert.ThrowsArgumentNull(bindUriFunction, "functionName");
        }

        [Fact]
        public void BindUriFunctionName_FunctionNameStringEmpty()
        {
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            Action bindUriFunction = () =>
                UriFunctionsToClrBinder.BindUriFunctionName(string.Empty, padRightStringMethodInfo);

            Assert.ThrowsArgumentNull(bindUriFunction, "functionName");
        }

        [Fact]
        public void BindUriFunctionName_MethodInfoNull()
        {
            Action bindUriFunction = () =>
                UriFunctionsToClrBinder.BindUriFunctionName("startswith", null);

            Assert.ThrowsArgumentNull(bindUriFunction, "methodInfo");
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
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

                MethodInfo resultMethoInfo;
                UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(int) }, out resultMethoInfo);

                Assert.Equal(padRightStringMethodInfo, resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CannotBindIfAlreadyBinded()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticMethodInfo = typeof(UriFunctionClrBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {
                // Add for the first time
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo);

                // Add for the second time
                Action bindExisting = () => UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo);

                Assert.Throws<ODataException>(bindExisting);
            }
            finally
            {
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CanBindStaticMethod()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticMethodInfo = typeof(UriFunctionClrBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);

            try
            {

                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo);

                MethodInfo resultMethoInfo;
                UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo);

                Assert.Equal(addStrTwiceStaticMethodInfo, resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo));
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
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticExtensionMethodInfo);

                MethodInfo resultMethoInfo;
                UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo);

                Assert.Equal(addStrTwiceStaticExtensionMethodInfo, resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticExtensionMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CanBindInstanceMethod()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightInstanceMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightInstanceMethodInfo);

                MethodInfo resultMethoInfo;
                UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(int) }, out resultMethoInfo);

                Assert.Equal(padRightInstanceMethodInfo, resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightInstanceMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CannotBindInstanceMethodOfDifferentDeclaringType()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addTwiceInstanceThisDelcaringTypeMethodInfo =
                typeof(UriFunctionClrBinderTests).GetMethod("AddStringTwiceInstance", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addTwiceInstanceThisDelcaringTypeMethodInfo);

                MethodInfo resultMethoInfo;
                bool couldFindBinding =
                    UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo);

                Assert.False(couldFindBinding);
                Assert.Null(resultMethoInfo);
            }
            finally
            {
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addTwiceInstanceThisDelcaringTypeMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CannotBindStaticAndInstanceMethodWithSameArguments()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStaticMethodInfo = typeof(UriFunctionClrBinderTests).GetMethod("PadRightStatic", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo padRightInstanceMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightStaticMethodInfo);
                Action bindingInstance = () => UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightInstanceMethodInfo);

                Assert.Throws<ODataException>(bindingInstance);
            }
            finally
            {
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStaticMethodInfo));
            }
        }

        [Fact]
        public void BindUriFunctionName_CanBindStaticAndInstanceOfDifferentDeclerationType()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addTwiceStaticMethodInfo = typeof(UriFunctionClrBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo addTwiceInstanceMethodInfo = typeof(UriFunctionClrBinderTests).GetMethod("AddStringTwiceInstance", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addTwiceStaticMethodInfo);
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addTwiceInstanceMethodInfo);

                MethodInfo resultMethoInfoStatic;
                UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfoStatic);

                Assert.Equal(addTwiceStaticMethodInfo, resultMethoInfoStatic);

                MethodInfo resultMethoInfoInstance;
                UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(UriFunctionClrBinderTests), typeof(string) }, out resultMethoInfoInstance);

                Assert.Equal(addTwiceInstanceMethodInfo, resultMethoInfoInstance);
            }
            finally
            {
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addTwiceStaticMethodInfo));
                Assert.True(
                    UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addTwiceInstanceMethodInfo));
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
                UriFunctionsToClrBinder.UnbindUriFunctionName(null, padRightStringMethodInfo);

            Assert.ThrowsArgumentNull(bindUriFunction, "functionName");
        }

        [Fact]
        public void UnbindUriFunctionName_FunctionNameStringEmpty()
        {
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            Action bindUriFunction = () =>
                UriFunctionsToClrBinder.UnbindUriFunctionName(string.Empty, padRightStringMethodInfo);

            Assert.ThrowsArgumentNull(bindUriFunction, "functionName");
        }

        [Fact]
        public void UnbindUriFunctionName_MethodInfoNull()
        {
            Action bindUriFunction = () =>
                UriFunctionsToClrBinder.UnbindUriFunctionName("startswith", null);

            Assert.ThrowsArgumentNull(bindUriFunction, "methodInfo");
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

            UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

            Assert.True(UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));


            MethodInfo resultMethoInfo;
            Assert.False(UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(int) }, out resultMethoInfo));

            Assert.Null(resultMethoInfo);
        }

        [Fact]
        public void UnbindUriFunctionName_CannotUnbindNotBindedFunction_DifferentFunctionName()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

                Assert.False(UriFunctionsToClrBinder.UnbindUriFunctionName("AnotherFunctionName", padRightStringMethodInfo));
            }
            finally
            {
                Assert.True(UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void UnbindUriFunctionName_CannotUnbindNotBindedFunction_DifferentMethodInfo()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

                MethodInfo differentMethodInfo = typeof(UriFunctionClrBinderTests).GetMethod("AddStringTwiceInstance", BindingFlags.NonPublic | BindingFlags.Instance);

                Assert.False(UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, differentMethodInfo));
            }
            finally
            {
                Assert.True(UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));
            }
        }

        [Fact]
        public void UnbindUriFunctionName_CanUnbindInstanceMethod()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

            Assert.True(UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo));


            MethodInfo resultMethoInfo;
            Assert.False(UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(int) }, out resultMethoInfo));

            Assert.Null(resultMethoInfo);
        }

        [Fact]
        public void UnbindUriFunctionName_CanUnbindStaticMethod()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticMethodInfo = typeof(UriFunctionClrBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);

            UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo);

            Assert.True(UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo));


            MethodInfo resultMethoInfo;
            Assert.False(UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo));

            Assert.Null(resultMethoInfo);
        }

        [Fact]
        public void UnbindUriFunctionName_CanUnbindExtensionStaticMethod()
        {
            const string FUNCTION_NAME = "addtwice";
            MethodInfo addStrTwiceStaticExtensionMethodInfo =
                typeof(UriFunctionClrBinderTestsStaticExtensionMethods).GetMethod("AddStringTwice", BindingFlags.NonPublic | BindingFlags.Static);

            UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticExtensionMethodInfo);

            Assert.True(UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticExtensionMethodInfo));

            MethodInfo resultMethoInfo;
            Assert.False(UriFunctionsToClrBinder.TryGetMethodInfo(FUNCTION_NAME, new Type[] { typeof(string), typeof(string) }, out resultMethoInfo));

            Assert.Null(resultMethoInfo);
        }

        [Fact]
        public void UnbindUriFunctionName_CannotUnbindWithDifferentMethod()
        {
            const string FUNCTION_NAME = "padright";
            MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });

            try
            {
                UriFunctionsToClrBinder.BindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);

                MethodInfo addStrTwiceStaticMethodInfo = typeof(UriFunctionClrBinderTests).GetMethod("AddStringTwiceStatic", BindingFlags.NonPublic | BindingFlags.Static);

                Assert.False(UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, addStrTwiceStaticMethodInfo));
            }
            finally
            {
                UriFunctionsToClrBinder.UnbindUriFunctionName(FUNCTION_NAME, padRightStringMethodInfo);
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
