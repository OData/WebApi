// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.OData.Batch;
using Microsoft.OData.Core;
using Microsoft.TestCommon;

namespace System.Web.OData.Test
{
    public class ODataBatchHttpRequestMessageExtensionsTest
    {
        [Fact]
        public void GetODataBatchId_NullRequest_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestMessageExtensions.GetODataBatchId(null),
                "request");
        }

        [Fact]
        public void SetODataBatchId_NullRequest_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestMessageExtensions.SetODataBatchId(null, Guid.NewGuid()),
                "request");
        }

        [Fact]
        public void SetODataBatchId_SetsTheBatchIdOnTheRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            var id = Guid.NewGuid();
            request.SetODataBatchId(id);

            Assert.Equal(id, request.GetODataBatchId());
        }

        [Fact]
        public void GetODataChangeSetId_NullRequest_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestMessageExtensions.GetODataChangeSetId(null),
                "request");
        }

        [Fact]
        public void SetODataChangeSetId_NullRequest_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestMessageExtensions.SetODataChangeSetId(null, Guid.NewGuid()),
                "request");
        }

        [Fact]
        public void SetODataChangeSetId_SetsTheChangeSetIdOnTheRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            var id = Guid.NewGuid();
            request.SetODataChangeSetId(id);

            Assert.Equal(id, request.GetODataChangeSetId());
        }

        [Fact]
        public void GetODataContentIdMapping_NullRequest_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestMessageExtensions.GetODataContentIdMapping(null),
                "request");
        }

        [Fact]
        public void SetODataContentIdMapping_NullRequest_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestMessageExtensions.SetODataContentIdMapping(null, new Dictionary<string, string>()),
                "request");
        }

        [Fact]
        public void SetODataContentIdMapping_SetsTheContentIdMappingOnTheRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            var mapping = new Dictionary<string, string>();
            request.SetODataContentIdMapping(mapping);

            Assert.Equal(mapping, request.GetODataContentIdMapping());
        }

        [Fact]
        public void CreateODataBatchResponseAsync_ReturnsHttpStatusCodeOK()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            var responses = new ODataBatchResponseItem[] { };
            var quotas = new ODataMessageQuotas();

            // Act
            var response = request.CreateODataBatchResponseAsync(responses, quotas).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void GetODataContentId_NullRequest_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestMessageExtensions.GetODataContentId(null),
                "request");
        }

        [Fact]
        public void SetODataContentId_NullRequest_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestMessageExtensions.SetODataContentId(null, Guid.NewGuid().ToString()),
                "request");
        }

        [Fact]
        public void SetODataContentId_SetsTheContentIdOnTheRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            var id = Guid.NewGuid().ToString();
            request.SetODataContentId(id);

            Assert.Equal(id, request.GetODataContentId());
        }
    }
}