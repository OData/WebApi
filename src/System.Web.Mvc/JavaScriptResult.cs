namespace System.Web.Mvc
{
    public class JavaScriptResult : ActionResult
    {
        public string Script { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = "application/x-javascript";

            if (Script != null)
            {
                response.Write(Script);
            }
        }
    }
}
