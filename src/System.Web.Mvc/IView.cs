using System.IO;

namespace System.Web.Mvc
{
    public interface IView
    {
        void Render(ViewContext viewContext, TextWriter writer);
    }
}
