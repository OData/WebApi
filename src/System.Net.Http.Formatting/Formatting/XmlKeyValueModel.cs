using System.Collections.Generic;
using System.Xml.Linq;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Provides an <see cref="IKeyValueModel"/> facade over an XML DOM. 
    /// </summary>
    internal class XmlKeyValueModel : IKeyValueModel
    {
        private readonly IDictionary<string, string> _keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // This implementation will eagerly read the entire DOM and populate the dictionary. 
        public XmlKeyValueModel(XElement root)
        {
            if (root.HasElements)
            {
                _keys[String.Empty] = String.Empty;
                ExpandValues(root, prefix: null);
            }
            else
            {
                // if this is the only element in the document, we can also bind it to any prefix.
                _keys[String.Empty] = root.Value;
            }
        }

        /// <summary>
        /// Gets all the keys for all the values.
        /// </summary>
        /// <returns>The set of all keys.</returns>
        public IEnumerable<string> Keys
        {
            get { return _keys.Keys; }
        }

        public bool TryGetValue(string key, out object value)
        {
            string result;
            bool found = _keys.TryGetValue(key, out result);
            value = result;
            return found;
        }

        private void ExpandValues(XElement node, string prefix)
        {
            foreach (var child in node.Elements())
            {
                string name = child.Name.LocalName;
                string key = (prefix == null) ? name : prefix + "." + name;

                if (child.HasElements)
                {
                    ExpandValues(child, name);
                }
                else
                {
                    _keys[key] = child.Value;
                }
            }
        }
    }
}
