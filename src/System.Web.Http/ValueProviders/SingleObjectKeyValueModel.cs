using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http.Common;

namespace System.Web.Http.ValueProviders
{
    /// <summary>
    /// This internal class exposes a single named value via a <see cref="IKeyValueModel"/> facade.
    /// It exists primarily to expose the request body content as a single value.
    /// </summary>
    internal class SingleObjectKeyValueModel : IKeyValueModel
    {
        private string _name;
        private object _value;

        public SingleObjectKeyValueModel(string name, object value)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }

            _name = name;
            _value = value;
        }

        public IEnumerable<string> Keys
        {
            get { return new string[] { _name }; }
        }

        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw Error.ArgumentNull("key");
            }

            if (String.Equals(key, _name, StringComparison.OrdinalIgnoreCase))
            {
                value = _value;
                return true;
            }

            value = null;
            return false;
        }
    }
}
