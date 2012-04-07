// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Threading.Tasks
{
    public class TaskHelpersTest
    {
        // -----------------------------------------------------------------
        //  TaskHelpers.Canceled

        [Fact]
        public void Canceled_ReturnsCanceledTask()
        {
            Task result = TaskHelpers.Canceled();

            Assert.NotNull(result);
            Assert.True(result.IsCanceled);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.Canceled<T>

        [Fact]
        public void Canceled_Generic_ReturnsCanceledTask()
        {
            Task<string> result = TaskHelpers.Canceled<string>();

            Assert.NotNull(result);
            Assert.True(result.IsCanceled);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.Completed

        [Fact]
        public void Completed_ReturnsCompletedTask()
        {
            Task result = TaskHelpers.Completed();

            Assert.NotNull(result);
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromError

        [Fact]
        public void FromError_ReturnsFaultedTaskWithGivenException()
        {
            var exception = new Exception();

            Task result = TaskHelpers.FromError(exception);

            Assert.NotNull(result);
            Assert.True(result.IsFaulted);
            Assert.Same(exception, result.Exception.InnerException);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromError<T>

        [Fact]
        public void FromError_Generic_ReturnsFaultedTaskWithGivenException()
        {
            var exception = new Exception();

            Task<string> result = TaskHelpers.FromError<string>(exception);

            Assert.NotNull(result);
            Assert.True(result.IsFaulted);
            Assert.Same(exception, result.Exception.InnerException);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromErrors

        [Fact]
        public void FromErrors_ReturnsFaultedTaskWithGivenExceptions()
        {
            var exceptions = new[] { new Exception(), new InvalidOperationException() };

            Task result = TaskHelpers.FromErrors(exceptions);

            Assert.NotNull(result);
            Assert.True(result.IsFaulted);
            Assert.Equal(exceptions, result.Exception.InnerExceptions.ToArray());
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromErrors<T>

        [Fact]
        public void FromErrors_Generic_ReturnsFaultedTaskWithGivenExceptions()
        {
            var exceptions = new[] { new Exception(), new InvalidOperationException() };

            Task<string> result = TaskHelpers.FromErrors<string>(exceptions);

            Assert.NotNull(result);
            Assert.True(result.IsFaulted);
            Assert.Equal(exceptions, result.Exception.InnerExceptions.ToArray());
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromResult<T>

        [Fact]
        public void FromResult_ReturnsCompletedTaskWithGivenResult()
        {
            string s = "ABC";

            Task<string> result = TaskHelpers.FromResult(s);

            Assert.NotNull(result);
            Assert.True(result.Status == TaskStatus.RanToCompletion);
            Assert.Same(s, result.Result);
        }

        // -----------------------------------------------------------------
        //  Task TaskHelpers.Iterate(IEnumerable<Task>)

        [Fact]
        public void Iterate_NonGeneric_IfProvidedEnumerationContainsNullValue_ReturnsFaultedTaskWithNullReferenceException()
        {
            List<string> log = new List<string>();

            var result = TaskHelpers.Iterate(NullTaskEnumerable(log));

            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.IsType<NullReferenceException>(result.Exception.GetBaseException());
        }

        private static IEnumerable<Task> NullTaskEnumerable(List<string> log)
        {
            log.Add("first");
            yield return null;
            log.Add("second");
        }

        [Fact]
        public void Iterate_NonGeneric_IfProvidedEnumerationThrowsException_ReturnsFaultedTask()
        {
            List<string> log = new List<string>();
            Exception exception = new Exception();

            var result = TaskHelpers.Iterate(ThrowingTaskEnumerable(exception, log));

            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.Same(exception, result.Exception.InnerException);
            Assert.Equal(new[] { "first" }, log.ToArray());
        }

        private static IEnumerable<Task> ThrowingTaskEnumerable(Exception e, List<string> log)
        {
            log.Add("first");
            bool a = true; // work around unreachable code warning
            if (a) throw e;
            log.Add("second");
            yield return null;
        }

        [Fact]
        public void Iterate_NonGeneric_IfProvidedEnumerableExecutesCancellingTask_ReturnsCanceledTaskAndHaltsEnumeration()
        {
            List<string> log = new List<string>();

            var result = TaskHelpers.Iterate(CanceledTaskEnumerable(log));

            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Canceled, result.Status);
            Assert.Equal(new[] { "first" }, log.ToArray());
        }

        private static IEnumerable<Task> CanceledTaskEnumerable(List<string> log)
        {
            log.Add("first");
            yield return TaskHelpers.Canceled();
            log.Add("second");
        }

        [Fact]
        public void Iterate_NonGeneric_IfProvidedEnumerableExecutesFaultingTask_ReturnsCanceledTaskAndHaltsEnumeration()
        {
            List<string> log = new List<string>();
            Exception exception = new Exception();

            var result = TaskHelpers.Iterate(FaultedTaskEnumerable(exception, log));

            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.Same(exception, result.Exception.InnerException);
            Assert.Equal(new[] { "first" }, log.ToArray());
        }

        private static IEnumerable<Task> FaultedTaskEnumerable(Exception e, List<string> log)
        {
            log.Add("first");
            yield return TaskHelpers.FromError(e);
            log.Add("second");
        }

        [Fact]
        public void Iterate_NonGeneric_ExecutesNextTaskOnlyAfterPreviousTaskSucceeded()
        {
            List<string> log = new List<string>();

            var result = TaskHelpers.Iterate(SuccessTaskEnumerable(log));

            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
            Assert.Equal(
                new[] { "first", "Executing first task. Log size: 1", "second", "Executing second task. Log size: 3" },
                log.ToArray());
        }

        private static IEnumerable<Task> SuccessTaskEnumerable(List<string> log)
        {
            log.Add("first");
            yield return Task.Factory.StartNew(() => log.Add("Executing first task. Log size: " + log.Count));
            log.Add("second");
            yield return Task.Factory.StartNew(() => log.Add("Executing second task. Log size: " + log.Count));
        }

        [Fact]
        public void Iterate_NonGeneric_TasksRunSequentiallyRegardlessOfExecutionTime()
        {
            List<string> log = new List<string>();

            Task task = TaskHelpers.Iterate(TasksWithVaryingDelays(log, 100, 1, 50, 2));

            task.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(new[] { "ENTER: 100", "EXIT: 100", "ENTER: 1", "EXIT: 1", "ENTER: 50", "EXIT: 50", "ENTER: 2", "EXIT: 2" }, log);
        }

        private static IEnumerable<Task> TasksWithVaryingDelays(List<string> log, params int[] delays)
        {
            foreach (int delay in delays)
                yield return Task.Factory.StartNew(timeToSleep =>
                {
                    log.Add("ENTER: " + timeToSleep);
                    Thread.Sleep((int)timeToSleep);
                    log.Add("EXIT: " + timeToSleep);
                }, delay);
        }

        [Fact]
        public void Iterate_NonGeneric_StopsTaskIterationIfCancellationWasRequested()
        {
            List<string> log = new List<string>();
            CancellationTokenSource cts = new CancellationTokenSource();

            var result = TaskHelpers.Iterate(CancelingTaskEnumerable(log, cts), cts.Token);

            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Canceled, result.Status);
            Assert.Equal(
               new[] { "first", "Executing first task. Log size: 1" },
               log.ToArray());
        }

        private static IEnumerable<Task> CancelingTaskEnumerable(List<string> log, CancellationTokenSource cts)
        {
            log.Add("first");
            yield return Task.Factory.StartNew(() =>
            {
                log.Add("Executing first task. Log size: " + log.Count);
                cts.Cancel();
            });
            log.Add("second");
            yield return Task.Factory.StartNew(() =>
            {
                log.Add("Executing second task. Log size: " + log.Count);
            });
        }

        [Fact, PreserveSyncContext]
        public Task Iterate_NonGeneric_IteratorRunsInSynchronizationContext()
        {
            ThreadPoolSyncContext sc = new ThreadPoolSyncContext();
            SynchronizationContext.SetSynchronizationContext(sc);

            return TaskHelpers.Iterate(SyncContextVerifyingEnumerable(sc)).Then(() =>
            {
                Assert.Same(sc, SynchronizationContext.Current);
            });
        }

        private static IEnumerable<Task> SyncContextVerifyingEnumerable(SynchronizationContext sc)
        {
            for (int i = 0; i < 10; i++)
            {
                Assert.Same(sc, SynchronizationContext.Current);
                yield return TaskHelpers.Completed();
            }
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.TrySetFromTask<T>

        [Fact]
        public void TrySetFromTask_IfSourceTaskIsCanceled_CancelsTaskCompletionSource()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task canceledTask = TaskHelpers.Canceled<object>();

            tcs.TrySetFromTask(canceledTask);

            Assert.Equal(TaskStatus.Canceled, tcs.Task.Status);
        }

        [Fact]
        public void TrySetFromTask_IfSourceTaskIsFaulted_FaultsTaskCompletionSource()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Exception exception = new Exception();
            Task faultedTask = TaskHelpers.FromError<object>(exception);

            tcs.TrySetFromTask(faultedTask);

            Assert.Equal(TaskStatus.Faulted, tcs.Task.Status);
            Assert.Same(exception, tcs.Task.Exception.InnerException);
        }

        [Fact]
        public void TrySetFromTask_IfSourceTaskIsSuccessfulAndOfSameResultType_SucceedsTaskCompletionSourceAndSetsResult()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task<string> successfulTask = TaskHelpers.FromResult("abc");

            tcs.TrySetFromTask(successfulTask);

            Assert.Equal(TaskStatus.RanToCompletion, tcs.Task.Status);
            Assert.Equal("abc", tcs.Task.Result);
        }

        [Fact]
        public void TrySetFromTask_IfSourceTaskIsSuccessfulAndOfDifferentResultType_SucceedsTaskCompletionSourceAndSetsDefaultValueAsResult()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task<object> successfulTask = TaskHelpers.FromResult(new object());

            tcs.TrySetFromTask(successfulTask);

            Assert.Equal(TaskStatus.RanToCompletion, tcs.Task.Status);
            Assert.Equal(null, tcs.Task.Result);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.RunSynchronously

        [Fact]
        public void RunSynchronously_Executes_Action()
        {
            bool wasRun = false;
            Task t = TaskHelpers.RunSynchronously(() => { wasRun = true; });
            t.WaitUntilCompleted();
            Assert.True(wasRun);
        }

        [Fact]
        public void RunSynchronously_Captures_Exception_In_AggregateException()
        {
            Task t = TaskHelpers.RunSynchronously(() => { throw new InvalidOperationException(); });
            Assert.Throws<InvalidOperationException>(() => t.Wait());
        }

        [Fact]
        public void RunSynchronously_Cancels()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            Task t = TaskHelpers.RunSynchronously(() => { throw new InvalidOperationException(); }, cts.Token);
            Assert.Throws<TaskCanceledException>(() => t.Wait());
        }
    }
}
