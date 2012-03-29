using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Net.Http.Formatting.Internal;

namespace System.Net.Http.Headers
{
    public class CookieState : ICloneable
    {
        private string _name;
        private NameValueCollection _values;

        public CookieState(string name)
            : this(name, String.Empty)
        {
        }

        public CookieState(string name, string value)
        {
            CheckNameFormat(name, "name");
            _name = name;

            CheckValueFormat(value, "value");
            Value = value;
        }

        public CookieState(string name, NameValueCollection values)
        {
            CheckNameFormat(name, "name");
            _name = name;

            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            Values.Add(values);
        }

        private CookieState(CookieState source)
        {
            Contract.Requires(source != null);

            _name = source._name;
            if (source._values != null)
            {
                Values.Add(source._values);
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                CheckNameFormat(value, "value");
                _name = value;
            }
        }

        /// <summary>
        /// If the cookie data is a simple string value then set or retrieve it using the <see cref="M:Value"/> property.
        /// If the cookie data is structured then use the <see cref="M:Values"/> property.
        /// </summary>
        public string Value
        {
            get
            {
                return Values.Count > 0 ? Values.AllKeys[0] : String.Empty;
            }

            set
            {
                CheckValueFormat(value, "value");
                if (Values.Count > 0)
                {
                    Values.AllKeys[0] = value;
                }
                else
                {
                    Values.Add(value, String.Empty);
                }
            }
        }

        /// <summary>
        /// If the cookie data is structured then use the <see cref="M:Values"/> property for setting and getting individual values.
        /// If the cookie data is a simple string value then set or retrieve it using the <see cref="M:Value"/> property.
        /// </summary>
        public NameValueCollection Values
        {
            get
            {
                if (_values == null)
                {
                    _values = HttpValueCollection.Create();
                }
                return _values;
            }
        }

        public string this[string name]
        {
            get { return Values[name]; }
            set { Values[name] = value; }
        }

        public override string ToString()
        {
            return _name + "=" + (_values != null ? _values.ToString() : String.Empty);
        }

        public object Clone()
        {
            return new CookieState(this);
        }

        private static void CheckNameFormat(string name, string parameterName)
        {
            if (!FormattingUtilities.ValidateHeaderToken(name))
            {
                throw new ArgumentException(Properties.Resources.CookieInvalidName, parameterName);
            }
        }

        private static void CheckValueFormat(string value, string parameterName)
        {
            // Empty string is a valid cookie value
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}