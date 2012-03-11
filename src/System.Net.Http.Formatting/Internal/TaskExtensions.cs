using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http.Internal
{
    // TODO, DevDiv 336175, This is copied from System.Web.Http.Common, remove this copy once the issue is addressed.
    internal static class TaskExtensions
    {
        private static Task<AsyncVoid> _defaultCompleted = TaskHelpers.FromResult<AsyncVoid>(default(AsyncVoid));
        private static readonly Action<Task> _rethrowWithNoStackLossDelegate = GetRethrowWithNoStackLossDelegate();

        /// <summary>
        /// Calls the given continuation, after the given task completes, if it ends in a faulted state.
        /// Will not be called if the task did not fault (meaning, it will not be called if the task ran
        /// to completion or was canceled). Intended to roughly emulate C# 5's support for "try/catch" in
        /// async methods. Note that this method allows you to return a Task, so that you can either return
        /// a completed Task (indicating that you swallowed the exception) or a faulted task (indicating that
        /// that the exception should be propagated). In C#, you cannot normally use await within a catch
        /// block, so returning a real async task should never be done from Catch().
        /// </summary>
        internal static Task Catch(this Task task, Func<Exception, Task> continuation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.CatchImpl(ex => continuation(ex).ToTask<AsyncVoid>(), cancellationToken);
        }

        /// <summary>
        /// Calls the given continuation, after the given task completes, if it ends in a faulted state.
        /// Will not be called if the task did not fault (meaning, it will not be called if the task ran
        /// to completion or was canceled). Intended to roughly emulate C# 5's support for "try/catch" in
        /// async methods. Note that this method allows you to return a Task, so that you can either return
        /// a completed Task (indicating that you swallowed the exception) or a faulted task (indicating that
        /// that the exception should be propagated). In C#, you cannot normally use await within a catch
        /// block, so returning a real async task should never be done from Catch().
        /// </summary>
        internal static Task<TResult> Catch<TResult>(this Task<TResult> task, Func<Exception, Task<TResult>> continuation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.CatchImpl(continuation, cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        private static Task<TResult> CatchImpl<TResult>(this Task task, Func<Exception, Task<TResult>> continuation, CancellationToken cancellationToken)
        {
            // Stay on the same thread if we can
            if (task.IsCanceled || cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled<TResult>();
            }
            if (task.IsFaulted)
            {
                try
                {
                    Task<TResult> resultTask = continuation(task.Exception.GetBaseException());
                    if (resultTask == null)
                    {
                        throw new InvalidOperationException(System.Net.Http.Properties.Resources.TaskExtensions_Catch_CannotReturnNull);
                    }

                    return resultTask;
                }
                catch (Exception ex)
                {
                    return TaskHelpers.FromError<TResult>(ex);
                }
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                tcs.TrySetFromTask(task);
                return tcs.Task;
            }

            SynchronizationContext syncContext = SynchronizationContext.Current;

            return task.ContinueWith(innerTask =>
            {
                TaskCompletionSource<Task<TResult>> tcs = new TaskCompletionSource<Task<TResult>>();

                if (innerTask.IsFaulted)
                {
                    if (syncContext != null)
                    {
                        syncContext.Post(state =>
                        {
                            try
                            {
                                Task<TResult> resultTask = continuation(innerTask.Exception.GetBaseException());
                                if (resultTask == null)
                                {
                                    throw new InvalidOperationException(System.Net.Http.Properties.Resources.TaskExtensions_Catch_CannotReturnNull);
                                }

                                tcs.TrySetResult(resultTask);
                            }
                            catch (Exception ex)
                            {
                                tcs.TrySetException(ex);
                            }
                        }, state: null);
                    }
                    else
                    {
                        Task<TResult> resultTask = continuation(innerTask.Exception.GetBaseException());
                        if (resultTask == null)
                        {
                            throw new InvalidOperationException(System.Net.Http.Properties.Resources.TaskExtensions_Catch_CannotReturnNull);
                        }

                        tcs.TrySetResult(resultTask);
                    }
                }
                else
                {
                    tcs.TrySetFromTask(innerTask);
                }

                return tcs.Task.FastUnwrap();
            }, cancellationToken).FastUnwrap();
        }

        /// <summary>
        /// Upon completion of the task, copies its result into the given task completion source, regardless of the
        /// completion state. This causes the original task to be fully observed, and the task that is returned by
        /// this method will always successfully run to completion, regardless of the original task state.
        /// Since this method consumes a task with no return value, you must provide the return value to be used
        /// when the inner task ran to successful completion.
        /// </summary>
        internal static Task CopyResultToCompletionSource<TResult>(this Task task, TaskCompletionSource<TResult> tcs, TResult completionResult)
        {
            return task.CopyResultToCompletionSourceImpl(tcs, innerTask => completionResult);
        }

        /// <summary>
        /// Upon completion of the task, copies its result into the given task completion source, regardless of the
        /// completion state. This causes the original task to be fully observed, and the task that is returned by
        /// this method will always successfully run to completion, regardless of the original task state.
        /// </summary>
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        internal static Task CopyResultToCompletionSource<TResult>(this Task<TResult> task, TaskCompletionSource<TResult> tcs)
        {
            return task.CopyResultToCompletionSourceImpl(tcs, innerTask => innerTask.Result);
        }

        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        private static Task CopyResultToCompletionSourceImpl<TTask, TResult>(this TTask task, TaskCompletionSource<TResult> tcs, Func<TTask, TResult> resultThunk)
            where TTask : Task
        {
            if (task.IsCompleted)
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        TaskHelpers.TrySetFromTask(tcs, task);
                        break;

                    case TaskStatus.RanToCompletion:
                        tcs.TrySetResult(resultThunk(task));
                        break;
                }

                return TaskHelpers.Completed();
            }

            return task.ContinueWith(innerTask =>
            {
                switch (innerTask.Status)
                {
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        TaskHelpers.TrySetFromTask(tcs, innerTask);
                        break;

                    case TaskStatus.RanToCompletion:
                        tcs.TrySetResult(resultThunk(task));
                        break;
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// A version of task.Unwrap that is optimized to prevent unnecessarily capturing the
        /// execution context when the antecedent task is already completed.
        /// </summary>
        [SuppressMessage("Microsoft.WebAPI", "CR4000:DoNotUseProblematicTaskTypes", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        internal static Task FastUnwrap(this Task<Task> task)
        {
            Task innerTask = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
            return innerTask ?? task.Unwrap();
        }

        /// <summary>
        /// A version of task.Unwrap that is optimized to prevent unnecessarily capturing the
        /// execution context when the antecedent task is already completed.
        /// </summary>
        [SuppressMessage("Microsoft.WebAPI", "CR4000:DoNotUseProblematicTaskTypes", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        internal static Task<TResult> FastUnwrap<TResult>(this Task<Task<TResult>> task)
        {
            Task<TResult> innerTask = task.Status == TaskStatus.RanToCompletion ? task.Result : null;
            return innerTask ?? task.Unwrap();
        }

        /// <summary>
        /// Calls the given continuation, after the given task has completed, regardless of the state
        /// the task ended in. Intended to roughly emulate C# 5's support for "finally" in async methods.
        /// </summary>
        internal static Task Finally(this Task task, Action continuation)
        {
            return task.FinallyImpl<AsyncVoid>(continuation);
        }

        /// <summary>
        /// Calls the given continuation, after the given task has completed, regardless of the state
        /// the task ended in. Intended to roughly emulate C# 5's support for "finally" in async methods.
        /// </summary>
        internal static Task<TResult> Finally<TResult>(this Task<TResult> task, Action continuation)
        {
            return task.FinallyImpl<TResult>(continuation);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        private static Task<TResult> FinallyImpl<TResult>(this Task task, Action continuation)
        {
            // Stay on the same thread if we can
            if (task.IsCompleted)
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                try
                {
                    continuation();
                    tcs.TrySetFromTask(task);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                return tcs.Task;
            }

            SynchronizationContext syncContext = SynchronizationContext.Current;

            return task.ContinueWith(innerTask =>
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                if (syncContext != null)
                {
                    syncContext.Post(state =>
                    {
                        try
                        {
                            continuation();
                            tcs.TrySetFromTask(innerTask);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    }, state: null);
                }
                else
                {
                    continuation();
                    tcs.TrySetFromTask(innerTask);
                }

                return tcs.Task;
            }).FastUnwrap();
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "This general exception is not intended to be seen by the user")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This general exception is not intended to be seen by the user")]
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        private static Action<Task> GetRethrowWithNoStackLossDelegate()
        {
            MethodInfo getAwaiterMethod = typeof(Task).GetMethod("GetAwaiter", Type.EmptyTypes);
            if (getAwaiterMethod != null)
            {
                // .NET 4.5 - dump the same code the 'await' keyword would have dumped
                // >> task.GetAwaiter().GetResult()
                // No-ops if the task completed successfully, else throws the originating exception complete with the correct call stack.
                var taskParameter = Expression.Parameter(typeof(Task));
                var getAwaiterCall = Expression.Call(taskParameter, getAwaiterMethod);
                var getResultCall = Expression.Call(getAwaiterCall, "GetResult", Type.EmptyTypes);
                var lambda = Expression.Lambda<Action<Task>>(getResultCall, taskParameter);
                return lambda.Compile();
            }
            else
            {
                Func<Exception, Exception> prepForRemoting = null;

                try
                {
                    if (AppDomain.CurrentDomain.IsFullyTrusted)
                    {
                        // .NET 4 - do the same thing Lazy<T> does by calling Exception.PrepForRemoting
                        // This is an internal method in mscorlib.dll, so pass a test Exception to it to make sure we can call it.
                        var exceptionParameter = Expression.Parameter(typeof(Exception));
                        var prepForRemotingCall = Expression.Call(exceptionParameter, "PrepForRemoting", Type.EmptyTypes);
                        var lambda = Expression.Lambda<Func<Exception, Exception>>(prepForRemotingCall, exceptionParameter);
                        var func = lambda.Compile();
                        func(new Exception()); // make sure the method call succeeds before assigning the 'prepForRemoting' local variable
                        prepForRemoting = func;
                    }
                }
                catch
                {
                } // If delegate creation fails (medium trust) we will simply throw the base exception.

                return task =>
                {
                    try
                    {
                        task.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        Exception baseException = ex.GetBaseException();
                        if (prepForRemoting != null)
                        {
                            baseException = prepForRemoting(baseException);
                        }
                        throw baseException;
                    }
                };
            }
        }

        /// <summary>
        /// Calls the given continuation, after the given task has completed, if the task successfully ran
        /// to completion (i.e., was not cancelled and did not fault).
        /// </summary>
        internal static Task Then(this Task task, Action continuation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.ThenImpl(t => ToAsyncVoidTask(continuation), cancellationToken);
        }

        /// <summary>
        /// Calls the given continuation, after the given task has completed, if the task successfully ran
        /// to completion (i.e., was not cancelled and did not fault).
        /// </summary>
        internal static Task<TOuterResult> Then<TOuterResult>(this Task task, Func<TOuterResult> continuation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.ThenImpl(t => TaskHelpers.FromResult(continuation()), cancellationToken);
        }

        /// <summary>
        /// Calls the given continuation, after the given task has completed, if the task successfully ran
        /// to completion (i.e., was not cancelled and did not fault).
        /// </summary>
        internal static Task<TOuterResult> Then<TOuterResult>(this Task task, Func<Task<TOuterResult>> continuation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.ThenImpl(t => continuation(), cancellationToken);
        }

        /// <summary>
        /// Calls the given continuation, after the given task has completed, if the task successfully ran
        /// to completion (i.e., was not cancelled and did not fault). The continuation is provided with the
        /// result of the task as its sole parameter.
        /// </summary>
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        internal static Task Then<TInnerResult>(this Task<TInnerResult> task, Action<TInnerResult> continuation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.ThenImpl(t => ToAsyncVoidTask(() => continuation(t.Result)), cancellationToken);
        }

        /// <summary>
        /// Calls the given continuation, after the given task has completed, if the task successfully ran
        /// to completion (i.e., was not cancelled and did not fault). The continuation is provided with the
        /// result of the task as its sole parameter.
        /// </summary>
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        internal static Task<TOuterResult> Then<TInnerResult, TOuterResult>(this Task<TInnerResult> task, Func<TInnerResult, TOuterResult> continuation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.ThenImpl(t => TaskHelpers.FromResult(continuation(t.Result)), cancellationToken);
        }

        /// <summary>
        /// Calls the given continuation, after the given task has completed, if the task successfully ran
        /// to completion (i.e., was not cancelled and did not fault). The continuation is provided with the
        /// result of the task as its sole parameter.
        /// </summary>
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        internal static Task<TOuterResult> Then<TInnerResult, TOuterResult>(this Task<TInnerResult> task, Func<TInnerResult, Task<TOuterResult>> continuation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.ThenImpl(t => continuation(t.Result), cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        private static Task<TOuterResult> ThenImpl<TTask, TOuterResult>(this TTask task, Func<TTask, Task<TOuterResult>> continuation, CancellationToken cancellationToken)
            where TTask : Task
        {
            // Stay on the same thread if we can
            if (task.IsCanceled || cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled<TOuterResult>();
            }
            if (task.IsFaulted)
            {
                return TaskHelpers.FromErrors<TOuterResult>(task.Exception.InnerExceptions);
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                try
                {
                    return continuation(task);
                }
                catch (Exception ex)
                {
                    return TaskHelpers.FromError<TOuterResult>(ex);
                }
            }

            SynchronizationContext syncContext = SynchronizationContext.Current;

            return task.ContinueWith(innerTask =>
            {
                if (innerTask.IsFaulted)
                {
                    return TaskHelpers.FromErrors<TOuterResult>(innerTask.Exception.InnerExceptions);
                }
                if (innerTask.IsCanceled)
                {
                    return TaskHelpers.Canceled<TOuterResult>();
                }

                TaskCompletionSource<Task<TOuterResult>> tcs = new TaskCompletionSource<Task<TOuterResult>>();
                if (syncContext != null)
                {
                    syncContext.Post(state =>
                    {
                        try
                        {
                            tcs.TrySetResult(continuation(task));
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    }, state: null);
                }
                else
                {
                    tcs.TrySetResult(continuation(task));
                }
                return tcs.Task.FastUnwrap();
            }, cancellationToken).FastUnwrap();
        }

        /// <summary>
        /// Throws the first faulting exception for a task which is faulted. It attempts to preserve the original
        /// stack trace when throwing the exception (which should always work in 4.5, and should also work in 4.0
        /// when running in full trust). Note: It is the caller's responsibility not to pass incomplete tasks to
        /// this method, because it does degenerate into a call to the equivalent of .Wait() on the task when it
        /// hasn't yet completed.
        /// </summary>
        internal static void ThrowIfFaulted(this Task task)
        {
            _rethrowWithNoStackLossDelegate(task);
        }

        /// <summary>
        /// Adapts any action into a Task (returning AsyncVoid, so that it's usable with Task{T} extension methods).
        /// </summary>
        private static Task<AsyncVoid> ToAsyncVoidTask(Action action)
        {
            return TaskHelpers.RunSynchronously<AsyncVoid>(() =>
            {
                action();
                return _defaultCompleted;
            });
        }

        /// <summary>
        /// Changes the return value of a task to the given result, if the task ends in the RanToCompletion state.
        /// This potentially imposes an extra ContinueWith to convert a non-completed task, so use this with caution.
        /// </summary>
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        internal static Task<TResult> ToTask<TResult>(this Task task, CancellationToken cancellationToken = default(CancellationToken), TResult result = default(TResult))
        {
            if (task == null)
            {
                return null;
            }

            // Stay on the same thread if we can
            if (task.IsCanceled || cancellationToken.IsCancellationRequested)
            {
                return TaskHelpers.Canceled<TResult>();
            }
            if (task.IsFaulted)
            {
                return TaskHelpers.FromErrors<TResult>(task.Exception.InnerExceptions);
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                return TaskHelpers.FromResult(result);
            }

            return task.ContinueWith(innerTask =>
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    tcs.TrySetResult(result);
                }
                else
                {
                    tcs.TrySetFromTask(innerTask);
                }

                return tcs.Task;
            }, TaskContinuationOptions.ExecuteSynchronously).FastUnwrap();
        }

        /// <summary>
        /// Attempts to get the result value for the given task. If the task ran to completion, then
        /// it will return true and set the result value; otherwise, it will return false.
        /// </summary>
        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
        internal static bool TryGetResult<TResult>(this Task<TResult> task, out TResult result)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                result = task.Result;
                return true;
            }

            result = default(TResult);
            return false;
        }

        /// <summary>
        /// Used as the T in a "conversion" of a Task into a Task{T}.
        /// </summary>
        private struct AsyncVoid
        {
        }
    }
}
