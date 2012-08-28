// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Linq;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing <see cref="IEdmPrimitiveType" />'s.
    /// </summary>
    internal class ODataPrimitiveSerializer : ODataEntrySerializer
    {
        public ODataPrimitiveSerializer(IEdmPrimitiveTypeReference edmPrimitiveType)
            : base(edmPrimitiveType, ODataPayloadKind.Property)
        {
        }

        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerWriteContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            messageWriter.WriteProperty(
                CreateProperty(graph, writeContext.ResponseContext.ServiceOperationName, writeContext));
        }

        public override ODataProperty CreateProperty(object graph, string elementName, ODataSerializerWriteContext writeContext)
        {
            if (String.IsNullOrWhiteSpace(elementName))
            {
                throw Error.ArgumentNullOrEmpty("elementName");
            }

            graph = ConvertUnsupportedPrimitives(graph);

            // TODO: Bug 467598: validate the type of the object being passed in here with the underlying primitive type. 
            return new ODataProperty() { Value = graph, Name = elementName };
        }

        internal static object ConvertUnsupportedPrimitives(object value)
        {
            if (value != null)
            {
                Type type = value.GetType();

                // Note that type cannot be a nullable type as value is not null and it is boxed.
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                        return new String((char)value, 1);

                    case TypeCode.UInt16:
                        return (int)(ushort)value;

                    case TypeCode.UInt32:
                        return (long)(uint)value;

                    case TypeCode.UInt64:
                        return checked((long)(ulong)value);

                    default:
                        if (type == typeof(char[]))
                        {
                            return new String(value as char[]);
                        }
                        else if (type == typeof(XElement))
                        {
                            return ((XElement)value).ToString();
                        }
                        else if (type == typeof(Binary))
                        {
                            return ((Binary)value).ToArray();
                        }
                        break;
                }
            }

            return value;
        }
    }
}
