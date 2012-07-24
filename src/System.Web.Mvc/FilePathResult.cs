// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class FilePathResult : FileResult
    {
        public FilePathResult(string fileName, string contentType)
            : base(contentType)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "fileName");
            }

            FileName = fileName;
        }

        public string FileName { get; private set; }

        protected override void WriteFile(HttpResponseBase response)
        {
            response.TransmitFile(FileName);
        }
    }
}
