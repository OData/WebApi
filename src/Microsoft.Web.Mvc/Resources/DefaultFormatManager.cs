// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Web.Mvc.Resources
{
    public class DefaultFormatManager : FormatManager
    {
        public DefaultFormatManager()
        {
            XmlFormatHandler xmlHandler = new XmlFormatHandler();
            JsonFormatHandler jsonHandler = new JsonFormatHandler();
            this.RequestFormatHandlers.Add(xmlHandler);
            this.RequestFormatHandlers.Add(jsonHandler);
            this.ResponseFormatHandlers.Add(xmlHandler);
            this.ResponseFormatHandlers.Add(jsonHandler);
        }
    }
}
