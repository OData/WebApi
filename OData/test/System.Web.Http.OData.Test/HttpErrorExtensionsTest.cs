// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.ModelBinding;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class HttpErrorExtensionsTest
    {
        [Fact]
        public void CreateODataError_CopiesAllErrorProperties()
        {
            var error = new HttpError();
            error["Message"] = "error";
            error["MessageLanguage"] = "language";
            error["ErrorCode"] = "42";
            error["ExceptionMessage"] = "exception";
            error["ExceptionType"] = "System.ReallyBadException";
            error["StackTrace"] = "stacktrace";

            ODataError oDataError = error.CreateODataError();

            Assert.Equal("error", oDataError.Message);
            Assert.Equal("language", oDataError.MessageLanguage);
            Assert.Equal("42", oDataError.ErrorCode);
            Assert.Equal("exception", oDataError.InnerError.Message);
            Assert.Equal("System.ReallyBadException", oDataError.InnerError.TypeName);
            Assert.Equal("stacktrace", oDataError.InnerError.StackTrace);
            Assert.Null(oDataError.InnerError.InnerError);
        }

        [Fact]
        public void CreateODataError_CopiesInnerExceptionInformation()
        {
            Exception innerException = new ArgumentException("innerException");
            Exception exception = new InvalidOperationException("exception", innerException);
            var error = new HttpError(exception, true);

            ODataError oDataError = error.CreateODataError();

            Assert.Equal("An error has occurred.", oDataError.Message);
            Assert.Equal("exception", oDataError.InnerError.Message);
            Assert.Equal("System.InvalidOperationException", oDataError.InnerError.TypeName);
            Assert.Equal("innerException", oDataError.InnerError.InnerError.Message);
            Assert.Equal("System.ArgumentException", oDataError.InnerError.InnerError.TypeName);
        }

        [Fact]
        public void CreateODataError_CopiesMessageDetailToInnerError()
        {
            var error = new HttpError();
            error["Message"] = "error";
            error["MessageDetail"] = "messagedetail";

            ODataError oDataError = error.CreateODataError();

            Assert.Equal("error", oDataError.Message);
            Assert.Equal("messagedetail", oDataError.InnerError.Message);
            Assert.Null(oDataError.InnerError.InnerError);
        }

        [Fact]
        public void CreateODataError_CopiesModelStateErrorsToInnerError()
        {
            ModelStateDictionary dict = new ModelStateDictionary();
            string errorMessage1 = "Object reference not set to an instance of an object.";
            string errorMessage2 = "Some ModelState error";
            string parameter1Name = "parameter1";
            string parameter2Name = "parameter2";

            dict.AddModelError(parameter1Name, new InvalidOperationException(errorMessage1));
            dict.AddModelError(parameter2Name, errorMessage2);
            var error = new HttpError(dict, includeErrorDetail: true);

            ODataError oDataError = error.CreateODataError();

            Assert.Equal(
                parameter1Name + " : " + errorMessage1 + Environment.NewLine +
                parameter2Name + " : " + errorMessage2 + Environment.NewLine,
                oDataError.InnerError.Message);
        }
    }
}
