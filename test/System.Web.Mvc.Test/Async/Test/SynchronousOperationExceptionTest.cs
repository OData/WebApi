// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Async.Test
{
    public class SynchronousOperationExceptionTest
    {
        [Fact]
        public void ConstructorWithMessageAndInnerExceptionParameter()
        {
            // Arrange
            Exception innerException = new Exception();

            // Act
            SynchronousOperationException ex = new SynchronousOperationException("the message", innerException);

            // Assert
            Assert.Equal("the message", ex.Message);
            Assert.Equal(innerException, ex.InnerException);
        }

        [Fact]
        public void ConstructorWithMessageParameter()
        {
            // Act
            SynchronousOperationException ex = new SynchronousOperationException("the message");

            // Assert
            Assert.Equal("the message", ex.Message);
        }

        [Fact]
        public void ConstructorWithoutParameters()
        {
            // Act & assert
            Assert.Throws<SynchronousOperationException>(
                delegate { throw new SynchronousOperationException(); });
        }

        [Fact]
        public void TypeIsSerializable()
        {
            // Arrange
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            SynchronousOperationException ex = new SynchronousOperationException("the message", new Exception("inner exception"));

            // Act
            formatter.Serialize(ms, ex);
            ms.Position = 0;
            SynchronousOperationException deserialized = formatter.Deserialize(ms) as SynchronousOperationException;

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("the message", deserialized.Message);
            Assert.NotNull(deserialized.InnerException);
            Assert.Equal("inner exception", deserialized.InnerException.Message);
        }
    }
}
