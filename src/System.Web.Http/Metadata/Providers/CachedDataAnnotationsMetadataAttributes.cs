using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;

namespace System.Web.Http.Metadata.Providers
{
    // REVIEW: No access to HiddenInputAttribute
    public class CachedDataAnnotationsMetadataAttributes
    {
        public CachedDataAnnotationsMetadataAttributes(Attribute[] attributes)
        {
            CacheAttributes(attributes);
        }

        // [SecuritySafeCritical] because it uses DataAnnotations type DataTypeAttribute
        public DataTypeAttribute DataType { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayAttribute
        public DisplayAttribute Display { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayColumnAttribute
        public DisplayColumnAttribute DisplayColumn { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayFormatAttribute
        public DisplayFormatAttribute DisplayFormat { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type DisplayNameAttribute
        public DisplayNameAttribute DisplayName { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type EditableAttribute
        public EditableAttribute Editable { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }
#if false
        public HiddenInputAttribute HiddenInput { get; protected set; }
#endif
        public ReadOnlyAttribute ReadOnly { get; protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type RequiredAttribute
        public RequiredAttribute Required { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type
        public ScaffoldColumnAttribute ScaffoldColumn { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses DataAnnotations type ScaffoldColumnAttribute
        public UIHintAttribute UIHint { [SecuritySafeCritical] get; [SecuritySafeCritical] protected set; }

        // [SecuritySafeCritical] because it uses several DataAnnotations attribute types
        [SecuritySafeCritical]
        private void CacheAttributes(Attribute[] attributes)
        {
            DataType = attributes.OfType<DataTypeAttribute>().FirstOrDefault();
            Display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            DisplayColumn = attributes.OfType<DisplayColumnAttribute>().FirstOrDefault();
            DisplayFormat = attributes.OfType<DisplayFormatAttribute>().FirstOrDefault();
            DisplayName = attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
            Editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
#if false
            HiddenInput = attributes.OfType<HiddenInputAttribute>().FirstOrDefault();
#endif
            ReadOnly = attributes.OfType<ReadOnlyAttribute>().FirstOrDefault();
            Required = attributes.OfType<RequiredAttribute>().FirstOrDefault();
            ScaffoldColumn = attributes.OfType<ScaffoldColumnAttribute>().FirstOrDefault();

            var uiHintAttributes = attributes.OfType<UIHintAttribute>();

            // Developer note: this loop is explicitly unrolled because Linq lambdas methods are not
            // [SecuritySafeCritical] and generate security exceptions accessing DataAnnotations types.
            UIHintAttribute bestUIHint = null;
            foreach (UIHintAttribute uiHintAttribute in uiHintAttributes)
            {
                string presentationLayer = uiHintAttribute.PresentationLayer;
                if (String.Equals(presentationLayer, "MVC", StringComparison.OrdinalIgnoreCase))
                {
                    bestUIHint = uiHintAttribute;
                    break;
                }

                if (bestUIHint == null && String.IsNullOrEmpty(presentationLayer))
                {
                    bestUIHint = uiHintAttribute;
                }
            }

            UIHint = bestUIHint;

            if (DisplayFormat == null && DataType != null)
            {
                DisplayFormat = DataType.DisplayFormat;
            }
        }
    }
}
