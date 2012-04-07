// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// Contains a value as well as an associated <see cref="MediaTypeFormatter"/> that will be
    /// used to serialize the value when writing this content.
    /// </summary>
    public class ObjectContent : HttpContent
    {
        private object _value;
        private readonly MediaTypeFormatter _formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectContent"/> class.
        /// </summary>
        /// <param name="type">The type of object this instance will contain.</param>
        /// <param name="value">The value of the object this instance will contain.</param>
        /// <param name="formatter">The formatter to use when serializing the value.</param>
        public ObjectContent(Type type, object value, MediaTypeFormatter formatter)
            : this(type, value, formatter, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectContent"/> class.
        /// </summary>
        /// <param name="type">The type of object this instance will contain.</param>
        /// <param name="value">The value of the object this instance will contain.</param>
        /// <param name="formatter">The formatter to use when serializing the value.</param>
        /// <param name="mediaType">The media type to associate with this object.</param>
        public ObjectContent(Type type, object value, MediaTypeFormatter formatter, string mediaType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }

            _formatter = formatter;
            ObjectType = type;

            VerifyAndSetObject(value);
            _formatter.SetDefaultContentHeaders(type, Headers, mediaType);
        }

        /// <summary>
        /// Gets the type of object managed by this <see cref="ObjectContent"/> instance.
        /// </summary>
        public Type ObjectType { get; private set; }

        /// <summary>
        /// The <see cref="MediaTypeFormatter">formatter</see> associated with this content instance.
        /// </summary>
        public MediaTypeFormatter Formatter
        {
            get { return _formatter; }
        }

        /// <summary>
        /// Gets or sets the value of the current <see cref="ObjectContent"/>.
        /// </summary>
        public object Value
        {
            get { return _value; }
            set { VerifyAndSetObject(value); }
        }

        /// <summary>
        /// Asynchronously serializes the object's content to the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="context">The associated <see cref="TransportContext"/>.</param>
        /// <returns>A <see cref="Task"/> instance that is asynchronously serializing the object's content.</returns>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _formatter.WriteToStreamAsync(ObjectType, Value, stream, Headers, context);
        }

        /// <summary>
        /// Computes the length of the stream if possible.
        /// </summary>
        /// <param name="length">The computed length of the stream.</param>
        /// <returns><c>true</c> if the length has been computed; otherwise <c>false</c>.</returns>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        private static bool IsTypeNullable(Type type)
        {
            return !type.IsValueType ||
                    (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        private void VerifyAndSetObject(object value)
        {
            Contract.Assert(ObjectType != null, "Type cannot be null");

            if (value == null)
            {
                // Null may not be assigned to value types (unless Nullable<T>)
                if (!IsTypeNullable(ObjectType))
                {
                    throw new InvalidOperationException(RS.Format(Properties.Resources.CannotUseNullValueType, typeof(ObjectContent).Name, ObjectType.Name));
                }
            }
            else
            {
                // Non-null objects must be a type assignable to Type
                Type objectType = value.GetType();
                if (!ObjectType.IsAssignableFrom(objectType))
                {
                    throw new ArgumentException(RS.Format(Properties.Resources.ObjectAndTypeDisagree, objectType.Name, ObjectType.Name), "value");
                }

                if (!_formatter.CanWriteType(objectType))
                {
                    throw new InvalidOperationException(RS.Format(Properties.Resources.ObjectContent_FormatterCannotWriteType, _formatter.GetType().FullName, objectType.Name));
                }
            }

            _value = value;
        }
    }

    /// <summary>
    /// Generic form of <see cref="ObjectContent"/>.
    /// </summary>
    /// <typeparam name="T">The type of object this <see cref="ObjectContent"/> class will contain.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Class contains generic forms")]
    public class ObjectContent<T> : ObjectContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectContent{T}"/> class.
        /// </summary>
        /// <param name="value">The value of the object this instance will contain.</param>
        /// <param name="formatter">The formatter to use when serializing the value.</param>
        public ObjectContent(T value, MediaTypeFormatter formatter)
            : this(value, formatter, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectContent{T}"/> class.
        /// </summary>
        /// <param name="value">The value of the object this instance will contain.</param>
        /// <param name="formatter">The formatter to use when serializing the value.</param>
        /// <param name="mediaType">The media type to associate with this object.</param>
        public ObjectContent(T value, MediaTypeFormatter formatter, string mediaType)
            : base(typeof(T), value, formatter, mediaType)
        {
        }
    }
}
