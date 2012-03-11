using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Http.Common.Properties;

namespace System.Web.Http.Common
{
    /// <summary>
    /// Utility class for creating and unwrapping <see cref="Exception"/> instances.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error", Justification = "This usage is okay.")]
    public static class Error
    {
        /// <summary>
        /// Formats the specified resource string using <see cref="M:CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>The formatted string.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#", Justification = "Standard String.Format pattern and names.")]
        public static string Format(string format, params object[] args)
        {
            return String.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with the provided properties and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentException Argument(string messageFormat, params object[] messageArgs)
        {
            return new ArgumentException(Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with the provided properties and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentException Argument(string parameterName, string messageFormat, params object[] messageArgs)
        {
            return new ArgumentException(Error.Format(messageFormat, messageArgs), parameterName);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with a message saying that the argument must be an "http" or "https" URI and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentException ArgumentUriNotHttpOrHttpsScheme(string parameterName, Uri actualValue)
        {
            return new ArgumentException(Error.Format(SRResources.ArgumentInvalidHttpUriScheme, actualValue, Uri.UriSchemeHttp, Uri.UriSchemeHttps), parameterName);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with a message saying that the argument must be an absolute URI and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentException ArgumentUriNotAbsolute(string parameterName, Uri actualValue)
        {
            return new ArgumentException(Error.Format(SRResources.ArgumentInvalidAbsoluteUri, actualValue), parameterName);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with a message saying that the argument must be an absolute URI 
        /// without a query or fragment identifier and then logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentException ArgumentUriHasQueryOrFragment(string parameterName, Uri actualValue)
        {
            return new ArgumentException(Error.Format(SRResources.ArgumentUriHasQueryOrFragment, actualValue), parameterName);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentNullException"/> with the provided properties and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The purpose of this API is to return an error for properties")]
        public static ArgumentNullException PropertyNull()
        {
            return new ArgumentNullException("value");
        }

        /// <summary>
        /// Creates an <see cref="ArgumentNullException"/> with the provided properties and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentNullException ArgumentNull(string parameterName)
        {
            return new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentNullException"/> with the provided properties and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentNullException ArgumentNull(string parameterName, string messageFormat, params object[] messageArgs)
        {
            return new ArgumentNullException(parameterName, Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with a default message and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentException ArgumentNullOrEmpty(string parameterName)
        {
            return Error.Argument(parameterName, SRResources.ArgumentNullOrEmpty, parameterName);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentOutOfRangeException"/> with the provided properties and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentOutOfRangeException ArgumentOutOfRange(string parameterName, object actualValue, string messageFormat, params object[] messageArgs)
        {
            return new ArgumentOutOfRangeException(parameterName, actualValue, Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="ArgumentOutOfRangeException"/> with a message saying that the argument must be greater than or equal to <paramref name="minValue"/> and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <param name="minValue">The minimum size of the argument.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentOutOfRangeException ArgumentTooSmall(string parameterName, object actualValue, object minValue)
        {
            return new ArgumentOutOfRangeException(parameterName, actualValue, Error.Format(SRResources.ArgumentMustBeGreaterThanOrEqualTo, actualValue, minValue));
        }

        /// <summary>
        /// Creates an <see cref="ArgumentOutOfRangeException"/> with a message saying that the argument must be less than or equal to <paramref name="maxValue"/> and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <param name="maxValue">The maximum size of the argument.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ArgumentOutOfRangeException ArgumentTooLarge(string parameterName, object actualValue, object maxValue)
        {
            return new ArgumentOutOfRangeException(parameterName, actualValue, Error.Format(SRResources.ArgumentMustBeLessThanOrEqualTo, actualValue, maxValue));
        }

        /// <summary>
        /// Creates an <see cref="KeyNotFoundException"/> with a message saying that the key was not found and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static KeyNotFoundException KeyNotFound()
        {
            return new KeyNotFoundException();
        }

        /// <summary>
        /// Creates an <see cref="KeyNotFoundException"/> with a message saying that the key was not found and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static KeyNotFoundException KeyNotFound(string messageFormat, params object[] messageArgs)
        {
            return new KeyNotFoundException(Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="ObjectDisposedException"/> initialized according to guidelines and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static ObjectDisposedException ObjectDisposed(string messageFormat, params object[] messageArgs)
        {
            // Pass in null, not disposedObject.GetType().FullName as per the above guideline
            return new ObjectDisposedException(null, Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="OperationCanceledException"/> initialized with the provided parameters and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static OperationCanceledException OperationCanceled()
        {
            return new OperationCanceledException();
        }

        /// <summary>
        /// Creates an <see cref="OperationCanceledException"/> initialized with the provided parameters and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static OperationCanceledException OperationCanceled(string messageFormat, params object[] messageArgs)
        {
            return new OperationCanceledException(Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="InvalidEnumArgumentException"/> and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="invalidValue">The value of the argument that failed.</param>
        /// <param name="enumClass">A <see cref="Type"/> that represents the enumeration class with the valid values.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static InvalidEnumArgumentException InvalidEnumArgument(string parameterName, int invalidValue, Type enumClass)
        {
            return new InvalidEnumArgumentException(parameterName, invalidValue, enumClass);
        }

        /// <summary>
        /// Creates an <see cref="InvalidOperationException"/> and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static InvalidOperationException InvalidOperation(string messageFormat, params object[] messageArgs)
        {
            return new InvalidOperationException(Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="InvalidOperationException"/> and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static InvalidOperationException InvalidOperation(Exception innerException, string messageFormat, params object[] messageArgs)
        {
            return new InvalidOperationException(Error.Format(messageFormat, messageArgs), innerException);
        }

        /// <summary>
        /// Creates an <see cref="NotSupportedException"/> and logs it with <see cref="F:TraceLevel.Error"/>.
        /// </summary>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        public static NotSupportedException NotSupported(string messageFormat, params object[] messageArgs)
        {
            return new NotSupportedException(Error.Format(messageFormat, messageArgs));
        }
    }
}
