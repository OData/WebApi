using System.Linq;
using System.Xml.Linq;

namespace WebStack.QA.Common
{
    public static class XElementExtensions
    {
        #region Public Methods and Operators

        public static XElement EnsureAttribute(this XElement element, string attributeName, string attributeValue)
        {
            XAttribute result = element.Attributes(attributeName).FirstOrDefault();

            if (result == null)
            {
                result = new XAttribute(attributeName, attributeValue);
                element.Add(result);
            }
            else
            {
                result.Value = attributeValue;
            }

            return element;
        }

        public static XElement EnsureElement(this XElement element, string elementName)
        {
            XElement result = element.Element(elementName);

            if (result == null)
            {
                result = new XElement(elementName);
                element.Add(result);
            }

            return result;
        }

        public static XElement EnsureElementWithAttribute(
            this XElement element, string elementName, string attributeName, string attributeValue)
        {
            XElement result = element.Elements(elementName).Where(
                e =>
                {
                    var attr = e.Attribute(attributeName);
                    return attr != null && attr.Value == attributeValue;
                }).FirstOrDefault();

            if (result == null)
            {
                result = new XElement(elementName);
                result.Add(new XAttribute(attributeName, attributeValue));
                element.Add(result);
            }

            return result;
        }

        public static XElement EnsureElementWithKeyValueAttribute(
            this XElement element, string elementName, string keyValue, string valueValue)
        {
            XElement result = EnsureElementWithAttribute(element, elementName, "key", keyValue);

            result = EnsureAttribute(result, "value", valueValue);

            return result;
        }

        #endregion
    }
}