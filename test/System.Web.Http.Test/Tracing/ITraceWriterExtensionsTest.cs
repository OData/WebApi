// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing
{
    public class ITraceWriterExtensionsTest
    {
        [Fact]
        public void Debug_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Debug) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Debug(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Debug_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Debug) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Debug(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Debug_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Debug) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Debug(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Info_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Info(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Info_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Info(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Info_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Info(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Warn_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Warn) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Warn(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Warn_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Warn) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Warn(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Warn_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Warn) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Warn(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Error_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Error(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Error_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Error(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Error_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Error(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Fatal_With_Message_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Fatal) { Kind = TraceKind.Trace, Message = "The formatted message" },
            };

            // Act
            traceWriter.Fatal(request, "testCategory", "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Fatal_With_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Fatal) { Kind = TraceKind.Trace, Exception = exception },
            };

            // Act
            traceWriter.Fatal(request, "testCategory", exception);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Fatal_With_Message_And_Exception_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Fatal) { Kind = TraceKind.Trace, Message = "The formatted message", Exception = exception },
            };

            // Act
            traceWriter.Fatal(request, "testCategory", exception, "The {0} message", "formatted");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void TraceBeginEnd_Throws_With_Null_This()
        {
            // Arrange
            TestTraceWriter traceWriter = null;
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEnd(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: () => { },
                                             endTrace: null,
                                             errorTrace: null),
                                       "traceWriter");
        }

        [Fact]
        public void TraceBeginEnd_Throws_With_Null_Execute_Action()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEnd(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: null,
                                             endTrace: null,
                                             errorTrace: null),
                                       "execute");
        }

        [Fact]
        public void TraceBeginEnd_Accepts_Null_Trace_Actions()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            traceWriter.TraceBeginEnd(request,
                     "",
                     TraceLevel.Off,
                     "",
                     "",
                     beginTrace: null,
                     execute: () => { },
                     endTrace: null,
                     errorTrace: null);
        }

        [Fact]
        public void TraceBeginEnd_Invokes_BeginTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => { },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEnd_Invokes_Execute()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { invoked = true; },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }


        [Fact]
        public void TraceBeginEnd_Invokes_EndTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { },
                                 endTrace: (tr) => { invoked = true; },
                                 errorTrace: (tr) => { });

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEnd_Invokes_ErrorTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            Exception exception = new InvalidOperationException();
            bool invoked = false;

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(
                    () => traceWriter.TraceBeginEnd(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => { throw exception; },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; }));

            // Assert
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEnd_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "endMessage" },
            };

            // Act
            traceWriter.TraceBeginEnd(request,
                                 "testCategory",
                                 TraceLevel.Info,
                                 "tester",
                                 "testOp",
                                 beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                 execute: () => { },
                                 endTrace: (tr) => { tr.Message = "endMessage"; },
                                 errorTrace: (tr) => { tr.Message = "won't happen"; });

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void TraceBeginEnd_Traces_And_Throws_When_Execute_Throws()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException("test exception");
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Exception = exception, Message = "errorMessage" },
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(
                                () => traceWriter.TraceBeginEnd(request,
                                    "testCategory",
                                    TraceLevel.Info,
                                    "tester",
                                    "testOp",
                                    beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                    execute: () => { throw exception; },
                                    endTrace: (tr) => { tr.Message = "won't happen"; },
                                    errorTrace: (tr) => { tr.Message = "errorMessage"; }));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEndAsync_Throws_With_Null_This()
        {
            // Arrange
            TestTraceWriter traceWriter = null;
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEndAsync(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: () => TaskHelpers.Completed(),
                                             endTrace: null,
                                             errorTrace: null),
                                       "traceWriter");
        }

        [Fact]
        public void TraceBeginEndAsync_Throws_With_Null_Execute_Action()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEndAsync(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: null,
                                             endTrace: null,
                                             errorTrace: null),
                                       "execute");
        }

        [Fact]
        public void TraceBeginEndAsync_Accepts_Null_Trace_Actions()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Task t = traceWriter.TraceBeginEndAsync(request,
                     "",
                     TraceLevel.Off,
                     "",
                     "",
                     beginTrace: null,
                     execute: () => TaskHelpers.Completed(),
                     endTrace: null,
                     errorTrace: null);
            t.Wait();
        }

        [Fact]
        public void TraceBeginEndAsync_Invokes_BeginTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => TaskHelpers.Completed(),
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { }).Wait();

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEndAsync_Invokes_Execute()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () =>
                                              {
                                                  invoked = true;
                                                  return TaskHelpers.Completed();
                                              },
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { }).Wait();

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEndAsync_Invokes_EndTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => TaskHelpers.Completed(),
                                 endTrace: (tr) => { invoked = true; },
                                 errorTrace: (tr) => { }).Wait();

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEndAsync_Invokes_ErrorTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr) => { },
                                 errorTrace: (tr) => { invoked = true; }).Wait());

            // Assert
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }


        [Fact]
        public void TraceBeginEndAsync_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "endMessage" },
            };

            // Act
            traceWriter.TraceBeginEndAsync(request,
                                 "testCategory",
                                 TraceLevel.Info,
                                 "tester",
                                 "testOp",
                                 beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                 execute: () => TaskHelpers.Completed(),
                                 endTrace: (tr) => { tr.Message = "endMessage"; },
                                 errorTrace: (tr) => { tr.Message = "won't happen"; }).Wait();

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void TraceBeginEndAsync_Traces_When_Inner_Cancels()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Warn) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "errorMessage" },
            };

            // Act & Assert
            Assert.Throws<TaskCanceledException>(
                () => traceWriter.TraceBeginEndAsync(request,
                     "testCategory",
                     TraceLevel.Info,
                     "tester",
                     "testOp",
                     beginTrace: (tr) => { tr.Message = "beginMessage"; },
                     execute: () => TaskHelpers.Canceled(),
                     endTrace: (tr) => { tr.Message = "won't happen"; },
                     errorTrace: (tr) => { tr.Message = "errorMessage"; }).Wait());

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void TraceBeginAsync_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "errorMessage", Exception = exception },
            };

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);

            // Act & Assert
            InvalidOperationException thrown = Assert.Throws<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync(request,
                                     "testCategory",
                                     TraceLevel.Info,
                                     "tester",
                                     "testOp",
                                     beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                     execute: () => tcs.Task,
                                     endTrace: (tr) => { tr.Message = "won't happen"; },
                                     errorTrace: (tr) => { tr.Message = "errorMessage"; }).Wait());

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Throws_With_Null_This()
        {
            // Arrange
            TestTraceWriter traceWriter = null;
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEndAsync<int>(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: () => TaskHelpers.FromResult<int>(1),
                                             endTrace: null,
                                             errorTrace: null),
                                       "traceWriter");
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Throws_With_Null_Execute_Action()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => traceWriter.TraceBeginEndAsync<int>(request,
                                             "",
                                             TraceLevel.Off,
                                             "",
                                             "",
                                             beginTrace: null,
                                             execute: null,
                                             endTrace: null,
                                             errorTrace: null),
                                       "execute");
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Accepts_Null_Trace_Actions()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act & Assert
            Task t = traceWriter.TraceBeginEndAsync<int>(request,
                     "",
                     TraceLevel.Off,
                     "",
                     "",
                     beginTrace: null,
                     execute: () => TaskHelpers.FromResult<int>(1),
                     endTrace: null,
                     errorTrace: null);
            t.Wait();
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Invokes_BeginTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { invoked = true; },
                                 execute: () => TaskHelpers.FromResult<int>(1),
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { }).Wait();

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Invokes_Execute()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;

            // Act
            traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Fatal,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () =>
                                 {
                                     invoked = true;
                                     return TaskHelpers.FromResult<int>(1);
                                 },
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { }).Wait();

            // Assert
            Assert.True(invoked);
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Invokes_EndTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            int invokedValue = 0;

            // Act
            traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => TaskHelpers.FromResult<int>(1),
                                 endTrace: (tr, value) => { invoked = true; invokedValue = value; },
                                 errorTrace: (tr) => { }).Wait();

            // Assert
            Assert.True(invoked);
            Assert.Equal(1, invokedValue);
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Invokes_ErrorTrace()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            bool invoked = false;
            Exception exception = new InvalidOperationException();
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(0);
            tcs.TrySetException(exception);

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(
                () => traceWriter.TraceBeginEndAsync<int>(request,
                                 "",
                                 TraceLevel.Off,
                                 "",
                                 "",
                                 beginTrace: (tr) => { },
                                 execute: () => tcs.Task,
                                 endTrace: (tr, value) => { },
                                 errorTrace: (tr) => { invoked = true; }).Wait());

            // Assert
            Assert.True(invoked);
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Traces()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "endMessage1" },
            };

            // Act
            traceWriter.TraceBeginEndAsync<int>(request,
                                 "testCategory",
                                 TraceLevel.Info,
                                 "tester",
                                 "testOp",
                                 beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                 execute: () => TaskHelpers.FromResult<int>(1),
                                 endTrace: (tr, value) => { tr.Message = "endMessage" + value; },
                                 errorTrace: (tr) => { tr.Message = "won't happen"; }).Wait();

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void TraceBeginEndAsyncGeneric_Traces_When_Inner_Cancels()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Warn) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "errorMessage" },
            };

            // Act & Assert
            Assert.Throws<TaskCanceledException>(
                () => traceWriter.TraceBeginEndAsync<int>(request,
                     "testCategory",
                     TraceLevel.Info,
                     "tester",
                     "testOp",
                     beginTrace: (tr) => { tr.Message = "beginMessage"; },
                     execute: () => TaskHelpers.Canceled<int>(),
                     endTrace: (tr, value) => { tr.Message = "won't happen"; },
                     errorTrace: (tr) => { tr.Message = "errorMessage"; }).Wait());

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void TraceBeginAsyncGeneric_Traces_And_Faults_When_Inner_Faults()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpRequestMessage request = new HttpRequestMessage();
            InvalidOperationException exception = new InvalidOperationException();
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, "testCategory", TraceLevel.Info) { Kind = TraceKind.Begin, Operator = "tester", Operation = "testOp", Message = "beginMessage" },
                new TraceRecord(request, "testCategory", TraceLevel.Error) { Kind = TraceKind.End, Operator = "tester", Operation = "testOp", Message = "errorMessage", Exception = exception },
            };

            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(1);
            tcs.TrySetException(exception);

            // Act & Assert
            InvalidOperationException thrown = Assert.Throws<InvalidOperationException>(
                        () => traceWriter.TraceBeginEndAsync<int>(request,
                                     "testCategory",
                                     TraceLevel.Info,
                                     "tester",
                                     "testOp",
                                     beginTrace: (tr) => { tr.Message = "beginMessage"; },
                                     execute: () => tcs.Task,
                                     endTrace: (tr, value) => { tr.Message = "won't happen"; },
                                     errorTrace: (tr) => { tr.Message = "errorMessage"; }).Wait());

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
        }
    }
}
