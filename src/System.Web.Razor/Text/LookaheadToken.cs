namespace System.Web.Razor.Text
{
    public class LookaheadToken : IDisposable
    {
        private Action _cancelAction;
        private bool _accepted;

        public LookaheadToken(Action cancelAction)
        {
            _cancelAction = cancelAction;
        }

        public void Accept()
        {
            _accepted = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_accepted)
            {
                _cancelAction();
            }
        }
    }
}
