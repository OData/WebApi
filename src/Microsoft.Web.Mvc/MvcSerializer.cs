// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Web.Security;
using System.Xml;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    public class MvcSerializer
    {
        public static readonly SerializationMode DefaultSerializationMode = SerializationMode.Signed;

        // Magic number (randomly generated) used to identify the MvcSerializer serialized stream format.
        private static readonly byte[] _magicHeader = { 0x2c, 0xf8, 0x06, 0x23, 0x57, 0x73, 0x11, 0xba };

        private static bool ArrayContainsMagicHeader(byte[] array)
        {
            if (array == null || array.Length < _magicHeader.Length)
            {
                return false;
            }

            for (int i = 0; i < _magicHeader.Length; i++)
            {
                if (_magicHeader[i] != array[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static SerializationException CreateSerializationException(Exception innerException)
        {
            return new SerializationException(MvcResources.MvcSerializer_DeserializationFailed, innerException);
        }

        public virtual object Deserialize(string serializedValue, SerializationMode mode)
        {
            return Deserialize(serializedValue, mode, new MachineKeyWrapper());
        }

        internal static object Deserialize(string serializedValue, SerializationMode mode, IMachineKey machineKey)
        {
            if (String.IsNullOrEmpty(serializedValue))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "serializedValue");
            }

            MachineKeyProtection protectionMode = GetMachineKeyProtectionMode(mode);

            try
            {
                // First, need to decrypt / verify data
                byte[] rawBytes = machineKey.Decode(serializedValue, protectionMode);

                // Next, verify magic header
                if (!ArrayContainsMagicHeader(rawBytes))
                {
                    throw new SerializationException(MvcResources.MvcSerializer_MagicHeaderCheckFailed);
                }

                // Finally, deserialize the object graph
                using (MemoryStream ms = new MemoryStream(rawBytes, _magicHeader.Length, rawBytes.Length - _magicHeader.Length))
                {
                    return DeserializeGraph(ms);
                }
            }
            catch (Exception ex)
            {
                throw CreateSerializationException(ex);
            }
        }

        // Deserializes a stream to a graph using the NetDataContractSerializer (binary mode)
        private static object DeserializeGraph(Stream rawBytes)
        {
            using (XmlDictionaryReader dr = XmlDictionaryReader.CreateBinaryReader(rawBytes, XmlDictionaryReaderQuotas.Max))
            {
                object deserialized = new NetDataContractSerializer().ReadObject(dr);
                return deserialized;
            }
        }

        private static MachineKeyProtection GetMachineKeyProtectionMode(SerializationMode mode)
        {
            switch (mode)
            {
                case SerializationMode.Signed:
                    return MachineKeyProtection.Validation;

                case SerializationMode.EncryptedAndSigned:
                    return MachineKeyProtection.All;

                default:
                    // bad
                    throw new ArgumentOutOfRangeException("mode", MvcResources.MvcSerializer_InvalidSerializationMode);
            }
        }

        public virtual string Serialize(object state, SerializationMode mode)
        {
            return Serialize(state, mode, new MachineKeyWrapper());
        }

        internal static string Serialize(object state, SerializationMode mode, IMachineKey machineKey)
        {
            MachineKeyProtection protectionMode = GetMachineKeyProtectionMode(mode);

            try
            {
                // First, need to append the magic header and serialize the object graph
                byte[] rawBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(_magicHeader, 0, _magicHeader.Length);
                    SerializeGraph(ms, state);
                    rawBytes = ms.ToArray();
                }

                // Then, encrypt / sign data
                return machineKey.Encode(rawBytes, protectionMode);
            }
            catch (Exception ex)
            {
                throw CreateSerializationException(ex);
            }
        }

        // Serializes a graph to a byte array using the NetDataContractSerializer (binary mode)
        private static void SerializeGraph(Stream outputStream, object graph)
        {
            using (XmlDictionaryWriter dw = XmlDictionaryWriter.CreateBinaryWriter(outputStream, null, null, false /* ownsStream */))
            {
                new NetDataContractSerializer().WriteObject(dw, graph);
            }
        }
    }
}
