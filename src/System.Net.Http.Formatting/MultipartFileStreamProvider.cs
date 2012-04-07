// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// An <see cref="IMultipartStreamProvider"/> suited for writing each MIME body parts of the MIME multipart
    /// message to a file using a <see cref="FileStream"/>.
    /// </summary>
    public class MultipartFileStreamProvider : IMultipartStreamProvider
    {
        private const int MinBufferSize = 1;
        private const int DefaultBufferSize = 0x1000;

        private List<string> _bodyPartFileNames = new List<string>();
        private readonly object _thisLock = new object();
        private string _rootPath;
        private int _bufferSize = DefaultBufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFileStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        public MultipartFileStreamProvider(string rootPath)
            : this(rootPath, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFileStreamProvider"/> class.
        /// </summary>
        /// <param name="rootPath">The root path where the content of MIME multipart body parts are written to.</param>
        /// <param name="bufferSize">The number of bytes buffered for writes to the file.</param>
        public MultipartFileStreamProvider(string rootPath, int bufferSize)
        {
            if (String.IsNullOrWhiteSpace(rootPath))
            {
                throw new ArgumentNullException("rootPath");
            }

            if (bufferSize < MinBufferSize)
            {
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, RS.Format(Properties.Resources.ArgumentMustBeGreaterThanOrEqualTo, MinBufferSize));
            }

            _rootPath = Path.GetFullPath(rootPath);
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Gets an <see cref="Collection{T}"/> containing the files names of MIME 
        /// body part written to file.
        /// </summary>
        public Collection<string> BodyPartFileNames
        {
            get
            {
                lock (_thisLock)
                {
                    return new Collection<string>(_bodyPartFileNames);
                }
            }
        }

        /// <summary>
        /// This body part stream provider examines the headers provided by the MIME multipart parser
        /// and decides which <see cref="FileStream"/> to write the body part to.
        /// </summary>
        /// <param name="headers">Header fields describing the body part</param>
        /// <returns>The <see cref="Stream"/> instance where the message body part is written to.</returns>
        public virtual Stream GetStream(HttpContentHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            string localFilePath;
            try
            {
                string filename = GetLocalFileName(headers);
                localFilePath = Path.Combine(_rootPath, Path.GetFileName(filename));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(Properties.Resources.MultipartStreamProviderInvalidLocalFileName, e);
            }

            // Add local file name 
            lock (_thisLock)
            {
                _bodyPartFileNames.Add(localFilePath);
            }

            return File.Create(localFilePath, _bufferSize, FileOptions.Asynchronous);
        }

        /// <summary>
        /// Gets the name of the local file which will be combined with the root path to
        /// create an absolute file name where the contents of the current MIME body part
        /// will be stored.
        /// </summary>
        /// <param name="headers">The headers for the current MIME body part.</param>
        /// <returns>A relative filename with no path component.</returns>
        public virtual string GetLocalFileName(HttpContentHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            return String.Format(CultureInfo.InvariantCulture, "BodyPart_{0}", Guid.NewGuid());
        }
    }
}
