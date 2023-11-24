//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointSelectorPolicyTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCOREAPP2_1
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    public class ODataEndpointSelectorPolicyTests
    {
        [Fact]
        public void AddODataServicesAddingODataEndpointSelectorPolicy()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            services.AddOData();
            IServiceProvider provider = services.BuildServiceProvider();

            // Act
            var policy = provider.GetService(typeof(MatcherPolicy));

            // Assert
            Assert.NotNull(policy);
            Assert.IsType<ODataEndpointSelectorPolicy>(policy);
        }

        [Fact]
        public void AppliesToEndpointsReturnsTrueAlways()
        {
            // Arrange
            ODataEndpointSelectorPolicy policy = new ODataEndpointSelectorPolicy();

            // Act & Assert
            Assert.True(policy.AppliesToEndpoints(endpoints: null));

            // Act & Assert
            Assert.True(policy.AppliesToEndpoints(endpoints: new List<Endpoint>()));
        }

        [Fact]
        public void ApplyAsyncDoNothingIfActionDescriptorNotSelected()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();

            CandidateSet candidateSet = CreateCandidateSet();
            Assert.True(candidateSet.IsValidCandidate(0)); // Guard

            // Act
            Task actual = new ODataEndpointSelectorPolicy().ApplyAsync(context, candidateSet);

            // Assert
            Assert.Equal(Task.CompletedTask, actual);
            Assert.True(candidateSet.IsValidCandidate(0));
        }

        [Fact]
        public void ApplyAsyncDoNothingIfEndpointHasODataSelectedActionDescriptor()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            Mock<ActionDescriptor> actionDescriptor = new Mock<ActionDescriptor>();
            context.ODataFeature().ActionDescriptor = actionDescriptor.Object;

            CandidateSet candidateSet = CreateCandidateSet(actionDescriptor.Object);
            Assert.True(candidateSet.IsValidCandidate(0)); // Guard

            // Act
            Task actual = new ODataEndpointSelectorPolicy().ApplyAsync(context, candidateSet);

            // Assert
            Assert.Equal(Task.CompletedTask, actual);
            Assert.True(candidateSet.IsValidCandidate(0));
        }

       [Fact]
        public void ApplyAsyncSetEndpointInvalidIfEndpointDoesnotHaveODataSelectedActionDescriptor()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            Mock<ActionDescriptor> actionDescriptor1 = new Mock<ActionDescriptor>();
            context.ODataFeature().ActionDescriptor = actionDescriptor1.Object;

            Mock<ActionDescriptor> actionDescriptor2 = new Mock<ActionDescriptor>();
            CandidateSet candidateSet = CreateCandidateSet(actionDescriptor2.Object);
            Assert.True(candidateSet.IsValidCandidate(0)); // Guard

            // Act
            Task actual = new ODataEndpointSelectorPolicy().ApplyAsync(context, candidateSet);

            // Assert
            Assert.Equal(Task.CompletedTask, actual);
            Assert.False(candidateSet.IsValidCandidate(0));
        }

        [Fact]
        public void ApplyAsyncDoNothingIfEndpointDoesnotHaveActionDescriptorMetadata()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            Mock<ActionDescriptor> actionDescriptor = new Mock<ActionDescriptor>();
            context.ODataFeature().ActionDescriptor = actionDescriptor.Object;

            CandidateSet candidateSet = CreateCandidateSet();
            Assert.True(candidateSet.IsValidCandidate(0)); // Guard

            // Act
            Task actual = new ODataEndpointSelectorPolicy().ApplyAsync(context, candidateSet);

            // Assert
            Assert.Equal(Task.CompletedTask, actual);
            Assert.True(candidateSet.IsValidCandidate(0));
        }

        private static CandidateSet CreateCandidateSet(object metadata = null)
        {
            IList<object> metadatas = new List<object>();
            if (metadata != null)
            {
                metadatas.Add(metadata);
            }

            Endpoint endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(metadatas), "odata");

            Endpoint[] endpoints = new[] { endpoint };
            RouteValueDictionary[] values = new RouteValueDictionary[] { null };
            int[] scores = new[] { 0 };
            return new CandidateSet(endpoints, values, scores);
        }
    }
}
#endif
