// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;

namespace System.Web.WebPages.Instrumentation
{
    public class InstrumentationService
    {
        private static readonly bool _isAvailable = HttpContextAdapter.IsInstrumentationAvailable;

        private bool _localIsAvailable = _isAvailable && PageInstrumentationServiceAdapter.IsEnabled;

        public InstrumentationService()
        {
            ExtractInstrumentationService = GetInstrumentationService;
            CreateContext = CreateSystemWebContext;
        }

        public bool IsAvailable
        {
            get { return _localIsAvailable; }
            internal set { _localIsAvailable = value; }
        }

        internal Func<HttpContextBase, PageInstrumentationServiceAdapter> ExtractInstrumentationService { get; set; }
        internal Func<string, TextWriter, int, int, bool, PageExecutionContextAdapter> CreateContext { get; set; }

        public void BeginContext(HttpContextBase context, string virtualPath, TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            RunOnListeners(context,
                           listener => listener.BeginContext(CreateContext(
                               virtualPath,
                               writer,
                               startPosition,
                               length,
                               isLiteral)));
        }

        public void EndContext(HttpContextBase context, string virtualPath, TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            RunOnListeners(context,
                           listener => listener.EndContext(CreateContext(
                               virtualPath,
                               writer,
                               startPosition,
                               length,
                               isLiteral)));
        }

        private PageExecutionContextAdapter CreateSystemWebContext(string virtualPath, TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            return new PageExecutionContextAdapter()
            {
                VirtualPath = virtualPath,
                TextWriter = writer,
                StartPosition = startPosition,
                Length = length,
                IsLiteral = isLiteral
            };
        }

        private PageInstrumentationServiceAdapter GetInstrumentationService(HttpContextBase context)
        {
            HttpContextAdapter ctx = new HttpContextAdapter(context);
            return ctx.PageInstrumentation;
        }

        private void RunOnListeners(HttpContextBase context, Action<PageExecutionListenerAdapter> act)
        {
            if (IsAvailable)
            {
                PageInstrumentationServiceAdapter instSvc = ExtractInstrumentationService(context);
                if (instSvc != null)
                {
                    foreach (PageExecutionListenerAdapter listener in instSvc.ExecutionListeners)
                    {
                        act(listener);
                    }
                }
            }
        }
    }
}
