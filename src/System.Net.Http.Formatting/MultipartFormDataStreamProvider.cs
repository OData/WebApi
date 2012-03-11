using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// An <see cref="IMultipartStreamProvider"/> suited for use with HTML file uploads for writing file 
    /// content to a <see cref="FileStream"/>. The stream provider looks at the <b>Content-Disposition</b> header 
    /// field and determines an output <see cref="Stream"/> based on the presence of a <b>filename</b> parameter.
    /// If a <b>filename</b> parameter is present in the <b>Content-Disposition</b> header field then the body 
    /// part is written to a <see cref="FileStream"/>, otherwise it is written to a <see cref="MemoryStream"/>.
    /// This makes it convenient to process MIME Multipart HTML Form data which is a combination of form 
    /// data and file content.
    /// </summary>
    public class MultipartFormDataStreamProvider : IMultipartStreamProvider
    {
        private const int DefaultBufferSize = 0x1000;

        private Dictionary<string, string> _bodyPartFileNames = new Dictionary<string, string>();
        private readonly object _thisLock = new object();
        private string _rootPath;
        private int _bufferSize = DefaultBufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataStreamProvider"/> class.
        /// </summary>
        public MultipartFormDataStreamProvider()
            : this(Path.GetTempPath(), DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        public MultipartFormDataStreamProvider(string rootPath)
            : this(rootPath, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        /// <param name="bufferSize">The number of bytes buffered for writes to the file.</param>
        public MultipartFormDataStreamProvider(string rootPath, int bufferSize)
        {
            if (String.IsNullOrWhiteSpace(rootPath))
            {
                throw new ArgumentNullException("rootPath");
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, Properties.Resources.NonZeroParameterSize);
            }

            _rootPath = Path.GetFullPath(rootPath);
        }

        /// <summary>
        /// Gets an <see cref="IDictionary{T1, T2}"/> instance containing mappings of each 
        /// <b>filename</b> parameter provided in a <b>Content-Disposition</b> header field 
        /// (represented as the keys) to a local file name where the contents of the body part is 
        /// stored (represented as the values).
        /// </summary>
        public IDictionary<string, string> BodyPartFileNames
        {
            get
            {
                lock (_thisLock)
                {
                    return _bodyPartFileNames != null
                               ? new Dictionary<string, string>(_bodyPartFileNames)
                               : new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// This body part stream provider examines the headers provided by the MIME multipart parser
        /// and decides whether it should return a file stream or a memory stream for the body part to be 
        /// written to.
        /// </summary>
        /// <param name="headers">Header fields describing the body part</param>
        /// <returns>The <see cref="Stream"/> instance where the message body part is written to.</returns>
        public Stream GetStream(HttpContentHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            return this.OnGetStream(headers);
        }

        /// <summary>
        /// Override this method in a derived class to examine the headers provided by the MIME multipart parser
        /// and decide whether it should return a file stream or a memory stream for the body part to be 
        /// written to.
        /// </summary>
        /// <param name="headers">Header fields describing the body part</param>
        /// <returns>The <see cref="Stream"/> instance where the message body part is written to.</returns>
        protected virtual Stream OnGetStream(HttpContentHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
            if (contentDisposition != null)
            {
                // If we have a file name then write contents out to temporary file. Otherwise just write to MemoryStream
                if (!String.IsNullOrEmpty(contentDisposition.FileName))
                {
                    string localFilePath;
                    try
                    {
                        string filename = this.GetLocalFileName(headers);
                        localFilePath = Path.Combine(_rootPath, Path.GetFileName(filename));
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException(Properties.Resources.MultipartStreamProviderInvalidLocalFileName, e);
                    }

                    if (!Directory.Exists(_rootPath))
                    {
                        Directory.CreateDirectory(_rootPath);
                    }

                    // Add mapping from Content-Disposition FileName parameter to local file name.
                    lock (_thisLock)
                    {
                        _bodyPartFileNames.Add(contentDisposition.FileName, localFilePath);
                    }

                    return File.Create(localFilePath, _bufferSize, FileOptions.Asynchronous);
                }

                // If no filename parameter was found in the Content-Disposition header then return a memory stream.
                return new MemoryStream();
            }

            // If no Content-Disposition header was present.
            throw new IOException(RS.Format(Properties.Resources.MultipartFormDataStreamProviderNoContentDisposition, "Content-Disposition"));
        }

        /// <summary>
        /// Gets the name of the local file which will be combined with the root path to
        /// create an absolute file name where the contents of the current MIME body part
        /// will be stored.
        /// </summary>
        /// <param name="headers">The headers for the current MIME body part.</param>
        /// <returns>A relative filename with no path component.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is propagated.")]
        protected virtual string GetLocalFileName(HttpContentHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            string filename = null;
            try
            {
                ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
                if (contentDisposition != null)
                {
                    filename = contentDisposition.ExtractLocalFileName();
                }
            }
            catch (Exception)
            {
                //// TODO: CSDMain 232171 -- review and fix swallowed exception
            }

            if (filename == null)
            {
                filename = String.Format(CultureInfo.InvariantCulture, "BodyPart_{0}", Guid.NewGuid());
            }

            return filename;
        }
    }
}
