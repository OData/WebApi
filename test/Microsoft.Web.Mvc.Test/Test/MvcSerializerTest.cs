// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using System.Web.Security;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Mvc.Test
{
    public class MvcSerializerTest
    {
        [Fact]
        public void DeserializeThrowsIfModeIsOutOfRange()
        {
            // Arrange
            MvcSerializer serializer = new MvcSerializer();

            // Act & assert            
            Assert.ThrowsArgumentOutOfRange(
                delegate { serializer.Serialize("blah", (SerializationMode)(-1)); },
                "mode",
                @"The provided SerializationMode is invalid.");
        }

        [Fact]
        public void DeserializeThrowsIfSerializedValueIsCorrupt()
        {
            // Arrange
            IMachineKey machineKey = new MockMachineKey();

            // Act & assert
            Exception exception = Assert.Throws<SerializationException>(
                delegate { MvcSerializer.Deserialize("This is a corrupted value.", SerializationMode.Signed, machineKey); },
                @"Deserialization failed. Verify that the data is being deserialized using the same SerializationMode with which it was serialized. Otherwise see the inner exception.");

            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void DeserializeThrowsIfSerializedValueIsEmpty()
        {
            // Arrange
            MvcSerializer serializer = new MvcSerializer();

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { serializer.Deserialize("", SerializationMode.Signed); }, "serializedValue");
        }

        [Fact]
        public void DeserializeThrowsIfSerializedValueIsNull()
        {
            // Arrange
            MvcSerializer serializer = new MvcSerializer();

            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { serializer.Deserialize(null, SerializationMode.Signed); }, "serializedValue");
        }

        [Fact]
        public void SerializeAllowsNullValues()
        {
            // Arrange
            IMachineKey machineKey = new MockMachineKey();

            // Act
            string serializedValue = MvcSerializer.Serialize(null, SerializationMode.EncryptedAndSigned, machineKey);

            // Assert
            Assert.Equal(@"All-LPgGI1dzEbp3B2FueVR5cGUuA25pbIYJAXozaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS8yMDAzLzEwL1NlcmlhbGl6YXRpb24vCQFpKWh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlAQ==", serializedValue);
        }

        [Fact]
        public void SerializeAndDeserializeRoundTripsValue()
        {
            // Arrange
            IMachineKey machineKey = new MockMachineKey();

            // Act
            string serializedValue = MvcSerializer.Serialize(42, SerializationMode.EncryptedAndSigned, machineKey);
            object deserializedValue = MvcSerializer.Deserialize(serializedValue, SerializationMode.EncryptedAndSigned, machineKey);

            // Assert
            Assert.Equal(42, deserializedValue);
        }

        [Fact]
        public void SerializeThrowsIfModeIsOutOfRange()
        {
            // Arrange
            MvcSerializer serializer = new MvcSerializer();

            // Act & assert
            Assert.ThrowsArgumentOutOfRange(
                delegate { serializer.Serialize(null, (SerializationMode)(-1)); },
                "mode",
                @"The provided SerializationMode is invalid.");
        }

        private sealed class MockMachineKey : IMachineKey
        {
            public byte[] Decode(string encodedData, MachineKeyProtection protectionOption)
            {
                string optionString = protectionOption.ToString();
                if (encodedData.StartsWith(optionString, StringComparison.Ordinal))
                {
                    encodedData = encodedData.Substring(optionString.Length + 1);
                }
                else
                {
                    throw new Exception("Corrupted data.");
                }
                return Convert.FromBase64String(encodedData);
            }

            public string Encode(byte[] data, MachineKeyProtection protectionOption)
            {
                return protectionOption.ToString() + "-" + Convert.ToBase64String(data);
            }
        }
    }
}
