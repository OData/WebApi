// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http.Formatting.Parsers;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods to read MIME multipart entities from <see cref="HttpContent"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpContentMultipartExtensions
    {
        private const int MinBufferSize = 256;
        private const int DefaultBufferSize = 32 * 1024;

        private static readonly AsyncCallback _onMultipartReadAsyncComplete = new AsyncCallback(OnMultipartReadAsyncComplete);
        private static readonly AsyncCallback _onMultipartWriteSegmentAsyncComplete = new AsyncCallback(OnMultipartWriteSegmentAsyncComplete);

        /// <summary>
        /// Determines whether the specified content is MIME multipart content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is MIME multipart content; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMimeMultipartContent(this HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            return MimeMultipartBodyPartParser.IsMimeMultipartContent(content);
        }

        /// <summary>
        /// Determines whether the specified content is MIME multipart content with the 
        /// specified subtype. For example, the subtype <c>mixed</c> would match content
        /// with a content type of <c>multipart/mixed</c>. 
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="subtype">The MIME multipart subtype to match.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is MIME multipart content with the specified subtype; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMimeMultipartContent(this HttpContent content, string subtype)
        {
            if (String.IsNullOrWhiteSpace(subtype))
            {
                throw new ArgumentNullException("subtype");
            }

            if (IsMimeMultipartContent(content))
            {
                if (content.Headers.ContentType.MediaType.Equals("multipart/" + subtype, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads all body parts within a MIME multipart message and produces a set of <see cref="HttpContent"/> instances as a result.
        /// </summary>
        /// <param name="content">An existing <see cref="HttpContent"/> instance to use for the object's content.</param>
        /// <returns>A <see cref="Task{T}"/> representing the tasks of getting the collection of <see cref="HttpContent"/> instances where each instance represents a body part.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nesting of generic types is required with Task<T>")]
        public static Task<IEnumerable<HttpContent>> ReadAsMultipartAsync(this HttpContent content)
        {
            return ReadAsMultipartAsync(content, MultipartMemoryStreamProvider.Instance, DefaultBufferSize);
        }

        /// <summary>
        /// Reads all body parts within a MIME multipart message and produces a set of <see cref="HttpContent"/> instances as a result
        /// using the <paramref name="streamProvider"/> instance to determine where the contents of each body part is written. 
        /// </summary>
        /// <param name="content">An existing <see cref="HttpContent"/> instance to use for the object's content.</param>
        /// <param name="streamProvider">A stream provider providing output streams for where to write body parts as they are parsed.</param>
        /// <returns>A <see cref="Task{T}"/> representing the tasks of getting the collection of <see cref="HttpContent"/> instances where each instance represents a body part.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nesting of generic types is required with Task<T>")]
        public static Task<IEnumerable<HttpContent>> ReadAsMultipartAsync(this HttpContent content, IMultipartStreamProvider streamProvider)
        {
            return ReadAsMultipartAsync(content, streamProvider, DefaultBufferSize);
        }

        /// <summary>
        /// Reads all body parts within a MIME multipart message and produces a set of <see cref="HttpContent"/> instances as a result
        /// using the <paramref name="streamProvider"/> instance to determine where the contents of each body part is written and 
        /// <paramref name="bufferSize"/> as read buffer size.
        /// </summary>
        /// <param name="content">An existing <see cref="HttpContent"/> instance to use for the object's content.</param>
        /// <param name="streamProvider">A stream provider providing output streams for where to write body parts as they are parsed.</param>
        /// <param name="bufferSize">Size of the buffer used to read the contents.</param>
        /// <returns>A <see cref="Task{T}"/> representing the tasks of getting the collection of <see cref="HttpContent"/> instances where each instance represents a body part.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller becomes owner.")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nesting of generic types is required with Task<T>")]
        public static Task<IEnumerable<HttpContent>> ReadAsMultipartAsync(this HttpContent content, IMultipartStreamProvider streamProvider, int bufferSize)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (streamProvider == null)
            {
                throw new ArgumentNullException("streamProvider");
            }

            if (bufferSize < MinBufferSize)
            {
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, RS.Format(Properties.Resources.ArgumentMustBeGreaterThanOrEqualTo, MinBufferSize));
            }

            return content.ReadAsStreamAsync().Then(stream =>
            {
                TaskCompletionSource<IEnumerable<HttpContent>> taskCompletionSource = new TaskCompletionSource<IEnumerable<HttpContent>>();
                MimeMultipartBodyPartParser parser = new MimeMultipartBodyPartParser(content, streamProvider);
                byte[] data = new byte[bufferSize];
                MultipartAsyncContext context = new MultipartAsyncContext(stream, taskCompletionSource, parser, data);

                // Start async read/write loop
                MultipartReadAsync(context);

                // Return task and complete later
                return taskCompletionSource.Task;
            });
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        private static void MultipartReadAsync(MultipartAsyncContext context)
        {
            Contract.Assert(context != null, "context cannot be null");
            IAsyncResult result = null;
            try
            {
                result = context.ContentStream.BeginRead(context.Data, 0, context.Data.Length, _onMultipartReadAsyncComplete, context);
                if (result.CompletedSynchronously)
                {
                    MultipartReadAsyncComplete(result);
                }
            }
            catch (Exception e)
            {
                Exception exception = (result != null && result.CompletedSynchronously) ? e : new IOException(Properties.Resources.ReadAsMimeMultipartErrorReading, e);
                context.TaskCompletionSource.TrySetException(exception);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        private static void OnMultipartReadAsyncComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            MultipartAsyncContext context = (MultipartAsyncContext)result.AsyncState;
            Contract.Assert(context != null, "context cannot be null");
            try
            {
                MultipartReadAsyncComplete(result);
            }
            catch (Exception e)
            {
                context.TaskCompletionSource.TrySetException(e);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        private static void MultipartReadAsyncComplete(IAsyncResult result)
        {
            Contract.Assert(result != null, "result cannot be null");
            MultipartAsyncContext context = (MultipartAsyncContext)result.AsyncState;
            int bytesRead = 0;

            try
            {
                bytesRead = context.ContentStream.EndRead(result);
            }
            catch (Exception e)
            {
                context.TaskCompletionSource.TrySetException(new IOException(Properties.Resources.ReadAsMimeMultipartErrorReading, e));
            }

            IEnumerable<MimeBodyPart> parts = context.MimeParser.ParseBuffer(context.Data, bytesRead);
            context.PartsEnumerator = parts.GetEnumerator();
            MoveNextPart(context);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        private static void MultipartWriteSegmentAsync(MultipartAsyncContext context)
        {
            Contract.Assert(context != null, "context cannot be null.");
            Stream output = context.PartsEnumerator.Current.GetOutputStream();
            ArraySegment<byte> segment = (ArraySegment<byte>)context.SegmentsEnumerator.Current;
            try
            {
                IAsyncResult result = output.BeginWrite(segment.Array, segment.Offset, segment.Count, _onMultipartWriteSegmentAsyncComplete, context);
                if (result.CompletedSynchronously)
                {
                    MultipartWriteSegmentAsyncComplete(result);
                }
            }
            catch (Exception e)
            {
                context.PartsEnumerator.Current.Dispose();
                context.TaskCompletionSource.TrySetException(new IOException(Properties.Resources.ReadAsMimeMultipartErrorWriting, e));
            }
        }

        private static void OnMultipartWriteSegmentAsyncComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            MultipartWriteSegmentAsyncComplete(result);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        private static void MultipartWriteSegmentAsyncComplete(IAsyncResult result)
        {
            Contract.Assert(result != null, "result cannot be null.");
            MultipartAsyncContext context = (MultipartAsyncContext)result.AsyncState;
            Contract.Assert(context != null, "context cannot be null");

            MimeBodyPart part = context.PartsEnumerator.Current;
            try
            {
                Stream output = context.PartsEnumerator.Current.GetOutputStream();
                output.EndWrite(result);
            }
            catch (Exception e)
            {
                part.Dispose();
                context.TaskCompletionSource.TrySetException(new IOException(Properties.Resources.ReadAsMimeMultipartErrorWriting, e));
            }

            if (!MoveNextSegment(context))
            {
                MoveNextPart(context);
            }
        }

        private static void MoveNextPart(MultipartAsyncContext context)
        {
            Contract.Assert(context != null, "context cannot be null");
            while (context.PartsEnumerator.MoveNext())
            {
                context.SegmentsEnumerator = context.PartsEnumerator.Current.Segments.GetEnumerator();
                if (MoveNextSegment(context))
                {
                    return;
                }
            }

            // Read some more
            MultipartReadAsync(context);
        }

        private static bool MoveNextSegment(MultipartAsyncContext context)
        {
            Contract.Assert(context != null, "context cannot be null");
            if (context.SegmentsEnumerator.MoveNext())
            {
                MultipartWriteSegmentAsync(context);
                return true;
            }
            else if (CheckPartCompletion(context.PartsEnumerator.Current, context.Result))
            {
                // We are done parsing
                context.TaskCompletionSource.TrySetResult(context.Result);
                return true;
            }

            return false;
        }

        private static bool CheckPartCompletion(MimeBodyPart part, List<HttpContent> result)
        {
            Contract.Assert(part != null, "part cannot be null.");
            Contract.Assert(result != null, "result cannot be null.");
            if (part.IsComplete)
            {
                if (part.HttpContent != null)
                {
                    result.Add(part.HttpContent);
                }

                bool isFinal = part.IsFinal;
                part.Dispose();
                return isFinal;
            }

            return false;
        }

        /// <summary>
        /// Managing state for asynchronous read and write operations
        /// </summary>
        private class MultipartAsyncContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MultipartAsyncContext"/> class.
            /// </summary>
            /// <param name="contentStream">The content stream.</param>
            /// <param name="taskCompletionSource">The task completion source.</param>
            /// <param name="mimeParser">The MIME parser.</param>
            /// <param name="data">The buffer that we read data from.</param>
            public MultipartAsyncContext(Stream contentStream, TaskCompletionSource<IEnumerable<HttpContent>> taskCompletionSource, MimeMultipartBodyPartParser mimeParser, byte[] data)
            {
                Contract.Assert(contentStream != null, "contentStream cannot be null");
                Contract.Assert(taskCompletionSource != null, "task cannot be null");
                Contract.Assert(mimeParser != null, "mimeParser cannot be null");
                Contract.Assert(data != null, "data cannot be null");

                ContentStream = contentStream;
                Result = new List<HttpContent>();
                TaskCompletionSource = taskCompletionSource;
                MimeParser = mimeParser;
                Data = data;
            }

            /// <summary>
            /// Gets the <see cref="Stream"/> that we read from.
            /// </summary>
            /// <value>
            /// The content stream.
            /// </value>
            public Stream ContentStream { get; private set; }

            /// <summary>
            /// Gets the collection of parsed <see cref="HttpContent"/> instances.
            /// </summary>
            /// <value>
            /// The result collection.
            /// </value>
            public List<HttpContent> Result { get; private set; }

            /// <summary>
            /// Gets the task completion source.
            /// </summary>
            /// <value>
            /// The task completion source.
            /// </value>
            public TaskCompletionSource<IEnumerable<HttpContent>> TaskCompletionSource { get; private set; }

            /// <summary>
            /// Gets the data.
            /// </summary>
            /// <value>
            /// The buffer that we read data from.
            /// </value>
            public byte[] Data { get; private set; }

            /// <summary>
            /// Gets the MIME parser.
            /// </summary>
            /// <value>
            /// The MIME parser.
            /// </value>
            public MimeMultipartBodyPartParser MimeParser { get; private set; }

            /// <summary>
            /// Gets or sets the parts enumerator for going through the parsed parts.
            /// </summary>
            /// <value>
            /// The parts enumerator.
            /// </value>
            public IEnumerator<MimeBodyPart> PartsEnumerator { get; set; }

            /// <summary>
            /// Gets or sets the segments enumerator for going through the segments within each part.
            /// </summary>
            /// <value>
            /// The segments enumerator.
            /// </value>
            public IEnumerator SegmentsEnumerator { get; set; }
        }
    }
}
