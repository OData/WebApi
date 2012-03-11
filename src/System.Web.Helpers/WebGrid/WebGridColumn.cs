namespace System.Web.Helpers
{
    public class WebGridColumn
    {
        public bool CanSort { get; set; }

        public string ColumnName { get; set; }

        public Func<dynamic, object> Format { get; set; }

        public string Header { get; set; }

        public string Style { get; set; }
    }
}
