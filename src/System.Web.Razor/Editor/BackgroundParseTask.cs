// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Razor.Text;

namespace System.Web.Razor.Editor
{
    internal class BackgroundParseTask : IDisposable
    {
        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private GeneratorResults _results;

        [SuppressMessage("Microsoft.WebAPI", "CR4002:DoNotConstructTaskInstances", Justification = "This rule is not applicable to this assembly.")]
        private BackgroundParseTask(RazorTemplateEngine engine, string sourceFileName, TextChange change)
        {
            Change = change;
            Engine = engine;
            SourceFileName = sourceFileName;
            InnerTask = new Task(() => Run(_cancelSource.Token), _cancelSource.Token);
        }

        public Task InnerTask { get; private set; }
        public TextChange Change { get; private set; }
        public RazorTemplateEngine Engine { get; private set; }
        public string SourceFileName { get; private set; }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "It is the caller's responsibility to dispose this object, which will dispose all members of this object")]
        public static BackgroundParseTask StartNew(RazorTemplateEngine engine, string sourceFileName, TextChange change)
        {
            BackgroundParseTask task = new BackgroundParseTask(engine, sourceFileName, change);
            task.Start();
            return task;
        }

        public void Cancel()
        {
            _cancelSource.Cancel();
        }

        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "This rule is not applicable to this assembly.")]
        public void Start()
        {
            InnerTask.Start();
        }

        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "This rule is not applicable to this assembly.")]
        public BackgroundParseTask ContinueWith(Action<GeneratorResults, BackgroundParseTask> continuation)
        {
            InnerTask.ContinueWith(t => RunContinuation(t, continuation));
            return this;
        }

        private void RunContinuation(Task completed, Action<GeneratorResults, BackgroundParseTask> continuation)
        {
            if (!completed.IsCanceled)
            {
                continuation(_results, this);
            }
        }

        internal virtual void Run(CancellationToken cancelToken)
        {
            if (!cancelToken.IsCancellationRequested)
            {
                // Seek the buffer to the beginning
                Change.NewBuffer.Position = 0;

                try
                {
                    _results = Engine.GenerateCode(Change.NewBuffer, className: null, rootNamespace: null, sourceFileName: SourceFileName, cancelToken: cancelToken);
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == cancelToken)
                    {
                        // We've been cancelled, so just return.
                        return;
                    }
                    else
                    {
                        // Exception was thrown for some other reason...
                        throw;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        [SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "This rule is not applicable to this assembly.")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // "If you start having to do strange gyrations in order to Dispose (or in the case of Tasks, 
                // use additional synchronization to ensure it's safe to dispose, since Dispose may only be 
                // used once a task has completed), it's likely better to rely on finalization to take care of things.
                //  - Stephen Toub [http://social.msdn.microsoft.com/Forums/en/parallelextensions/thread/7b3a42e5-4ebf-405a-8ee6-bcd2f0214f85]
                // So, dispose the task if we can
                if (InnerTask.IsCanceled || InnerTask.IsCompleted || InnerTask.IsFaulted)
                {
                    InnerTask.Dispose();
                }
                // But if we can't, the finalizer will do it
                InnerTask = null;
                if (_cancelSource != null)
                {
                    _cancelSource.Dispose();
                }
            }
        }
    }
}
