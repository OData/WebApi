// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.Internal;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http
{
    /// <summary>
    /// A <see cref="MultipartStreamProvider"/> implementation suited for use with HTML file uploads for writing file 
    /// content to a <see cref="FileStream"/>. The stream provider looks at the <b>Content-Disposition</b> header 
    /// field and determines an output <see cref="Stream"/> based on the presence of a <b>filename</b> parameter.
    /// If a <b>filename</b> parameter is present in the <b>Content-Disposition</b> header field then the body 
    /// part is written to a <see cref="FileStream"/>, otherwise it is written to a <see cref="MemoryStream"/>.
    /// This makes it convenient to process MIME Multipart HTML Form data which is a combination of form 
    /// data and file content.
    /// </summary>
    public class MultipartFormDataStreamProvider : MultipartFileStreamProvider
    {
        private NameValueCollection _formData = HttpValueCollection.Create();

        // Set of indexes of which HttpContents we designate as form data
        private Collection<bool> _isFormData = new Collection<bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        public MultipartFormDataStreamProvider(string rootPath)
            : base(rootPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        /// <param name="bufferSize">The number of bytes buffered for writes to the file.</param>
        public MultipartFormDataStreamProvider(string rootPath, int bufferSize)
            : base(rootPath, bufferSize)
        {
        }

        /// <summary>
        /// Gets a <see cref="NameValueCollection"/> of form data passed as part of the multipart form data.
        /// </summary>
        public NameValueCollection FormData
        {
            get { return _formData; }
        }

        /// <summary>
        /// This body part stream provider examines the headers provided by the MIME multipart parser
        /// and decides whether it should return a file stream or a memory stream for the body part to be 
        /// written to.
        /// </summary>
        /// <param name="parent">The parent MIME multipart HttpContent instance.</param>
        /// <param name="headers">Header fields describing the body part</param>
        /// <returns>The <see cref="Stream"/> instance where the message body part is written to.</returns>
        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (parent == null)
            {
                throw Error.ArgumentNull("parent");
            }

            if (headers == null)
            {
                throw Error.ArgumentNull("headers");
            }

            // For form data, Content-Disposition header is a requirement
            ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
            if (contentDisposition != null)
            {
                // If we have a file name then write contents out to temporary file. Otherwise just write to MemoryStream
                if (!String.IsNullOrEmpty(contentDisposition.FileName))
                {
                    // We won't post process files as form data
                    _isFormData.Add(false);

                    return base.GetStream(parent, headers);
                }

                // We will post process this as form data
                _isFormData.Add(true);

                // If no filename parameter was found in the Content-Disposition header then return a memory stream.
                return new MemoryStream();
            }

            // If no Content-Disposition header was present.
            throw Error.InvalidOperation(Properties.Resources.MultipartFormDataStreamProviderNoContentDisposition, "Content-Disposition");
        }

        /// <summary>
        /// Read the non-file contents as form data.
        /// </summary>
        /// <returns></returns>
        public override Task ExecutePostProcessingAsync()
        {
            // Find instances of HttpContent for which we created a memory stream and read them asynchronously
            // to get the string content and then add that as form data
            IEnumerable<Task> readTasks = Contents.Where((content, index) => _isFormData[index] == true).Select(
                formContent =>
                {
                    // Extract name from Content-Disposition header. We know from earlier that the header is present.
                    ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
                    string formFieldName = FormattingUtilities.UnquoteToken(contentDisposition.Name) ?? String.Empty;

                    // Read the contents as string data and add to form data
                    return formContent.ReadAsStringAsync().Then(
                        formFieldValue =>
                        {
                            FormData.Add(formFieldName, formFieldValue);
                        }, runSynchronously: true);
                });

            // Actually execute the read tasks while trying to stay on the same thread
            return TaskHelpers.Iterate(readTasks);
        }
    }
}
