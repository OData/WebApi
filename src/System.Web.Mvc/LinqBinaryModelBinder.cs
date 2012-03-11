using System.Data.Linq;

namespace System.Web.Mvc
{
    public class LinqBinaryModelBinder : ByteArrayModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            byte[] byteValue = (byte[])base.BindModel(controllerContext, bindingContext);
            if (byteValue == null)
            {
                return null;
            }

            return new Binary(byteValue);
        }
    }
}
