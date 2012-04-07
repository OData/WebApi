// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Web;

namespace Microsoft.TestCommon
{
    public partial class AssertEx
    {
        /// <summary>
        /// Determines if your thread's current culture and current UI culture is English.
        /// </summary>
        public static bool CurrentCultureIsEnglish
        {
            get
            {
                return String.Equals(CultureInfo.CurrentCulture.TwoLetterISOLanguageName, "en", StringComparison.OrdinalIgnoreCase)
                    && String.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "en", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Determines whether the specified exception is of the given type (or optionally of a derived type).
        /// The exception is not allowed to be null;
        /// </summary>
        /// <param name="exceptionType">The type of the exception to test for.</param>
        /// <param name="exception">The exception to be tested.</param>
        /// <param name="expectedMessage">The expected exception message (only verified on US English OSes).</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        public static void IsException(Type exceptionType, Exception exception, string expectedMessage = null, bool allowDerivedExceptions = false)
        {
            exception = UnwrapException(exception);
            NotNull(exception);

            if (allowDerivedExceptions)
                IsAssignableFrom(exceptionType, exception);
            else
                IsType(exceptionType, exception);

            VerifyExceptionMessage(exception, expectedMessage, partialMatch: false);
        }

        /// <summary>
        /// Determines whether the specified exception is of the given type (or optionally of a derived type).
        /// The exception is not allowed to be null;
        /// </summary>
        /// <typeparam name="TException">The type of the exception to test for.</typeparam>
        /// <param name="exception">The exception to be tested.</param>
        /// <param name="expectedMessage">The expected exception message (only verified on US English OSes).</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception cast to TException.</returns>
        public static TException IsException<TException>(Exception exception, string expectedMessage = null, bool allowDerivedExceptions = false)
            where TException : Exception
        {
            TException result;

            exception = UnwrapException(exception);
            NotNull(exception);

            if (allowDerivedExceptions)
                result = IsAssignableFrom<TException>(exception);
            else
                result = IsType<TException>(exception);

            VerifyExceptionMessage(exception, expectedMessage, partialMatch: false);
            return result;
        }

        // We've re-implemented all the xUnit.net Throws code so that we can get this
        // updated implementation of RecordException which silently unwraps any instances
        // of AggregateException. This lets our tests better simulate what "await" would do
        // and thus makes them easier to port to .NET 4.5.
        private static Exception RecordException(Action testCode)
        {
            try
            {
                testCode();
                return null;
            }
            catch (Exception exception)
            {
                return UnwrapException(exception);
            }
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(Action testCode)
            where T : Exception
        {
            return (T)Throws(typeof(T), testCode);
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// Generally used to test property accessors.
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(Func<object> testCode)
            where T : Exception
        {
            return (T)Throws(typeof(T), testCode);
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <param name="exceptionType">The type of the exception expected to be thrown</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static Exception Throws(Type exceptionType, Action testCode)
        {
            Exception exception = RecordException(testCode);

            if (exception == null)
                throw new ThrowsException(exceptionType);

            if (!exceptionType.Equals(exception.GetType()))
                throw new ThrowsException(exceptionType, exception);

            return exception;
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// Generally used to test property accessors.
        /// </summary>
        /// <param name="exceptionType">The type of the exception expected to be thrown</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static Exception Throws(Type exceptionType, Func<object> testCode)
        {
            return Throws(exceptionType, () => { object unused = testCode(); });
        }

        /// <summary>
        /// Verifies that an exception of the given type (or optionally a derived type) is thrown.
        /// </summary>
        /// <typeparam name="TException">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static TException Throws<TException>(Action testCode, bool allowDerivedExceptions)
            where TException : Exception
        {
            Type exceptionType = typeof(TException);
            Exception exception = RecordException(testCode);

            TargetInvocationException tie = exception as TargetInvocationException;
            if (tie != null)
            {
                exception = tie.InnerException;
            }

            if (exception == null)
            {
                throw new ThrowsException(exceptionType);
            }

            var typedException = exception as TException;
            if (typedException == null || (!allowDerivedExceptions && typedException.GetType() != typeof(TException)))
            {
                throw new ThrowsException(exceptionType, exception);
            }

            return typedException;
        }

        /// <summary>
        /// Verifies that an exception of the given type (or optionally a derived type) is thrown.
        /// Generally used to test property accessors.
        /// </summary>
        /// <typeparam name="TException">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static TException Throws<TException>(Func<object> testCode, bool allowDerivedExceptions)
            where TException : Exception
        {
            return Throws<TException>(() => { testCode(); }, allowDerivedExceptions);
        }

        /// <summary>
        /// Verifies that an exception of the given type (or optionally a derived type) is thrown.
        /// Also verified that the exception message matches if the current thread locale is English.
        /// </summary>
        /// <typeparam name="TException">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="exceptionMessage">The exception message to verify</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static TException Throws<TException>(Action testCode, string exceptionMessage, bool allowDerivedExceptions = false)
            where TException : Exception
        {
            var ex = Throws<TException>(testCode, allowDerivedExceptions);
            VerifyExceptionMessage(ex, exceptionMessage);
            return ex;
        }

        /// <summary>
        /// Verifies that an exception of the given type (or optionally a derived type) is thrown.
        /// Also verified that the exception message matches if the current thread locale is English.
        /// </summary>
        /// <typeparam name="TException">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="exceptionMessage">The exception message to verify</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static TException Throws<TException>(Func<object> testCode, string exceptionMessage, bool allowDerivedExceptions = false)
            where TException : Exception
        {
            return Throws<TException>(() => { testCode(); }, exceptionMessage, allowDerivedExceptions);
        }

        /// <summary>
        /// Verifies that the code throws an <see cref="ArgumentException"/> (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentException ThrowsArgument(Action testCode, string paramName, bool allowDerivedExceptions = false)
        {
            var ex = Throws<ArgumentException>(testCode, allowDerivedExceptions);

            if (paramName != null)
            {
                Equal(paramName, ex.ParamName);
            }

            return ex;
        }

        /// <summary>
        /// Verifies that the code throws an <see cref="ArgumentException"/> (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="exceptionMessage">The exception message to verify</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentException ThrowsArgument(Action testCode, string paramName, string exceptionMessage, bool allowDerivedExceptions = false)
        {
            var ex = Throws<ArgumentException>(testCode, allowDerivedExceptions);

            if (paramName != null)
            {
                Equal(paramName, ex.ParamName);
            }

            VerifyExceptionMessage(ex, exceptionMessage, partialMatch: true);

            return ex;
        }

        /// <summary>
        /// Verifies that the code throws an ArgumentException (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentException ThrowsArgument(Func<object> testCode, string paramName, bool allowDerivedExceptions = false)
        {
            var ex = Throws<ArgumentException>(testCode, allowDerivedExceptions);

            if (paramName != null)
            {
                Equal(paramName, ex.ParamName);
            }

            return ex;
        }

        /// <summary>
        /// Verifies that the code throws an ArgumentNullException (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentNullException ThrowsArgumentNull(Action testCode, string paramName)
        {
            var ex = Throws<ArgumentNullException>(testCode, allowDerivedExceptions: false);

            if (paramName != null)
            {
                Equal(paramName, ex.ParamName);
            }

            return ex;
        }

        /// <summary>
        /// Verifies that the code throws an ArgumentNullException with the expected message that indicates that the value cannot
        /// be null or empty.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentException ThrowsArgumentNullOrEmpty(Action testCode, string paramName)
        {
            return Throws<ArgumentException>(testCode, "Value cannot be null or empty.\r\nParameter name: " + paramName, allowDerivedExceptions: false);
        }

        /// <summary>
        /// Verifies that the code throws an ArgumentNullException with the expected message that indicates that the value cannot
        /// be null or empty string.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentException ThrowsArgumentNullOrEmptyString(Action testCode, string paramName)
        {
            return ThrowsArgument(testCode, paramName, "Value cannot be null or an empty string.", allowDerivedExceptions: true);
        }

        /// <summary>
        /// Verifies that the code throws an ArgumentOutOfRangeException (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="exceptionMessage">The exception message to verify</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <param name="actualValue">The actual value provided</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentOutOfRangeException ThrowsArgumentOutOfRange(Action testCode, string paramName, string exceptionMessage, bool allowDerivedExceptions = false, object actualValue = null)
        {
            exceptionMessage = exceptionMessage != null
                                   ? exceptionMessage + "\r\nParameter name: " + paramName +
                                       (actualValue != null ? "\r\nActual value was " + actualValue.ToString() + "." : "")
                                   : exceptionMessage;
            var ex = Throws<ArgumentOutOfRangeException>(testCode, exceptionMessage, allowDerivedExceptions);

            if (paramName != null)
            {
                Equal(paramName, ex.ParamName);
            }

            return ex;
        }

        /// <summary>
        /// Verifies that the code throws an <see cref="ArgumentOutOfRangeException"/> with the expected message that indicates that
        /// the value must be greater than the given <paramref name="value"/>.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="actualValue">The actual value provided.</param>
        /// <param name="value">The expected limit value.</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentOutOfRangeException ThrowsArgumentGreaterThan(Action testCode, string paramName, string value, object actualValue = null)
        {
            return ThrowsArgumentOutOfRange(testCode, paramName, String.Format("Value must be greater than {0}.", value), false, actualValue);
        }

        /// <summary>
        /// Verifies that the code throws an <see cref="ArgumentOutOfRangeException"/> with the expected message that indicates that
        /// the value must be greater than or equal to the given value.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="value">The expected limit value.</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentOutOfRangeException ThrowsArgumentGreaterThanOrEqualTo(Action testCode, string paramName, string value, object actualValue = null)
        {
            return ThrowsArgumentOutOfRange(testCode, paramName, String.Format("Value must be greater than or equal to {0}.", value), false, actualValue);
        }

        /// <summary>
        /// Verifies that the code throws an <see cref="ArgumentOutOfRangeException"/> with the expected message that indicates that
        /// the value must be less than the given <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="actualValue">The actual value provided.</param>
        /// <param name="maxValue">The expected limit value.</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentOutOfRangeException ThrowsArgumentLessThan(Action testCode, string paramName, string maxValue, object actualValue = null)
        {
            return ThrowsArgumentOutOfRange(testCode, paramName, String.Format("Value must be less than {0}.", maxValue), false, actualValue);
        }

        /// <summary>
        /// Verifies that the code throws an <see cref="ArgumentOutOfRangeException"/> with the expected message that indicates that
        /// the value must be less than or equal to the given <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="actualValue">The actual value provided.</param>
        /// <param name="maxValue">The expected limit value.</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ArgumentOutOfRangeException ThrowsArgumentLessThanOrEqualTo(Action testCode, string paramName, string maxValue, object actualValue = null)
        {
            return ThrowsArgumentOutOfRange(testCode, paramName, String.Format("Value must be less than or equal to {0}.", maxValue), false, actualValue);
        }

        /// <summary>
        /// Verifies that the code throws an HttpException (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="exceptionMessage">The exception message to verify</param>
        /// <param name="httpCode">The expected HTTP status code of the exception</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static HttpException ThrowsHttpException(Action testCode, string exceptionMessage, int httpCode, bool allowDerivedExceptions = false)
        {
            var ex = Throws<HttpException>(testCode, exceptionMessage, allowDerivedExceptions);
            Equal(httpCode, ex.GetHttpCode());
            return ex;
        }

        /// <summary>
        /// Verifies that the code throws an InvalidEnumArgumentException (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="paramName">The name of the parameter that should throw the exception</param>
        /// <param name="invalidValue">The expected invalid value that should appear in the message</param>
        /// <param name="enumType">The type of the enumeration</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static InvalidEnumArgumentException ThrowsInvalidEnumArgument(Action testCode, string paramName, int invalidValue, Type enumType, bool allowDerivedExceptions = false)
        {
            return Throws<InvalidEnumArgumentException>(
                testCode,
                String.Format("The value of argument '{0}' ({1}) is invalid for Enum type '{2}'.{3}Parameter name: {0}", paramName, invalidValue, enumType.Name, Environment.NewLine),
                allowDerivedExceptions
            );
        }

        /// <summary>
        /// Verifies that the code throws an HttpException (or optionally any exception which derives from it).
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <param name="objectName">The name of the object that was dispose</param>
        /// <param name="allowDerivedExceptions">Pass true to allow exceptions which derive from TException; pass false, otherwise</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static ObjectDisposedException ThrowsObjectDisposed(Action testCode, string objectName, bool allowDerivedExceptions = false)
        {
            var ex = Throws<ObjectDisposedException>(testCode, allowDerivedExceptions);

            if (objectName != null)
            {
                Equal(objectName, ex.ObjectName);
            }

            return ex;
        }

        private static Exception UnwrapException(Exception exception)
        {
            AggregateException aggEx;
            while ((aggEx = exception as AggregateException) != null)
                exception = aggEx.GetBaseException();

            return exception;
        }

        private static void VerifyExceptionMessage(Exception exception, string expectedMessage, bool partialMatch = false)
        {
            if (expectedMessage != null && CurrentCultureIsEnglish)
            {
                if (!partialMatch)
                {
                    Equal(expectedMessage, exception.Message);
                }
                else
                {
                    Contains(expectedMessage, exception.Message);
                }
            }
        }

        // Custom ThrowsException so we can filter the stack trace.
        private class ThrowsException : Xunit.Sdk.ThrowsException
        {
            public ThrowsException(Type type) : base(type) { }

            public ThrowsException(Type type, Exception ex) : base(type, ex) { }

            protected override bool ExcludeStackFrame(string stackFrame)
            {
                if (stackFrame.StartsWith("at Microsoft.TestCommon.AssertEx.", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return base.ExcludeStackFrame(stackFrame);
            }
        }
    }
}
