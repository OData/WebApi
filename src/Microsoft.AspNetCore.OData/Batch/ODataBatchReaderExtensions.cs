// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="ODataBatchReader"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataBatchReaderExtensions
    {
        private static readonly string[] nonInheritableHeaders = new string[] { "content-length", "content-type" };
        // do not inherit respond-async and continue-on-error (odata.continue-on-error in OData 4.0) from Prefer header
        private static readonly string[] nonInheritablePreferences = new string[] { "respond-async", "continue-on-error", "odata.continue-on-error" };

        /// <summary>
        /// Reads a ChangeSet request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch Id.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A collection of <see cref="HttpRequest"/> in the ChangeSet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of request messages asynchronously.")]
        public static async Task<IList<HttpContext>> ReadChangeSetRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.ChangesetStart)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.ChangesetStart.ToString());
            }

            Guid changeSetId = Guid.NewGuid();
            List<HttpContext> contexts = new List<HttpContext>();
            while (await reader.ReadAsync() && reader.State != ODataBatchReaderState.ChangesetEnd)
            {
                if (reader.State == ODataBatchReaderState.Operation)
                {
                    contexts.Add(await ReadOperationInternalAsync(reader, context, batchId, changeSetId, cancellationToken));
                }
            }
            return contexts;
        }

        /// <summary>
        /// Reads an Operation request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpRequest"/> representing the operation.</returns>
        public static Task<HttpContext> ReadOperationRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, bool bufferContentStream, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.Operation)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.Operation.ToString());
            }

            return ReadOperationInternalAsync(reader, context, batchId, null, cancellationToken, bufferContentStream);
        }

        /// <summary>
        /// Reads an Operation request in a ChangeSet.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="changeSetId">The ChangeSet ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpRequest"/> representing a ChangeSet operation</returns>
        public static Task<HttpContext> ReadChangeSetOperationRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, Guid changeSetId, bool bufferContentStream, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.Operation)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.Operation.ToString());
            }

            return ReadOperationInternalAsync(reader, context, batchId, changeSetId, cancellationToken, bufferContentStream);
        }

        private static async Task<HttpContext> ReadOperationInternalAsync(
            ODataBatchReader reader, HttpContext originalContext, Guid batchId, Guid? changeSetId, CancellationToken cancellationToken, bool bufferContentStream = true)
        {
            ODataBatchOperationRequestMessage batchRequest = await reader.CreateOperationRequestMessageAsync();

            HttpContext context = CreateHttpContext(originalContext);
            HttpRequest request = context.Request;

            request.Method = batchRequest.Method;
            request.CopyAbsoluteUrl(batchRequest.Url);

            // Not using bufferContentStream. Unlike AspNet, AspNetCore cannot guarantee the disposal
            // of the stream in the context of execution so there is no choice but to copy the stream
            // from the batch reader.
            using (Stream stream = batchRequest.GetStream())
            {
                MemoryStream bufferedStream = new MemoryStream();
                // Passing in the default buffer size of 81920 so that we can also pass in a cancellation token
                await stream.CopyToAsync(bufferedStream, bufferSize: 81920, cancellationToken: cancellationToken);
                bufferedStream.Position = 0;
                request.Body = bufferedStream;
            }

            foreach (var header in batchRequest.Headers)
            {
                string headerName = header.Key;
                string headerValue = header.Value;

                if (headerName.Trim().ToLowerInvariant() == "prefer")
                {
                    // in the case of Prefer header, we don't want to overwrite,
                    // instead we merge preferences defined in the individual request with those inherited from the batch
                    request.Headers.TryGetValue(headerName, out StringValues batchReferences);
                    request.Headers[headerName] = MergeIndividualAndBatchPreferences(headerValue, batchReferences);
                    continue;
                }

                // Copy headers from batch, overwriting any existing headers.
                request.Headers[headerName] = headerValue;
            }

            request.SetODataBatchId(batchId);
            request.SetODataContentId(batchRequest.ContentId);

            if (changeSetId != null && changeSetId.HasValue)
            {
                request.SetODataChangeSetId(changeSetId.Value);
            }

            return context;
        }

        private static HttpContext CreateHttpContext(HttpContext originalContext)
        {
            // Clone the features so that a new set is used for each context.
            // The features themselves will be reused but not the collection. We
            // store the request container as a feature of the request and we don't want
            // the features added to one context/request to be visible on another.
            //
            // Note that just about everything inm the HttpContext and HttpRequest is
            // backed by one of these features. So reusing the features means the HttContext
            // and HttpRequests are the same without needing to copy properties. To make them
            // different, we need to avoid copying certain features to that the objects don't
            // share the same storage/
            IFeatureCollection features = new FeatureCollection();
            string pathBase = "";
            foreach (KeyValuePair<Type, object> kvp in originalContext.Features)
            {
                // Don't include the OData features. They may already
                // be present. This will get re-created later.
                //
                // Also, clear out the items feature, which is used
                // to store a few object, the one that is an issue here is the Url
                // helper, which has an affinity to the context. If we leave it,
                // the context of the helper no longer matches the new context and
                // the resulting url helper doesn't have access to the OData feature
                // because it's looking in the wrong context.
                //
                // Because we need a different request and response, leave those features
                // out as well.
                if (kvp.Key == typeof(IHttpRequestFeature))
                {
                    pathBase = ((IHttpRequestFeature)kvp.Value).PathBase;
                }

                if (kvp.Key == typeof(IODataBatchFeature) ||
                    kvp.Key == typeof(IODataFeature) ||
                    kvp.Key == typeof(IItemsFeature) ||
                    kvp.Key == typeof(IHttpRequestFeature) ||
                    kvp.Key == typeof(IHttpResponseFeature))
                {
                    continue;
                }

#if !NETSTANDARD2_0
                if (kvp.Key == typeof(IEndpointFeature))
                {
                    continue;
                }
#endif

                features[kvp.Key] = kvp.Value;
            }

            // Add in an items, request and response feature.
            features[typeof(IItemsFeature)] = new ItemsFeature();
            features[typeof(IHttpRequestFeature)] = new HttpRequestFeature
            {
                PathBase = pathBase
            };
            features[typeof(IHttpResponseFeature)] = new HttpResponseFeature();

            // Create a context from the factory or use the default context.
            HttpContext context = null;
            IHttpContextFactory httpContextFactory = originalContext.RequestServices.GetRequiredService<IHttpContextFactory>();
            if (httpContextFactory != null)
            {
                context = httpContextFactory.Create(features);
            }
            else
            {
                context = new DefaultHttpContext(features);
            }

            // Clone parts of the request. All other parts of the request will be 
            // populated during batch processing.
            context.Request.Cookies = originalContext.Request.Cookies;
            foreach (KeyValuePair<string, StringValues> header in originalContext.Request.Headers)
            {
                string headerKey = header.Key.ToLowerInvariant();
                // do not copy over headers that should not be inherited from batch to individual requests
                if (nonInheritableHeaders.Contains(headerKey))
                {
                    continue;
                }
                // some preferences may be inherited, others discarded
                if (headerKey == "prefer")
                {
                    string preferencesToInherit = GetPreferencesToInheritFromBatch(header.Value);
                    if (!string.IsNullOrEmpty(preferencesToInherit))
                    {
                        context.Request.Headers.Add(header.Key, preferencesToInherit);
                    }
                    continue;
                }
                context.Request.Headers.Add(header);
            }

            // Create a response body as the default response feature does not
            // have a valid stream.
            // Use a special batch stream that remains open after the writer is disposed.
            context.Response.Body = new ODataBatchStream();

            return context;
        }

        /// <summary>
        /// Extract preferences that can be inherited from the overall batch request to
        /// an individual request.
        /// </summary>
        /// <param name="batchPreferences">The value of the Prefer header from the batch request</param>
        /// <returns>comma-separated preferences that can be passed down to an individual request</returns>
        private static string GetPreferencesToInheritFromBatch(string batchPreferences)
        {
            IEnumerable<string> preferencesToInherit = batchPreferences.SplitPreferences()
                .Where(value => 
                !nonInheritablePreferences.Any(
                    prefToIgnore => value.Trim().ToLowerInvariant().StartsWith(prefToIgnore)))
                .Select(value => value.Trim());
            return string.Join(", ", preferencesToInherit);
        }

        /// <summary>
        /// Merges the preferences from the batch request and an individual request inside the batch into one value.
        /// If a given preference is defined in both the batch and individual request, the one from the individual
        /// request is retained and the one from the batch is discarded.
        /// </summary>
        /// <param name="individualPreferences">The value of the Prefer header from the individual request inside the batch</param>
        /// <param name="batchPreferences">The value of the Prefer header from the overall batch request</param>
        /// <returns>Value containing the combined preferences</returns>
        private static string MergeIndividualAndBatchPreferences(string individualPreferences, string batchPreferences)
        {
            if (string.IsNullOrEmpty(individualPreferences))
            {
                return batchPreferences;
            }
            if (string.IsNullOrEmpty(batchPreferences))
            {
                return individualPreferences;
            }
            // get the name of each preference to avoid adding duplicates from batch
            IEnumerable<string> individualList = individualPreferences.SplitPreferences().Select(pref => pref.Trim());
            HashSet<string> individualPreferenceNames = new HashSet<string>(individualList.Select(pref => pref.Split('=').FirstOrDefault()));
            
            
            IEnumerable<string> filteredBatchList = batchPreferences.SplitPreferences().Select(pref => pref.Trim())
                // do not add duplicate preferences from batch
                .Where(pref => !individualPreferenceNames.Contains(pref.Split('=').FirstOrDefault()));
            string filteredBatchPreferences = string.Join(", ", filteredBatchList);

            return string.Join(", ", individualPreferences, filteredBatchPreferences);
        }

        /// <summary>
        /// Splits the value of a Prefer header into separate preferences
        /// e.g. a value like 'a, b=c, foo="bar,baz"' will return an IEnumerable with
        /// - a
        /// - b=c
        /// - foo="bar,baz"
        /// </summary>
        /// <param name="preferences"></param>
        /// <returns></returns>
        private static IEnumerable<string> SplitPreferences(this string preferences)
        {
            StringBuilder currentPreference = new StringBuilder();
            HashSet<string> addedPreferences = new HashSet<string>();
            bool insideQuotedValue = false;
            char prevChar = '\0';
            foreach (char c in preferences)
            {
                if (c == '"')
                {
                    if (!insideQuotedValue)
                    {
                        // we are starting a double-quoted value
                        insideQuotedValue = true;
                    }
                    else
                    {
                        // this could be the end of a quoted value, or it could be an escaped quote
                        insideQuotedValue = prevChar == '\\';
                    }
                }

                if (c == ',' && !insideQuotedValue)
                {
                    string result = currentPreference.ToString();
                    string prefName = result.Split('=')[0];
                    // do not add duplicate preference
                    if (!addedPreferences.Contains(prefName))
                    {
                        yield return result;
                        addedPreferences.Add(prefName);
                    }
                    currentPreference.Clear();
                }
                else
                {
                    currentPreference.Append(c);
                }

                prevChar = c;
            }

            if (currentPreference.Length > 0)
            {
                yield return currentPreference.ToString();
            }
        }
    }
}