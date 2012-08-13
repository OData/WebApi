// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// This class contains information that the DataContractODataSerializer will use during
    /// serialization.
    /// </summary>
    public class ODataResponseContext
    {
        private IODataResponseMessage _responseMessage;
        private IODataRequestMessage _requestMessage;
        private Uri _baseAddress;
        private string _serviceOperationName;

        /// <summary>
        /// This constructor is for unit testing purposes only.
        /// </summary>
        public ODataResponseContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of ODataResponseContext with the specified responseMessage,
        /// baseAddress and serviceOperationName.
        /// </summary>
        /// <param name="responseMessage">An instance of the IODataResponseMessage.</param>
        /// <param name="format">ODataFormat to be used.</param>
        /// <param name="version">DataServiceversion to be used</param>
        /// <param name="baseAddress">The baseAddress to be used while serializing feed/entry.</param>
        /// <param name="serviceOperationName">The serviceOperationName to use while serializing primitives and complex types.</param>
        public ODataResponseContext(IODataResponseMessage responseMessage, ODataFormat format, ODataVersion version, Uri baseAddress, string serviceOperationName)
        {
            if (responseMessage == null)
            {
                throw Error.ArgumentNull("responseMessage");
            }

            if (baseAddress == null)
            {
                throw Error.ArgumentNull("baseAddress");
            }

            if (String.IsNullOrEmpty(serviceOperationName))
            {
                throw Error.ArgumentNullOrEmpty("serviceOperationName");
            }

            this._responseMessage = responseMessage;
            this.ODataFormat = format;
            this.ODataVersion = version;
            this._baseAddress = baseAddress;
            this._serviceOperationName = serviceOperationName;
            this.IsIndented = true;
        }

        public ODataResponseContext(IODataRequestMessage requestMessage, ODataFormat format, ODataVersion version, Uri baseAddress, string serviceOperationName)
        {
            if (requestMessage == null)
            {
                throw Error.ArgumentNull("requestMessage");
            }

            if (baseAddress == null)
            {
                throw Error.ArgumentNull("baseAddress");
            }

            if (String.IsNullOrEmpty(serviceOperationName))
            {
                throw Error.ArgumentNullOrEmpty("serviceOperationName");
            }

            this._requestMessage = requestMessage;
            this.ODataFormat = format;
            this.ODataVersion = version;
            this._baseAddress = baseAddress;
            this._serviceOperationName = serviceOperationName;
            this.IsIndented = true;
        }

        /// <summary>
        /// Gets or sets the instance of IODataResponseMessage which gives information
        /// such as contentType, ODataFormatVersion, stream etc.
        /// </summary>
        public IODataResponseMessage ODataResponseMessage
        {
            get { return this._responseMessage; }

            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                this._responseMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the instance of IODataRequestMessage which gives information
        /// such as contentType, ODataFormatVersion, stream etc.
        /// </summary>
        public IODataRequestMessage ODataRequestMessage
        {
            get { return this._requestMessage; }

            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                this._requestMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the ODataFormat that should be used for writing the content payload
        /// </summary>
        public ODataFormat ODataFormat { get; set; }

        /// <summary>
        /// Gets or sets the DataServiceVersion that would be used by <see cref="ODataMessageWriter"/>.
        /// </summary>
        public ODataVersion ODataVersion { get; set; }

        /// <summary>
        /// Gets or sets the BaseAddress which is used when writing an entry or feed.
        /// </summary>
        public Uri BaseAddress
        {
            get { return this._baseAddress; }

            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                this._baseAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the ServiceOperationName which is used when writing primitive types
        /// and complex types.
        /// </summary>
        public string ServiceOperationName
        {
            get { return this._serviceOperationName; }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw Error.ArgumentNullOrEmpty("value");
                }

                this._serviceOperationName = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the serialized content should be indented.
        /// </summary>
        public bool IsIndented { get; set; }
    }
}
