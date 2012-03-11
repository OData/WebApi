using System.ComponentModel;
using System.Web.UI;

namespace System.Web.Mvc
{
    [ControlBuilder(typeof(ViewTypeControlBuilder))]
    [NonVisualControl]
    public class ViewType : Control
    {
        private string _typeName;

        [DefaultValue("")]
        public string TypeName
        {
            get { return _typeName ?? String.Empty; }
            set { _typeName = value; }
        }
    }
}
