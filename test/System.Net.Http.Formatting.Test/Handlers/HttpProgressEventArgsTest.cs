// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Net.Http.Handlers
{
    public class HttpProgressEventArgsTest
    {
        [Fact]
        public void Constructor_Initializes()
        {
            // Arrange
            int progressPercentage = 10;
            object userState = new object();
            long bytesTransferred = 10L * 1024 * 1024 * 1024;
            long? totalBytes = 10L * 1024 * 1024 * 1024;

            // Act
            HttpProgressEventArgs args = new HttpProgressEventArgs(progressPercentage, userState, bytesTransferred, totalBytes);

            // Assert
            Assert.Equal(progressPercentage, args.ProgressPercentage);
            Assert.Equal(userState, args.UserState);
            Assert.Equal(bytesTransferred, args.BytesTransferred);
            Assert.Equal(totalBytes, args.TotalBytes);
        }
    }
}
