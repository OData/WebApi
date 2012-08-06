using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;

namespace System.Web.Razor.Editor
{
    internal class BackgroundParser : IDisposable
    {
        private MainThreadState _main;
        private BackgroundThread _bg;

        public bool IsIdle
        {
            get { return _main.IsIdle; }
        }

        /// <summary>
        /// Fired on the main thread.
        /// </summary>
        public event EventHandler<DocumentParseCompleteEventArgs> ResultsReady;

        public BackgroundParser(RazorEngineHost host, string fileName)
        {
            _main = new MainThreadState(fileName);
            _bg = new BackgroundThread(_main, host, fileName);

            _main.ResultsReady += (sender, args) => OnResultsReady(args);
        }

        public void Start()
        {
            _bg.Start();
        }

        public void Cancel()
        {
            _main.Cancel();
        }

        public void QueueChange(TextChange change)
        {
            _main.QueueChange(change);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _main.Cancel();
            }
        }

        protected virtual void OnResultsReady(DocumentParseCompleteEventArgs args)
        {
            var handler = ResultsReady;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        internal static bool TreesAreDifferent(Block leftTree, Block rightTree, IEnumerable<TextChange> changes)
        {
            return TreesAreDifferent(leftTree, rightTree, changes, CancellationToken.None);
        }

        internal static bool TreesAreDifferent(Block leftTree, Block rightTree, IEnumerable<TextChange> changes, CancellationToken cancelToken)
        {
            // Apply all the pending changes to the original tree
            // PERF: If this becomes a bottleneck, we can probably do it the other way around,
            //  i.e. visit the tree and find applicable changes for each node.
            foreach (TextChange change in changes)
            {
                cancelToken.ThrowIfCancellationRequested();
                Span changeOwner = leftTree.LocateOwner(change);

                // Apply the change to the tree
                if (changeOwner == null)
                {
                    return true;
                }
                EditResult result = changeOwner.EditHandler.ApplyChange(changeOwner, change, force: true);
                changeOwner.ReplaceWith(result.EditedSpan);
            }

            // Now compare the trees
            bool treesDifferent = !leftTree.EquivalentTo(rightTree);
            return treesDifferent;
        }

        private abstract class ThreadStateBase
        {
#if DEBUG
            private int _id = -1;
#endif
            protected ThreadStateBase()
            {
            }

            [Conditional("DEBUG")]
            protected void SetThreadId(int id)
            {
#if DEBUG
                _id = id;
#endif
            }

            [Conditional("DEBUG")]
            protected void EnsureOnThread()
            {
#if DEBUG
                Debug.Assert(_id != -1, "SetThreadId was never called!");
                Debug.Assert(Thread.CurrentThread.ManagedThreadId == _id, "Called from an unexpected thread!");
#endif
            }

            [Conditional("DEBUG")]
            protected void EnsureNotOnThread()
            {
#if DEBUG
                Debug.Assert(_id != -1, "SetThreadId was never called!");
                Debug.Assert(Thread.CurrentThread.ManagedThreadId != _id, "Called from an unexpected thread!");
#endif
            }
        }

        private class MainThreadState : ThreadStateBase, IDisposable
        {
            private CancellationTokenSource _cancelSource = new CancellationTokenSource();
            private ManualResetEventSlim _hasParcel = new ManualResetEventSlim(false);
            private CancellationTokenSource _currentParcelCancelSource;
            private string _fileName;
            
            private object _stateLock = new object();
            private IList<TextChange> _changes = new List<TextChange>();

            public CancellationToken CancelToken { get { return _cancelSource.Token; } }

            public event EventHandler<DocumentParseCompleteEventArgs> ResultsReady;

            public bool IsIdle
            {
                get {
                    lock (_stateLock)
                    {
                        return _currentParcelCancelSource == null;
                    }
                }
            }

            public MainThreadState(string fileName)
            {
                _fileName = fileName;

                SetThreadId(Thread.CurrentThread.ManagedThreadId);
            }

            public void Cancel()
            {
                EnsureOnThread();
                _cancelSource.Cancel();
            }

            public void QueueChange(TextChange change)
            {
                RazorEditorTrace.TraceLine("[M][{0}] Queuing Parse for: {1}", Path.GetFileName(_fileName), change);
                EnsureOnThread();
                lock (_stateLock)
                {
                    // CurrentParcel token source is not null ==> There's a parse underway
                    if (_currentParcelCancelSource != null)
                    {
                        _currentParcelCancelSource.Cancel();
                    }

                    _changes.Add(change);
                    _hasParcel.Set();
                }
            }

            public WorkParcel GetParcel()
            {
                EnsureNotOnThread(); // Only the background thread can get a parcel
                _hasParcel.Wait(_cancelSource.Token);
                _hasParcel.Reset();
                lock (_stateLock)
                {
                    // Create a cancellation source for this parcel
                    _currentParcelCancelSource = new CancellationTokenSource();

                    var changes = _changes;
                    _changes = new List<TextChange>();
                    return new WorkParcel(changes, _currentParcelCancelSource.Token);
                }
            }

            public void ReturnParcel(DocumentParseCompleteEventArgs args)
            {
                lock (_stateLock)
                {
                    // Clear the current parcel cancellation source
                    if (_currentParcelCancelSource != null)
                    {
                        _currentParcelCancelSource.Dispose();
                        _currentParcelCancelSource = null;
                    }

                    // If there are things waiting to be parsed, just don't fire the event because we're already out of date
                    if (_changes.Any())
                    {
                        return;
                    }
                }
                var handler = ResultsReady;
                if (handler != null)
                {
                    handler(this, args);
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _cancelSource.Dispose();
                    _hasParcel.Dispose();
                }
            }
        }

        private class BackgroundThread : ThreadStateBase
        {
            private MainThreadState _main;
            private Thread _bgThread;
            private CancellationToken _shutdownToken;
            private RazorEngineHost _host;
            private string _fileName;
            private Block _currentParseTree;

            public BackgroundThread(MainThreadState main, RazorEngineHost host, string fileName)
            {
                // Run on MAIN thread!
                _main = main;
                _bgThread = new Thread(WorkerLoop);
                _shutdownToken = _main.CancelToken;
                _host = host;
                _fileName = fileName;

                SetThreadId(_bgThread.ManagedThreadId);
            }

            // **** ANY THREAD ****
            public void Start()
            {
                _bgThread.Start();
            }

            // **** BACKGROUND THREAD ****
            private void WorkerLoop()
            {
                long? elapsedMs = null;
#if EDITOR_TRACING
                Stopwatch sw = new Stopwatch();
#endif

                try
                {
                    RazorEditorTrace.TraceLine("[BG][{0}] Startup", Path.GetFileName(_fileName));
                    EnsureOnThread();
                    while (!_shutdownToken.IsCancellationRequested)
                    {
                        // Grab the parcel of work to do
                        WorkParcel parcel = _main.GetParcel();
                        if (parcel.Changes.Any())
                        {
                            RazorEditorTrace.TraceLine("[BG][{0}] {1} changes arrived", Path.GetFileName(_fileName), parcel.Changes.Count);
                            try
                            {
                                DocumentParseCompleteEventArgs args = null;
                                using (var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(_shutdownToken, parcel.CancelToken))
                                {
                                    if (parcel != null && !linkedCancel.IsCancellationRequested)
                                    {
#if EDITOR_TRACING
                                        sw.Start();
#endif
                                        GeneratorResults results = ParseChange(parcel.Buffer, linkedCancel.Token);
#if EDITOR_TRACING
                                        sw.Stop();
                                        elapsedMs = sw.ElapsedMilliseconds;
                                        sw.Reset();
#endif
                                        RazorEditorTrace.TraceLine(
                                            "[BG][{0}] Parse Complete in {1}ms",
                                            Path.GetFileName(_fileName),
                                            elapsedMs.HasValue ? elapsedMs.Value.ToString() : "?");

                                        if (results != null && !linkedCancel.IsCancellationRequested)
                                        {
                                            // Take the current tree and check for differences
#if EDITOR_TRACING
                                            sw.Start();
#endif
                                            bool treeStructureChanged = _currentParseTree == null || TreesAreDifferent(_currentParseTree, results.Document, parcel.Changes, parcel.CancelToken);
#if EDITOR_TRACING
                                            sw.Stop();
                                            elapsedMs = sw.ElapsedMilliseconds;
                                            sw.Reset();
#endif
                                            _currentParseTree = results.Document;
                                            RazorEditorTrace.TraceLine("[BG][{0}] Trees Compared in {1}ms. Different = {2}",
                                                Path.GetFileName(_fileName),
                                                elapsedMs.HasValue ? elapsedMs.Value.ToString() : "?",
                                                treeStructureChanged);

                                            // Build Arguments
                                            args = new DocumentParseCompleteEventArgs()
                                            {
                                                GeneratorResults = results,
                                                SourceChange = parcel.LastChange,
                                                TreeStructureChanged = treeStructureChanged
                                            };
                                        }
                                    }

#if EDITOR_TRACING
                                    if (args != null)
                                    {
                                        // Rewind the buffer and sanity check the line mappings
                                        parcel.Buffer.Position = 0;
                                        int lineCount = parcel.Buffer.ReadToEnd().Split(new string[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None).Count();
                                        Debug.Assert(
                                            !args.GeneratorResults.DesignTimeLineMappings.Any(pair => pair.Value.StartLine > lineCount),
                                            "Found a design-time line mapping referring to a line outside the source file!");
                                        Debug.Assert(
                                            !args.GeneratorResults.Document.Flatten().Any(span => span.Start.LineIndex > lineCount),
                                            "Found a span with a line number outside the source file");
                                        Debug.Assert(
                                            !args.GeneratorResults.Document.Flatten().Any(span => span.Start.AbsoluteIndex > parcel.Buffer.Length),
                                            "Found a span with an absolute offset outside the source file");
                                    }
#endif
                                }
                                if (args != null)
                                {
                                    _main.ReturnParcel(args);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                            }
                        }
                        else
                        {
                            RazorEditorTrace.TraceLine("[BG][{0}] no changes arrived?", Path.GetFileName(_fileName), parcel.Changes.Count);
                            Thread.Yield();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just shut down.
                }
                finally
                {
                    RazorEditorTrace.TraceLine("[BG][{0}] Shutdown", Path.GetFileName(_fileName));

                    // Clean up main thread resources
                    _main.Dispose();
                }
            }

            private GeneratorResults ParseChange(ITextBuffer buffer, CancellationToken token)
            {
                EnsureOnThread();

                // Create a template engine
                RazorTemplateEngine engine = new RazorTemplateEngine(_host);

                // Seek the buffer to the beginning
                buffer.Position = 0;

                try
                {
                    return engine.GenerateCode(
                        input: buffer,
                        className: null,
                        rootNamespace: null,
                        sourceFileName: _fileName,
                        cancelToken: token);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }
        }

        private class WorkParcel
        {
            public CancellationToken CancelToken { get; private set; }

            public ITextBuffer Buffer
            {
                get
                {
                    var change = LastChange;
                    if (change != null)
                    {
                        return change.NewBuffer;
                    }
                    return null;
                }
            }
            public TextChange LastChange
            {
                get { return Changes.LastOrDefault(); }
            }
            public IList<TextChange> Changes { get; private set; }

            public WorkParcel(IList<TextChange> changes, CancellationToken cancelToken)
            {
                Changes = changes;
                CancelToken = cancelToken;
            }
        }
    }
}
