// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpErrorsExtensionTest
    {
        [Fact]
        public void ToODataError_CopiesAllErrorProperties()
        {
            var error = new HttpError();
            error["Message"] = "error";
            error["MessageLanguage"] = "language";
            error["ErrorCode"] = "42";
            error["ExceptionMessage"] = "exception";
            error["ExceptionType"] = "System.ReallyBadException";
            error["StackTrace"] = "stacktrace";

            ODataError oDataError = error.ToODataError();

            Assert.Equal("error", oDataError.Message);
            Assert.Equal("language", oDataError.MessageLanguage);
            Assert.Equal("42", oDataError.ErrorCode);
            Assert.Equal("exception", oDataError.InnerError.Message);
            Assert.Equal("System.ReallyBadException", oDataError.InnerError.TypeName);
            Assert.Equal("stacktrace", oDataError.InnerError.StackTrace);
            Assert.Null(oDataError.InnerError.InnerError);
        }

        [Fact]
        public void ToODataError_CopiesInnerExceptionInformation()
        {
            Exception innerException = new ArgumentException("innerException");
            Exception exception = new InvalidOperationException("exception", innerException);
            var error = new HttpError(exception, true);

            ODataError oDataError = error.ToODataError();

            Assert.Equal("An error has occurred.", oDataError.Message);
            Assert.Equal("exception", oDataError.InnerError.Message);
            Assert.Equal("System.InvalidOperationException", oDataError.InnerError.TypeName);
            Assert.Equal("innerException", oDataError.InnerError.InnerError.Message);
            Assert.Equal("System.ArgumentException", oDataError.InnerError.InnerError.TypeName);
        }
    }
}
