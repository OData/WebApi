// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json.Linq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Dispatcher
{
    public class HttpErrorTest
    {
        [Fact]
        public void Constructor_GuardClauses()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpError(message: null),
                "message");
            Assert.ThrowsArgumentNull(
                () => new HttpError(exception: null),
                "exception");
            Assert.ThrowsArgumentNull(
                () => new HttpError(modelState: null),
                "modelState");
        }

        [Fact]
        public void StringConstructor_AddsCorrectDictionaryItems()
        {
            HttpError error = new HttpError("something bad happened");

            Assert.Contains(new KeyValuePair<string, object>("Message", "something bad happened"), error);
        }

        [Fact]
        public void ExceptionConstructor_AddsCorrectDictionaryItems()
        {
            HttpError error = new HttpError(new ArgumentException("error", new Exception()));

            Assert.Contains(new KeyValuePair<string, object>("Message", "An exception has occurred."), error);
            Assert.Contains(new KeyValuePair<string, object>("ExceptionMessage", "error"), error);
            Assert.Contains(new KeyValuePair<string, object>("ExceptionType", "System.ArgumentException"), error);
            Assert.True(error.ContainsKey("StackTrace"));
            Assert.True(error.ContainsKey("InnerException"));
        }

        [Fact]
        public void ModelStateConstructor_AddsCorrectDictionaryItems()
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("[0].Name", "error1");
            modelState.AddModelError("[0].Name", "error2");
            modelState.AddModelError("[0].Address", "error");
            modelState.AddModelError("[2].Name", new Exception("exception"));

            HttpError error = new HttpError(modelState);

            Assert.Contains(new KeyValuePair<string, object>("Message", "The model state is invalid."), error);
            Assert.Contains(new KeyValuePair<string, object>("[0].Name", "error1, error2"), error);
            Assert.Contains(new KeyValuePair<string, object>("[0].Address", "error"), error);
            Assert.True(error.ContainsKey("[2].Name"));
        }

        [Fact]
        public void HttpError_Roundtrips_WithJsonFormatter()
        {
            HttpError error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            MemoryStream stream = new MemoryStream();

            formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null).Wait();
            stream.Position = 0;
            HttpError roundtrippedError = formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null).Result as HttpError;

            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal(42L, roundtrippedError["ErrorCode"]);
            JArray data = roundtrippedError["Data"] as JArray;
            Assert.Equal(3, data.Count);
            Assert.Contains("a", data);
            Assert.Contains("b", data);
            Assert.Contains("c", data);
        }

        [Fact]
        public void HttpError_Roundtrips_WithXmlFormatter()
        {
            HttpError error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            MemoryStream stream = new MemoryStream();

            formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null).Wait();
            stream.Position = 0;
            HttpError roundtrippedError = formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null).Result as HttpError;

            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal("42", roundtrippedError["ErrorCode"]);
            Assert.Equal("a b c", roundtrippedError["Data"]);
        }

        [Fact]
        public void HttpError_Roundtrips_WithXmlSerializer()
        {
            HttpError error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter() { UseXmlSerializer = true };
            MemoryStream stream = new MemoryStream();

            formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null).Wait();
            stream.Position = 0;
            HttpError roundtrippedError = formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null).Result as HttpError;

            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal("42", roundtrippedError["ErrorCode"]);
            Assert.Equal("a b c", roundtrippedError["Data"]);
        }
    }
}
